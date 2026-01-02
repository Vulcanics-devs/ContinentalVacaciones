using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models
{
    [Table("AsignacionesBloque")]
    public class AsignacionesBloque
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BloqueId { get; set; }

        [Required]
        public int EmpleadoId { get; set; }

        [Required]
        public int PosicionEnBloque { get; set; } // Orden dentro del bloque (1, 2, 3...)

        [Required]
        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

        [Required]
        public int AsignedoPor { get; set; }

        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "Asignado"; // 'Asignado', 'Reservado', 'Completado', 'Transferido', 'NoRespondio'

        public DateTime? FechaCompletado { get; set; }

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // Navigation properties
        [ForeignKey("BloqueId")]
        public virtual BloquesReservacion Bloque { get; set; } = null!;

        [ForeignKey("EmpleadoId")]
        public virtual User Empleado { get; set; } = null!;

        [ForeignKey("AsignedoPor")]
        public virtual User AsignedoPorUser { get; set; } = null!;
    }
}