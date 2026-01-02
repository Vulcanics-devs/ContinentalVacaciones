using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace tiempo_libre.DTOs
{
    public class CalendarioGrupoRequest
    {
        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public DateTime Inicio { get; set; }
        
        [Required(ErrorMessage = "La fecha de fin es requerida")]
        public DateTime Fin { get; set; }
    }

    public class CalendarioGrupoDiaDto
    {
        public DateTime Fecha { get; set; }
        public string Turno { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string? Incidencia { get; set; }
        public string? TipoIncidencia { get; set; }
    }

    public class CalendarioGrupoResponse
    {
        public int GrupoId { get; set; }
        public string NombreGrupo { get; set; } = string.Empty;
        public string Regla { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public List<CalendarioGrupoDiaDto> Calendario { get; set; } = new();
    }

    public class CalendarioUsuarioRequest
    {
        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public DateTime Inicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es requerida")]
        public DateTime Fin { get; set; }
    }

    public class CalendarioUsuarioResponse
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public int? GrupoId { get; set; }
        public string? NombreGrupo { get; set; }
        public string? Regla { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public List<CalendarioGrupoDiaDto> Calendario { get; set; } = new();
    }
}
