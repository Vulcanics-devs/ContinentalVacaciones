using System;
using System.Collections.Generic;

namespace tiempo_libre.DTOs
{
    public class WeeklyRoleEmployeeDto
    {
        public int Id { get; set; }
        public string Nomina { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }

    public class WeeklyRoleEntryDto
    {
        public string Fecha { get; set; } = string.Empty; // yyyy-MM-dd
        public string CodigoTurno { get; set; } = string.Empty; // D,1,2,3 u otros códigos
        public WeeklyRoleEmployeeDto Empleado { get; set; } = new WeeklyRoleEmployeeDto();
    }

    public class WeeklyRolesResponseDto
    {
        public int GrupoId { get; set; }
        public string? GrupoNombre { get; set; }
        public List<WeeklyRoleEntryDto> Semana { get; set; } = new();
    }
}
