using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models
{
    public class ManningPorMes
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int IdUsuarioModifica { get; set; }
        [ForeignKey("IdUsuarioModifica")]
        public User? UsuarioModifica { get; set; }

        [Required]
        public int IdJefeArea { get; set; }
        [ForeignKey("IdJefeArea")]
        public User? JefeArea { get; set; }

        [Required]
        public int IdArea { get; set; }
        [ForeignKey("IdArea")]
        public Area? Area { get; set; }

        [Required]
        public int Anio { get; set; }

        [Required]
        public int Mes { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal PorcentajeManning { get; set; }

        public bool BorradoLogico { get; set; } = false;

        public DateTime Created_At { get; set; }
        public DateTime? Updated_At { get; set; }
    }
}
