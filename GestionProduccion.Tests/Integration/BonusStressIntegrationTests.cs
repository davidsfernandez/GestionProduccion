/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Models.DTOs;
using Xunit;

namespace GestionProduccion.Tests.Integration;

public class BonusStressIntegrationTests : BaseIntegrationTest
{
    public BonusStressIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Alejandro_Scenario_ShouldCalculateCorrectBonus_WithIdentityFix()
    {
        // ARRANGE: Setup factory environment
        await SeedDataAsync();
        
        // 1. Create Production Order as Admin
        AuthenticateAs(UserRole.Administrator, 1);
        var orderRequest = new CreateProductionOrderRequest
        {
            ProductId = 1,
            Quantity = 200,
            EstimatedCompletionAt = DateTime.UtcNow.AddDays(5),
            UserId = 2, // Alejandro (Operator Test from SeedData)
            Sizes = new List<ProductionOrderSizeRequest> { new() { Size = "M", Quantity = 200 } }
        };

        var createResp = await Client.PostAsJsonAsync("/api/ProductionOrders", orderRequest);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var orderDto = (await createResp.Content.ReadFromJsonAsync<ApiResponse<ProductionOrderDto>>(JsonOptions))!.Data!;

        // 2. Advance to Review stage
        await Client.PostAsync($"/api/ProductionOrders/{orderDto.Id}/advance", null); // To Sewing
        await Client.PostAsync($"/api/ProductionOrders/{orderDto.Id}/advance", null); // To Review

        // 3. Register Partial Output as ALEJANDRO (UserId 2)
        AuthenticateAs(UserRole.Operational, 2);
        var outputRequest = new PartialOutputRequest 
        { 
            SizeOutputs = new Dictionary<int, int> { { orderDto.Sizes!.First().Id, 10 } } 
        };
        var outputResp = await Client.PostAsJsonAsync($"/api/ProductionOrders/{orderDto.Id}/partial-output", outputRequest);
        outputResp.IsSuccessStatusCode.Should().BeTrue();

        // 4. Register 8 Defects as Admin (attributed to Alejandro)
        AuthenticateAs(UserRole.Administrator, 1);
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(orderDto.Id.ToString()), "ProductionOrderId");
        content.Add(new StringContent("Costura Torta"), "Reason");
        content.Add(new StringContent("8"), "Quantity");
        content.Add(new StringContent("2"), "ResponsibleUserId"); // Attributed to Alejandro

        var defectResp = await Client.PostAsync("/api/QA", content);
        defectResp.IsSuccessStatusCode.Should().BeTrue();

        // ACT: Calculate Bonus for Alejandro
        var start = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
        var end = DateTime.UtcNow.Date.AddDays(1).ToString("yyyy-MM-dd");
        
        var bonusUrl = $"/api/BonusCalculation?userId=2&startDate={start}&endDate={end}";
        var bonusResp = await Client.GetAsync(bonusUrl);
        
        // ASSERT
        bonusResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = (await bonusResp.Content.ReadFromJsonAsync<ApiResponse<BonusReportDto>>(JsonOptions))!.Data!;

        // Check correct volume (10 pieces registered by Alejandro)
        result.TotalProduced.Should().Be(10, "Should only count pieces that Alejandro actually registered in the period");
        
        // Check correct defect attribution
        result.TotalDefects.Should().Be(8, "Defects must be attributed to the responsible user (Alejandro)");

        // Quality ratio: 8 defects / (200 corte + 200 costura + 10 revisión) = 8 / 410 = ~1.9%
        // The fix in Phase 4 uses total operations as denominator
        result.DefectPercentage.Should().BeGreaterThan(0);
        result.QualityFactor.Should().BeLessThan(1, "Quality factor must penalize the bonus");
        
        // Final check
        result.FinalBonusPercentage.Should().BeLessThan(95, "Bonus must be significantly lower than the default 95%");
    }
}
