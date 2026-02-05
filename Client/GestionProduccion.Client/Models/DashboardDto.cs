namespace GestionProduccion.Client.Models
{
    public class DashboardDto
    {
        public Dictionary<string, int> OpsPorEtapa { get; set; } = new();
        public List<OpAlertaDto> OpsParadas { get; set; } = new();
        public List<OpPorUsuarioDto> CargaTrabalhoPorUsuario { get; set; } = new();
    }

    public class OpAlertaDto
    {
        public int Id { get; set; }
        public string CodigoUnico { get; set; } = string.Empty;
        public string DescricaoProduto { get; set; } = string.Empty;
        public DateTime DataEstimadaEntrega { get; set; }
    }

    public class OpPorUsuarioDto
    {
        public string NomeUsuario { get; set; } = string.Empty;
        public int QuantidadeOps { get; set; }
    }
}
