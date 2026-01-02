using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models
{
    [Table("CambiosBloque")]
    public class CambiosBloque
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmpleadoId { get; set; }

        [Required]
        public int BloqueOrigenId { get; set; }

        [Required]
        public int BloqueDestinoId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Motivo { get; set; } = string.Empty;

        [Required]
        public DateTime FechaCambio { get; set; } = DateTime.UtcNow;

        [Required]
        public int AutorizadoPor { get; set; }

        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "Aplicado"; // 'Aplicado', 'Revertido'

        [MaxLength(500)]
        public string? ObservacionesAdicionales { get; set; }

        // Navigation properties
        [ForeignKey("EmpleadoId")]
        public virtual User Empleado { get; set; } = null!;

        [ForeignKey("BloqueOrigenId")]
        public virtual BloquesReservacion BloqueOrigen { get; set; } = null!;

        [ForeignKey("BloqueDestinoId")]
        public virtual BloquesReservacion BloqueDestino { get; set; } = null!;

        [ForeignKey("AutorizadoPor")]
        public virtual User AutorizadoPorUser { get; set; } = null!;
    }
}