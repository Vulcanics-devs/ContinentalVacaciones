using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using tiempo_libre.Controllers;
using tiempo_libre.Models;
using Xunit;

namespace tiempo_libre.Tests.Controllers
{
    public class VacacionesControllerTest
    {
        private FreeTimeDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<FreeTimeDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new FreeTimeDbContext(options);
        }

        [Fact]
        public void GetVacacionesPorAntiguedad_ReturnsAllRules()
        {
            // Arrange
            var db = GetInMemoryDbContext("GetVacacionesPorAntiguedad_ReturnsAllRules");
            db.VacacionesPorAntiguedad.AddRange(new List<VacacionesPorAntiguedad>
            {
                new VacacionesPorAntiguedad { AntiguedadEnAniosRangoInicial = 1, TotalDiasDeVacaciones = 12 },
                new VacacionesPorAntiguedad { AntiguedadEnAniosRangoInicial = 2, TotalDiasDeVacaciones = 14 }
            });
            db.SaveChanges();
            var controller = new VacacionesController(db);

            // Act
            var result = controller.GetVacacionesPorAntiguedad();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            var response = okResult.Value as ApiResponse<List<VacacionesPorAntiguedad>>;
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.Should().HaveCount(2);
            response.Data.Should().Contain(r => r.AntiguedadEnAniosRangoInicial == 1 && r.TotalDiasDeVacaciones == 12);
        }

        [Fact]
        public void GetVacacionesPorAntiguedad_Returns500OnException()
        {
            // Arrange
            var mockContext = new Mock<FreeTimeDbContext>();
            mockContext.Setup(c => c.VacacionesPorAntiguedad).Throws(new System.Exception("DB error"));
            var controller = new VacacionesController(mockContext.Object);

            // Act
            var result = controller.GetVacacionesPorAntiguedad();

            // Assert
            var objectResult = result as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult.StatusCode.Should().Be(500);
            var response = objectResult.Value as ApiResponse<List<VacacionesPorAntiguedad>>;
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.ErrorMsg.Should().Contain("DB error");
        }
    }
}
