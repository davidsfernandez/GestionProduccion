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

namespace GestionProduccion.Tests
{
    public class LeadServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateLeadAsync_ShouldCreateLeadAndSendNotifications()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var leadRepo = new LeadRepository(context);
            var mockEmail = new Mock<IEmailService>();
            var mockNotification = new Mock<INotificationService>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockLogger = new Mock<ILogger<LeadService>>();
            var mapper = new MainMapper();

            var service = new LeadService(leadRepo, mockEmail.Object, mockNotification.Object, mockUserRepo.Object, mapper, mockLogger.Object);
            var dto = new CreateLeadDto { Name = "Test Client", Email = "client@test.com", Message = "I need clothes" };

            // Act
            var result = await service.CreateLeadAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(LeadStatus.New, result.Status);

            var dbLead = await context.Leads.FirstOrDefaultAsync(l => l.Email == dto.Email);
            Assert.NotNull(dbLead);

            mockEmail.Verify(e => e.SendEmailAsync(dto.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockNotification.Verify(n => n.NotifyNewLeadAsync(dto.Name, dto.Email, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateLeadStatusAsync_ShouldUpdateStatusAndAddNote()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var lead = new Lead { Name = "Old Client", Email = "old@test.com", Status = LeadStatus.New };
            context.Leads.Add(lead);
            await context.SaveChangesAsync();

            var leadRepo = new LeadRepository(context);
            var mockEmail = new Mock<IEmailService>();
            var mockNotification = new Mock<INotificationService>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockLogger = new Mock<ILogger<LeadService>>();
            var mapper = new MainMapper();

            var service = new LeadService(leadRepo, mockEmail.Object, mockNotification.Object, mockUserRepo.Object, mapper, mockLogger.Object);

            // Act
            var result = await service.UpdateLeadStatusAsync(lead.Id, LeadStatus.Qualified, "Strong interest");

            // Assert
            Assert.Equal(LeadStatus.Qualified, result.Status);
            Assert.Contains("Strong interest", result.CommercialNotes!);
            
            var dbLead = await context.Leads.FindAsync(lead.Id);
            Assert.Equal(LeadStatus.Qualified, dbLead!.Status);
        }
    }
}