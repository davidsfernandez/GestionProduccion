using GestionProduccion.Domain.Enums;

namespace GestionProduccion.Domain.Constants;

public static class TranslationHelper
{
    public static string TranslateStage(ProductionStage stage) => stage switch
    {
        ProductionStage.Cutting => "Corte",
        ProductionStage.Sewing => "Costura",
        ProductionStage.Review => "Revisão",
        ProductionStage.Packaging => "Embalagem",
        _ => stage.ToString()
    };

    public static string TranslateStatus(ProductionStatus status) => status switch
    {
        ProductionStatus.Pending => "Pendente",
        ProductionStatus.InProduction => "Em Produção",
        ProductionStatus.Stopped => "Parado",
        ProductionStatus.Completed => "Finalizado",
        ProductionStatus.Paused => "Pausado",
        ProductionStatus.Finished => "Concluído",
        ProductionStatus.Cancelled => "Cancelado",
        _ => status.ToString()
    };

    public static string TranslateStage(string? stage)
    {
        if (string.IsNullOrEmpty(stage)) return "";
        if (Enum.TryParse<ProductionStage>(stage, true, out var result))
            return TranslateStage(result);
        
        return stage.ToLower() switch
        {
            "cutting" => "Corte",
            "preparation" or "prep" => "Preparação",
            "sewing" => "Costura",
            "review" or "qa" => "Revisão",
            "packaging" => "Embalagem",
            _ => stage
        };
    }

    public static string TranslateStatus(string? status)
    {
        if (string.IsNullOrEmpty(status)) return "";
        if (Enum.TryParse<ProductionStatus>(status, true, out var result))
            return TranslateStatus(result);

        return status.ToLower() switch
        {
            "pending" => "Pendente",
            "inproduction" => "Em Produção",
            "stopped" => "Parado",
            "completed" => "Finalizado",
            "paused" => "Pausado",
            "finished" => "Concluído",
            "cancelled" => "Cancelado",
            _ => status
        };
    }
}
