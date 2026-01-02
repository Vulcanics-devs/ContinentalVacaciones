using System.ComponentModel.DataAnnotations;
using tiempo_libre.Models.Enums;

namespace tiempo_libre.Models;

public class CalendarioEmpleado
{
    [Key]
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int IdRegla { get; set; }
    public int IdArea { get; set; }
    public int IdGrupo { get; set; }
    public int? IdProgramacionAnual { get; set; }
    public int IdUsuarioEmpleadoSindicalizado { get; set; }
    public int NominaEmpleado { get; set; } // INDICE
    public int IdRolSemanalIniciaGeneracionDeCalendario { get; set; }
    public DateOnly FechaInicioGeneracionDeCalendario { get; set; }
    public int? IdVacacionesPorAntiguedad { get; set; }
    public virtual Regla Regla { get; set; } = null!;
    public virtual RolSemanal RolSemanalIniciaGeneracionDeCalendario { get; set; } = null!;
    public virtual Area Area { get; set; } = null!;
    public virtual Grupo Grupo { get; set; } = null!;
    public ProgramacionesAnuales? ProgramacionAnual { get; set; }
    public virtual User UsuarioEmpleadoSindicalizado { get; set; } = null!;
    public virtual VacacionesPorAntiguedad? VacacionesPorAntiguedad { get; set; }
}

