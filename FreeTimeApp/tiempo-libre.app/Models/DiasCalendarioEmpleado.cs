using System.ComponentModel.DataAnnotations;
using tiempo_libre.Models.Enums;

namespace tiempo_libre.Models;

public class DiasCalendarioEmpleado
{
    [Key]
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateOnly FechaDelDia { get; set; } // INDICE
    public int AnioFecha { get; set; }
    public int MesFecha { get; set; }
    public int DiaFecha { get; set; }
    public DiasDeLaSemanaEnum DiaDeLaSemana { get; set; }
    public TipoActividadDelDiaEnum TipoActividadDelDia { get; set; }
    public TurnosEnum Turno { get; set; }
    public TiposDeIncidenciasEnum? TipoDeIncedencia { get; set; }
    [Required]
    public int IdProgramacionAnual { get; set; }
    [Required]
    public int IdCalendarioEmpleado { get; set; }
    [Required]
    public int IdArea { get; set; }
    [Required]
    public int IdGrupo { get; set; }
    [Required]
    public int IdRegla { get; set; }
    [Required]
    public int IdRolSemanal { get; set; }
    [Required]
    public int IdTurnoXRolSemanalXRegla { get; set; }
    [Required]
    public int IdUsuarioEmpleadoSindicalizado { get; set; }
    public int NominaEmpleado { get; set; } // INDICE
    public int? IdVacaciones { get; set; }
    public int? IdIncidenciaOPermiso { get; set; }
    public int? IdDiaInhabil { get; set; }
    public string? DetallesDiaInhabil { get; set; }
    public int? IdIntercambioDiaFestivoPorDescanso { get; set; }
    public int? IdReprogramacionDeVacaciones { get; set; }
    public bool EsDiaFestivo { get; set; } = false;
    public bool EsDiaDeDescanso { get; set; } = false;
    public bool EsDiaLaboral { get; set; } = true;
    public bool EsDiaInhabil { get; set; } = false;
    public bool EsDiaDeVacaciones { get; set; } = false;
    public bool EsDiaDePermiso { get; set; } = false;
    public bool EsDiaReprogramado { get; set; } = false;
    public bool EsDiaIntercambiado { get; set; } = false;
    [Required]
    public virtual ProgramacionesAnuales ProgramacionAnual { get; set; }
    [Required]
    public virtual CalendarioEmpleado CalendarioEmpleado { get; set; }
    [Required]
    public virtual Area Area { get; set; }
    [Required]
    public virtual Grupo Grupo { get; set; }
    [Required]
    public virtual Regla Regla { get; set; }
    [Required]
    public virtual RolSemanal RolSemanal { get; set; }
    [Required]
    public virtual TurnoXRolSemanalXRegla TurnoXRolSemanalXRegla { get; set; }
    [Required]
    public virtual User UsuarioEmpleadoSindicalizado { get; set; }
    public virtual Vacaciones? Vacaciones { get; set; }
    public virtual IncidenciaOPermiso? IncidenciaOPermiso { get; set; }
    public virtual DiasInhabiles? DiaInhabil { get; set; }
    public ReprogramacionesDeVacaciones? ReprogramacionDeVacaciones { get; set; }
    public IntercambiosDiaFestivoPorDescanso? IntercambioDiaFestivoPorDescanso { get; set; }
}

