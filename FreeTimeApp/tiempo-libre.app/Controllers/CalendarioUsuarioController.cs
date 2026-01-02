using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using tiempo_libre.Services;
using tiempo_libre.DTOs;
using tiempo_libre.Models;

namespace tiempo_libre.Controllers
{
    [ApiController]
    [Route("api/calendario/usuario")]
    [Authorize]
    public class CalendarioUsuarioController : ControllerBase
    {
        private readonly CalendarioGrupoService _calendarioService;
        private readonly ILogger<CalendarioUsuarioController> _logger;

        public CalendarioUsuarioController(
            CalendarioGrupoService calendarioService,
            ILogger<CalendarioUsuarioController> logger)
        {
            _calendarioService = calendarioService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener el calendario de un usuario específico incluyendo incidencias
        /// </summary>
        /// <param name="usuarioId">ID del usuario</param>
        /// <param name="request">Rango de fechas</param>
        /// <returns>Calendario del usuario con turnos e incidencias</returns>
        [HttpPost("{usuarioId}")]
        public async Task<IActionResult> ObtenerCalendarioUsuario(
            [FromRoute] int usuarioId,
            [FromBody] CalendarioUsuarioRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage));
                    return BadRequest(new ApiResponse<CalendarioUsuarioResponse>(false, null, $"Datos de entrada inválidos: {errors}"));
                }

                var response = await _calendarioService.ObtenerCalendarioUsuarioAsync(usuarioId, request.Inicio, request.Fin);

                if (!response.Success)
                {
                    if (response.ErrorMsg == "El usuario especificado no existe.")
                        return NotFound(response);
                    if (!string.IsNullOrEmpty(response.ErrorMsg))
                        return BadRequest(response);
                    return Ok(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener calendario del usuario {UsuarioId}", usuarioId);
                var errorResponse = new ApiResponse<CalendarioUsuarioResponse>(false, null, $"Error inesperado: {ex.Message}");
                return StatusCode(500, errorResponse);
            }
        }
    }
}