using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionProduccion.Migrations
{
    /// <inheritdoc />
    public partial class FixAdminSeedAndPublicId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PublicId" },
                values: new object[] { "$2a$11$SiQ7dTsxEyIkkZgM83lzDuWMUSxBUesGCZwHBkRSYw292e7cdpn4y", "ADMIN-001" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PublicId" },
                values: new object[] { "$2a$11$u6KtHoo4cnQScZivLwRwH.eUBdFKbjTeIx60H3j9/19nxtagmQTiK", "" });
        }
    }
}
