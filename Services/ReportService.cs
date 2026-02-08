using GestionProduccion.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using System.Threading.Tasks;
using GestionProduccion.Models.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace GestionProduccion.Services;

public class ReportService : IReportService
{
    private readonly IProductionOrderService _productionOrderService;

    public ReportService(IProductionOrderService productionOrderService)
    {
        _productionOrderService = productionOrderService;
        
        // QuestPDF License initialization
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateProductionOrderReportAsync(int orderId)
    {
        var order = await _productionOrderService.GetProductionOrderByIdAsync(orderId);
        if (order == null) return null;

        var history = await _productionOrderService.GetHistoryByProductionOrderIdAsync(orderId);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Relatório de Ordem de Produção").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                        col.Item().Text($"{order.UniqueCode}").FontSize(14);
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("Sistema Gestão Produção").FontSize(12).SemiBold();
                        col.Item().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                    });
                });

                page.Content().PaddingVertical(10).Column(x =>
                {
                    x.Spacing(10);

                    // Order Details Section
                    x.Item().BorderBottom(1).PaddingBottom(5).Text("Detalhes da Ordem").SemiBold().FontSize(12);
                    
                    x.Item().Grid(grid =>
                    {
                        grid.VerticalSpacing(5);
                        grid.HorizontalSpacing(10);
                        grid.Columns(2);

                        grid.Item().Text(t => { t.Span("Produto: ").SemiBold(); t.Span(order.ProductDescription); });
                        grid.Item().Text(t => { t.Span("Quantidade: ").SemiBold(); t.Span(order.Quantity.ToString()); });
                        grid.Item().Text(t => { t.Span("Etapa Atual: ").SemiBold(); t.Span(TranslateStage(order.CurrentStage)); });
                        grid.Item().Text(t => { t.Span("Status: ").SemiBold(); t.Span(TranslateStatus(order.CurrentStatus)); });
                        grid.Item().Text(t => { t.Span("Atribuído a: ").SemiBold(); t.Span(order.AssignedUserName ?? "Não atribuído"); });
                        grid.Item().Text(t => { t.Span("Prazo Entrega: ").SemiBold(); t.Span(order.EstimatedDeliveryDate.ToShortDateString()); });
                    });

                    // History Section
                    x.Item().PaddingTop(20).BorderBottom(1).PaddingBottom(5).Text("Histórico de Produção").SemiBold().FontSize(12);

                    x.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // Date
                            columns.RelativeColumn(2); // Stage
                            columns.RelativeColumn(2); // Status
                            columns.RelativeColumn(3); // Responsible
                            columns.RelativeColumn(4); // Note
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Data");
                            header.Cell().Element(CellStyle).Text("Etapa");
                            header.Cell().Element(CellStyle).Text("Status");
                            header.Cell().Element(CellStyle).Text("Usuário");
                            header.Cell().Element(CellStyle).Text("Observação");

                            static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                        });

                        foreach (var item in history.OrderBy(h => h.ModificationDate))
                        {
                            table.Cell().Element(CellStyle).Text(item.ModificationDate.ToString("dd/MM/yyyy HH:mm"));
                            table.Cell().Element(CellStyle).Text(TranslateStage(item.NewStage));
                            table.Cell().Element(CellStyle).Text(TranslateStatus(item.NewStatus));
                            table.Cell().Element(CellStyle).Text(item.UserName);
                            table.Cell().Element(CellStyle).Text(item.Note ?? "-");

                            static IContainer CellStyle(IContainer container) => container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                        }
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateDailyProductionReportAsync()
    {
        var dashboard = await _productionOrderService.GetDashboardAsync();
        var orders = await _productionOrderService.ListProductionOrdersAsync(null);
        
        var today = DateTime.UtcNow.Date;
        var ordersCreatedToday = orders.Where(o => o.CreationDate.Date == today).ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Resumo Diário de Produção").FontSize(20).SemiBold().FontColor(Colors.Green.Medium);
                        col.Item().Text($"Data: {today:dd/MM/yyyy}").FontSize(14);
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("Sistema Gestão Produção").FontSize(12).SemiBold();
                    });
                });

                page.Content().PaddingVertical(10).Column(x =>
                {
                    x.Spacing(20);

                    // Statistics Grid
                    x.Item().Grid(grid =>
                    {
                        grid.VerticalSpacing(10);
                        grid.HorizontalSpacing(10);
                        grid.Columns(3);

                        grid.Item().Border(1).Padding(10).Column(c =>
                        {
                            c.Item().AlignCenter().Text("Total OPs Ativas").SemiBold();
                            c.Item().AlignCenter().Text(orders.Count(o => o.CurrentStatus != "Completed").ToString()).FontSize(18).Medium();
                        });

                        grid.Item().Border(1).Padding(10).Column(c =>
                        {
                            c.Item().AlignCenter().Text("Criadas Hoje").SemiBold();
                            c.Item().AlignCenter().Text(ordersCreatedToday.Count.ToString()).FontSize(18).Medium();
                        });

                        grid.Item().Border(1).Padding(10).Column(c =>
                        {
                            c.Item().AlignCenter().Text("Taxa de Conclusão").SemiBold();
                            c.Item().AlignCenter().Text($"{dashboard.CompletionRate:F1}%").FontSize(18).Medium();
                        });
                    });

                    // Summary Table
                    x.Item().Text("Ordens de Produção Ativas").SemiBold().FontSize(12);

                    x.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // Code
                            columns.RelativeColumn(4); // Product
                            columns.RelativeColumn(2); // Stage
                            columns.RelativeColumn(2); // Status
                            columns.RelativeColumn(2); // User
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Código");
                            header.Cell().Element(CellStyle).Text("Produto");
                            header.Cell().Element(CellStyle).Text("Etapa");
                            header.Cell().Element(CellStyle).Text("Status");
                            header.Cell().Element(CellStyle).Text("Atribuído");

                            static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1);
                        });

                        foreach (var item in orders.Where(o => o.CurrentStatus != "Completed").OrderBy(o => o.CreationDate))
                        {
                            table.Cell().Element(CellStyle).Text(item.UniqueCode);
                            table.Cell().Element(CellStyle).Text(item.ProductDescription);
                            table.Cell().Element(CellStyle).Text(TranslateStage(item.CurrentStage));
                            table.Cell().Element(CellStyle).Text(TranslateStatus(item.CurrentStatus));
                            table.Cell().Element(CellStyle).Text(item.AssignedUserName ?? "-");

                            static IContainer CellStyle(IContainer container) => container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                        }
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Relatório Diário Gerado em ");
                    x.Span(DateTime.Now.ToString("f"));
                });
            });
        });

        return document.GeneratePdf();
    }

    private string TranslateStage(string stage) => stage?.ToLower() switch
    {
        "cutting" => "Corte",
        "sewing" => "Costura",
        "review" => "Revisão",
        "packaging" => "Embalagem",
        _ => stage ?? ""
    };

    private string TranslateStatus(string status) => status?.ToLower() switch
    {
        "inproduction" => "Em Produção",
        "stopped" => "Parado",
        "completed" => "Finalizado",
        "paused" => "Pausado",
        "finished" => "Concluído",
        _ => status ?? ""
    };
}
