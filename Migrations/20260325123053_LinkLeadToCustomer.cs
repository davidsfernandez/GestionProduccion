using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionProduccion.Migrations
{
    /// <inheritdoc />
    public partial class LinkLeadToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerUserId",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CustomerUserId",
                table: "Leads",
                column: "CustomerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_Users_CustomerUserId",
                table: "Leads",
                column: "CustomerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leads_Users_CustomerUserId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_CustomerUserId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "CustomerUserId",
                table: "Leads");
        }
    }
}
