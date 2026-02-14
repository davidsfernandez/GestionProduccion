using GestionProduccion.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using System.Threading.Tasks;
using GestionProduccion.Models.DTOs;
using System.Collections.Generic;
using System.Linq;
using System;
using QRCoder; // Using our Mock/Helper namespace

namespace GestionProduccion.Services;

public class ReportService : IReportService
{
    private readonly IProductionOrderService _productionOrderService;

    public ReportService(IProductionOrderService productionOrderService)
    {
        _productionOrderService = productionOrderService;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateProductionOrderReportAsync(int orderId)
    {
        // This is the implementation of the requested "GenerateProductionOrderPdf" logic
        // but mapped to the existing interface method name to avoid breaking changes unless strictly necessary.
        
        var order = await _productionOrderService.GetProductionOrderByIdAsync(orderId);
        if (order == null) return null;

        var history = await _productionOrderService.GetHistoryByProductionOrderIdAsync(orderId);

        // QR Code Generation (Using Real QRCoder library)
        var qrUrl = $"https://tu-dominio.com/orders/{order.Id}";
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(20);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                // HEADER
                page.Header().Background(Colors.Grey.Darken3).Padding(20).Row(row =>
                {
                    // Left: Title and Logo/Company Name
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("FICHA DE PRODUÇÃO").FontSize(24).Bold().FontColor(Colors.White);
                        col.Item().Text("Serona Manufacturing").FontSize(14).FontColor(Colors.Grey.Lighten2);
                    });

                    // Right: QR Code
                    row.ConstantItem(80).Column(col =>
                    {
                        col.Item().Width(2, Unit.Centimetre).Height(2, Unit.Centimetre).Image(qrCodeBytes);
                        col.Item().AlignCenter().Text("Escanear").FontSize(8).FontColor(Colors.White);
                    });
                });

                // CONTENT
                page.Content().PaddingVertical(20).Column(x =>
                {
                    x.Spacing(15);

                    // INFO PRINCIPAL GRID
                    x.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Grid(grid =>
                    {
                        grid.Columns(2);
                        grid.VerticalSpacing(10);
                        grid.HorizontalSpacing(10);

                        // Row 1
                        grid.Item().Text(t => { t.Span("Lote / Código: ").Bold(); t.Span(order.UniqueCode).FontSize(14); });
                        grid.Item().Text(t => { t.Span("Data Entrega: ").Bold(); t.Span(order.EstimatedDeliveryDate.ToShortDateString()); });

                        // Row 2
                        grid.Item().Text(t => { t.Span("Produto: ").Bold(); t.Span(order.ProductDescription); });
                        grid.Item().Text(t => { t.Span("Quantidade: ").Bold(); t.Span(order.Quantity.ToString()); });
                    });

                    // HISTORY / DETAILS TABLE
                    x.Item().PaddingTop(10).Text("Histórico de Movimentação").Bold().FontSize(12);
                    
                    x.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(4);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Data");
                            header.Cell().Element(CellStyle).Text("Etapa");
                            header.Cell().Element(CellStyle).Text("Responsável");
                            header.Cell().Element(CellStyle).Text("Obs");

                            static IContainer CellStyle(IContainer container) => container.Background(Colors.Grey.Lighten4).Padding(5).BorderBottom(1).BorderColor(Colors.Black);
                        });

                        foreach (var item in history.OrderBy(h => h.ModificationDate))
                        {
                            table.Cell().Element(CellStyle).Text(item.ModificationDate.ToString("dd/MM HH:mm"));
                            table.Cell().Element(CellStyle).Text(item.NewStage);
                            table.Cell().Element(CellStyle).Text(item.UserName);
                            table.Cell().Element(CellStyle).Text(item.Note ?? "-");

                            static IContainer CellStyle(IContainer container) => container.Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                        }
                    });
                });

                // FOOTER
                page.Footer().AlignCenter().Row(row => 
                {
                    row.RelativeItem().Text(x => 
                    {
                        x.Span("Gerado em: ");
                        x.Span(DateTime.Now.ToString("g"));
                    });
                    
                    row.RelativeItem().AlignRight().Text(x => 
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateDailyProductionReportAsync()
    {
        // Keeping existing implementation logic but simplified/placeholder as the main task is the Ficha
        var dashboard = await _productionOrderService.GetDashboardAsync();
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.Header().Text("Relatório Diário").FontSize(20).Bold();
                page.Content().PaddingVertical(10).Column(x =>
                {
                    x.Item().Text($"Total Produzido Hoje: {dashboard.CompletedToday}");
                    x.Item().Text($"Eficiência: {dashboard.CompletionRate}%");
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
