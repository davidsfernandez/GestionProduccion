using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionProduccion.Migrations
{
    /// <inheritdoc />
    public partial class AddResponsibleUserIdToQADefects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResponsibleUserId",
                table: "QADefects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QADefects_ResponsibleUserId",
                table: "QADefects",
                column: "ResponsibleUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_QADefects_Users_ResponsibleUserId",
                table: "QADefects",
                column: "ResponsibleUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QADefects_Users_ResponsibleUserId",
                table: "QADefects");

            migrationBuilder.DropIndex(
                name: "IX_QADefects_ResponsibleUserId",
                table: "QADefects");

            migrationBuilder.DropColumn(
                name: "ResponsibleUserId",
                table: "QADefects");
        }
    }
}
