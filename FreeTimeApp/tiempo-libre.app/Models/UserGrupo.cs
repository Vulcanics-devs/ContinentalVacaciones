using System.ComponentModel.DataAnnotations;

namespace tiempo_libre.Models;

public class UserGrupo
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public int GrupoId { get; set; }
    public virtual User User { get; set; } = null!;
    public virtual Grupo Grupo { get; set; } = null!;
}
