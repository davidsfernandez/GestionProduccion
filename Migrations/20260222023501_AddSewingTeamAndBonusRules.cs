using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionProduccion.Migrations
{
    /// <inheritdoc />
    public partial class AddSewingTeamAndBonusRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SewingTeamId",
                table: "ProductionOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BonusRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductivityPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DeadlineBonusPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DefectLimitPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DelayPenaltyPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonusRules", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SewingTeams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SewingTeams", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SewingTeamMembers",
                columns: table => new
                {
                    MembersId = table.Column<int>(type: "int", nullable: false),
                    TeamsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SewingTeamMembers", x => new { x.MembersId, x.TeamsId });
                    table.ForeignKey(
                        name: "FK_SewingTeamMembers_SewingTeams_TeamsId",
                        column: x => x.TeamsId,
                        principalTable: "SewingTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SewingTeamMembers_Users_MembersId",
                        column: x => x.MembersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionOrders_SewingTeamId",
                table: "ProductionOrders",
                column: "SewingTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_SewingTeamMembers_TeamsId",
                table: "SewingTeamMembers",
                column: "TeamsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionOrders_SewingTeams_SewingTeamId",
                table: "ProductionOrders",
                column: "SewingTeamId",
                principalTable: "SewingTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionOrders_SewingTeams_SewingTeamId",
                table: "ProductionOrders");

            migrationBuilder.DropTable(
                name: "BonusRules");

            migrationBuilder.DropTable(
                name: "SewingTeamMembers");

            migrationBuilder.DropTable(
                name: "SewingTeams");

            migrationBuilder.DropIndex(
                name: "IX_ProductionOrders_SewingTeamId",
                table: "ProductionOrders");

            migrationBuilder.DropColumn(
                name: "SewingTeamId",
                table: "ProductionOrders");
        }
    }
}
