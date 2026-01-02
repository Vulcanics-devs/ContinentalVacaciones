using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace tiempo_libre.Models;

public partial class Grupo
{
    [Key]
    public int GrupoId { get; set; }
    [Required]
    public int AreaId { get; set; }
    [Required]
    [MaxLength(25)]
    public string Rol { get; set; } = null!;
    [Required]
    [MaxLength(100)]
    public string IdentificadorSAP { get; set; }
    public int PersonasPorTurno { get; set; } = 3;
    public int DuracionDeturno { get; set; } = 24;
    public virtual Area Area { get; set; } = null!;
    public int? LiderId { get; set; }
    public virtual User? Lider { get; set; } = null!;
    public int? LiderSuplenteId { get; set; }
    public virtual User? LiderSuplente { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<Sindicalizado> Sindicalizados { get; set; } = new List<Sindicalizado>();
    public Grupo() { }
}
