using System;
using System.Collections.Generic;

namespace tiempo_libre.DTOs
{
    public class EmpleadoFaltanteCapturaDto
    {
        public int EmpleadoId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Nomina { get; set; } = string.Empty;
        public string? Maquina { get; set; }
        public int GrupoId { get; set; }
        public string NombreGrupo { get; set; } = string.Empty;
        public int AreaId { get; set; }
        public string NombreArea { get; set; } = string.Empty;
        public int BloqueId { get; set; }
        public int NumeroBloque { get; set; }
        public bool EsBloqueCola { get; set; }
        public DateTime FechaLimiteBloque { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public string? Observaciones { get; set; }
        public bool RequiereAccionUrgente { get; set; }
    }

    public class EmpleadosFaltantesCapturaResponse
    {
        public int Anio { get; set; }
        public int TotalEmpleados { get; set; }
        public int TotalCriticos { get; set; }
        public List<EmpleadoFaltanteCapturaDto> Empleados { get; set; } = new();
        public DateTime FechaReporte { get; set; } = DateTime.Now;
    }

    public class VacacionAsignadaEmpresaDto
    {
        public int EmpleadoId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Nomina { get; set; } = string.Empty;
        public string? Maquina { get; set; }
        public int? AreaId { get; set; }
        public string? NombreArea { get; set; }
        public int? GrupoId { get; set; }
        public string? NombreGrupo { get; set; }
        public DateOnly FechaVacacion { get; set; }
        public string TipoVacacion { get; set; } = string.Empty;
        public string OrigenAsignacion { get; set; } = string.Empty;
        public string EstadoVacacion { get; set; } = string.Empty;
        public string PeriodoProgramacion { get; set; } = string.Empty;
        public DateTime FechaProgramacion { get; set; }
        public string? Observaciones { get; set; }
    }

    public class VacacionesAsignadasEmpresaResponse
    {
        public int Anio { get; set; }
        public int TotalVacaciones { get; set; }
        public int TotalEmpleados { get; set; }
        public List<VacacionAsignadaEmpresaDto> Vacaciones { get; set; } = new();
        public DateTime FechaReporte { get; set; } = DateTime.Now;
    }

    public class EmpleadoEnVacacionesDto
    {
        public int EmpleadoId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Nomina { get; set; } = string.Empty;
        public string? Maquina { get; set; }
        public int? AreaId { get; set; }
        public string? NombreArea { get; set; }
        public int? GrupoId { get; set; }
        public string? NombreGrupo { get; set; }
        public DateOnly FechaVacacion { get; set; }
        public string TipoVacacion { get; set; } = string.Empty;
        public string OrigenAsignacion { get; set; } = string.Empty;
        public string EstadoVacacion { get; set; } = string.Empty;
        public string PeriodoProgramacion { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
    }

    public class EmpleadosEnVacacionesResponse
    {
        public DateOnly FechaConsulta { get; set; }
        public int TotalRegistros { get; set; }
        public int TotalEmpleados { get; set; }
        public List<EmpleadoEnVacacionesDto> Empleados { get; set; } = new();
        public DateTime FechaReporte { get; set; } = DateTime.Now;
    }
}
