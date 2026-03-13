/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited. 
 * 
 * Proprietary and Confidential.
 */

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
using GestionProduccion.Resources;

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
                                col.Item().Text("SERONA ERP").FontSize(20).Bold().FontColor(Colors.White);
                            }
                            col.Item().Text(Portuguese.OP_Report.ToUpper()).FontSize(16).Bold().FontColor(Colors.Grey.Lighten2);
                            col.Item().Text(config?.CompanyName ?? "Serona Corporación").FontSize(12).FontColor(Colors.Grey.Lighten2);  
                        });

                        if (qrCodeBytes != null)
                        {
                            row.ConstantItem(80).Column(col =>
                            {
                                col.Item().Width(2, Unit.Centimetre).Height(2, Unit.Centimetre).Image(qrCodeBytes);
                                col.Item().AlignCenter().Text(Portuguese.SCAN).FontSize(8).FontColor(Colors.White);
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

                            table.Cell().Text(t => { t.Span($"{Portuguese.OP_Code}: ").Bold(); t.Span(order.LotCode ?? "N/A").FontSize(12); });
                            table.Cell().Text(t => { t.Span($"{Portuguese.SKU}: ").Bold(); t.Span(order.ProductCode ?? "N/A").FontSize(12); });    

                            table.Cell().Text(t => { t.Span($"{Portuguese.Product}: ").Bold(); t.Span($"{order.ProductName ?? "N/A"}"); });
                            table.Cell().Text(t => { t.Span($"{Portuguese.Quantity}: ").Bold(); t.Span(order.Quantity.ToString()); });

                            table.Cell().Column(col =>
                            {
                                col.Item().Text($"{Portuguese.Cat_Sizes}:").Bold();
                                if (order.Sizes != null && order.Sizes.Any())
                                {
                                    col.Item().Text(string.Join(" | ", order.Sizes.Select(s => $"{s.Size}: {s.Quantity}")));
                                }
                                else
                                {
                                    col.Item().Text(order.Size ?? "-");
                                }
                            });
                            table.Cell().Text(t => { t.Span($"{Portuguese.OP_Status}: ").Bold(); t.Span(TranslateStatus(order.CurrentStatus)); });    

                            table.Cell().Text(t => { t.Span($"{Portuguese.Team_Title}: ").Bold(); t.Span(order.SewingTeamName ?? Portuguese.OP_Unassigned); });
                            table.Cell().Text(t => { t.Span($"{Portuguese.Role_Operator}: ").Bold(); t.Span(order.AssignedUserName ?? Portuguese.OP_Unassigned); });

                            table.Cell().Text(t => { t.Span($"{Portuguese.Start}: ").Bold(); t.Span(order.StartedAt?.ToLocalTime().ToString("g") ?? "-"); });
                            table.Cell().Text(t => { t.Span($"{Portuguese.End}: ").Bold(); t.Span(order.CompletedAt?.ToLocalTime().ToString("g") ?? "-"); });

                            table.Cell().Text(t => { t.Span($"{Portuguese.OP_EstimatedDelivery}: ").Bold(); t.Span(order.EstimatedCompletionAt.ToLocalTime().ToShortDateString()); });
                            table.Cell();
                        });

                        // METRICS & PERFORMANCE
                        x.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                var pauseCount = history.Count(h => h.NewStatus == "Stopped" || h.NewStatus == "Paused");
                                var totalEffectiveMinutes = order.EffectiveMinutes;
                                var avgHistoricalMinutes = order.Product?.AverageProductionTimeMinutes ?? 0;
                                
                                c.Item().Text(Portuguese.OP_ProductionMetrics).Bold().FontSize(11).FontColor(Colors.Grey.Darken2);
                                c.Item().Text(t => { t.Span($"{Portuguese.EffectiveMinutes}: ").Bold(); t.Span($"{totalEffectiveMinutes:N1} min"); });
                                c.Item().Text(t => { t.Span($"{Portuguese.TotalPauses}: ").Bold(); t.Span(pauseCount.ToString()); });

                                if (avgHistoricalMinutes > 0 && totalEffectiveMinutes >= 5)
                                {
                                    var perfIndex = (avgHistoricalMinutes / (totalEffectiveMinutes / Math.Max(1, order.Quantity))) * 100;
                                    // Cap at 200% to avoid extreme outliers if data is still settling
                                    if (perfIndex > 200) perfIndex = 200;

                                    c.Item().Text(t => {
                                        t.Span($"{Portuguese.Performance}: ").Bold();
                                        t.Span($"{perfIndex:N1}% ").FontColor(perfIndex >= 90 ? Colors.Green.Medium : Colors.Red.Medium);
                                        if (perfIndex > 100) t.Span($"({Portuguese.Dash_AboveAverage})");
                                    });
                                }
                                else if (avgHistoricalMinutes > 0 && totalEffectiveMinutes > 0)
                                {
                                    c.Item().Text(t => { 
                                        t.Span($"{Portuguese.Performance}: ").Bold(); 
                                        t.Span("Calibrando...").FontColor(Colors.Grey.Medium); 
                                    });
                                }
                            });

                            if (order.EstimatedCompletionAt < (order.CompletedAt ?? DateTime.UtcNow))
                            {
                                row.ConstantItem(150).Background(Colors.Red.Lighten5).Padding(10).AlignCenter().Column(c =>
                                {
                                    c.Item().Text(Portuguese.Attention_Delay.ToUpper()).Bold().FontColor(Colors.Red.Medium);
                                    var delay = ((order.CompletedAt ?? DateTime.UtcNow) - order.EstimatedCompletionAt).TotalDays;    
                                    c.Item().Text($"{delay:N1} {Portuguese.Days_Delay}").FontSize(9);
                                });
                            }
                        });

                        // RESUMO FINANCEIRO
                        if (order.CurrentStatus?.Equals("Completed", StringComparison.OrdinalIgnoreCase) == true && order.AverageCostPerPiece > 0)
                        {
                            x.Item().Background(Colors.Blue.Lighten5).Border(1).BorderColor(Colors.Blue.Lighten3).Padding(10).Column(c =>
                            {
                                c.Spacing(5);
                                c.Item().Text(Portuguese.FinancialSummary.ToUpper()).Bold().FontColor(Colors.Blue.Darken3);
                                c.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(t => { t.Span($"{Portuguese.TotalCostBatch}: ").Bold(); r.RelativeItem().Text($"R$ {order.TotalCost:N2}"); });
                                    r.RelativeItem().Text(t => { t.Span($"{Portuguese.UnitRealCost}: ").Bold(); r.RelativeItem().Text($"R$ {order.AverageCostPerPiece:N2}"); });
                                    r.RelativeItem().Text(t => { t.Span($"{Portuguese.Margin}: ").Bold(); r.RelativeItem().Text($"{order.ProfitMargin:N1}%"); }); 
                                });
                            });
                        }

                        // HISTORY TABLE
                        x.Item().PaddingTop(10).Text(Portuguese.OP_History).Bold().FontSize(12);
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
                                header.Cell().Element(CellStyle).Text(Portuguese.Date);
                                header.Cell().Element(CellStyle).Text(Portuguese.OP_Stage);
                                header.Cell().Element(CellStyle).Text(Portuguese.Role_Operator);
                                header.Cell().Element(CellStyle).Text(Portuguese.Note);
                                static IContainer CellStyle(IContainer container) => container.Background(Colors.Grey.Lighten4).Padding(5).BorderBottom(1).BorderColor(Colors.Black);
                            });

                            foreach (var item in history.OrderBy(h => h.ChangedAt))
                            {
                                table.Cell().Element(CellStyle).Text(item.ChangedAt.ToLocalTime().ToString("dd/MM HH:mm"));
                                table.Cell().Element(CellStyle).Text(TranslateStage(item.NewStage));
                                table.Cell().Element(CellStyle).Text(item.UserName ?? "Sistema");
                                table.Cell().Element(CellStyle).Text(TranslateNote(item.Note));
                                static IContainer CellStyle(IContainer container) => container.Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                            }
                        });
                    });

                    page.Footer().AlignCenter().Row(row =>
                    {
                        row.RelativeItem().Text(x => { x.Span($"{Portuguese.GeneratedAt}: "); x.Span(DateTime.Now.ToString("g")); });
                        row.RelativeItem().AlignRight().Text(x => { x.Span($"{Portuguese.Page} "); x.CurrentPageNumber(); });
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
                            else col.Item().Text("SERONA ERP").FontSize(20).Bold();

                            col.Item().Text(Portuguese.OP_DailyPDF.ToUpper()).FontSize(16).Bold().FontColor(Colors.Grey.Darken3);  
                            col.Item().Text(config?.CompanyName ?? "Serona Corporación").FontSize(12).FontColor(Colors.Grey.Medium);    
                            col.Item().Text($"{Portuguese.Date}: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).FontColor(Colors.Grey.Medium);    
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
                                c.Item().Text(Portuguese.StatisticalSummary).Bold();
                                c.Item().Text($"{Portuguese.TotalProducedToday}: {dashboard?.CompletedToday ?? 0}");
                                c.Item().Text($"{Portuguese.CompletionRate}: {dashboard?.CompletionRate ?? 0:N1}%");
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
                                header.Cell().Element(HeaderStyle).Text(Portuguese.SKU);
                                header.Cell().Element(HeaderStyle).Text($"{Portuguese.OP_Code}");
                                header.Cell().Element(HeaderStyle).Text(Portuguese.Product);
                                header.Cell().Element(HeaderStyle).Text(Portuguese.Team_Title);
                                header.Cell().Element(HeaderStyle).Text(Portuguese.Role_Operator);
                                header.Cell().Element(HeaderStyle).Text(Portuguese.OP_Status);
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

                    page.Footer().AlignCenter().Text(x => { x.Span($"{Portuguese.Page} "); x.CurrentPageNumber(); });
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

    public Task<byte[]> GenerateOrdersCsvAsync(List<ProductionOrderDto> orders)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{Portuguese.OP_Code};{Portuguese.Product};{Portuguese.Quantity};{Portuguese.Cat_Sizes};{Portuguese.OP_Stage};{Portuguese.OP_Status};{Portuguese.OP_EstimatedDelivery};{Portuguese.Role_Operator}");
        foreach (var order in orders)
        {
            var sizesStr = (order.Sizes != null && order.Sizes.Any()) 
                ? string.Join(" | ", order.Sizes.Select(s => $"{s.Size}:{s.Quantity}"))
                : order.Size ?? "-";

            sb.AppendLine($"{order.LotCode};{(order.ProductName ?? "-").Replace(";", ",")};{order.Quantity};{sizesStr};{TranslateStage(order.CurrentStage)};{TranslateStatus(order.CurrentStatus)};{order.EstimatedCompletionAt:dd/MM/yyyy};{order.AssignedUserName ?? "-"}");
        }
        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        var bom = System.Text.Encoding.UTF8.GetPreamble();
        var result = new byte[bom.Length + bytes.Length];
        System.Buffer.BlockCopy(bom, 0, result, 0, bom.Length);
        System.Buffer.BlockCopy(bytes, 0, result, bom.Length, bytes.Length);
        return Task.FromResult(result);
    }

    public async Task<byte[]> GenerateBonusReportPdfAsync(BonusReportDto report, string mode)
    {
        try
        {
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
                    page.Header().Background(Colors.Blue.Darken3).Padding(20).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            if (logoBytes != null) col.Item().Width(4, Unit.Centimetre).Image(logoBytes);
                            else col.Item().Text("SERONA ERP").FontSize(20).Bold().FontColor(Colors.White);

                            col.Item().Text("RELATÓRIO DE BONIFICAÇÃO").FontSize(16).Bold().FontColor(Colors.White);
                            col.Item().Text(config?.CompanyName ?? "Serona Corporación").FontSize(12).FontColor(Colors.Grey.Lighten3);
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text($"{Portuguese.Date}: {DateTime.Now:dd/MM/yyyy}").FontColor(Colors.White);
                            col.Item().Text($"{Portuguese.Analysis_Mode}: {mode.ToUpper()}").FontSize(8).Bold().FontColor(Colors.Yellow.Medium);
                        });
                    });

                    // CONTENT
                    page.Content().PaddingVertical(20).Column(x =>
                    {
                        x.Spacing(20);

                        // TARGET ENTITY
                        x.Item().Text(t =>
                        {
                            t.Span(mode == "team" ? $"{Portuguese.Team_Title}: " : $"{Portuguese.Employee}: ").Bold().FontSize(14);
                            t.Span(report.TeamName).FontSize(14);
                        });

                        // KPI ROW
                        x.Item().Row(row =>
                        {
                            row.RelativeItem().Component(new KpiComponent("B\u00D4NUS FINAL", $"{report.FinalBonusPercentage:F1}%", report.FinalBonusPercentage >= 80 ? Colors.Green.Medium : Colors.Orange.Medium));
                            row.Spacing(10);
                            row.RelativeItem().Component(new KpiComponent(Portuguese.Productivity.ToUpper(), $"{report.ProductivityPercentage:F1}%", Colors.Blue.Medium));
                            row.Spacing(10);
                            row.RelativeItem().Component(new KpiComponent(Portuguese.Quality.ToUpper(), $"{report.DefectPercentage:F1}%", report.DefectPercentage <= 5 ? Colors.Green.Medium : Colors.Red.Medium));
                            row.Spacing(10);
                            row.RelativeItem().Component(new KpiComponent("PRAZO", $"{report.DeadlinePerformance:F1}%", Colors.Purple.Medium));
                        });

                        // BREAKDOWN (IF USER MODES)
                        if (mode != "team")
                        {
                            x.Item().Padding(10).Background(Colors.Grey.Lighten4).Column(col =>
                            {
                                col.Spacing(5);
                                col.Item().Text("MÉTRICAS INDIVIDUAIS").Bold().FontSize(9);
                                col.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(t => { t.Span("Fator de Qualidade: ").FontSize(8); t.Span($"x{report.QualityFactor:F1}").Bold().FontSize(8); });
                                });
                            });
                        }

                        // DETAILS TABLE
                        x.Item().Text(Portuguese.Production_Details.ToUpper()).Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
                        x.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text(Portuguese.OP_Code);
                                header.Cell().Element(HeaderStyle).Text("STATUS");
                                header.Cell().Element(HeaderStyle).Text("DEFEITOS");
                                header.Cell().Element(HeaderStyle).Text("CONTRIBUIÇÃO");
                                static IContainer HeaderStyle(IContainer container) => container.Background(Colors.Grey.Lighten2).Padding(5).DefaultTextStyle(x => x.Bold().FontSize(9));
                            });

                            foreach (var order in report.Orders)
                            {
                                table.Cell().Element(CellStyle).Text(order.LotCode);
                                table.Cell().Element(CellStyle).Text(order.IsOnTime ? "NO PRAZO" : "ATRASADO").FontColor(order.IsOnTime ? Colors.Green.Medium : Colors.Red.Medium);
                                table.Cell().Element(CellStyle).Text(order.Defects.ToString());
                                table.Cell().Element(CellStyle).Text($"{order.Contribution:F2}%");
                                static IContainer CellStyle(IContainer container) => container.Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten4).DefaultTextStyle(x => x.FontSize(8));
                            }
                        });

                        // SUMMARY
                        x.Item().AlignRight().Background(Colors.Grey.Lighten5).Padding(10).Column(col =>
                        {
                            col.Item().Text(t => { t.Span($"{Portuguese.TotalProducedToday}: ").FontSize(9); t.Span(report.TotalProduced.ToString()).Bold(); });
                            col.Item().Text(t => { t.Span($"{Portuguese.Total_Defects}: ").FontSize(9); t.Span(report.TotalDefects.ToString()).Bold(); });
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span($"{Portuguese.Page} ");
                        x.CurrentPageNumber();
                    });
                });
            });

            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Bonus Report PDF");
            throw;
        }
    }

    private class KpiComponent : IComponent
    {
        private string Title { get; }
        private string Value { get; }
        private string Color { get; }

        public KpiComponent(string title, string value, string color)
        {
            Title = title;
            Value = value;
            Color = color;
        }

        public void Compose(IContainer container)
        {
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(10)
                .Column(column =>
                {
                    column.Item().AlignCenter().Text(Title).FontSize(8).Bold().FontColor(Colors.Grey.Medium);
                    column.Item().AlignCenter().Text(Value).FontSize(16).Bold().FontColor(Color);
                });
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

    private string TranslateNote(string? note)
    {
        if (string.IsNullOrEmpty(note)) return "-";
        
        var translated = note;
        
        // Patterns from ProductionOrderLifecycleService
        if (note.StartsWith("Advanced to ")) 
        {
            var stage = note.Replace("Advanced to ", "").Trim();
            translated = $"{Portuguese.OP_AdvanceStage}: {TranslateStage(stage)}";
        }
        else if (note.StartsWith("Assigned to operator "))
        {
             translated = note.Replace("Assigned to operator ", Portuguese.OP_OperatorAssigned + ": ");
        }
        else if (note.Contains("Transitioned from "))
        {
            translated = "Transição automática de etapa";
        }
        else if (note == "Started production")
        {
             translated = Portuguese.OP_ResumeProduction;
        }
        else if (note == "Stopped production")
        {
             translated = Portuguese.OP_StopProduction;
        }
        else if (note == "Production completed")
        {
             translated = Portuguese.Status_Completed;
        }

        return translated;
    }

    private string TranslateStage(string? stage) => stage?.ToLower() switch
    {
        "cutting" => Portuguese.Stage_Cutting,
        "sewing" => Portuguese.Stage_Sewing,
        "review" => Portuguese.Stage_Review,
        "packaging" => Portuguese.Stage_Packaging,
        _ => stage ?? ""
    };

    private string TranslateStatus(string? status) => status?.ToLower() switch
    {
        "pending" => Portuguese.Pending,
        "inproduction" => Portuguese.Status_InProgress,
        "stopped" => Portuguese.Status_Stopped,
        "completed" => Portuguese.Status_Completed,
        "paused" => Portuguese.Status_Paused,
        "finished" => Portuguese.Status_Finished,
        "cancelled" => Portuguese.Cancelled,
        _ => status ?? ""
    };
}
