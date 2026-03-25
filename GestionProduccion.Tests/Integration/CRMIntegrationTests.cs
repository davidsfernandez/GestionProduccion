using Moq;
using GestionProduccion.Data;
using GestionProduccion.Data.Repositories;
using GestionProduccion.Domain.Entities.CRM;
using GestionProduccion.Domain.Enums;
using GestionProduccion.Domain.Interfaces.Repositories;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services;
using GestionProduccion.Services.Interfaces;
using GestionProduccion.Application.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace GestionProduccion.Tests.Integration
{
    public class CRMIntegrationTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task FullLeadToQuoteFlow_ShouldWorkCorrectly()
        {
            // 1. Setup
            using var context = GetInMemoryDbContext();
            var leadRepo = new LeadRepository(context);
            var mockEmail = new Mock<IEmailService>();
            var mockNotification = new Mock<INotificationService>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockLoggerLead = new Mock<ILogger<LeadService>>();
            var mapper = new MainMapper();

            var leadService = new LeadService(leadRepo, mockEmail.Object, mockNotification.Object, mockUserRepo.Object, mapper, mockLoggerLead.Object);
            var quoteService = new QuoteService(context);

            // 2. Create Lead (Public API simulation)
            var createLeadDto = new CreateLeadDto 
            { 
                Name = "Igor's Big Client", 
                Email = "big@client.com", 
                Message = "We want 1000 uniforms." 
            };
            var leadDto = await leadService.CreateLeadAsync(createLeadDto);
            Assert.Equal(LeadStatus.New, leadDto.Status);

            // 3. Update Lead Status (CRM Management simulation)
            var qualifiedLead = await leadService.UpdateLeadStatusAsync(leadDto.Id, LeadStatus.Qualified, "Verified company");
            Assert.Equal(LeadStatus.Qualified, qualifiedLead.Status);

            // 4. Create Quote
            var createQuoteRequest = new CreateQuoteRequest
            {
                LeadId = qualifiedLead.Id,
                Notes = "Standard production pricing",
                Items = new List<CreateQuoteItemRequest>
                {
                    new CreateQuoteItemRequest { Description = "Corporate Uniforms", Quantity = 1000, UnitPrice = 45.00m }
                }
            };
            var quoteDto = await quoteService.CreateQuoteAsync(createQuoteRequest);
            Assert.NotNull(quoteDto);
            Assert.Equal(45000m, quoteDto.TotalAmount);
            Assert.Equal(QuoteStatusDto.Draft, quoteDto.Status);

            // 5. Verify persistence
            var quotesInDb = await quoteService.GetLeadQuotesAsync(qualifiedLead.Id);
            Assert.Single(quotesInDb);
            Assert.Equal(quoteDto.Id, quotesInDb[0].Id);
        }
    }
}