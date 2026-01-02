using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using tiempo_libre.Controllers;
using tiempo_libre.DTOs;
using tiempo_libre.Models;
using tiempo_libre.Services;
using Xunit;

namespace tiempo_libre.Tests.Controllers
{
    public class GeneraReservacionTurnosControllerTests
    {
        private GeneraReservacionTurnosController CreateController(ApiResponse<string> serviceResponse)
        {
            var serviceMock = new Mock<GeneraReservacionTurnosService>(null!, null!);
            serviceMock.Setup(s => s.EjecutarAsync(It.IsAny<AsignacionDeVacacionesRequest>()))
                .ReturnsAsync(serviceResponse);
            var loggerMock = new Mock<ILogger<GeneraReservacionTurnosController>>();
            return new GeneraReservacionTurnosController(serviceMock.Object, loggerMock.Object);
        }

        [Fact]
        public async Task Ejecutar_ValidRequest_ReturnsOkApiResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<string>(true, "OK", null);
            var controller = CreateController(apiResponse);
            var request = new AsignacionDeVacacionesRequest();

            // Act
            var result = await controller.Ejecutar(request);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(apiResponse);
        }

        [Fact]
        public async Task Ejecutar_InvalidRequest_ReturnsBadRequestApiResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<string>(false, null, "Error de validación");
            var controller = CreateController(apiResponse);
            var request = new AsignacionDeVacacionesRequest();

            // Act
            var result = await controller.Ejecutar(request);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().BeEquivalentTo(apiResponse);
        }

        [Fact]
        public async Task Ejecutar_InternalError_ReturnsOkApiResponseWithError()
        {
            // Arrange
            var apiResponse = new ApiResponse<string>(false, null, null); // Simula error interno sin mensaje
            var controller = CreateController(apiResponse);
            var request = new AsignacionDeVacacionesRequest();

            // Act
            var result = await controller.Ejecutar(request);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(apiResponse);
        }

        [Fact]
        public async Task Ejecutar_AlwaysReturnsApiResponseStandard()
        {
            // Arrange
            var apiResponse = new ApiResponse<string>(true, "Proceso iniciado", null);
            var controller = CreateController(apiResponse);
            var request = new AsignacionDeVacacionesRequest();

            // Act
            var result = await controller.Ejecutar(request);

            // Assert
            (result is ObjectResult).Should().BeTrue();
            var objectResult = result as ObjectResult;
            objectResult!.Value.Should().BeOfType<ApiResponse<string>>();
        }
    }
}
