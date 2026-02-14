using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionProduccion.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingOrderFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientName",
                table: "ProductionOrders",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModificationDate",
                table: "ProductionOrders",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "ProductionOrders",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$u6KtHoo4cnQScZivLwRwH.eUBdFKbjTeIx60H3j9/19nxtagmQTiK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientName",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "ModificationDate",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "ProductionOrders");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$SiQ7dTsxEyIkkZgM83lzDuWMUSxBUesGCZwHBkRSYw292e7cdpn4y");
        }
    }
}
