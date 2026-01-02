using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using tiempo_libre.Models;
using tiempo_libre.DTOs;
using tiempo_libre.Models.Enums;
using tiempo_libre.Logic;
using tiempo_libre.Services;

namespace tiempo_libre.Controllers
{
    [ApiController]
    [Route("api/genera-reservacion-turnos")]
    public class GeneraReservacionTurnosController : ControllerBase
    {
        private readonly GeneraReservacionTurnosService _service;
        private readonly ILogger<GeneraReservacionTurnosController> _logger;
        private readonly FreeTimeDbContext _dbContext;

        public GeneraReservacionTurnosController(GeneraReservacionTurnosService service, ILogger<GeneraReservacionTurnosController> logger, FreeTimeDbContext dbContext)
        {
            _service = service;
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpPost("ejecutar")]
        [Authorize(Roles = "SuperUsuario")]
        public async Task<IActionResult> Ejecutar([FromBody] AsignacionDeVacacionesRequest request)
        {
            // Get current user ID
            var currentUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUsername))
            {
                return Unauthorized(new ApiResponse<string>(false, null, "Usuario no autenticado"));
            }

            var currentUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == currentUsername);
            if (currentUser == null)
            {
                return Unauthorized(new ApiResponse<string>(false, null, "Usuario no encontrado"));
            }

            var response = await _service.EjecutarAsync(request, currentUser.Id);
            if (!response.Success)
            {
                // Si hay mensaje de error de validación, regresa BadRequest con el APIResponse
                if (!string.IsNullOrEmpty(response.ErrorMsg))
                    return BadRequest(response);
                // Si es error interno, regresa 200 pero con Success=false y el mensaje
                return Ok(response);
            }
            return Ok(response);
        }
    }
}
