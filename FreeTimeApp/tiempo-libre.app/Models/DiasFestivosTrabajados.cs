using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models
{
    public class DiasFestivosTrabajados
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int IdUsuarioEmpleadoSindicalizado { get; set; }
        public virtual User User { get; set; }

        [Required]
        public int NominaEmpleadoSindical { get; set; }

        [Required]
        [ForeignKey("Area")]
        public int IdArea { get; set; }
        public virtual Area Area { get; set; }

        [Required]
        [ForeignKey("Grupo")]
        public int IdGrupo { get; set; }
        public virtual Grupo Grupo { get; set; }

        [Required]
        public DateOnly FechaDiaFestivoTrabajado { get; set; }

        [Required]
        public bool Compensado { get; set; } = false;

        [Required]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
        public DateTime? Updated_At { get; set; }
    }
}
