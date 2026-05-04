using LogisticsApp.Server.Data;
using LogisticsApp.Server.Services;
using LogisticsApp.Shared.Protos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace LogisticsApp.Tests
{
    public class LogisticsServiceTests
    {
        // Допоміжний метод: створює порожню базу даних у пам'яті комп'ютера для кожного тесту
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        // Допоміжний метод: створює фейковий конфіг, щоб сервіс не скаржився на відсутність ключів
        private IConfiguration GetMockConfig()
        {
            return new Mock<IConfiguration>().Object;
        }

        [Fact]
        public async Task SaveOrder_ShouldAddNewOrder_WhenIdIsZero()
        {
            // Arrange (Підготовка)
            var db = GetInMemoryDbContext();
            var service = new LogisticsServiceImpl(db, GetMockConfig());
            var request = new SaveOrderRequest
            {
                Id = 0, // 0 означає створення нового
                OrderNumber = "TEST-001",
                WatercraftModel = "Баржа",
                FuelConsumption = 150.5
            };

            // Act (Дія)
            // ServerCallContext передаємо як null, бо для цих тестів він не потрібен
            var response = await service.SaveOrder(request, null);

            // Assert (Перевірка результату)
            Assert.True(response.Success);

            // Перевіряємо, чи реально запис з'явився у фейковій базі
            var orderInDb = await db.Orders.FirstOrDefaultAsync();
            Assert.NotNull(orderInDb);
            Assert.Equal("TEST-001", orderInDb.OrderNumber);
        }

        [Fact]
        public async Task GetOrdersList_ShouldReturnAllOrders()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var service = new LogisticsServiceImpl(db, GetMockConfig());

            // Додаємо два тестових замовлення в пам'ять
            db.Orders.Add(new Server.Models.Order { OrderNumber = "ORD-1", WatercraftModel = "Катер" });
            db.Orders.Add(new Server.Models.Order { OrderNumber = "ORD-2", WatercraftModel = "Буксир" });
            await db.SaveChangesAsync();

            // Act
            var response = await service.GetOrdersList(new SearchRequest(), null);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(2, response.Orders.Count); // Має повернути рівно 2 записи
        }

        // ЛАБА 6: СТРЕС-ТЕСТУВАННЯ
        [Fact]
        public async Task StressTest_ShouldHandle1000RequestsQuickly()
        {
            // Arrange
            var db = GetInMemoryDbContext();
            var service = new LogisticsServiceImpl(db, GetMockConfig());

            // Додаємо один запис
            db.Orders.Add(new Server.Models.Order { OrderNumber = "STRESS-TEST" });
            await db.SaveChangesAsync();

            int numberOfRequests = 1000;
            var request = new SearchRequest();

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Симулюємо 1000 запитів до сервера підряд
            for (int i = 0; i < numberOfRequests; i++)
            {
                var response = await service.GetOrdersList(request, null);
                Assert.NotEmpty(response.Orders); // Перевіряємо, що сервер не впав і віддав дані
            }

            stopwatch.Stop();

            // Assert
            // Переконуємося, що 1000 запитів обробилися швидше, ніж за 3 секунди (зазвичай це займає мілісекунди)
            Assert.True(stopwatch.ElapsedMilliseconds < 3000, $"Стрес-тест провалився. Час: {stopwatch.ElapsedMilliseconds} мс");
        }
    }
}