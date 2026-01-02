using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using tiempo_libre.Models.Enums;

namespace tiempo_libre.Models
{
    public class ProgramacionesAnuales
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int IdSuperUser { get; set; }
        public virtual User User { get; set; }

        [Required]
        public int Anio { get; set; }

        [Required]
        public DateTime FechaInicia { get; set; }

        [Required]
        public DateTime FechaTermina { get; set; }

        [MaxLength(250)]
        public string? Detalles { get; set; }

        public DateTime? FechaInicioReservaTurnos { get; set; } = null!;

        [Required]
        public EstatusProgramacionAnualEnum Estatus { get; set; } = EstatusProgramacionAnualEnum.EnProceso;

        [Required]
        public bool BorradoLogico { get; set; } = false;

        [Required]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
        public DateTime? Updated_At { get; set; }
        public DateTime? Deleted_At { get; set; }
    }
}
