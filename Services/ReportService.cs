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
    private readonly ISystemConfigurationService _configService;

    public ReportService(IProductionOrderService productionOrderService, ISystemConfigurationService configService)
    {
        _productionOrderService = productionOrderService;
        _configService = configService;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateProductionOrderReportAsync(int orderId)
    {
        // This is the implementation of the requested "GenerateProductionOrderPdf" logic
        // but mapped to the existing interface method name to avoid breaking changes unless strictly necessary.
        
        var order = await _productionOrderService.GetProductionOrderByIdAsync(orderId);
        if (order == null) return Array.Empty<byte>();

        var history = await _productionOrderService.GetHistoryByProductionOrderIdAsync(orderId);
        var logoBase64 = await _configService.GetLogoAsync();
        byte[]? logoBytes = null;

        if (!string.IsNullOrEmpty(logoBase64))
        {
            try 
            {
                var base64Data = logoBase64.Contains(",") ? logoBase64.Split(',')[1] : logoBase64;
                logoBytes = Convert.FromBase64String(base64Data);
            }
            catch { /* Ignore invalid logo */ }
        }

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
                        if (logoBytes != null)
                        {
                            col.Item().Width(4, Unit.Centimetre).Image(logoBytes);
                        }
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

                    // INFO PRINCIPAL TABLE (Replacing obsolete Grid)
                    x.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // Row 1
                        table.Cell().Text(t => { t.Span("Lote / Código: ").Bold(); t.Span(order.UniqueCode).FontSize(14); });
                        table.Cell().Text(t => { t.Span("Data Entrega: ").Bold(); t.Span(order.EstimatedDeliveryDate.ToShortDateString()); });

                        // Row 2
                        var productDesc = order.Product != null
                            ? $"{order.Product.Name} ({order.Product.FabricType})"
                            : order.ProductDescription;

                        table.Cell().Text(t => { t.Span("Produto: ").Bold(); t.Span(productDesc); });
                        table.Cell().Text(t => { t.Span("Quantidade: ").Bold(); t.Span(order.Quantity.ToString()); });
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
        var dashboard = await _productionOrderService.GetDashboardAsync();
        var logoBase64 = await _configService.GetLogoAsync();
        byte[]? logoBytes = null;

        if (!string.IsNullOrEmpty(logoBase64))
        {
            try 
            {
                var base64Data = logoBase64.Contains(",") ? logoBase64.Split(',')[1] : logoBase64;
                logoBytes = Convert.FromBase64String(base64Data);
            }
            catch { /* Ignore */ }
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                // HEADER
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        if (logoBytes != null)
                        {
                            col.Item().Width(4, Unit.Centimetre).Image(logoBytes);
                        }
                        col.Item().Text("Relatório Diário de Produção").FontSize(20).Bold().FontColor(Colors.Grey.Darken3);
                        col.Item().Text($"Data: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).FontColor(Colors.Grey.Medium);
                    });
                });

                // CONTENT
                page.Content().PaddingVertical(10).Column(x =>
                {
                    x.Spacing(20);

                    // SUMMARY BOX
                    x.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Resumo Estatístico").Bold();
                            c.Item().Text($"Total Produzido Hoje: {dashboard.CompletedToday}");
                            c.Item().Text($"Taxa de Conclusão: {dashboard.CompletionRate}%");
                            
                            // Efficiency Calculation (Completed vs Total Today)
                            var totalToday = dashboard.TodaysOrders.Count;
                            var efficiency = totalToday > 0 ? (decimal)dashboard.CompletedToday / totalToday * 100 : 0;
                            c.Item().Text($"Eficiência Global do Dia: {Math.Round(efficiency, 1)}%").FontColor(efficiency > 80 ? Colors.Green.Darken2 : Colors.Red.Medium);
                        });
                    });

                    // DETAILED TABLE
                    x.Item().Text("Detalhamento das Ordens do Dia").Bold().FontSize(12);

                    x.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Lot
                            columns.RelativeColumn(4); // Client
                            columns.RelativeColumn(3); // Team
                            columns.RelativeColumn(3); // Status
                            columns.RelativeColumn(4); // Operator
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderStyle).Text("Lote");
                            header.Cell().Element(HeaderStyle).Text("Cliente");
                            header.Cell().Element(HeaderStyle).Text("Equipe");
                            header.Cell().Element(HeaderStyle).Text("Status");
                            header.Cell().Element(HeaderStyle).Text("Operador");

                            static IContainer HeaderStyle(IContainer container) => 
                                container.Background(Colors.Grey.Darken3).Padding(5).AlignCenter().DefaultTextStyle(x => x.Bold().FontColor(Colors.White));
                        });

                        // Rows
                        for (int i = 0; i < dashboard.TodaysOrders.Count; i++)
                        {
                            var order = dashboard.TodaysOrders[i];
                            var bgColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;

                            table.Cell().Element(c => CellStyle(c, bgColor)).Text(order.UniqueCode);
                            table.Cell().Element(c => CellStyle(c, bgColor)).Text(order.ClientName ?? "-");
                            table.Cell().Element(c => CellStyle(c, bgColor)).Text(order.SewingTeamName ?? "-");
                            table.Cell().Element(c => CellStyle(c, bgColor)).Text(TranslateStatus(order.CurrentStatus));
                            table.Cell().Element(c => CellStyle(c, bgColor)).Text(order.AssignedUserName ?? "Não atribuído");

                            static IContainer CellStyle(IContainer container, string bgColor) => 
                                container.Background(bgColor).Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                        }
                    });
                });

                // FOOTER
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                });
            });
        });
        return document.GeneratePdf();
    }

    public Task<byte[]> GenerateOrdersCsvAsync(List<ProductionOrderDto> orders)
    {
        var sb = new System.Text.StringBuilder();
        // Header
        sb.AppendLine("Codigo;Produto;Quantidade;Etapa;Status;Entrega;Responsavel");

        foreach (var order in orders)
        {
            var user = order.AssignedUserName ?? "N/A";
            // Sanitize CSV fields
            var prod = order.ProductDescription.Replace(";", ",");
            
            sb.AppendLine($"{order.UniqueCode};{prod};{order.Quantity};{TranslateStage(order.CurrentStage)};{TranslateStatus(order.CurrentStatus)};{order.EstimatedDeliveryDate:dd/MM/yyyy};{user}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        // Add BOM for Excel compatibility
        var bom = System.Text.Encoding.UTF8.GetPreamble();
        var result = new byte[bom.Length + bytes.Length];
        System.Buffer.BlockCopy(bom, 0, result, 0, bom.Length);
        System.Buffer.BlockCopy(bytes, 0, result, bom.Length, bytes.Length);

        return Task.FromResult(result);
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
