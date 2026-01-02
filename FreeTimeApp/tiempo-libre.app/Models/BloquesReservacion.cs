using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models
{
    [Table("BloquesReservacion")]
    public class BloquesReservacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GrupoId { get; set; }

        [Required]
        public int AnioGeneracion { get; set; }

        [Required]
        public int NumeroBloque { get; set; }

        [Required]
        public DateTime FechaHoraInicio { get; set; }

        [Required]
        public DateTime FechaHoraFin { get; set; }

        [Required]
        public int PersonasPorBloque { get; set; }

        [Required]
        public int DuracionHoras { get; set; }

        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "Activo"; // 'Activo', 'Completado', 'Cancelado'

        [Required]
        public bool EsBloqueCola { get; set; } = false;

        public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;

        public int GeneradoPor { get; set; }

        public DateTime? FechaAprobacion { get; set; }

        public int? AprobadoPor { get; set; }

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // Navigation properties
        [ForeignKey("GrupoId")]
        public virtual Grupo Grupo { get; set; } = null!;

        [ForeignKey("GeneradoPor")]
        public virtual User GeneradoPorUser { get; set; } = null!;

        [ForeignKey("AprobadoPor")]
        public virtual User? AprobadoPorUser { get; set; }

        public virtual ICollection<AsignacionesBloque> AsignacionesBloque { get; set; } = new HashSet<AsignacionesBloque>();
    }
}