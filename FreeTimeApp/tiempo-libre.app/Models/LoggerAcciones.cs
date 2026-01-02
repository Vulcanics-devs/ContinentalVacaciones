using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using tiempo_libre.Models.Enums;

namespace tiempo_libre.Models
{
    public class LoggerAcciones
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public TiposDeAccionesEnum Accion { get; set; }

        [Required]
        [ForeignKey("User")]
        public int IdUsuario { get; set; }
        public virtual User User { get; set; }

        [Required]
        [MaxLength(150)]
        public string NombreCompletoUsuario { get; set; }

        public int? NominaUsuario { get; set; }

        [Required]
        public int IdRegistro { get; set; }

        [Required]
        [MaxLength(100)]
        public string Modelo { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [MaxLength(250)]
        public string Detalles { get; set; }

        [ForeignKey("Area")]
        public int? IdArea { get; set; }
        public virtual Area Area { get; set; }

        [ForeignKey("Grupo")]
        public int? IdGrupo { get; set; }
        public virtual Grupo Grupo { get; set; }

        [Required]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
    }
}
