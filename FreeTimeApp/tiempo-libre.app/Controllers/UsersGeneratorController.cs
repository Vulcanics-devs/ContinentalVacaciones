using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using tiempo_libre.Models;
using tiempo_libre;
using Microsoft.AspNetCore.Authorization;

namespace tiempo_libre.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersGeneratorController : ControllerBase
    {
    private readonly FreeTimeDbContext _dbContext;
    private readonly GenericUsersGenerator _generator;
    private readonly ILogger<UsersGeneratorController> _logger;
    private readonly ILogger<GenericUsersGenerator> _generatorLogger;

        public UsersGeneratorController(FreeTimeDbContext dbContext, ILogger<UsersGeneratorController> logger, ILogger<GenericUsersGenerator> generatorLogger)
        {
            _dbContext = dbContext;
            _logger = logger;
            _generatorLogger = generatorLogger;
            _generator = new GenericUsersGenerator(_dbContext, _generatorLogger);
        }

        [HttpPost("generate-users-from-empleados")]
        [Authorize(Roles = "SuperUsuario")]
        public async Task<IActionResult> GenerateUsersFromEmpleados()
        {
            _logger.LogInformation("POST /api/UsersGenerator/generate-users-from-empleados invocado por usuario: {User}", User?.Identity?.Name ?? "Desconocido");
            try
            {
                var created = await _generator.GenerateUsersFromEmpleadosAsync();
                _logger.LogInformation("Usuarios generados correctamente: {Created}", created);
                var response = new ApiResponse<object>(true, new { created }, $"Usuarios generados correctamente: {created}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al generar usuarios");
                var response = new ApiResponse<object>(false, null, "Error inesperado al generar usuarios");
                return StatusCode(500, response);
            }
        }
    }
}
