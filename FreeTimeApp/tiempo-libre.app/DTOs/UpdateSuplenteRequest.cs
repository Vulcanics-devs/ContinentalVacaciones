using System.ComponentModel.DataAnnotations;

namespace tiempo_libre.DTOs
{
    public class UpdateSuplenteRequest
    {
        [Required]
        public string Rol { get; set; } = null!;
        
        public int? GrupoId { get; set; }
        
        public int? AreaId { get; set; }
        
        public int? SuplenteId { get; set; }
        public string? FechaInicio { get; set; }

        public string? FechaFin { get; set; }

        public string? Comentarios { get; set; }
    }
}
