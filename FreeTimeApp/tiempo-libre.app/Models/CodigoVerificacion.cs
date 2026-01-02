using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models
{
    [Table("CodigosVerificacion")]
    public class CodigoVerificacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual User Usuario { get; set; }

        [Required]
        [MaxLength(10)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TipoCodigo { get; set; } = string.Empty; // "CambioPassword", "ActivacionCuenta", etc.

        [Required]
        public DateTime FechaCreacion { get; set; }

        [Required]
        public DateTime FechaExpiracion { get; set; }

        public DateTime? FechaUso { get; set; }

        public bool Usado { get; set; } = false;

        public int IntentosRestantes { get; set; } = 3;

        [MaxLength(100)]
        public string? IpSolicitud { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }
    }
}