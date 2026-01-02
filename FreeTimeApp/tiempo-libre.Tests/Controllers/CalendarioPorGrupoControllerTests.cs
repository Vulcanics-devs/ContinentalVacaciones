using System;
using System.Collections.Generic;
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
    public class CalendarioPorGrupoControllerTests
    {
        private CalendarioPorGrupoController CreateController(
            ApiResponse<List<EmpleadoCalendarioGrupoDto>> serviceResponse,
            Exception? exception = null)
        {
            var serviceMock = new Mock<CalendariosEmpleadosService>(null!);
            if (exception == null)
            {
                serviceMock.Setup(s => s.ObtenerCalendarioPorGrupoAsync(
                    It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                    .ReturnsAsync(serviceResponse);
            }
            else
            {
                serviceMock.Setup(s => s.ObtenerCalendarioPorGrupoAsync(
                    It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                    .ThrowsAsync(exception);
            }
            var loggerMock = new Mock<ILogger<CalendarioPorGrupoController>>();
            return new CalendarioPorGrupoController(serviceMock.Object, loggerMock.Object);
        }

        [Fact]
        public async Task ObtenerCalendarioPorGrupo_ValidRequestWithEmpleados_ReturnsOkWithList()
        {
            var empleados = new List<EmpleadoCalendarioGrupoDto> {
                new EmpleadoCalendarioGrupoDto {
                    IdUsuarioEmpleadoSindicalizado = 1,
                    IdGrupo = 1,
                    NominaEmpleado = "123",
                    NombreCompletoEmpleado = "Empleado Uno",
                    Dias = new List<DiaCalendarioEmpleadoDto> {
                        new DiaCalendarioEmpleadoDto {
                            IdDiaCalendarioEmpleado = 1,
                            Fecha = DateTime.Today,
                            TipoActividadDelDia = "1",
                            Detalles = "Laboral"
                        }
                    }
                }
            };
            var apiResponse = new ApiResponse<List<EmpleadoCalendarioGrupoDto>>(true, empleados, null);
            var controller = CreateController(apiResponse);

            var result = await controller.ObtenerCalendarioPorGrupo(1, DateTime.Today, DateTime.Today.AddDays(1));

            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            var response = okResult.Value as ApiResponse<List<EmpleadoCalendarioGrupoDto>>;
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Data.Should().NotBeNull();
            response.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task ObtenerCalendarioPorGrupo_ValidRequestNoEmpleados_ReturnsOkWithEmptyList()
        {
            var apiResponse = new ApiResponse<List<EmpleadoCalendarioGrupoDto>>(true, new List<EmpleadoCalendarioGrupoDto>(), null);
            var controller = CreateController(apiResponse);

            var result = await controller.ObtenerCalendarioPorGrupo(1, DateTime.Today, DateTime.Today.AddDays(1));

            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            var response = okResult.Value as ApiResponse<List<EmpleadoCalendarioGrupoDto>>;
            response.Should().NotBeNull();
            response!.Success.Should().BeTrue();
            response.Data.Should().NotBeNull();
            response.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task ObtenerCalendarioPorGrupo_InvalidGrupoId_ReturnsBadRequest()
        {
            var apiResponse = new ApiResponse<List<EmpleadoCalendarioGrupoDto>>(false, null, "El grupoId debe ser un entero positivo.");
            var controller = CreateController(apiResponse);

            var result = await controller.ObtenerCalendarioPorGrupo(-1, DateTime.Today, DateTime.Today.AddDays(1));

            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.StatusCode.Should().Be(400);
            var response = badRequest.Value as ApiResponse<List<EmpleadoCalendarioGrupoDto>>;
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.ErrorMsg.Should().Be("El grupoId debe ser un entero positivo.");
        }

        [Fact]
        public async Task ObtenerCalendarioPorGrupo_FechaInicioMayorAFechaFinal_ReturnsBadRequest()
        {
            var apiResponse = new ApiResponse<List<EmpleadoCalendarioGrupoDto>>(false, null, "La fechaInicio debe ser menor a la fechaFinal.");
            var controller = CreateController(apiResponse);

            var result = await controller.ObtenerCalendarioPorGrupo(1, DateTime.Today.AddDays(2), DateTime.Today);

            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.StatusCode.Should().Be(400);
            var response = badRequest.Value as ApiResponse<List<EmpleadoCalendarioGrupoDto>>;
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.ErrorMsg.Should().Be("La fechaInicio debe ser menor a la fechaFinal.");
        }

        [Fact]
        public async Task ObtenerCalendarioPorGrupo_RangoMayorA31Dias_ReturnsBadRequest()
        {
            var apiResponse = new ApiResponse<List<EmpleadoCalendarioGrupoDto>>(false, null, "El rango de fechas no puede ser mayor a 31 días.");
            var controller = CreateController(apiResponse);

            var result = await controller.ObtenerCalendarioPorGrupo(1, DateTime.Today, DateTime.Today.AddDays(32));

            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.StatusCode.Should().Be(400);
            var response = badRequest.Value as ApiResponse<List<EmpleadoCalendarioGrupoDto>>;
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.ErrorMsg.Should().Be("El rango de fechas no puede ser mayor a 31 días.");
        }

        [Fact]
        public async Task ObtenerCalendarioPorGrupo_GrupoNoExistente_ReturnsNotFound()
        {
            var apiResponse = new ApiResponse<List<EmpleadoCalendarioGrupoDto>>(false, null, "El grupo especificado no existe.");
            var controller = CreateController(apiResponse);

            var result = await controller.ObtenerCalendarioPorGrupo(999, DateTime.Today, DateTime.Today.AddDays(1));

            var notFound = result as NotFoundObjectResult;
            notFound.Should().NotBeNull();
            notFound!.StatusCode.Should().Be(404);
            var response = notFound.Value as ApiResponse<List<EmpleadoCalendarioGrupoDto>>;
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.ErrorMsg.Should().Be("El grupo especificado no existe.");
        }

        [Fact]
        public async Task ObtenerCalendarioPorGrupo_UnexpectedException_ReturnsInternalServerError()
        {
            var controller = CreateController(null!, new Exception("Error inesperado"));

            var result = await controller.ObtenerCalendarioPorGrupo(1, DateTime.Today, DateTime.Today.AddDays(1));

            var objectResult = result as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be(500);
            var response = objectResult.Value as ApiResponse<List<EmpleadoCalendarioGrupoDto>>;
            response.Should().NotBeNull();
            response!.Success.Should().BeFalse();
            response.ErrorMsg.Should().Contain("Error inesperado");
        }
    }
}
