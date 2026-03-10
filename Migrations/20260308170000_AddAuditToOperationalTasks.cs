using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionProduccion.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditToOperationalTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "OperationalTasks",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "LastModifiedByUserId",
                table: "OperationalTasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "OperationalTasks",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OperationalTaskHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OperationalTaskId = table.Column<int>(type: "int", nullable: false),
                    PreviousStatus = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewStatus = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationalTaskHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationalTaskHistories_OperationalTasks_OperationalTaskId",
                        column: x => x.OperationalTaskId,
                        principalTable: "OperationalTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OperationalTaskHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalTasks_LastModifiedByUserId",
                table: "OperationalTasks",
                column: "LastModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalTaskHistories_OperationalTaskId",
                table: "OperationalTaskHistories",
                column: "OperationalTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalTaskHistories_UserId",
                table: "OperationalTaskHistories",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_OperationalTasks_Users_LastModifiedByUserId",
                table: "OperationalTasks",
                column: "LastModifiedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OperationalTasks_Users_LastModifiedByUserId",
                table: "OperationalTasks");

            migrationBuilder.DropTable(
                name: "OperationalTaskHistories");

            migrationBuilder.DropIndex(
                name: "IX_OperationalTasks_LastModifiedByUserId",
                table: "OperationalTasks");

            migrationBuilder.DropColumn(
                name: "LastModifiedByUserId",
                table: "OperationalTasks");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "OperationalTasks");
        }
    }
}
