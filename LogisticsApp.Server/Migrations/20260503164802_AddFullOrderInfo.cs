using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogisticsApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddFullOrderInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "AmountOfFuelPoured",
                table: "Orders",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

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

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateFinish",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateStart",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Destination",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "FuelConsumption",
                table: "Orders",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FuelOnFinish",
                table: "Orders",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "FuelOnStart",
                table: "Orders",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfRefills",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "RouteLengthKm",
                table: "Orders",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "TypeOfCargo",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WatercraftModel",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WatercraftNumber",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "WeightTons",
                table: "Orders",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AmountOfFuelPoured", "CrewMember1", "CrewMember2", "CrewMember3", "CustomerName", "DateFinish", "DateStart", "Destination", "FuelConsumption", "FuelOnFinish", "FuelOnStart", "NumberOfRefills", "RouteLengthKm", "TypeOfCargo", "WatercraftModel", "WatercraftNumber", "WeightTons" },
                values: new object[] { 0.0, "", "", "", "", null, null, "", 0.0, 0.0, 0.0, 0, 0.0, "", "", "", 0.0 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AmountOfFuelPoured", "CrewMember1", "CrewMember2", "CrewMember3", "CustomerName", "DateFinish", "DateStart", "Destination", "FuelConsumption", "FuelOnFinish", "FuelOnStart", "NumberOfRefills", "RouteLengthKm", "TypeOfCargo", "WatercraftModel", "WatercraftNumber", "WeightTons" },
                values: new object[] { 0.0, "", "", "", "", null, null, "", 0.0, 0.0, 0.0, 0, 0.0, "", "", "", 0.0 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AmountOfFuelPoured", "CrewMember1", "CrewMember2", "CrewMember3", "CustomerName", "DateFinish", "DateStart", "Destination", "FuelConsumption", "FuelOnFinish", "FuelOnStart", "NumberOfRefills", "RouteLengthKm", "TypeOfCargo", "WatercraftModel", "WatercraftNumber", "WeightTons" },
                values: new object[] { 0.0, "", "", "", "", null, null, "", 0.0, 0.0, 0.0, 0, 0.0, "", "", "", 0.0 });

            migrationBuilder.UpdateData(
                table: "Orders",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AmountOfFuelPoured", "CrewMember1", "CrewMember2", "CrewMember3", "CustomerName", "DateFinish", "DateStart", "Destination", "FuelConsumption", "FuelOnFinish", "FuelOnStart", "NumberOfRefills", "RouteLengthKm", "TypeOfCargo", "WatercraftModel", "WatercraftNumber", "WeightTons" },
                values: new object[] { 0.0, "", "", "", "", null, null, "", 0.0, 0.0, 0.0, 0, 0.0, "", "", "", 0.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountOfFuelPoured",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CrewMember1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CrewMember2",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CrewMember3",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DateFinish",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DateStart",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Destination",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FuelConsumption",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FuelOnFinish",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "FuelOnStart",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "NumberOfRefills",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RouteLengthKm",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TypeOfCargo",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WatercraftModel",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WatercraftNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WeightTons",
                table: "Orders");
        }
    }
}
