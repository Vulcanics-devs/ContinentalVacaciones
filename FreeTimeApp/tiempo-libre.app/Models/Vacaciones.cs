using System.ComponentModel.DataAnnotations;
using tiempo_libre.Models.Enums;

namespace tiempo_libre.Models;

public class Vacaciones
{
    [Key]
    public int Id { get; set; }
    public int NominaEmpleado { get; set; } // INDICE
    public DateOnly Fecha { get; set; }
    public int AnioFecha { get; set; }
    public int MesFecha { get; set; }
    public int DiaFecha { get; set; }
    public TurnosEnum TurnoCubria { get; set; }
    public TipoActividadDelDiaEnum ActividadDelDia { get; set; }
    public int IdGrupo { get; set; }
    public int IdTurnoXRolSemanalXRegla { get; set; }
    public bool AsignadaPorJefe { get; set; }
    public DateOnly CreatedAt { get; set; }
    public int IdUsuarioEmpleadoSindicalizado { get; set; }
    public virtual User UsuarioEmpleadoSindicalizado { get; set; } = null!;
    public virtual Grupo Grupo { get; set; } = null!;
    public virtual TurnoXRolSemanalXRegla TurnoXRolSemanalXRegla { get; set; } = null!;
}
