using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models
{
    [Table("VacacionesProgramadas")]
    public class VacacionesProgramadas
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmpleadoId { get; set; }

        [Required]
        public DateOnly FechaVacacion { get; set; }

        [Required]
        [MaxLength(50)]
        public string TipoVacacion { get; set; } = string.Empty; // 'Anual', 'Reprogramacion', 'AsignadaAutomaticamente'

        [Required]
        [MaxLength(30)]
        public string OrigenAsignacion { get; set; } = "Manual"; // 'Manual', 'Automatica'

        [MaxLength(20)]
        public string EstadoVacacion { get; set; } = "Activa"; // 'Activa', 'Intercambiada', 'Cancelada'

        [Required]
        [MaxLength(20)]
        public string PeriodoProgramacion { get; set; } = string.Empty; // 'ProgramacionAnual', 'Reprogramacion'

        public DateTime FechaProgramacion { get; set; } = DateTime.Now;

        public bool PuedeSerIntercambiada { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // Navigation properties
        [ForeignKey("EmpleadoId")]
        public virtual User Empleado { get; set; } = null!;

        [ForeignKey("CreatedBy")]
        public virtual User? CreatedByUser { get; set; }

        [ForeignKey("UpdatedBy")]
        public virtual User? UpdatedByUser { get; set; }
    }
}
