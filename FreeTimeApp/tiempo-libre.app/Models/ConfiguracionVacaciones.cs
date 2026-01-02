using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models
{
    [Table("ConfiguracionVacaciones")]
    public class ConfiguracionVacaciones
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal PorcentajeAusenciaMaximo { get; set; } = 4.5m;

        [Required]
        [MaxLength(20)]
        public string PeriodoActual { get; set; } = "Cerrado"; // 'ProgramacionAnual', 'Reprogramacion', 'Cerrado'

        [Required]
        public int AnioVigente { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
