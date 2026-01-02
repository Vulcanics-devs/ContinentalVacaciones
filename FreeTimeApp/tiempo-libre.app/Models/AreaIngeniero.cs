using System.ComponentModel.DataAnnotations;

namespace tiempo_libre.Models;

public class AreaIngeniero
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int AreaId { get; set; }
    public virtual Area Area { get; set; } = null!;

    [Required]
    public int IngenieroId { get; set; }
    public virtual User Ingeniero { get; set; } = null!;

    public int? SuplenteId { get; set; }
    public virtual User? Suplente { get; set; }

    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaDesasignacion { get; set; }
    public bool Activo { get; set; } = true;
}
