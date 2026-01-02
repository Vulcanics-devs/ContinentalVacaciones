using System;
using System.ComponentModel.DataAnnotations;

namespace tiempo_libre.DTOs
{
    public class VacacionesEmpleadoRequest
    {
        [Required(ErrorMessage = "El año es requerido")]
        [Range(2000, 2100, ErrorMessage = "El año debe estar entre 2000 y 2100")]
        public int Anio { get; set; }
    }

    public class VacacionesEmpleadoResponse
    {
        public int EmpleadoId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public DateOnly FechaIngreso { get; set; }
        public int AnioConsulta { get; set; }
        public int AntiguedadEnAnios { get; set; }
        public int DiasEmpresa { get; set; }
        public int DiasAsignadosAutomaticamente { get; set; }
        public int DiasProgramables { get; set; }
        public int TotalDias { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public class VacacionesPorAntiguedadResponse
    {
        public int AntiguedadEnAnios { get; set; }
        public int DiasEmpresa { get; set; }
        public int DiasAsignadosAutomaticamente { get; set; }
        public int DiasProgramables { get; set; }
        public int TotalDias { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}
