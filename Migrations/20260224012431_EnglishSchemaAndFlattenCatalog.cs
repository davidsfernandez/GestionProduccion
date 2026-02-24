using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionProduccion.Migrations
{
    /// <inheritdoc />
    public partial class EnglishSchemaAndFlattenCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QADefects_Users_ReportedByUserId",
                table: "QADefects");

            migrationBuilder.DropIndex(
                name: "IX_QADefects_ReportedByUserId",
                table: "QADefects");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "SewingTeams");

            migrationBuilder.DropColumn(
                name: "ReportedByUserId",
                table: "QADefects");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Users",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "RegistrationDate",
                table: "QADefects",
                newName: "ReportedAt");

            migrationBuilder.RenameColumn(
                name: "UniqueCode",
                table: "ProductionOrders",
                newName: "LotCode");

            migrationBuilder.RenameColumn(
                name: "Tamanho",
                table: "ProductionOrders",
                newName: "Size");

            migrationBuilder.RenameColumn(
                name: "ModificationDate",
                table: "ProductionOrders",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "EstimatedProfitMargin",
                table: "ProductionOrders",
                newName: "TotalCost");

            migrationBuilder.RenameColumn(
                name: "EstimatedDeliveryDate",
                table: "ProductionOrders",
                newName: "EstimatedCompletionAt");

            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "ProductionOrders",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CalculatedTotalCost",
                table: "ProductionOrders",
                newName: "ProfitMargin");

            migrationBuilder.RenameColumn(
                name: "ActualStartDate",
                table: "ProductionOrders",
                newName: "StartedAt");

            migrationBuilder.RenameColumn(
                name: "ActualEndDate",
                table: "ProductionOrders",
                newName: "CompletedAt");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionOrders_UniqueCode",
                table: "ProductionOrders",
                newName: "IX_ProductionOrders_LotCode");

            migrationBuilder.RenameColumn(
                name: "ModificationDate",
                table: "ProductionHistories",
                newName: "ChangedAt");

            migrationBuilder.RenameColumn(
                name: "CreationDate",
                table: "OperationalTasks",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "LastUpdate",
                table: "BonusRules",
                newName: "UpdatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(250)",
                oldMaxLength: 250,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "QADefects",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "PhotoUrl",
                table: "QADefects",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsResolved",
                table: "QADefects",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "OperationalTasks",
                type: "int",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "OperationalTasks",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "AssignedUserId",
                table: "OperationalTasks",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "ProductivityPercentage",
                table: "BonusRules",
                type: "double",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DelayPenaltyPercentage",
                table: "BonusRules",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DefectLimitPercentage",
                table: "BonusRules",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DeadlineBonusPercentage",
                table: "BonusRules",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "BonusAmount",
                table: "BonusRules",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "BonusRules",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsResolved",
                table: "QADefects");

            migrationBuilder.DropColumn(
                name: "BonusAmount",
                table: "BonusRules");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "BonusRules");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Users",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ReportedAt",
                table: "QADefects",
                newName: "RegistrationDate");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "ProductionOrders",
                newName: "ModificationDate");

            migrationBuilder.RenameColumn(
                name: "TotalCost",
                table: "ProductionOrders",
                newName: "EstimatedProfitMargin");

            migrationBuilder.RenameColumn(
                name: "StartedAt",
                table: "ProductionOrders",
                newName: "ActualStartDate");

            migrationBuilder.RenameColumn(
                name: "Size",
                table: "ProductionOrders",
                newName: "Tamanho");

            migrationBuilder.RenameColumn(
                name: "ProfitMargin",
                table: "ProductionOrders",
                newName: "CalculatedTotalCost");

            migrationBuilder.RenameColumn(
                name: "LotCode",
                table: "ProductionOrders",
                newName: "UniqueCode");

            migrationBuilder.RenameColumn(
                name: "EstimatedCompletionAt",
                table: "ProductionOrders",
                newName: "EstimatedDeliveryDate");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ProductionOrders",
                newName: "CreationDate");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "ProductionOrders",
                newName: "ActualEndDate");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionOrders_LotCode",
                table: "ProductionOrders",
                newName: "IX_ProductionOrders_UniqueCode");

            migrationBuilder.RenameColumn(
                name: "ChangedAt",
                table: "ProductionHistories",
                newName: "ModificationDate");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "OperationalTasks",
                newName: "CreationDate");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "BonusRules",
                newName: "LastUpdate");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "Users",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "SewingTeams",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "QADefects",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "PhotoUrl",
                table: "QADefects",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ReportedByUserId",
                table: "QADefects",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "OperationalTasks",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "OperationalTasks",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "AssignedUserId",
                table: "OperationalTasks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ProductivityPercentage",
                table: "BonusRules",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<decimal>(
                name: "DelayPenaltyPercentage",
                table: "BonusRules",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DefectLimitPercentage",
                table: "BonusRules",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DeadlineBonusPercentage",
                table: "BonusRules",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.CreateIndex(
                name: "IX_QADefects_ReportedByUserId",
                table: "QADefects",
                column: "ReportedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_QADefects_Users_ReportedByUserId",
                table: "QADefects",
                column: "ReportedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
