using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models
{
    public class SuplentePeriodo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int SuplenteId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Rol { get; set; } = null!;

        public int? AreaId { get; set; }

        public int? GrupoId { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [MaxLength(500)]
        public string? Comentarios { get; set; }

        public bool Activo { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(100)]
        public string CreatedBy { get; set; } = null!;

        // Navigation properties
        [ForeignKey("UsuarioId")]
        public User? Usuario { get; set; }

        [ForeignKey("SuplenteId")]
        public User? Suplente { get; set; }

        [ForeignKey("AreaId")]
        public Area? Area { get; set; }

        [ForeignKey("GrupoId")]
        public Grupo? Grupo { get; set; }
    }
}