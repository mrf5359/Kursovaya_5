using Microsoft.EntityFrameworkCore;
using LogisticsApp.Server.Models;

namespace LogisticsApp.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<CrewMember> CrewMembers { get; set; }
        public DbSet<User> Users { get; set; }
        //Тестовые данные
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>().HasData(
                new Order
                {
                    Id = 1,
                    OrderNumber = "ORD-001",
                    BriefDescription = "Перевезення вугілля",
                    WatercraftModel = "Буксир-штовхач",
                    WatercraftNumber = "БШ-45",
                    Destination = "Порт А",
                    TypeOfCargo = "Вугілля",
                    WeightTons = 500,
                    CustomerName = "ТОВ ВугілляПром",
                    FuelOnStart = 1000,
                    FuelOnFinish = 800,
                    FuelConsumption = 200,
                    DateStart = new DateTime(2026, 5, 1),
                    DateFinish = new DateTime(2026, 5, 3)
                }
            );
            modelBuilder.Entity<CrewMember>().HasData(
                new CrewMember { Id = 1, FullName = "Іванов І.І. (Капітан)", OrderId = 1 },
                new CrewMember { Id = 2, FullName = "Петров П.П. (Механік)", OrderId = 1 },
                new CrewMember { Id = 3, FullName = "Сидоров С.С. (Матрос)", OrderId = 1 }
            );
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Password = "123", Role = "Admin" },
                new User { Id = 2, Username = "super", Password = "123", Role = "SuperUser" },
                new User { Id = 3, Username = "user", Password = "123", Role = "User" }
            );
        }
    }
}