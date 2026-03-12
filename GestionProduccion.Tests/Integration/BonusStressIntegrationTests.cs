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
using GestionProduccion.Resources;
using Xunit;

namespace GestionProduccion.Tests.Integration;

public class BonusStressIntegrationTests : BaseIntegrationTest
{
    public BonusStressIntegrationTests(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Alejandro_Scenario_ShouldCalculateCorrectBonus_WithLowQuality()
    {
        // ARRANGE: Setup factory environment
        await SeedDataAsync();
        AuthenticateAs(UserRole.Administrator);

        // 1. Create Production Order (200 pieces)
        var orderRequest = new CreateProductionOrderRequest
        {
            ProductId = 1,
            Quantity = 200,
            EstimatedCompletionAt = DateTime.UtcNow.AddDays(5),
            UserId = 2, // Alejandro (Operator Test from SeedData)
            Sizes = new List<ProductionOrderSizeDto> { new() { Size = "M", Quantity = 200 } }
        };

        var createResp = await Client.PostAsJsonAsync("/api/ProductionOrders", orderRequest);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var orderDto = (await createResp.Content.ReadFromJsonAsync<ApiResponse<ProductionOrderDto>>(JsonOptions))!.Data!;

        // 2. Advance to Review stage (simulating production flow)
        await Client.PostAsync($"/api/ProductionOrders/{orderDto.Id}/advance", null); // To Sewing
        await Client.PostAsync($"/api/ProductionOrders/{orderDto.Id}/advance", null); // To Review

        // 3. Register Partial Output (10 pieces completed today)
        var outputRequest = new Dictionary<int, int> { { orderDto.Sizes!.First().Id, 10 } };
        var outputResp = await Client.PostAsJsonAsync($"/api/ProductionOrders/{orderDto.Id}/partial-output", outputRequest);
        outputResp.IsSuccessStatusCode.Should().BeTrue();

        // 4. Register 8 Defects attributed specifically to Alejandro
        // Note: Using MultiPart content as per QAController implementation
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(orderDto.Id.ToString()), "ProductionOrderId");
        content.Add(new StringContent("Costura Torta"), "Reason");
        content.Add(new StringContent("8"), "Quantity");
        content.Add(new StringContent("2"), "ResponsibleUserId"); // Alejandro (Id 2 in SeedData)

        var defectResp = await Client.PostAsync("/api/QA", content);
        defectResp.IsSuccessStatusCode.Should().BeTrue();

        // ACT: Calculate Bonus for Alejandro
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var start = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
        var end = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");
        
        var bonusUrl = $"/api/BonusCalculation?userId=2&startDate={start}&endDate={end}&isProfessional=true";
        var bonusResp = await Client.GetAsync(bonusUrl);
        
        // ASSERT
        bonusResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = (await bonusResp.Content.ReadFromJsonAsync<ApiResponse<BonusReportDto>>(JsonOptions))!.Data!;

        // Total Produced should be 10 (Avoided double counting from stages)
        result.TotalProduced.Should().Be(10, "Should only count pieces that reached the current stage/output");
        
        // Defects should be 8
        result.TotalDefects.Should().Be(8, "Should correctly attribute defects to the responsible user");

        // Quality ratio: 8 defects / 10 produced * 100 = 80%
        result.DefectPercentage.Should().BeInRange(70, 90);

        // With 80% defect ratio and standard limit being likely 5%, QualityFactor should be 0
        result.QualityFactor.Should().Be(0, "High defect ratio should nullify the quality component of the bonus");
        
        // Final bonus should be significantly low or zero depending on weights
        // Given 70% is individual effort and individual quality is 0, bonus should be <= 30% (from team share)
        result.FinalBonusPercentage.Should().BeLessThan(40, "Bonus must be penalized by poor individual quality");
    }
}
