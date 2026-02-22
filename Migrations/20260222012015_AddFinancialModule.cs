using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionProduccion.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrders_Products_ProductId",
                table: "ProductionOrders");

            migrationBuilder.AddColumn<decimal>(
                name: "DailyFixedCost",
                table: "SystemConfigurations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OperationalHourlyCost",
                table: "SystemConfigurations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedSalePrice",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualEndDate",
                table: "ProductionOrders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualStartDate",
                table: "ProductionOrders",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AverageCostPerPiece",
                table: "ProductionOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CalculatedTotalCost",
                table: "ProductionOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedProfitMargin",
                table: "ProductionOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrders_Products_ProductId",
                table: "ProductionOrders",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrders_Products_ProductId",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "DailyFixedCost",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "OperationalHourlyCost",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "EstimatedSalePrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ActualEndDate",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "ActualStartDate",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "AverageCostPerPiece",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "CalculatedTotalCost",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "EstimatedProfitMargin",
                table: "ProductionOrders");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrders_Products_ProductId",
                table: "ProductionOrders",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
