using Grpc.Core;
using LogisticsApp.Server.Data;
using LogisticsApp.Server.Models;
using LogisticsApp.Shared.Protos;
using Microsoft.EntityFrameworkCore;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace LogisticsApp.Server.Services
{
    public class LogisticsServiceImpl : LogisticsService.LogisticsServiceBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        public LogisticsServiceImpl(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }
        public override async Task<OrdersListResponse> GetOrdersList(SearchRequest request, ServerCallContext context)
        {
            var response = new OrdersListResponse();
            //Запрос в базу
            var query = _db.Orders.AsQueryable();
            //Если фильтры есть - фильтруем. Если нет - вернуть все
            if (!string.IsNullOrWhiteSpace(request.OrderNumber))
                query = query.Where(o => o.OrderNumber != null && o.OrderNumber.Contains(request.OrderNumber));
            if (!string.IsNullOrWhiteSpace(request.WatercraftNumber))
                query = query.Where(o => o.WatercraftNumber != null && o.WatercraftNumber.Contains(request.WatercraftNumber));
            if (!string.IsNullOrWhiteSpace(request.WatercraftModel))
                query = query.Where(o => o.WatercraftModel == request.WatercraftModel);
            if (!string.IsNullOrWhiteSpace(request.CrewMemberName))
                query = query.Where(o => o.Crew.Any(c => c.FullName.Contains(request.CrewMemberName)));
            if (!string.IsNullOrWhiteSpace(request.CustomerName))
                query = query.Where(o => o.CustomerName != null && o.CustomerName.Contains(request.CustomerName));
            if (!string.IsNullOrWhiteSpace(request.TypeOfCargo))
                query = query.Where(o => o.TypeOfCargo == request.TypeOfCargo);
            if (DateTime.TryParse(request.DateStart, out DateTime dStart))
                query = query.Where(o => o.DateStart >= dStart);
            if (DateTime.TryParse(request.DateFinish, out DateTime dFinish))
                query = query.Where(o => o.DateFinish <= dFinish);
            // Выполнять
            var orders = await query.ToListAsync();
            foreach (var order in orders)
            {
                response.Orders.Add(new OrderPreview
                {
                    Id = order.Id.ToString(),
                    OrderNumber = order.OrderNumber ?? "",
                    BriefDescription = order.BriefDescription ?? ""
                });
            }
            return response;
        }
        public override async Task<OrderDetailsResponse> GetOrderDetails(OrderIdRequest request, ServerCallContext context)
        {
            // Ищем заказ в базе.
            var order = await _db.Orders
                .Include(o => o.Crew)
                .FirstOrDefaultAsync(o => o.Id == request.Id);
            if (order == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Замовлення з ID {request.Id} не знайдено"));
            }
            // Перекладываем из сущности БД в ответ gRPC
            var response = new OrderDetailsResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber ?? "",
                BriefDescription = order.BriefDescription ?? "",
                WatercraftModel = order.WatercraftModel ?? "",
                WatercraftNumber = order.WatercraftNumber ?? "",
                DateStart = order.DateStart?.ToString("dd.MM.yyyy") ?? "",
                DateFinish = order.DateFinish?.ToString("dd.MM.yyyy") ?? "",
                Destination = order.Destination ?? "",
                TypeOfCargo = order.TypeOfCargo ?? "",
                WeightTons = order.WeightTons,
                CustomerName = order.CustomerName ?? "",
                RouteLengthKm = order.RouteLengthKm,
                FuelOnStart = order.FuelOnStart,
                FuelOnFinish = order.FuelOnFinish,
                FuelConsumption = order.FuelConsumption,
                NumberOfRefills = order.NumberOfRefills,
                AmountOfFuelPoured = order.AmountOfFuelPoured
            };
            // Добавляем экипаж
            foreach (var member in order.Crew)
            {
                response.CrewMembers.Add(member.FullName);
            }
            return response;
        }

        // Создать\обновить заказ (ID=0 - новый, ID > 0 - обновить)
        public override async Task<SaveOrderResponse> SaveOrder(SaveOrderRequest request, ServerCallContext context)
        {
            try
            {
                Order order;
                if (request.Id == 0)
                {
                    order = new Order();
                    _db.Orders.Add(order);
                }
                else
                {
                    order = await _db.Orders
                        .Include(o => o.Crew)
                        .FirstOrDefaultAsync(o => o.Id == request.Id);

                    if (order == null)
                    {
                        return new SaveOrderResponse { Success = false, Message = "Замовлення не знайдено в базі." };
                    }
                    _db.CrewMembers.RemoveRange(order.Crew);
                }
                order.OrderNumber = request.OrderNumber;
                order.BriefDescription = request.BriefDescription;
                order.WatercraftModel = request.WatercraftModel;
                order.WatercraftNumber = request.WatercraftNumber;
                if (DateTime.TryParse(request.DateStart, out DateTime start)) order.DateStart = start;
                if (DateTime.TryParse(request.DateFinish, out DateTime finish)) order.DateFinish = finish;
                order.Destination = request.Destination;
                order.TypeOfCargo = request.TypeOfCargo;
                order.WeightTons = request.WeightTons;
                order.CustomerName = request.CustomerName;
                order.RouteLengthKm = request.RouteLengthKm;
                order.FuelOnStart = request.FuelOnStart;
                order.FuelOnFinish = request.FuelOnFinish;
                order.FuelConsumption = request.FuelConsumption;
                order.NumberOfRefills = request.NumberOfRefills;
                order.AmountOfFuelPoured = request.AmountOfFuelPoured;
                order.Crew = new List<CrewMember>();
                foreach (var memberName in request.CrewMembers)
                {
                    order.Crew.Add(new CrewMember { FullName = memberName });
                }
                //Хоп и сохранили
                await _db.SaveChangesAsync();
                return new SaveOrderResponse
                {
                    Success = true,
                    NewOrderId = order.Id
                };
            }
            catch (Exception ex)
            {
                return new SaveOrderResponse { Success = false, Message = ex.Message };
            }
        }

        // Защита от дурака - получаем всех матросов
        public override async Task<CrewNamesResponse> GetUniqueCrewNames(EmptyRequest request, ServerCallContext context)
        {
            var uniqueNames = await _db.CrewMembers
                .Select(c => c.FullName)
                .Distinct()
                .ToListAsync();
            var response = new CrewNamesResponse();
            response.Names.AddRange(uniqueNames);
            return response;
        }
        //Чтоббы автоматом номера заказов генерировались
        public override async Task<NextOrderNumberResponse> GetNextOrderNumber(EmptyRequest request, ServerCallContext context)
        {
            // Берем последний
            var lastOrder = await _db.Orders.OrderByDescending(o => o.Id).FirstOrDefaultAsync();
            string nextNumber = "ORD-001"; // Если нема
            if (lastOrder != null && !string.IsNullOrEmpty(lastOrder.OrderNumber))
            {
                var parts = lastOrder.OrderNumber.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out int num))
                {
                    nextNumber = $"ORD-{(num + 1):D3}";
                }
            }
            return new NextOrderNumberResponse { NextNumber = nextNumber };
        }
        //Логин
        public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);
            if (user != null)
            {
                return new LoginResponse
                {
                    Success = true,
                    Username = user.Username,
                    Role = user.Role
                };
            }
            return new LoginResponse { Success = false, Message = "Невірний логін або пароль!" };
        }
        //Удалить заказ
        public override async Task<DeleteOrderResponse> DeleteOrder(OrderIdRequest request, ServerCallContext context)
        {
            try
            {
                var order = await _db.Orders
                    .Include(o => o.Crew)
                    .FirstOrDefaultAsync(o => o.Id == request.Id);
                if (order == null)
                {
                    return new DeleteOrderResponse { Success = false, Message = "Замовлення не знайдено в базі." };
                }
                _db.CrewMembers.RemoveRange(order.Crew);
                _db.Orders.Remove(order);
                await _db.SaveChangesAsync();
                return new DeleteOrderResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new DeleteOrderResponse { Success = false, Message = ex.Message };
            }
        }
        //Отчетики
        public override async Task<ReportResponse> GetReportData(ReportRequest request, ServerCallContext context)
        {
            var response = new ReportResponse();
            var allOrders = await _db.Orders.ToListAsync();
            //По суднам
            var summaries = allOrders
                .GroupBy(o => $"{o.WatercraftModel} {o.WatercraftNumber}")
                .Select(g => new WatercraftSummary
                {
                    WatercraftName = g.Key,
                    TotalTrips = g.Count(),
                    TotalFuelConsumed = g.Sum(o => o.FuelConsumption)
                });
            response.Summaries.AddRange(summaries);
            //По заправкам
            foreach (var order in allOrders)
            {
                if (order.NumberOfRefills > 0 || order.AmountOfFuelPoured > 0)
                {
                    response.Bunkerings.Add(new BunkeringDetail
                    {
                        OrderNumber = order.OrderNumber ?? "N/A",
                        Dates = $"{order.DateStart:dd.MM.yyyy} - {order.DateFinish:dd.MM.yyyy}",
                        WatercraftName = $"{order.WatercraftModel} {order.WatercraftNumber}",
                        Refills = order.NumberOfRefills,
                        PouredFuel = order.AmountOfFuelPoured
                    });
                }
            }
            return response;
        }
        //Конект к облаку
        private AmazonS3Client GetS3Client()
        {
            string accessKey = _config["CloudflareR2:AccessKey"];
            string secretKey = _config["CloudflareR2:SecretKey"];
            string serviceUrl = _config["CloudflareR2:ServiceUrl"];
            var config = new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                AuthenticationRegion = "auto"
            };
            return new AmazonS3Client(accessKey, secretKey, config);
        }
        //Грузим в облако
        public override async Task<UploadResponse> UploadDocument(UploadRequest request, ServerCallContext context)
        {
            try
            {
                string bucketName = _config["CloudflareR2:BucketName"];
                using var client = GetS3Client();
                using var ms = new MemoryStream(request.FileData.ToByteArray());
                string folderName = await GetOrderFolderAsync(request.OrderId);
                string objectKey = $"{folderName}/{request.FileName}";
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = objectKey,
                    InputStream = ms,
                    DisablePayloadSigning = true
                };
                await client.PutObjectAsync(putRequest);
                return new UploadResponse { Success = true, Message = "Файл успішно завантажено в Cloudflare R2!" };
            }
            catch (Exception ex)
            {
                return new UploadResponse { Success = false, Message = $"Помилка завантаження: {ex.Message}" };
            }
        }
        //Посмотреть что там в облаке
        public override async Task<DocumentsListResponse> GetOrderDocuments(OrderIdRequest request, ServerCallContext context)
        {
            var response = new DocumentsListResponse();
            try
            {
                string bucketName = _config["CloudflareR2:BucketName"];
                using var client = GetS3Client();
                string folderName = await GetOrderFolderAsync(request.Id);
                string prefix = $"{folderName}/";
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    Prefix = prefix
                };
                var listResponse = await client.ListObjectsV2Async(listRequest);
                foreach (var s3Obj in listResponse.S3Objects ?? new List<S3Object>())
                {
                    string url = client.GetPreSignedURL(new GetPreSignedUrlRequest
                    {
                        BucketName = bucketName,
                        Key = s3Obj.Key,
                        Expires = DateTime.UtcNow.AddHours(1)
                    });
                    response.Documents.Add(new DocumentInfo
                    {
                        FileName = s3Obj.Key.Replace(prefix, ""),
                        DownloadUrl = url
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка отримання списку файлів: {ex.Message}");
            }
            return response;
        }
        //Структуририем папки в облаке по номерам заказов
        private async Task<string> GetOrderFolderAsync(int orderId)
        {
            var order = await _db.Orders.FindAsync(orderId);
            string orderNum = string.IsNullOrWhiteSpace(order?.OrderNumber) ? orderId.ToString("D3") : order.OrderNumber;
            return $"ord-{orderNum}";
        }
        //Удалить из облака
        public override async Task<DeleteDocumentResponse> DeleteDocument(DeleteDocumentRequest request, ServerCallContext context)
        {
            try
            {
                string bucketName = _config["CloudflareR2:BucketName"];
                string folderName = await GetOrderFolderAsync(request.OrderId);
                string objectKey = $"{folderName}/{request.FileName}";
                using var client = GetS3Client();
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = objectKey
                };
                await client.DeleteObjectAsync(deleteRequest);
                return new DeleteDocumentResponse { Success = true, Message = "Файл успішно видалено з хмари!" };
            }
            catch (Exception ex)
            {
                return new DeleteDocumentResponse { Success = false, Message = $"Помилка видалення: {ex.Message}" };
            }
        }
    }
}