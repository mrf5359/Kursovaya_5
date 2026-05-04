using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LogisticsApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddFullOrderInfo1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "CrewMember1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CrewMember2",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CrewMember3",
                table: "Orders");

            migrationBuilder.CreateTable(
                name: "CrewMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewMembers_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CrewMembers",
                columns: new[] { "Id", "FullName", "OrderId" },
                values: new object[,]
                {
                    { 1, "Іванов І.І. (Капітан)", 1 },
                    { 2, "Петров П.П. (Механік)", 1 },
                    { 3, "Сидоров С.С. (Матрос)", 1 }
                });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BriefDescription", "CustomerName", "DateFinish", "DateStart", "Destination", "FuelConsumption", "FuelOnFinish", "FuelOnStart", "TypeOfCargo", "WatercraftModel", "WatercraftNumber", "WeightTons" },
                values: new object[] { "Перевезення вугілля", "ТОВ ВугілляПром", new DateTime(2026, 5, 3, 19, 59, 52, 88, DateTimeKind.Local).AddTicks(8220), new DateTime(2026, 5, 1, 19, 59, 52, 85, DateTimeKind.Local).AddTicks(1183), "Порт А", 200.0, 800.0, 1000.0, "Вугілля", "Буксир-штовхач", "БШ-45", 500.0 });

            migrationBuilder.CreateIndex(
                name: "IX_CrewMembers_OrderId",
                table: "CrewMembers",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrewMembers");

            migrationBuilder.AddColumn<string>(
                name: "CrewMember1",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CrewMember2",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CrewMember3",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BriefDescription", "CrewMember1", "CrewMember2", "CrewMember3", "CustomerName", "DateFinish", "DateStart", "Destination", "FuelConsumption", "FuelOnFinish", "FuelOnStart", "TypeOfCargo", "WatercraftModel", "WatercraftNumber", "WeightTons" },
                values: new object[] { "Перевезення вугілля, баржа 1", "", "", "", "", null, null, "", 0.0, 0.0, 0.0, "", "", "", 0.0 });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "AmountOfFuelPoured", "BriefDescription", "CrewMember1", "CrewMember2", "CrewMember3", "CustomerName", "DateFinish", "DateStart", "Destination", "FuelConsumption", "FuelOnFinish", "FuelOnStart", "NumberOfRefills", "OrderNumber", "RouteLengthKm", "TypeOfCargo", "WatercraftModel", "WatercraftNumber", "WeightTons" },
                values: new object[,]
                {
                    { 2, 0.0, "Бункерування буксира 'Богатир'", "", "", "", "", null, null, "", 0.0, 0.0, 0.0, 0, "ORD-002", 0.0, "", "", "", 0.0 },
                    { 3, 0.0, "Транспортування контейнерів", "", "", "", "", null, null, "", 0.0, 0.0, 0.0, 0, "ORD-003", 0.0, "", "", "", 0.0 },
                    { 4, 0.0, "Технічне обслуговування екіпажу", "", "", "", "", null, null, "", 0.0, 0.0, 0.0, 0, "ORD-004", 0.0, "", "", "", 0.0 }
                });
        }
    }
}
