using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogisticsApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddFullOrderInfo2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DateFinish", "DateStart" },
                values: new object[] { new DateTime(2026, 5, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DateFinish", "DateStart" },
                values: new object[] { new DateTime(2026, 5, 3, 19, 59, 52, 88, DateTimeKind.Local).AddTicks(8220), new DateTime(2026, 5, 1, 19, 59, 52, 85, DateTimeKind.Local).AddTicks(1183) });
        }
    }
}
