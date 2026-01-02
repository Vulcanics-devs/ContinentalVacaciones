namespace tiempo_libre.DTOs;

public class AreaIngenieroDTO
{
    public int Id { get; set; }
    public int AreaId { get; set; }
    public string AreaNombre { get; set; } = null!;
    public int IngenieroId { get; set; }
    public string FullName { get; set; } = null!;
    public string Username { get; set; } = null!;
    public DateTime FechaAsignacion { get; set; }
    public DateTime? FechaDesasignacion { get; set; }
    public bool Activo { get; set; }
}
