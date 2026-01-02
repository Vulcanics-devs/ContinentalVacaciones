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
    [Route("api/calendario/por-grupo")]
    public class CalendarioPorGrupoController : ControllerBase
    {
        private readonly CalendariosEmpleadosService _service;
        private readonly CalendarioGrupoService _calendarioGrupoService;
        private readonly ILogger<CalendarioPorGrupoController> _logger;

        public CalendarioPorGrupoController(CalendariosEmpleadosService service, CalendarioGrupoService calendarioGrupoService, ILogger<CalendarioPorGrupoController> logger)
        {
            _service = service;
            _calendarioGrupoService = calendarioGrupoService;
            _logger = logger;
        }

        [HttpGet("{grupoId}")]
        public virtual async Task<IActionResult> ObtenerCalendarioPorGrupo(
            [FromRoute] int grupoId,
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFinal)
        {
            try
            {
                var response = await _service.ObtenerCalendarioPorGrupoAsync(grupoId, fechaInicio, fechaFinal);
                if (!response.Success)
                {
                    if (response.ErrorMsg == "El grupo especificado no existe.")
                        return NotFound(response);
                    if (!string.IsNullOrEmpty(response.ErrorMsg))
                        return BadRequest(response);
                    return Ok(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<List<EmpleadoCalendarioGrupoDto>>(false, null, $"Error inesperado: {ex.Message}");
                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost("{grupoId}")]
        [Authorize]
        public virtual async Task<IActionResult> ObtenerCalendarioGrupo(
            [FromRoute] int grupoId,
            [FromBody] CalendarioGrupoRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage));
                    return BadRequest(new ApiResponse<CalendarioGrupoResponse>(false, null, $"Datos de entrada inválidos: {errors}"));
                }

                var response = await _calendarioGrupoService.ObtenerCalendarioGrupoAsync(grupoId, request.Inicio, request.Fin);
                
                if (!response.Success)
                {
                    if (response.ErrorMsg == "El grupo especificado no existe.")
                        return NotFound(response);
                    if (!string.IsNullOrEmpty(response.ErrorMsg))
                        return BadRequest(response);
                    return Ok(response);
                }
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener calendario del grupo {GrupoId}", grupoId);
                var errorResponse = new ApiResponse<CalendarioGrupoResponse>(false, null, $"Error inesperado: {ex.Message}");
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Obtener calendario del grupo incluyendo información de incidencias
        /// </summary>
        /// <param name="grupoId">ID del grupo</param>
        /// <param name="request">Rango de fechas</param>
        /// <returns>Calendario del grupo con turnos e incidencias</returns>
        [HttpPost("{grupoId}/con-incidencias")]
        [Authorize]
        public async Task<IActionResult> ObtenerCalendarioGrupoConIncidencias(
            [FromRoute] int grupoId,
            [FromBody] CalendarioGrupoRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join("; ", ModelState
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage));
                    return BadRequest(new ApiResponse<CalendarioGrupoResponse>(false, null, $"Datos de entrada inválidos: {errors}"));
                }

                var response = await _calendarioGrupoService.ObtenerCalendarioGrupoConIncidenciasAsync(grupoId, request.Inicio, request.Fin);

                if (!response.Success)
                {
                    if (response.ErrorMsg == "El grupo especificado no existe.")
                        return NotFound(response);
                    if (!string.IsNullOrEmpty(response.ErrorMsg))
                        return BadRequest(response);
                    return Ok(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener calendario con incidencias del grupo {GrupoId}", grupoId);
                var errorResponse = new ApiResponse<CalendarioGrupoResponse>(false, null, $"Error inesperado: {ex.Message}");
                return StatusCode(500, errorResponse);
            }
        }
    }
}
