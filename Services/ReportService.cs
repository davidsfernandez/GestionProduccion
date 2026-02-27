using GestionProduccion.Services.Interfaces;
using GestionProduccion.Services.ProductionOrders;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Threading.Tasks;
using GestionProduccion.Models.DTOs;
using System.Collections.Generic;
using System.Linq;
using System;
using QRCoder;
using Microsoft.Extensions.Logging;

namespace GestionProduccion.Services;

public class ReportService : IReportService
{
    private readonly IProductionOrderQueryService _queryService;
    private readonly ISystemConfigurationService _configService;
    private readonly ILogger<ReportService> _logger;
    private static readonly string DefaultFont = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux) ? "Liberation Sans" : "Arial";

    public ReportService(IProductionOrderQueryService queryService, ISystemConfigurationService configService, ILogger<ReportService> logger)
    {
        _queryService = queryService;
        _configService = configService;
        _logger = logger;
        try { QuestPDF.Settings.License = LicenseType.Community; } catch { }
    }

    public async Task<byte[]> GenerateProductionOrderReportAsync(int orderId, string baseUrl)
    {
        try
        {
            var order = await _queryService.GetProductionOrderByIdAsync(orderId);
            if (order == null) return Array.Empty<byte>();

            var history = await _queryService.GetHistoryByProductionOrderIdAsync(orderId);
            var config = await _configService.GetConfigurationAsync();

            byte[]? logoBytes = ExtractLogoBytes(config?.LogoBase64);

            // QR Code Generation
            byte[]? qrCodeBytes = null;
            try
            {
                var qrUrl = $"{baseUrl.TrimEnd('/')}/orders/{order.Id}";
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(qrUrl, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                qrCodeBytes = qrCode.GetGraphic(20);
            }
            catch { /* QR fail should not break report */ }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(DefaultFont));

                    // HEADER
                    page.Header().Background(Colors.Grey.Darken3).Padding(20).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            if (logoBytes != null)
                            {
                                col.Item().Width(4, Unit.Centimetre).Image(logoBytes);
                            }
                            else
                            {
                                col.Item().Text("ERP CONFECÇÃO").FontSize(20).Bold().FontColor(Colors.White);
                            }
                            col.Item().Text("FICHA DE PRODUÇÃO").FontSize(16).Bold().FontColor(Colors.Grey.Lighten2);
                            col.Item().Text(config?.CompanyName ?? "David Fernandez").FontSize(12).FontColor(Colors.Grey.Lighten2);  
                        });

                        if (qrCodeBytes != null)
                        {
                            row.ConstantItem(80).Column(col =>
                            {
                                col.Item().Width(2, Unit.Centimetre).Height(2, Unit.Centimetre).Image(qrCodeBytes);
                                col.Item().AlignCenter().Text("Escanear").FontSize(8).FontColor(Colors.White);
                            });
                        }
                    });

                    // CONTENT
                    page.Content().PaddingVertical(20).Column(x =>
                    {
                        x.Spacing(15);

                        // INFO PRINCIPAL TABLE
                        x.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Cell().Text(t => { t.Span("Lote / Código: ").Bold(); t.Span(order.LotCode ?? "N/A").FontSize(12); });
                            table.Cell().Text(t => { t.Span("SKU: ").Bold(); t.Span(order.ProductCode ?? "N/A").FontSize(12); });    

                            table.Cell().Text(t => { t.Span("Produto: ").Bold(); t.Span($"{order.ProductName ?? "N/A"} (Tam: {order.Size ?? "N/A"})"); });
                            table.Cell().Text(t => { t.Span("Quantidade: ").Bold(); t.Span(order.Quantity.ToString()); });

                            table.Cell().Text(t => { t.Span("Equipe: ").Bold(); t.Span(order.SewingTeamName ?? "Não atribuída"); });
                            table.Cell().Text(t => { t.Span("Operário: ").Bold(); t.Span(order.AssignedUserName ?? "Não atribuído"); });

                            table.Cell().Text(t => { t.Span("Início: ").Bold(); t.Span(order.StartedAt?.ToString("g") ?? "Não iniciado"); });
                            table.Cell().Text(t => { t.Span("Fim Real: ").Bold(); t.Span(order.CompletedAt?.ToString("g") ?? "-"); });

                            table.Cell().Text(t => { t.Span("Prazo Estimado: ").Bold(); t.Span(order.EstimatedCompletionAt.ToShortDateString()); });
                            table.Cell().Text(t => { t.Span("Status: ").Bold(); t.Span(TranslateStatus(order.CurrentStatus)); });    
                        });

                        // METRICS & PERFORMANCE
                        x.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                var pauseCount = history.Count(h => h.NewStatus == "Stopped" || h.NewStatus == "Paused");
                                var totalEffectiveMinutes = order.EffectiveMinutes;
                                var avgHistoricalMinutes = order.Product?.AverageProductionTimeMinutes ?? 0;
                                var perfIndex = avgHistoricalMinutes > 0 ? (avgHistoricalMinutes / (totalEffectiveMinutes / Math.Max(1, order.Quantity))) * 100 : 100;

                                c.Item().Text("Métricas de Produção").Bold().FontSize(11).FontColor(Colors.Grey.Darken2);
                                c.Item().Text(t => { t.Span("Tempo Efetivo: ").Bold(); t.Span($"{totalEffectiveMinutes:N1} min"); });
                                c.Item().Text(t => { t.Span("Total de Pausas: ").Bold(); t.Span(pauseCount.ToString()); });

                                if (avgHistoricalMinutes > 0)
                                {
                                    c.Item().Text(t => {
                                        t.Span("Desempenho: ").Bold();
                                        t.Span($"{perfIndex:N1}% ").FontColor(perfIndex >= 90 ? Colors.Green.Medium : Colors.Red.Medium);
                                        t.Span(perfIndex >= 100 ? "(Acima da média)" : "(Abaixo da média)").FontSize(8).Italic();  
                                    });
                                }
                            });

                            if (order.EstimatedCompletionAt < (order.CompletedAt ?? DateTime.UtcNow))
                            {
                                row.ConstantItem(150).Background(Colors.Red.Lighten5).Padding(10).AlignCenter().Column(c =>
                                {
                                    c.Item().Text("ATENÇÃO: ATRASO").Bold().FontColor(Colors.Red.Medium);
                                    var delay = ((order.CompletedAt ?? DateTime.UtcNow) - order.EstimatedCompletionAt).TotalDays;    
                                    c.Item().Text($"{delay:N1} dias de atraso").FontSize(9);
                                });
                            }
                        });

                        // RESUMO FINANCEIRO
                        if (order.CurrentStatus?.Equals("Completed", StringComparison.OrdinalIgnoreCase) == true && order.AverageCostPerPiece > 0)
                        {
                            x.Item().Background(Colors.Blue.Lighten5).Border(1).BorderColor(Colors.Blue.Lighten3).Padding(10).Column(c =>
                            {
                                c.Spacing(5);
                                c.Item().Text("RESUMO FINANCEIRO").Bold().FontColor(Colors.Blue.Darken3);
                                c.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(t => { t.Span("Custo Total Lote: ").Bold(); t.Span($"R$ {order.TotalCost:N2}"); });
                                    r.RelativeItem().Text(t => { t.Span("Custo Real Unitário: ").Bold(); t.Span($"R$ {order.AverageCostPerPiece:N2}"); });
                                    r.RelativeItem().Text(t => { t.Span("Margem: ").Bold(); t.Span($"{order.ProfitMargin:N1}%"); }); 
                                });
                            });
                        }

                        // HISTORY TABLE
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

                            foreach (var item in history.OrderBy(h => h.ChangedAt))
                            {
                                table.Cell().Element(CellStyle).Text(item.ChangedAt.ToString("dd/MM HH:mm"));
                                table.Cell().Element(CellStyle).Text(item.NewStage ?? "-");
                                table.Cell().Element(CellStyle).Text(item.UserName ?? "Sistema");
                                table.Cell().Element(CellStyle).Text(item.Note ?? "-");
                                static IContainer CellStyle(IContainer container) => container.Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                            }
                        });
                    });

                    page.Footer().AlignCenter().Row(row =>
                    {
                        row.RelativeItem().Text(x => { x.Span("Gerado em: "); x.Span(DateTime.Now.ToString("g")); });
                        row.RelativeItem().AlignRight().Text(x => { x.Span("Página "); x.CurrentPageNumber(); });
                    });
                });
            });

            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Individual Production Order PDF for OrderId: {OrderId}", orderId);
            throw;
        }
    }

    public async Task<byte[]> GenerateDailyProductionReportAsync()
    {
        try
        {
            var dashboard = await _queryService.GetDashboardAsync();
            var config = await _configService.GetConfigurationAsync();
            byte[]? logoBytes = ExtractLogoBytes(config?.LogoBase64);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(DefaultFont));

                    // HEADER
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            if (logoBytes != null) col.Item().Width(4, Unit.Centimetre).Image(logoBytes);
                            else col.Item().Text("ERP CONFECÇÃO").FontSize(20).Bold();

                            col.Item().Text("Relatório Diário de Produção").FontSize(16).Bold().FontColor(Colors.Grey.Darken3);  
                            col.Item().Text(config?.CompanyName ?? "David Fernandez").FontSize(12).FontColor(Colors.Grey.Medium);    
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
                                c.Item().Text($"Total Produzido Hoje: {dashboard?.CompletedToday ?? 0}");
                                c.Item().Text($"Taxa de Conclusão: {dashboard?.CompletionRate ?? 0:N1}%");
                            });
                        });

                        // TABLE
                        x.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // SKU
                                columns.RelativeColumn(3); // Lote
                                columns.RelativeColumn(4); // Produto
                                columns.RelativeColumn(3); // Equipe
                                columns.RelativeColumn(3); // Operário
                                columns.RelativeColumn(2); // Status
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("SKU");
                                header.Cell().Element(HeaderStyle).Text("Lote/OP");
                                header.Cell().Element(HeaderStyle).Text("Produto");
                                header.Cell().Element(HeaderStyle).Text("Equipe");
                                                            header.Cell().Element(HeaderStyle).Text("Operário");
                                                            header.Cell().Element(HeaderStyle).Text("Status");
                                                            static IContainer HeaderStyle(IContainer container) => container.Background(Colors.Grey.Darken3).Padding(5).AlignCenter().DefaultTextStyle(x => x.Bold().FontColor(Colors.White).FontSize(9).FontFamily(DefaultFont));
                                                        });
                            if (dashboard?.TodaysOrders != null)
                            {
                                for (int i = 0; i < dashboard.TodaysOrders.Count; i++)
                                {
                                    var order = dashboard.TodaysOrders[i];
                                    var bgColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;
                                    table.Cell().Element(c => CellStyle(c, bgColor)).Text(order.ProductCode ?? "-").FontSize(8);     
                                    table.Cell().Element(c => CellStyle(c, bgColor)).Text(order.LotCode ?? "-").FontSize(8);
                                    table.Cell().Element(c => CellStyle(c, bgColor)).Text(order.ProductName ?? "-").FontSize(8);     
                                    table.Cell().Element(c => CellStyle(c, bgColor)).Text(order.SewingTeamName ?? "-").FontSize(8);  
                                    table.Cell().Element(c => CellStyle(c, bgColor)).Text(order.AssignedUserName ?? "-").FontSize(8);
                                    table.Cell().Element(c => CellStyle(c, bgColor)).Text(TranslateStatus(order.CurrentStatus)).FontSize(8);
                                    static IContainer CellStyle(IContainer container, string bgColor) => container.Background(bgColor).Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                                }
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x => { x.Span("Página "); x.CurrentPageNumber(); });
                });
            });
            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Daily Production PDF Report");
            throw;
        }
    }

    private byte[]? ExtractLogoBytes(string? logoBase64)
    {
        if (string.IsNullOrEmpty(logoBase64)) return null;
        try
        {
            string cleanBase64 = logoBase64;
            int commaIndex = cleanBase64.IndexOf(",");
            if (commaIndex >= 0) cleanBase64 = cleanBase64.Substring(commaIndex + 1);
            return Convert.FromBase64String(cleanBase64);
        }
        catch { return null; }
    }

    public Task<byte[]> GenerateOrdersCsvAsync(List<ProductionOrderDto> orders)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Codigo;Produto;Quantidade;Etapa;Status;Entrega;Responsavel");
        foreach (var order in orders)
        {
            sb.AppendLine($"{order.LotCode};{(order.ProductName ?? "N/A").Replace(";", ",")};{order.Quantity};{TranslateStage(order.CurrentStage)};{TranslateStatus(order.CurrentStatus)};{order.EstimatedCompletionAt:dd/MM/yyyy};{order.AssignedUserName ?? "N/A"}");
        }
        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        var bom = System.Text.Encoding.UTF8.GetPreamble();
        var result = new byte[bom.Length + bytes.Length];
        System.Buffer.BlockCopy(bom, 0, result, 0, bom.Length);
        System.Buffer.BlockCopy(bytes, 0, result, bom.Length, bytes.Length);
        return Task.FromResult(result);
    }

    private string TranslateStage(string? stage) => stage?.ToLower() switch
    {
        "cutting" => "Corte",
        "sewing" => "Costura",
        "review" => "Revisão",
        "packaging" => "Embalagem",
        _ => stage ?? ""
    };

    private string TranslateStatus(string? status) => status?.ToLower() switch
    {
        "inproduction" => "Em Produção",
        "stopped" => "Parado",
        "completed" => "Finalizado",
        "paused" => "Pausado",
        "finished" => "Concluído",
        _ => status ?? ""
    };
}
