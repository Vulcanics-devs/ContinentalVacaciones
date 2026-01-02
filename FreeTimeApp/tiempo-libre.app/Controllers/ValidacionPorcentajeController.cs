using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using tiempo_libre.Services;
using tiempo_libre.Models;
using tiempo_libre.DTOs;

namespace tiempo_libre.Controllers
{
    [ApiController]
    [Route("api/validacion-porcentaje")]
    [Authorize]
    public class ValidacionPorcentajeController : ControllerBase
    {
        private readonly ValidadorPorcentajeService _validadorService;
        private readonly ILogger<ValidacionPorcentajeController> _logger;

        public ValidacionPorcentajeController(
            ValidadorPorcentajeService validadorService,
            ILogger<ValidacionPorcentajeController> logger)
        {
            _validadorService = validadorService;
            _logger = logger;
        }

        /// <summary>
        /// Valida si un grupo puede tener ausencias adicionales respetando el porcentaje configurado
        /// </summary>
        /// <param name="grupoId">ID del grupo a validar</param>
        /// <param name="ausenciasSolicitadas">Número de ausencias adicionales solicitadas (por defecto 1)</param>
        /// <returns>True si se pueden agregar las ausencias, false si excedería el límite</returns>
        [HttpGet("validar-grupo/{grupoId}")]
        public async Task<IActionResult> ValidarAusenciasGrupo(
            [FromRoute] int grupoId,
            [FromQuery] int ausenciasSolicitadas = 1)
        {
            try
            {
                _logger.LogInformation(
                    "Validando ausencias para grupo {GrupoId}, ausencias solicitadas: {Ausencias}",
                    grupoId, ausenciasSolicitadas);

                var puedeAusentarse = await _validadorService.PuedeGrupoTenerAusencias(
                    grupoId, ausenciasSolicitadas);

                return Ok(new ApiResponse<object>(
                    true,
                    new {
                        grupoId,
                        puedeAgregarAusencias = puedeAusentarse,
                        ausenciasSolicitadas
                    },
                    null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar ausencias para grupo {GrupoId}", grupoId);
                return StatusCode(500, new ApiResponse<object>(false, null, $"Error inesperado: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene información detallada sobre el estado de ausencias de un grupo
        /// </summary>
        /// <param name="grupoId">ID del grupo</param>
        /// <returns>Estado detallado de ausencias del grupo</returns>
        [HttpGet("estado-grupo/{grupoId}")]
        public async Task<IActionResult> ObtenerEstadoGrupo([FromRoute] int grupoId)
        {
            try
            {
                _logger.LogInformation("Consultando estado de ausencias para grupo {GrupoId}", grupoId);

                var estado = await _validadorService.ObtenerEstadoAusenciasGrupo(grupoId);

                if (estado == null)
                {
                    return NotFound(new ApiResponse<object>(
                        false,
                        null,
                        $"No se encontró el grupo {grupoId} o la configuración"));
                }

                return Ok(new ApiResponse<EstadoAusenciasGrupo>(true, estado, null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estado del grupo {GrupoId}", grupoId);
                return StatusCode(500, new ApiResponse<object>(false, null, $"Error inesperado: {ex.Message}"));
            }
        }

        /// <summary>
        /// Calcula el tamaño mínimo de grupo para poder aplicar el porcentaje configurado
        /// </summary>
        /// <param name="porcentaje">Porcentaje a evaluar (opcional, usa el configurado si no se especifica)</param>
        /// <returns>Número mínimo de empleados</returns>
        [HttpGet("calcular-minimo-empleados")]
        public async Task<IActionResult> CalcularMinimoEmpleados([FromQuery] decimal? porcentaje = null)
        {
            try
            {
                decimal porcentajeUsar;

                if (porcentaje.HasValue)
                {
                    porcentajeUsar = porcentaje.Value;
                }
                else
                {
                    // Obtener el porcentaje de la configuración
                    var config = await _validadorService.ObtenerEstadoAusenciasGrupo(1); // Dummy para obtener config
                    porcentajeUsar = 4.6m; // Default si no hay config

                    if (config != null)
                    {
                        porcentajeUsar = config.PorcentajeMaximoPermitido;
                    }
                }

                var minimo = _validadorService.CalcularMinimoEmpleadosParaPorcentaje(porcentajeUsar);

                return Ok(new ApiResponse<object>(
                    true,
                    new
                    {
                        porcentajeMaximo = porcentajeUsar,
                        minimoEmpleados = minimo,
                        descripcion = $"Con {porcentajeUsar}% de déficit máximo, se necesitan al menos {minimo} empleados para que 1 ausencia no exceda el límite",
                        nota = $"Grupos con menos de {minimo} empleados usarán reglas especiales (máximo 1 ausencia)"
                    },
                    null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al calcular mínimo de empleados");
                return StatusCode(500, new ApiResponse<object>(false, null, $"Error inesperado: {ex.Message}"));
            }
        }
    }
}