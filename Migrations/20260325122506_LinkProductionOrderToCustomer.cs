using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionProduccion.Migrations
{
    /// <inheritdoc />
    public partial class LinkProductionOrderToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerUserId",
                table: "ProductionOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_CustomerUserId",
                table: "ProductionOrders",
                column: "CustomerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrders_Users_CustomerUserId",
                table: "ProductionOrders",
                column: "CustomerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrders_Users_CustomerUserId",
                table: "ProductionOrders");

            migrationBuilder.DropIndex(
                name: "IX_ProductionOrders_CustomerUserId",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "CustomerUserId",
                table: "ProductionOrders");
        }
    }
}
