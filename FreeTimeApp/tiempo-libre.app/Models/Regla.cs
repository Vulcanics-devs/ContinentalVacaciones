using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace tiempo_libre.Models;

public class Regla
{
    [Key]
    public int Id { get; set; }
    public ReglaEnum ReglaEnumId { get; set; }
    [Required]
    [MaxLength(25)]
    public string Nombre { get; set; } = null!;
    [MaxLength(250)]
    public string Descripcion { get; set; } = null!;
    public int NumDeGrupos { get; set; }
    public int Prioridad { get; set; }
    public virtual ICollection<TurnoXRolSemanalXRegla> TurnosXRolSemanalXRegla { get; set; } = new List<TurnoXRolSemanalXRegla>();
    public virtual ICollection<RolSemanal> RolesSemanales { get; set; } = new List<RolSemanal>();
}
