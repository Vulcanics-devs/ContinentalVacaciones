using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using tiempo_libre.Models;
using tiempo_libre.DTOs;
using tiempo_libre.Models.Enums;

namespace tiempo_libre.Services
{
    public class CalendariosEmpleadosService
    {
        private readonly FreeTimeDbContext _db;

        public CalendariosEmpleadosService(FreeTimeDbContext db)
        {
            _db = db;
        }

        public virtual async Task<ApiResponse<List<EmpleadoCalendarioGrupoDto>>> ObtenerCalendarioPorGrupoAsync(int grupoId, DateTime fechaInicio, DateTime fechaFinal)
        {
            if (grupoId <= 0)
                return new ApiResponse<List<EmpleadoCalendarioGrupoDto>>(false, null, "El grupoId debe ser un entero positivo.");
            if (fechaInicio >= fechaFinal)
                return new ApiResponse<List<EmpleadoCalendarioGrupoDto>>(false, null, "La fechaInicio debe ser menor a la fechaFinal.");
            if ((fechaFinal - fechaInicio).TotalDays > 31)
                return new ApiResponse<List<EmpleadoCalendarioGrupoDto>>(false, null, "El rango de fechas no puede ser mayor a 31 días.");

            var grupo = await _db.Grupos.FirstOrDefaultAsync(g => g.GrupoId == grupoId);
            if (grupo == null)
                return new ApiResponse<List<EmpleadoCalendarioGrupoDto>>(false, null, "El grupo especificado no existe.");

            var empleados = await _db.Users
                .Where(u => u.GrupoId == grupoId)
                .ToListAsync();

            var resultado = new List<EmpleadoCalendarioGrupoDto>();
            var fechaInicialDateOnly = DateOnly.FromDateTime(fechaInicio);
            var fechaFinalDateOnly = DateOnly.FromDateTime(fechaFinal);
            foreach (var empleado in empleados)
            {
                var dias = await _db.DiasCalendarioEmpleado
                    .Where(d => d.IdUsuarioEmpleadoSindicalizado == empleado.Id &&
                                d.IdGrupo == grupoId &&
                                d.FechaDelDia >= fechaInicialDateOnly &&
                                d.FechaDelDia <= fechaFinalDateOnly)
                    .ToListAsync();

                var diasDto = dias.Select(d => new DiaCalendarioEmpleadoDto
                {
                    IdDiaCalendarioEmpleado = d.Id,
                    Fecha = new DateTime(d.AnioFecha, d.MesFecha, d.DiaFecha),
                    TipoActividadDelDia = MapTipoActividad(d.TipoActividadDelDia, d.Turno, d.TipoDeIncedencia),
                    Detalles = d.DetallesDiaInhabil ?? string.Empty
                }).ToList();

                resultado.Add(new EmpleadoCalendarioGrupoDto
                {
                    IdUsuarioEmpleadoSindicalizado = empleado.Id,
                    IdGrupo = grupoId,
                    NominaEmpleado = empleado.Nomina.ToString(),
                    NombreCompletoEmpleado = empleado.FullName,
                    Dias = diasDto
                });
            }

            return new ApiResponse<List<EmpleadoCalendarioGrupoDto>>(true, resultado, null);
        }

        private string MapTipoActividad(TipoActividadDelDiaEnum tipo, TurnosEnum turno, TiposDeIncidenciasEnum? tipoIncidencia)
        {
            switch (tipo)
            {
                case TipoActividadDelDiaEnum.Laboral:
                    return turno switch
                    {
                        TurnosEnum.Matutino => "1",
                        TurnosEnum.Vespertino => "2",
                        TurnosEnum.Nocturno => "3",
                        TurnosEnum.Descanso => "D",
                        _ => ""
                    };
                case TipoActividadDelDiaEnum.InhabilPorContinental:
                case TipoActividadDelDiaEnum.VacacionesAutoAsignadasPorApp:
                    return "VA";
                case TipoActividadDelDiaEnum.VacacionesSeleccionadasPorEmp:
                    return "V";
                case TipoActividadDelDiaEnum.IncidenciaOPermiso:
                    if (tipoIncidencia == null) return "";
                    return tipoIncidencia switch
                    {
                        TiposDeIncidenciasEnum.PermisoConGoce => "P",
                        TiposDeIncidenciasEnum.PermisoDefuncion => "PD",
                        TiposDeIncidenciasEnum.PermisoSinGoce => "G",
                        TiposDeIncidenciasEnum.IncapacidadEnfermedadGeneral => "E",
                        TiposDeIncidenciasEnum.IncapacidadAccidenteTrabajo => "A",
                        TiposDeIncidenciasEnum.IncapacidadPorMaternidad => "M",
                        TiposDeIncidenciasEnum.IncapacidadProbableRiesgoTrabajo => "R",
                        TiposDeIncidenciasEnum.Suspension => "S",
                        TiposDeIncidenciasEnum.PCGPorPaternidad => "PP",
                        _ => ""
                    };
                default:
                    return "";
            }
        }
    }
}
