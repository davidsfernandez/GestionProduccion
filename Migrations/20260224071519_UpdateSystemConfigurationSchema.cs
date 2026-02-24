using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionProduccion.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSystemConfigurationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Logo",
                table: "SystemConfigurations",
                newName: "LogoBase64");

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "SystemConfigurations",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CompanyTaxId",
                table: "SystemConfigurations",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "CompanyTaxId",
                table: "SystemConfigurations");

            migrationBuilder.RenameColumn(
                name: "LogoBase64",
                table: "SystemConfigurations",
                newName: "Logo");
        }
    }
}
