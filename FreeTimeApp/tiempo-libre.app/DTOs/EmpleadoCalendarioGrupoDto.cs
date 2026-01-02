using System;
using System.Collections.Generic;

namespace tiempo_libre.DTOs
{
    public class DiaCalendarioEmpleadoDto
    {
        public int IdDiaCalendarioEmpleado { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoActividadDelDia { get; set; } = string.Empty;
        public string Detalles { get; set; } = string.Empty;
    }

    public class EmpleadoCalendarioGrupoDto
    {
        public int IdUsuarioEmpleadoSindicalizado { get; set; }
        public int IdGrupo { get; set; }
        public string NominaEmpleado { get; set; } = string.Empty;
        public string NombreCompletoEmpleado { get; set; } = string.Empty;
        public List<DiaCalendarioEmpleadoDto> Dias { get; set; } = new();
    }
}
