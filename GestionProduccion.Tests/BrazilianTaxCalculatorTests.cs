using GestionProduccion.Application.Utils.HR;
using Xunit;

namespace GestionProduccion.Tests
{
    public class BrazilianTaxCalculatorTests
    {
        [Theory]
        [InlineData(1412.00, 105.90)]  // Minimum wage bracket (7.5%)
        [InlineData(2500.00, 203.82)]  // Second bracket (9%) -> (2500 * 0.09) - 21.18
        [InlineData(3500.00, 318.82)]  // Third bracket (12%) -> (3500 * 0.12) - 101.18
        [InlineData(5000.00, 518.82)]  // Fourth bracket (14%) -> (5000 * 0.14) - 181.18
        [InlineData(8000.00, 908.85)]  // Above ceiling
        public void CalculateInss_ShouldReturnCorrectDeduction(decimal gross, decimal expected)
        {
            var result = BrazilianTaxCalculator.CalculateInss(gross);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(2000.00, 160.00)]
        [InlineData(5000.00, 400.00)]
        public void CalculateFgts_ShouldReturnEightPercent(decimal gross, decimal expected)
        {
            var result = BrazilianTaxCalculator.CalculateFgts(gross);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(2824.00, 0)]       // Exemption threshold with simplified discount
        [InlineData(3000.00, 13.20)]   // (3000 - 564.80) * 0.075 - 169.44
        [InlineData(5000.00, 335.15)]  // (5000 - 564.80) * 0.225 - 662.77
        public void CalculateIrrf_ShouldReturnCorrectTax(decimal taxable, decimal expected)
        {
            var result = BrazilianTaxCalculator.CalculateIrrf(taxable);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateTransportationDeduction_ShouldReturnSixPercent_WhenOptsIn()
        {
            var result = BrazilianTaxCalculator.CalculateTransportationDeduction(2000m, true);
            Assert.Equal(120m, result);
        }

        [Fact]
        public void CalculateTransportationDeduction_ShouldReturnZero_WhenOptsOut()
        {
            var result = BrazilianTaxCalculator.CalculateTransportationDeduction(2000m, false);
            Assert.Equal(0m, result);
        }
    }
}