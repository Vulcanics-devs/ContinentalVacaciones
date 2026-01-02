using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models
{
    public class ReservacionesDeVacacionesPorEmpleado
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("BloqueDeTurnosAgendarVacaciones")]
        public int IdBloqueDeTurnos { get; set; }

        [Required]
        [ForeignKey("EmpleadoSindicalizado")]
        public int IdEmpleadoSindicalizado { get; set; }

        [Required]
        public int NominaEmpleadoSindical { get; set; }

        [Required]
        [ForeignKey("ProgramacionAnual")]
        public int IdProgramacionAnual { get; set; }

        [Required]
        [ForeignKey("CalendarioEmpleado")]
        public int IdCalendarioEmpleado { get; set; }

        [Required]
        [ForeignKey("DiaCalendarioEmpleado")]
        public int IdDiaCalendarioEmpleado { get; set; }

        [Required]
        public DateOnly FechaDiaDeVacacion { get; set; }
        [Required]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
        public DateTime? Updated_At { get; set; }

        public virtual BloqueDeTurnosAgendarVacaciones BloqueDeTurnosAgendarVacaciones { get; set; }
        public virtual User EmpleadoSindicalizado { get; set; }
        public virtual ProgramacionesAnuales ProgramacionAnual { get; set; }
        public virtual CalendarioEmpleado CalendarioEmpleado { get; set; }
        public virtual DiasCalendarioEmpleado DiaCalendarioEmpleado { get; set; }
    }
}
