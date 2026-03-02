using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionProduccion.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeExistingData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- 1. Normalize Roles in Users table ---
            migrationBuilder.Sql("UPDATE Users SET Role = 'Administrator' WHERE Role IN ('Administrador', 'Admin');");
            migrationBuilder.Sql("UPDATE Users SET Role = 'Leader' WHERE Role IN ('Líder', 'Lider');");
            migrationBuilder.Sql("UPDATE Users SET Role = 'Operational' WHERE Role IN ('Costureira', 'Costureiro', 'Operacional', 'Operator');");
            migrationBuilder.Sql("UPDATE Users SET Role = 'Office' WHERE Role IN ('Oficina', 'Escritório');");

            // --- 2. Normalize CurrentStage in ProductionOrders ---
            migrationBuilder.Sql("UPDATE ProductionOrders SET CurrentStage = 'Cutting' WHERE CurrentStage IN ('Corte');");
            migrationBuilder.Sql("UPDATE ProductionOrders SET CurrentStage = 'Sewing' WHERE CurrentStage IN ('Costura');");
            migrationBuilder.Sql("UPDATE ProductionOrders SET CurrentStage = 'Review' WHERE CurrentStage IN ('Revisão', 'Revisao', 'Review');");
            migrationBuilder.Sql("UPDATE ProductionOrders SET CurrentStage = 'Packaging' WHERE CurrentStage IN ('Embalagem', 'Embalado');");

            // --- 3. Normalize CurrentStatus in ProductionOrders ---
            migrationBuilder.Sql("UPDATE ProductionOrders SET CurrentStatus = 'Pending' WHERE CurrentStatus IN ('Pendente');");
            migrationBuilder.Sql("UPDATE ProductionOrders SET CurrentStatus = 'InProduction' WHERE CurrentStatus IN ('Em Produção', 'Em Producao', 'In Production');");
            migrationBuilder.Sql("UPDATE ProductionOrders SET CurrentStatus = 'Paused' WHERE CurrentStatus IN ('Pausado');");
            migrationBuilder.Sql("UPDATE ProductionOrders SET CurrentStatus = 'Completed' WHERE CurrentStatus IN ('Finalizado', 'Concluído', 'Concluido');");
            migrationBuilder.Sql("UPDATE ProductionOrders SET CurrentStatus = 'Stopped' WHERE CurrentStatus IN ('Parado');");

            // --- 4. Normalize History Stages and Status ---
            migrationBuilder.Sql("UPDATE ProductionHistories SET PreviousStage = 'Cutting' WHERE PreviousStage IN ('Corte');");
            migrationBuilder.Sql("UPDATE ProductionHistories SET PreviousStage = 'Sewing' WHERE PreviousStage IN ('Costura');");
            migrationBuilder.Sql("UPDATE ProductionHistories SET PreviousStage = 'Review' WHERE PreviousStage IN ('Revisão', 'Revisao');");
            migrationBuilder.Sql("UPDATE ProductionHistories SET PreviousStage = 'Packaging' WHERE PreviousStage IN ('Embalagem');");

            migrationBuilder.Sql("UPDATE ProductionHistories SET NewStage = 'Cutting' WHERE NewStage IN ('Corte');");
            migrationBuilder.Sql("UPDATE ProductionHistories SET NewStage = 'Sewing' WHERE NewStage IN ('Costura');");
            migrationBuilder.Sql("UPDATE ProductionHistories SET NewStage = 'Review' WHERE NewStage IN ('Revisão', 'Revisao');");
            migrationBuilder.Sql("UPDATE ProductionHistories SET NewStage = 'Packaging' WHERE NewStage IN ('Embalagem');");

            migrationBuilder.Sql("UPDATE ProductionHistories SET PreviousStatus = 'Pending' WHERE PreviousStatus IN ('Pendente');");
            migrationBuilder.Sql("UPDATE ProductionHistories SET PreviousStatus = 'InProduction' WHERE PreviousStatus IN ('Em Produção', 'Em Producao');");
            migrationBuilder.Sql("UPDATE ProductionHistories SET PreviousStatus = 'Paused' WHERE PreviousStatus IN ('Pausado');");
            migrationBuilder.Sql("UPDATE ProductionHistories SET PreviousStatus = 'Completed' WHERE PreviousStatus IN ('Finalizado', 'Concluído');");
            migrationBuilder.Sql("UPDATE ProductionHistories SET PreviousStatus = 'Stopped' WHERE PreviousStatus IN ('Parado');");

            migrationBuilder.Sql("UPDATE ProductionHistories SET NewStatus = 'Pending' WHERE NewStatus IN ('Pendente');");
            migrationBuilder.Sql("UPDATE ProductionHistories SET NewStatus = 'InProduction' WHERE NewStatus IN ('Em Produção', 'Em Producao');");
            migrationBuilder.Sql("UPDATE ProductionHistories SET NewStatus = 'Paused' WHERE NewStatus IN ('Pausado');");
            migrationBuilder.Sql("UPDATE ProductionHistories SET NewStatus = 'Completed' WHERE NewStatus IN ('Finalizado', 'Concluído');");
            migrationBuilder.Sql("UPDATE ProductionHistories SET NewStatus = 'Stopped' WHERE NewStatus IN ('Parado');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
