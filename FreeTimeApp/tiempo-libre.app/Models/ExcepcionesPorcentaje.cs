using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models
{
    [Table("ExcepcionesPorcentaje")]
    public class ExcepcionesPorcentaje
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GrupoId { get; set; }

        [Required]
        public DateOnly Fecha { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal PorcentajeMaximoPermitido { get; set; }

        [MaxLength(200)]
        public string? Motivo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("GrupoId")]
        public virtual Grupo Grupo { get; set; } = null!;
    }
}
