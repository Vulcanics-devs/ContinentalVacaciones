using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models
{
    [Table("SolicitudesFestivosTrabajados")]
    public class SolicitudesFestivosTrabajados
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmpleadoId { get; set; }

        [Required]
        public int FestivoTrabajadoOriginalId { get; set; }

        [Required]
        public DateOnly FechaNuevaSolicitada { get; set; }

        [Required]
        [MaxLength(500)]
        public string Motivo { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string EstadoSolicitud { get; set; } = "Pendiente"; // Pendiente, Aprobada, Rechazada

        public decimal? PorcentajeCalculado { get; set; }

        [Required]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        [Required]
        public int SolicitadoPorId { get; set; }

        public int? JefeAreaId { get; set; }

        public DateTime? FechaRespuesta { get; set; }

        public int? AprobadoPorId { get; set; }

        [MaxLength(500)]
        public string? MotivoRechazo { get; set; }

        // Para rastrear el festivo original
        public DateOnly FestivoOriginal { get; set; }

        [Required]
        public int Nomina { get; set; }

        // Para rastrear si ya se creó la vacación después de aprobación
        public int? VacacionCreadaId { get; set; }

        // Navigation properties
        [ForeignKey("EmpleadoId")]
        public virtual User Empleado { get; set; } = null!;

        [ForeignKey("FestivoTrabajadoOriginalId")]
        public virtual DiasFestivosTrabajadosOriginalTable FestivoTrabajadoOriginal { get; set; } = null!;

        [ForeignKey("SolicitadoPorId")]
        public virtual User SolicitadoPor { get; set; } = null!;

        [ForeignKey("JefeAreaId")]
        public virtual User? JefeArea { get; set; }

        [ForeignKey("AprobadoPorId")]
        public virtual User? AprobadoPor { get; set; }

        [ForeignKey("VacacionCreadaId")]
        public virtual VacacionesProgramadas? VacacionCreada { get; set; }
    }
}