using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models
{
    public class ConfiguracionBloquesTurnosAgendarVacaciones
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Area")]
        public int IdArea { get; set; }
        public virtual Area Area { get; set; }

        [Required]
        [ForeignKey("Grupo")]
        public int IdGrupo { get; set; }
        public virtual Grupo Grupo { get; set; }

        [Required]
        public int MaximoDeEmpleadosQuePuedenAgendar { get; set; }

        [Required]
        public int TiempoEnHorasDelTurno { get; set; }

        [Required]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
        public DateTime? Updated_At { get; set; }
    }
}
