using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models
{
    public class BloqueDeTurnosAgendarVacaciones
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("JefeArea")]
        public int IdJefeArea { get; set; }
        public virtual User JefeArea { get; set; }

        [Required]
        [ForeignKey("Area")]
        public int IdArea { get; set; }
        public virtual Area Area { get; set; }

        [Required]
        [ForeignKey("Grupo")]
        public int IdGrupo { get; set; }
        public virtual Grupo Grupo { get; set; }

        [Required]
        [ForeignKey("PeriodoDeProgramacionAnual")]
        public int IdPeriodoDeProgramacionAnual { get; set; }
        public virtual ProgramacionesAnuales PeriodoDeProgramacionAnual { get; set; }

        [Required] // EF Core: uniqueness will be set in Fluent API
        public int IndiceBloqueDeTurnos { get; set; }

        [Required]
        [MaxLength(50)]
        public string NombreDelBloque { get; set; }

        [Required]
        public DateTime FechaYHoraInicio { get; set; }

        [Required]
        public DateTime FechaYHoraFin { get; set; }

        [Required]
        public int DuracionEnHoras { get; set; }

        [Required]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
        public DateTime? Updated_At { get; set; }
    }
}
