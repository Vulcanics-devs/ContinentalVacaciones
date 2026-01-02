using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using tiempo_libre.Models.Enums;
using tiempo_libre.Models;
using System.Collections.Generic;
using System.Linq;

namespace tiempo_libre.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DiasInhabilesController : ControllerBase
{
	private readonly ILogger<DiasInhabilesController> _logger;

	public DiasInhabilesController(ILogger<DiasInhabilesController> logger)
	{
		_logger = logger;
	}
	[HttpPost]
	[Authorize(Roles = "SuperUsuario")]
	public IActionResult CreateDiasInhabiles([FromBody] DiasInhabilesCreateRequest request, [FromServices] FreeTimeDbContext db)
	{
		_logger.LogInformation("Intentando crear días inhábiles: {@Request}", request);
		// Validar tipo de actividad
		if (!Enum.IsDefined(typeof(TipoActividadDelDiaEnum), request.TipoActividadDelDia))
		{
			_logger.LogWarning("TipoActividadDelDia inválido: {Tipo}", request.TipoActividadDelDia);
			return base.BadRequest(new ApiResponse<object>(false, null, "TipoActividadDelDia inválido"));
		}
		if (string.IsNullOrWhiteSpace(request.Detalles))
		{
			_logger.LogWarning("Detalles es requerido");
			return base.BadRequest(new ApiResponse<object>(false, null, "El campo Detalles es requerido"));
		}
		if (request.Detalles.Length > 250)
		{
			_logger.LogWarning("Detalles excede el máximo de 250 caracteres");
			return base.BadRequest(new ApiResponse<object>(false, null, "Detalles excede el máximo de 250 caracteres"));
		}
		// Validar duplicidad en Detalles con fechas superpuestas
		var existingWithSameDetalles = db.DiasInhabiles
			.Where(d => d.Detalles.ToUpper() == request.Detalles.ToUpper())
			.Where(d => (d.Fecha >= request.FechaInicial && d.Fecha <= request.FechaFinal) ||
						(request.FechaInicial >= d.FechaInicial && request.FechaInicial <= d.FechaFinal) ||
						(request.FechaFinal >= d.FechaInicial && request.FechaFinal <= d.FechaFinal))
			.Any();

		if (existingWithSameDetalles)
		{
			_logger.LogWarning("Intento de duplicidad en Detalles con fechas superpuestas: {Detalles}", request.Detalles);
			return Conflict(new ApiResponse<object>(false, null, "Ya existe un día inhábil con ese valor en Detalles en el rango de fechas especificado"));
		}
		var tipoActividad = (TipoActividadDelDiaEnum)request.TipoActividadDelDia;
		var dias = new List<DiasInhabiles>();
		if (request.FechaInicial > request.FechaFinal)
		{
			_logger.LogWarning("La fecha inicial {FechaInicial} es mayor que la fecha final {FechaFinal}", request.FechaInicial, request.FechaFinal);
			return base.BadRequest(new ApiResponse<object>(false, null, "La fecha inicial no puede ser mayor que la fecha final"));
		}
		for (var fecha = request.FechaInicial; fecha <= request.FechaFinal; fecha = fecha.AddDays(1))
		{
			dias.Add(new DiasInhabiles
			{
				Fecha = fecha,
				FechaInicial = request.FechaInicial,
				FechaFinal = request.FechaFinal,
				AnioFechaInicial = request.FechaInicial.Year,
				MesFechaInicial = request.FechaInicial.Month,
				DiaFechaInicial = request.FechaInicial.Day,
				AnioFechaFinal = request.FechaFinal.Year,
				MesFechaFinal = request.FechaFinal.Month,
				DiaFechaFinal = request.FechaFinal.Day,
				Detalles = request.Detalles ?? string.Empty,
				TipoActividadDelDia = tipoActividad
			});
		}
		_logger.LogInformation("Agregando {Count} días inhábiles", dias.Count);
		try
		{
			db.DiasInhabiles.AddRange(dias);
			db.SaveChanges();
			_logger.LogInformation("Días inhábiles creados correctamente: {Ids}", dias.Select(d => d.Id).ToList());
			return base.Ok(new ApiResponse<object>(true, dias.Select(d => d.Id).ToList(), "Días inhábiles creados correctamente"));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al crear días inhábiles en la base de datos");
			return StatusCode(500, new ApiResponse<object>(false, null, "Error interno al crear los días inhábiles"));
		}
	}

	[HttpGet]
	public IActionResult GetDiasInhabilesDelAnioActual(
		[FromQuery] DateOnly? fechaInicio, 
		[FromQuery] DateOnly? fechaFin, 
		[FromQuery] int? tipoDiaInhabil, 
		[FromServices] FreeTimeDbContext db)
	{
		_logger.LogInformation("Consulta de días inhábiles con filtros - FechaInicio: {FechaInicio}, FechaFin: {FechaFin}, TipoDiaInhabil: {TipoDiaInhabil}", 
			fechaInicio, fechaFin, tipoDiaInhabil);

		// Validaciones
		if (fechaInicio.HasValue && fechaFin.HasValue)
		{
			if (fechaInicio > fechaFin)
			{
				_logger.LogWarning("La fecha de inicio {FechaInicio} es mayor que la fecha de fin {FechaFin}", fechaInicio, fechaFin);
				return BadRequest(new ApiResponse<object>(false, null, "La fecha de inicio no puede ser mayor que la fecha de fin"));
			}
		}
		if (tipoDiaInhabil.HasValue && !System.Enum.IsDefined(typeof(TipoActividadDelDiaEnum), tipoDiaInhabil.Value))
		{
			_logger.LogWarning("Tipo de día inhábil inválido: {Tipo}", tipoDiaInhabil);
			return BadRequest(new ApiResponse<object>(false, null, "Tipo de día inhábil inválido"));
		}

		// Lógica de filtrado
		var query = db.DiasInhabiles.AsQueryable();
		if (fechaInicio.HasValue && fechaFin.HasValue)
		{
			query = query.Where(d => d.Fecha >= fechaInicio.Value && d.Fecha <= fechaFin.Value);
		}
		if (tipoDiaInhabil.HasValue)
		{
			var tipoEnum = (TipoActividadDelDiaEnum)tipoDiaInhabil.Value;
			query = query.Where(d => d.TipoActividadDelDia == tipoEnum);
		}

		var dias = query
			.Select(d => new DiasInhabilesGetListRequestDto
			{
				Id = d.Id,
				Fecha = d.Fecha,
				FechaInicial = d.FechaInicial,
				FechaFinal = d.FechaFinal,
				Detalles = d.Detalles,
				TipoActividadDelDia = d.TipoActividadDelDia
			})
			.ToList();
		_logger.LogInformation("Consulta de días inhábiles retornó {Count} resultados", dias.Count);
		return Ok(new ApiResponse<object>(true, dias, dias.Count > 0 ? null : "No hay días inhábiles para los parámetros proporcionados"));
	}

	[HttpDelete("{id}")]
	[Authorize(Roles = "SuperUsuario")]
	public IActionResult DeleteDiasInhabil(int id, [FromServices] FreeTimeDbContext db)
	{
		try
		{
			_logger.LogInformation("Intentando eliminar día inhábil con id: {Id}", id);
			if (id <= 0)
			{
				_logger.LogWarning("Id inválido para eliminación: {Id}", id);
				return BadRequest(new ApiResponse<object>(false, null, "El id debe ser un entero positivo"));
			}
			var dia = db.DiasInhabiles.FirstOrDefault(d => d.Id == id);
			if (dia == null)
			{
				_logger.LogWarning("No existe día inhábil con id: {Id}", id);
				return NotFound(new ApiResponse<object>(false, null, "No existe un día inhábil con el id especificado"));
			}
			// Buscar grupo
			var grupo = db.DiasInhabiles.Where(d => d.Detalles.ToUpper() == dia.Detalles.ToUpper() && d.TipoActividadDelDia == dia.TipoActividadDelDia && d.FechaInicial == dia.FechaInicial && d.FechaFinal == dia.FechaFinal).ToList();
			int eliminados = 0;
			if (grupo.Count > 1)
			{
				_logger.LogInformation("Eliminando grupo de días inhábiles. Detalles: {Detalles}, Tipo: {Tipo}, Cantidad: {Cantidad}", dia.Detalles, dia.TipoActividadDelDia, grupo.Count);
				db.DiasInhabiles.RemoveRange(grupo);
				eliminados = grupo.Count;
			}
			else
			{
				_logger.LogInformation("Eliminando día inhábil único. Id: {Id}", dia.Id);
				db.DiasInhabiles.Remove(dia);
				eliminados = 1;
			}
			db.SaveChanges();
			_logger.LogInformation("Eliminación exitosa. Días eliminados: {Eliminados}", eliminados);
			return Ok(new ApiResponse<object>(true, eliminados, $"Se eliminaron {eliminados} día(s) inhábil(es) correctamente"));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error interno al eliminar día inhábil con id: {Id}", id);
			return StatusCode(500, new ApiResponse<object>(false, null, $"Error interno: {ex.Message}"));
		}
	}

    public class DiasInhabilesFilterRequest
    {
        public DateOnly? FechaInicio { get; set; }
        public DateOnly? FechaFin { get; set; }
        public int? TipoDiaInhabil { get; set; }
    }

	public class DiasInhabilesGetListRequestDto
	{
		public int Id { get; set; }
		public DateOnly Fecha { get; set; }
		public DateOnly FechaInicial { get; set; }
		public DateOnly FechaFinal { get; set; }
		public string Detalles { get; set; } = string.Empty;
		public TipoActividadDelDiaEnum TipoActividadDelDia { get; set; }
	}

	public class DiasInhabilesCreateRequest
	{
		public DateOnly FechaInicial { get; set; }
		public DateOnly FechaFinal { get; set;  }
		[System.ComponentModel.DataAnnotations.Required]
		public string Detalles { get; set; } = string.Empty;
		public int TipoActividadDelDia { get; set; }
	}
}
