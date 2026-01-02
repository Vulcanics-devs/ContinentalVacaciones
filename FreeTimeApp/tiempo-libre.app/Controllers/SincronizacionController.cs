using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using tiempo_libre.Models;
using tiempo_libre.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using tiempo_libre.Services;

namespace tiempo_libre.app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperUsuario")]
    public class SincronizacionController : ControllerBase
    {
        private readonly tiempo_libre.Services.SincronizacionRolesService _sincronizacionService;

        public SincronizacionController(tiempo_libre.Services.SincronizacionRolesService sincronizacionService)
        {
            _sincronizacionService = sincronizacionService;
        }

        [HttpPost("sincronizar-roles")]
        public async Task<IActionResult> SincronizarRoles()
        {
            try
            {
                var actualizados = await _sincronizacionService.SincronizarRolesDesdeRegla();
                return Ok(new ApiResponse<object>(true, new { RegistrosActualizados = actualizados }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, null, $"Error: {ex.Message}"));
            }
        }
    }
}