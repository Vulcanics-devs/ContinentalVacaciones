using System.ComponentModel.DataAnnotations;
using tiempo_libre.Models.Enums;

namespace tiempo_libre.Models;

public class IncidenciaOPermiso
{
    [Key]
    public int Id { get; set; }
    public DateOnly Fecha { get; set; }
    public DateOnly FechaInicial { get; set; }
    public DateOnly FechaFinal { get; set; }
    public int AnioFechaInicial { get; set; }
    public int MesFechaInicial { get; set; }
    public int DiaFechaInicial { get; set; }
    public int AnioFechaFinal { get; set; }
    public int MesFechaFinal { get; set; }
    public int DiaFechaFinal { get; set; }
    public DiasDeLaSemanaEnum DiaDeLaSemana { get; set; }
    public int IdUsuarioAutoiza { get; set; }
    public int? IdUsuarioSindicato { get; set; }
    public DateTime FechaRegistro { get; set; }
    public int IdUsuarioEmpleado { get; set; }
    public int NominaEmpleado { get; set; } // INDICE
    public int? IdGrupo { get; set; }
    public int? IdRegla { get; set; }
    [MaxLength(250)]
    public string Detalles { get; set; } = string.Empty;
    public TiposDeIncidenciasEnum TiposDeIncedencia { get; set; }
    public virtual User UsuarioAutoriza { get; set; } = null!;
    public virtual User? UsuarioSindicato { get; set; }
    public virtual User UsuarioEmpleado { get; set; } = null!;
    public virtual Grupo? Grupo { get; set; } = null!;
    public virtual Regla? Regla { get; set; } = null!;
}
