namespace tiempo_libre.DTOs;

public class AreaWithIngenierosDTO
{
    public int AreaId { get; set; }
    public string UnidadOrganizativaSap { get; set; } = null!;
    public string NombreGeneral { get; set; } = null!;
    public int? JefeId { get; set; }
    public string? JefeNombre { get; set; }
    public int? JefeSuplenteId { get; set; }
    public string? JefeSuplenteNombre { get; set; }
    public List<IngenieroAsignadoDTO> Ingenieros { get; set; } = new();
}

public class IngenieroAsignadoDTO
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Username { get; set; } = null!;
    public DateTime FechaAsignacion { get; set; }
    public bool Activo { get; set; }
}
