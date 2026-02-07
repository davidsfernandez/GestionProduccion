namespace GestionProduccion.Models.DTOs
{
    public class FilterProductionOrderDto
    {
        public string? ProductDescription { get; set; }
        public string? CurrentStage { get; set; }
        public string? CurrentStatus { get; set; }
        public int? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
