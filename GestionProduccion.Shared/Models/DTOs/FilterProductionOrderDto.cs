namespace GestionProduccion.Models.DTOs
{
    public class FilterProductionOrderDto
    {
        public string? CurrentStage { get; set; }
        public string? CurrentStatus { get; set; }
        public int? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ClientName { get; set; }
        public string? Tamanho { get; set; }
    }
}
