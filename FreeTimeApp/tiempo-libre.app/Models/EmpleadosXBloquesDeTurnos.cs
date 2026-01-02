using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models
{
    public class EmpleadosXBloquesDeTurnos
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("BloqueDeTurnosAgendarVacaciones")]
        public int IdBloqueDeTurnos { get; set; }
        public virtual BloqueDeTurnosAgendarVacaciones BloqueDeTurnosAgendarVacaciones { get; set; }

        [Required]
        [ForeignKey("EmpleadoSindicalAgendara")]
        public int IdEmpleadoSindicalAgendara { get; set; }
        public virtual User EmpleadoSindicalAgendara { get; set; }

        [Required]
        public int NominaEmpleadoSindicalAgendara { get; set; }

        [Required]
        public int AntiguedadEnAniosAlMomentoDeAgendar { get; set; }

        [Required]
        public DateTime FechaYHoraAgendacion { get; set; }

        [Required]
        public bool AgendooVacaciones { get; set; } = false;

        [Required]
        public bool AgendoTodo { get; set; } = false;

        public DateTime? FechaYHoraReservoVacaciones { get; set; }

        [Required]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
        public DateTime? Updated_At { get; set; }
    }
}
