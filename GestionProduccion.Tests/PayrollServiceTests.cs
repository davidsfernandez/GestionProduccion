using Moq;
using GestionProduccion.Data;
using GestionProduccion.Domain.Entities;
using GestionProduccion.Domain.Entities.HR;
using GestionProduccion.Domain.Enums.HR;
using GestionProduccion.Models.DTOs;
using GestionProduccion.Services;
using GestionProduccion.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace GestionProduccion.Tests
{
    public class PayrollServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task CalculateMonthlyPayrollAsync_ShouldIncorporateProductionBonuses()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var mockBonus = new Mock<IBonusCalculationService>();
            var mockAttendance = new Mock<IAttendanceService>();
            var mockAbsence = new Mock<IAbsenceService>();

            var userId = 10;
            var user = new User { Id = userId, FullName = "Expert Worker", Email = "worker@test.com" };
            var employee = new EmployeeProfile 
            { 
                UserId = userId, 
                BaseSalary = 2000m, 
                CPF = "123.456.789-00",
                JoinedDate = DateTime.UtcNow.AddYears(-2)
            };
            context.Users.Add(user);
            context.EmployeeProfiles.Add(employee);
            await context.SaveChangesAsync();

            // Mock 500 BRL in production bonuses (TotalAmount in BonusReportDto)
            mockBonus.Setup(s => s.CalculateUserBonusAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new BonusReportDto { TotalAmount = 500m, IsAtomicFailure = false });

            mockAbsence.Setup(s => s.GetEmployeeLeavesAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EmployeeLeaveDto>());

            var service = new PayrollService(context, mockBonus.Object, mockAttendance.Object, mockAbsence.Object);

            // Act
            var result = await service.CalculateMonthlyPayrollAsync(userId, 2024, 3);

            // Assert
            Assert.Equal(2000m, result.BaseSalary);
            Assert.Equal(500m, result.ProductionBonus);
            Assert.Equal(2500m, result.TotalEarnings);
            // INSS for 2500 (2024): (2500 * 0.09) - 21.18 = 203.82
            Assert.Equal(203.82m, result.InssDeduction);
        }

        [Fact]
        public async Task CalculateMonthlyPayrollAsync_ShouldDeductUnjustifiedAbsences()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var mockBonus = new Mock<IBonusCalculationService>();
            var mockAttendance = new Mock<IAttendanceService>();
            var mockAbsence = new Mock<IAbsenceService>();

            var userId = 20;
            var user = new User { Id = userId, FullName = "Late Worker", Email = "late@test.com" };
            var employee = new EmployeeProfile { UserId = userId, BaseSalary = 3000m, CPF = "000.000.000-00" };
            context.Users.Add(user);
            context.EmployeeProfiles.Add(employee);
            await context.SaveChangesAsync();

            mockBonus.Setup(s => s.CalculateUserBonusAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new BonusReportDto { TotalAmount = 0, IsAtomicFailure = false });

            // Simulate 2 unjustified absences
            mockAbsence.Setup(s => s.GetEmployeeLeavesAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EmployeeLeaveDto> 
                { 
                    new EmployeeLeaveDto { Type = LeaveType.Unjustified },
                    new EmployeeLeaveDto { Type = LeaveType.Unjustified }
                });

            var service = new PayrollService(context, mockBonus.Object, mockAttendance.Object, mockAbsence.Object);

            // Act
            var result = await service.CalculateMonthlyPayrollAsync(userId, 2024, 3);

            // Assert
            // Day value: 3000 / 30 = 100. 2 days = 200 reduction.
            Assert.Equal(200m, result.AbsenceDeduction);
            Assert.Equal(2800m, result.TotalEarnings - result.AbsenceDeduction);
        }
    }
}