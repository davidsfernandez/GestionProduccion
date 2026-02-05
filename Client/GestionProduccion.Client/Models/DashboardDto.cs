namespace GestionProduccion.Client.Models
{
    public class DashboardDto
    {
        public Dictionary<string, int> OpsPorEtapa { get; set; }
        public List<OpAlertaDto> OpsParadas { get; set; }
        public List<OpPorUsuarioDto> CargaTrabalhoPorUsuario { get; set; }
    }

    public class OpAlertaDto
    {
        public int Id { get; set; }
        public string CodigoUnico { get; set; }
        public string DescricaoProduto { get; set; }
        public DateTime DataEstimadaEntrega { get; set; }
    }

    public class OpPorUsuarioDto
    {
        public string NomeUsuario { get; set; }
        public int QuantidadeOps { get; set; }
    }
}
