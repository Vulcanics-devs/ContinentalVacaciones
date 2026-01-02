using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tiempo_libre.Models;
using tiempo_libre.Models.Enums;
using tiempo_libre.DTOs;
using System.Security.Claims;

namespace tiempo_libre.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IncidenciaOPermisosController : ControllerBase
    {
        private readonly FreeTimeDbContext _db;
        private readonly ILogger<IncidenciaOPermisosController> _logger;
        public IncidenciaOPermisosController(FreeTimeDbContext db, ILogger<IncidenciaOPermisosController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public class IncidenciaOPermisoCreateRequest
        {
            [Required]
            public DateOnly FechaInicial { get; set; }
            [Required]
            public DateOnly FechaFinal { get; set; }
            [MaxLength(250)]
            public string Detalles { get; set; } = string.Empty;
            [Required]
            public int TiposDeIncedencia { get; set; }
            public int? IdUsuarioSindicato { get; set; }
            [Required]
            public int IdUsuarioEmpleado { get; set; }
            public int? NominaEmpleado { get; set; }
        }

        [HttpPost]
        public IActionResult CreateIncidencias([FromBody] IncidenciaOPermisoCreateRequest request)
        {
            // Validar modelo antes de operar con fechas
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<string>(false, null, "Datos de entrada inválidos"));
            }
            // Validaciones de fechas
            _logger.LogInformation("Solicitud de creación de incidencias recibida: {@Request}", request);
            if (request.FechaInicial > request.FechaFinal)
            {
                _logger.LogWarning("Validación fallida: FechaInicial > FechaFinal. FechaInicial: {FechaInicial}, FechaFinal: {FechaFinal}", request.FechaInicial, request.FechaFinal);
                return BadRequest(new ApiResponse<string>(false, null, "La fecha inicial no puede ser mayor a la fecha final"));
            }
            if (request.FechaFinal.AddYears(-1) > request.FechaInicial)
            {
                _logger.LogWarning("Validación fallida: Rango de fechas mayor a un año. FechaInicial: {FechaInicial}, FechaFinal: {FechaFinal}", request.FechaInicial, request.FechaFinal);
                return BadRequest(new ApiResponse<string>(false, null, "El rango de fechas no puede ser mayor a un año"));
            }

            // Validar tipo de incidencia
            if (!Enum.IsDefined(typeof(TiposDeIncidenciasEnum), request.TiposDeIncedencia))
            {
                _logger.LogWarning("Tipo de incidencia inválido: {Tipo}", request.TiposDeIncedencia);
                return BadRequest(new ApiResponse<string>(false, null, "El tipo de incidencia no es válido"));
            }

            // Validar usuarios
            var usuarioEmpleado = _db.Users.FirstOrDefault(u => u.Id == request.IdUsuarioEmpleado);
            if (usuarioEmpleado == null)
            {
                _logger.LogWarning("IdUsuarioEmpleado no existe: {IdUsuarioEmpleado}", request.IdUsuarioEmpleado);
                return BadRequest(new ApiResponse<string>(false, null, $"El IdUsuarioEmpleado {request.IdUsuarioEmpleado} no existe"));
            }
            User? usuarioSindicato = null;
            if (request.IdUsuarioSindicato.HasValue)
            {
                usuarioSindicato = _db.Users.FirstOrDefault(u => u.Id == request.IdUsuarioSindicato.Value);
                if (usuarioSindicato == null)
                {
                    _logger.LogWarning("IdUsuarioSindicato no existe: {IdUsuarioSindicato}", request.IdUsuarioSindicato.Value);
                    return BadRequest(new ApiResponse<string>(false, null, $"El IdUsuarioSindicato {request.IdUsuarioSindicato.Value} no existe"));
                }
            }

            // Validar nomina
            var nominaEmpleado = request.NominaEmpleado ?? usuarioEmpleado.Nomina;
            if (request.NominaEmpleado.HasValue && request.NominaEmpleado.Value != usuarioEmpleado.Nomina)
            {
                _logger.LogWarning("La nómina enviada no coincide con la del usuario empleado. Nomina enviada: {NominaEnviada}, Nomina real: {NominaReal}", request.NominaEmpleado.Value, usuarioEmpleado.Nomina);
                return BadRequest(new ApiResponse<string>(false, null, "La nómina enviada no coincide con la del usuario empleado"));
            }

            // Obtener usuario autoriza del token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int idUsuarioAutoriza))
            {
                _logger.LogError("No se pudo obtener el usuario que autoriza desde el token");
                return BadRequest(new ApiResponse<string>(false, null, "No se pudo obtener el usuario que autoriza desde el token"));
            }
            var usuarioAutoriza = _db.Users.FirstOrDefault(u => u.Id == idUsuarioAutoriza);
            if (usuarioAutoriza == null)
            {
                _logger.LogError("El usuario autoriza (Id: {idUsuarioAutoriza}) no existe", idUsuarioAutoriza);
                return BadRequest(new ApiResponse<string>(false, null, $"El usuario autoriza (Id: {idUsuarioAutoriza}) no existe"));
            }

            // Crear incidencias
            var incidencias = new List<IncidenciaOPermiso>();
            for (var fecha = request.FechaInicial; fecha <= request.FechaFinal; fecha = fecha.AddDays(1))
            {
                var calendario = _db.DiasCalendarioEmpleado.FirstOrDefault(c => c.IdUsuarioEmpleadoSindicalizado == request.IdUsuarioEmpleado && c.FechaDelDia == fecha);
                int? idGrupo = calendario?.IdGrupo;
                int? idRegla = calendario?.IdRegla;

                var incidencia = new IncidenciaOPermiso
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
                    DiaDeLaSemana = (DiasDeLaSemanaEnum)fecha.DayOfWeek,
                    IdUsuarioAutoiza = idUsuarioAutoriza,
                    IdUsuarioSindicato = request.IdUsuarioSindicato,
                    FechaRegistro = DateTime.Now,
                    IdUsuarioEmpleado = request.IdUsuarioEmpleado,
                    NominaEmpleado = nominaEmpleado ?? 0,
                    IdGrupo = idGrupo,
                    IdRegla = idRegla,
                    Detalles = request.Detalles,
                    TiposDeIncedencia = (TiposDeIncidenciasEnum)request.TiposDeIncedencia,
                    UsuarioAutoriza = usuarioAutoriza,
                    UsuarioSindicato = usuarioSindicato,
                    UsuarioEmpleado = usuarioEmpleado,
                    Grupo = idGrupo.HasValue ? _db.Grupos.FirstOrDefault(g => g.GrupoId == idGrupo.Value) : null,
                    Regla = idRegla.HasValue ? _db.Reglas.FirstOrDefault(r => r.Id == idRegla.Value) : null
                };
                incidencias.Add(incidencia);
            }
            _logger.LogInformation("Se crearán {Count} incidencia(s) para el usuario {IdUsuarioEmpleado}", incidencias.Count, request.IdUsuarioEmpleado);
            try
            {
                _db.IncidenciasOPermisos.AddRange(incidencias);
                _db.SaveChanges();
                _logger.LogInformation("Incidencias creadas exitosamente para usuario {IdUsuarioEmpleado}", request.IdUsuarioEmpleado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar incidencias en la base de datos");
                return StatusCode(500, new ApiResponse<string>(false, null, "Error interno al guardar las incidencias"));
            }

            // Mapear a DTO
            var createdDtos = incidencias.Select(i => new IncidenciaOPermisoCreatedDTO
            {
                Id = i.Id,
                Fecha = i.Fecha,
                FechaInicial = i.FechaInicial,
                FechaFinal = i.FechaFinal,
                DiaDeLaSemana = i.DiaDeLaSemana,
                FechaRegistro = i.FechaRegistro,
                NominaEmpleado = i.NominaEmpleado,
                IdGrupo = i.IdGrupo,
                IdRegla = i.IdRegla,
                Detalles = i.Detalles,
                TiposDeIncedencia = i.TiposDeIncedencia,
                UsuarioAutoriza = i.UsuarioAutoriza != null ? new IoPUserBasicInfoDTO { Id = i.UsuarioAutoriza.Id, FullName = i.UsuarioAutoriza.FullName } : null,
                UsuarioSindicato = i.UsuarioSindicato != null ? new IoPUserBasicInfoDTO { Id = i.UsuarioSindicato.Id, FullName = i.UsuarioSindicato.FullName } : null,
                UsuarioEmpleado = i.UsuarioEmpleado != null ? new IoPUserBasicInfoDTO { Id = i.UsuarioEmpleado.Id, FullName = i.UsuarioEmpleado.FullName } : null,
                Grupo = i.Grupo != null ? new IoPGrupoDTO { GrupoId = i.Grupo.GrupoId, Rol = i.Grupo.Rol } : null,
                Regla = i.Regla != null ? new IoPReglaDTO { Id = i.Regla.Id, Nombre = i.Regla.Nombre } : null
            }).ToList();

            var response = new ApiResponse<List<IncidenciaOPermisoCreatedDTO>>(true, createdDtos, $"Se crearon {createdDtos.Count} incidencia(s)");
            return StatusCode(201, response);
        }

        // Consulta de incidencias por filtros
        public class IncidenciaOPermisoRetrieveRequest
        {
            public int? IdUsuarioEmpleado { get; set; }
            public DateTime? FechaInicial { get; set; }
            public DateTime? FechaFinal { get; set; }
            public int? IdUsuarioSindicato { get; set; }
            public int? GrupoId { get; set; }
            public int? ReglaId { get; set; }
        }

        [HttpPost("retrieve")]
        public IActionResult RetrieveIncidencias([FromBody] IncidenciaOPermisoRetrieveRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<object>(false, null, "Datos de entrada inválidos"));

                // Validaciones de existencia
                if (request.IdUsuarioEmpleado.HasValue && !_db.Users.Any(u => u.Id == request.IdUsuarioEmpleado.Value))
                    return BadRequest(new ApiResponse<object>(false, null, $"El IdUsuarioEmpleado {request.IdUsuarioEmpleado.Value} no existe"));
                if (request.IdUsuarioSindicato.HasValue && !_db.Users.Any(u => u.Id == request.IdUsuarioSindicato.Value))
                    return BadRequest(new ApiResponse<object>(false, null, $"El IdUsuarioSindicato {request.IdUsuarioSindicato.Value} no existe"));
                if (request.GrupoId.HasValue && !_db.Grupos.Any(g => g.GrupoId == request.GrupoId.Value))
                    return BadRequest(new ApiResponse<object>(false, null, $"El GrupoId {request.GrupoId.Value} no existe"));
                if (request.ReglaId.HasValue && !_db.Reglas.Any(r => r.Id == request.ReglaId.Value))
                    return BadRequest(new ApiResponse<object>(false, null, $"El ReglaId {request.ReglaId.Value} no existe"));

                // Validaciones de fechas
                if (request.FechaInicial.HasValue && request.FechaFinal.HasValue)
                {
                    if (request.FechaInicial > request.FechaFinal)
                        return BadRequest(new ApiResponse<object>(false, null, "La fecha inicial no puede ser mayor que la fecha final"));
                    if ((request.FechaFinal.Value - request.FechaInicial.Value).TotalDays > 366)
                        return BadRequest(new ApiResponse<object>(false, null, "El rango de fechas no puede ser mayor a un año"));
                }

                var query = _db.IncidenciasOPermisos.AsQueryable();

                if (request.IdUsuarioEmpleado.HasValue)
                {
                    query = query.Where(i => i.UsuarioEmpleado != null && i.UsuarioEmpleado.Id == request.IdUsuarioEmpleado.Value);
                    if (request.FechaInicial.HasValue && request.FechaFinal.HasValue)
                    {
                        var fechaIni = DateOnly.FromDateTime(request.FechaInicial.Value);
                        var fechaFin = DateOnly.FromDateTime(request.FechaFinal.Value);
                        query = query.Where(i => i.Fecha >= fechaIni && i.Fecha <= fechaFin);
                    }
                }
                else if (request.FechaInicial.HasValue && request.FechaFinal.HasValue)
                {
                    var fechaIni = DateOnly.FromDateTime(request.FechaInicial.Value);
                    var fechaFin = DateOnly.FromDateTime(request.FechaFinal.Value);
                    query = query.Where(i => i.Fecha >= fechaIni && i.Fecha <= fechaFin);
                }
                // Si no se envía IdUsuarioEmpleado ni rango de fechas, no se filtra por esos campos

                if (request.IdUsuarioSindicato.HasValue)
                    query = query.Where(i => i.UsuarioSindicato != null && i.UsuarioSindicato.Id == request.IdUsuarioSindicato.Value);
                if (request.GrupoId.HasValue)
                    query = query.Where(i => i.IdGrupo == request.GrupoId.Value);
                if (request.ReglaId.HasValue)
                    query = query.Where(i => i.IdRegla == request.ReglaId.Value);

                var incidencias = query.ToList();
                var dtos = incidencias.Select(i => new IncidenciaOPermisoRetrieveDTO
                {
                    Id = i.Id,
                    Fecha = i.Fecha,
                    FechaInicial = i.FechaInicial,
                    FechaFinal = i.FechaFinal,
                    DiaDeLaSemana = i.DiaDeLaSemana,
                    FechaRegistro = i.FechaRegistro,
                    NominaEmpleado = i.NominaEmpleado,
                    IdGrupo = i.IdGrupo,
                    IdRegla = i.IdRegla,
                    Detalles = i.Detalles,
                    TiposDeIncedencia = i.TiposDeIncedencia,
                    UsuarioAutoriza = i.UsuarioAutoriza != null ? new IoPUserBasicInfoDTO { Id = i.UsuarioAutoriza.Id, FullName = i.UsuarioAutoriza.FullName } : null,
                    UsuarioSindicato = i.UsuarioSindicato != null ? new IoPUserBasicInfoDTO { Id = i.UsuarioSindicato.Id, FullName = i.UsuarioSindicato.FullName } : null,
                    UsuarioEmpleado = i.UsuarioEmpleado != null ? new IoPUserBasicInfoDTO { Id = i.UsuarioEmpleado.Id, FullName = i.UsuarioEmpleado.FullName } : null,
                    Grupo = i.Grupo != null ? new IoPGrupoDTO { GrupoId = i.Grupo.GrupoId, Rol = i.Grupo.Rol } : null,
                    Regla = i.Regla != null ? new IoPReglaDTO { Id = i.Regla.Id, Nombre = i.Regla.Nombre } : null
                }).ToList();

                var response = new ApiResponse<object>(true, new RetrieveResponse(dtos.Count, dtos), null);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar incidencias");
                return StatusCode(500, new ApiResponse<object>(false, null, "Error interno al consultar las incidencias"));
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteIncidencia(int id)
        {
            try
            {
                // Validar que el id sea positivo
                if (id <= 0)
                {
                    return BadRequest(new ApiResponse<object>(false, null, "El id debe ser un entero positivo"));
                }

                var incidencia = _db.IncidenciasOPermisos.FirstOrDefault(i => i.Id == id);
                if (incidencia == null)
                {
                    return NotFound(new ApiResponse<object>(false, null, $"No se encontró la incidencia o permiso con id {id}"));
                }

                // Buscar grupo de incidencias con los mismos criterios
                var grupo = _db.IncidenciasOPermisos.Where(i =>
                    i.Detalles == incidencia.Detalles &&
                    i.TiposDeIncedencia == incidencia.TiposDeIncedencia &&
                    i.IdUsuarioEmpleado == incidencia.IdUsuarioEmpleado &&
                    i.FechaInicial == incidencia.FechaInicial &&
                    i.FechaFinal == incidencia.FechaFinal
                ).ToList();

                int deletedCount = 0;
                if (grupo.Count > 1)
                {
                    _db.IncidenciasOPermisos.RemoveRange(grupo);
                    deletedCount = grupo.Count;
                }
                else
                {
                    _db.IncidenciasOPermisos.Remove(incidencia);
                    deletedCount = 1;
                }

                _db.SaveChanges();
                return Ok(new ApiResponse<object>(true, new { eliminados = deletedCount }, $"Se eliminaron {deletedCount} incidencia(s) o permiso(s)"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar incidencia o permiso");
                return StatusCode(500, new ApiResponse<object>(false, null, "Error interno al eliminar la incidencia o permiso"));
            }
        }

    }

    // DTOs
    public class IncidenciaOPermisoRetrieveDTO
    {
        public int Id { get; set; }
        public DateOnly Fecha { get; set; }
        public DateOnly FechaInicial { get; set; }
        public DateOnly FechaFinal { get; set; }
        public DiasDeLaSemanaEnum DiaDeLaSemana { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int NominaEmpleado { get; set; }
        public int? IdGrupo { get; set; }
        public int? IdRegla { get; set; }
        public string Detalles { get; set; } = string.Empty;
        public TiposDeIncidenciasEnum TiposDeIncedencia { get; set; }
        public IoPUserBasicInfoDTO? UsuarioAutoriza { get; set; }
        public IoPUserBasicInfoDTO? UsuarioSindicato { get; set; }
        public IoPUserBasicInfoDTO? UsuarioEmpleado { get; set; }
        public IoPGrupoDTO? Grupo { get; set; }
        public IoPReglaDTO? Regla { get; set; }
    }

    public class IncidenciaOPermisoCreatedDTO
    {
        public int Id { get; set; }
        public DateOnly Fecha { get; set; }
        public DateOnly FechaInicial { get; set; }
        public DateOnly FechaFinal { get; set; }
        public DiasDeLaSemanaEnum DiaDeLaSemana { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int NominaEmpleado { get; set; }
        public int? IdGrupo { get; set; }
        public int? IdRegla { get; set; }
        public string Detalles { get; set; } = string.Empty;
        public TiposDeIncidenciasEnum TiposDeIncedencia { get; set; }
        public IoPUserBasicInfoDTO? UsuarioAutoriza { get; set; }
        public IoPUserBasicInfoDTO? UsuarioSindicato { get; set; }
        public IoPUserBasicInfoDTO? UsuarioEmpleado { get; set; }
        public IoPGrupoDTO? Grupo { get; set; }
        public IoPReglaDTO? Regla { get; set; }
    }

    public class IoPUserBasicInfoDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    public class IoPGrupoDTO
    {
        public int GrupoId { get; set; }
        public string Rol { get; set; } = string.Empty;
    }

    public class IoPReglaDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class RetrieveResponse
    {
        public int Count { get; set; }
        public List<IncidenciaOPermisoRetrieveDTO> Items { get; set; } = new List<IncidenciaOPermisoRetrieveDTO>();

        public RetrieveResponse() { }

        public RetrieveResponse(int count, List<IncidenciaOPermisoRetrieveDTO> items)
        {
            Count = count;
            Items = items;
        }
    }
}