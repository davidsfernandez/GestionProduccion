using ClosedXML.Excel;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GestionProduccion.Services;

public class ExcelExportService : IExcelExportService
{
    public ExcelExportService()
    {
        // No longer needs IProductionOrderRepository as orders are passed directly
    }

    public async Task<byte[]> ExportProductionOrdersToExcelAsync(List<ProductionOrderDto> orders)
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Production Orders");
            
            // Header
            worksheet.Cell(1, 1).Value = "Unique Code";
            worksheet.Cell(1, 2).Value = "Product";
            worksheet.Cell(1, 3).Value = "Quantity";
            worksheet.Cell(1, 4).Value = "Client";
            worksheet.Cell(1, 5).Value = "Tamanho";
            worksheet.Cell(1, 6).Value = "Current Stage";
            worksheet.Cell(1, 7).Value = "Current Status";
            worksheet.Cell(1, 8).Value = "Creation Date";
            worksheet.Cell(1, 9).Value = "Assigned To";

            var headerRange = worksheet.Range(1, 1, 1, 9);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

            // Data
            for (int i = 0; i < orders.Count; i++)
            {
                var order = orders[i];
                int row = i + 2;
                worksheet.Cell(row, 1).Value = order.UniqueCode;
                worksheet.Cell(row, 2).Value = order.ProductName ?? "N/A";
                worksheet.Cell(row, 3).Value = order.Quantity;
                worksheet.Cell(row, 4).Value = order.ClientName ?? "N/A";
                worksheet.Cell(row, 5).Value = order.Tamanho ?? "N/A";
                worksheet.Cell(row, 6).Value = order.CurrentStage;
                worksheet.Cell(row, 7).Value = order.CurrentStatus;
                worksheet.Cell(row, 8).Value = order.CreationDate;
                worksheet.Cell(row, 9).Value = order.AssignedUserName ?? "Unassigned";
            }

            worksheet.Columns().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }
}
