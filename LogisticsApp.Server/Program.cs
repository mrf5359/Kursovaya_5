using LogisticsApp.Server.Data;
using LogisticsApp.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=LogisticsDB;Trusted_Connection=True;"));

var app = builder.Build();
app.MapGrpcService<LogisticsServiceImpl>();
app.MapGet("/", () => "gRPC Server is running.");
app.Run();