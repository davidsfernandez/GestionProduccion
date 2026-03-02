using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionProduccion.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTeamRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SewingTeamMembers");

            migrationBuilder.AddColumn<int>(
                name: "SewingTeamId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_SewingTeamId",
                table: "Users",
                column: "SewingTeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_SewingTeams_SewingTeamId",
                table: "Users",
                column: "SewingTeamId",
                principalTable: "SewingTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_SewingTeams_SewingTeamId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_SewingTeamId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SewingTeamId",
                table: "Users");

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
                name: "IX_SewingTeamMembers_TeamsId",
                table: "SewingTeamMembers",
                column: "TeamsId");
        }
    }
}
