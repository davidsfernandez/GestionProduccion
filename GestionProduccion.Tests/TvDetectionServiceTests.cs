using Moq;
using Microsoft.JSInterop;
using GestionProduccion.Client.Services;
using Xunit;

namespace GestionProduccion.Tests
{
    public class TvDetectionServiceTests
    {
        private readonly Mock<IJSRuntime> _mockJsRuntime;
        private readonly TvDetectionService _service;

        public TvDetectionServiceTests()
        {
            _mockJsRuntime = new Mock<IJSRuntime>();
            _service = new TvDetectionService(_mockJsRuntime.Object);
        }

        [Fact]
        public async Task IsTvDeviceAsync_ShouldReturnTrue_WhenJsReturnsTrue()
        {
            // Arrange
            _mockJsRuntime.Setup(js => js.InvokeAsync<bool>("tvDetection.isTvDevice", It.IsAny<object[]>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.IsTvDeviceAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsTvDeviceAsync_ShouldReturnFalse_WhenJsReturnsFalse()
        {
            // Arrange
            _mockJsRuntime.Setup(js => js.InvokeAsync<bool>("tvDetection.isTvDevice", It.IsAny<object[]>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.IsTvDeviceAsync();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetScreenResolutionAsync_ShouldReturnResolution_WhenJsReturnsData()
        {
            // Arrange
            var expected = new ScreenResolution { Width = 1920, Height = 1080 };
            _mockJsRuntime.Setup(js => js.InvokeAsync<ScreenResolution>("tvDetection.getScreenResolution", It.IsAny<object[]>()))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetScreenResolutionAsync();

            // Assert
            Assert.Equal(expected.Width, result.Width);
            Assert.Equal(expected.Height, result.Height);
        }
    }
}