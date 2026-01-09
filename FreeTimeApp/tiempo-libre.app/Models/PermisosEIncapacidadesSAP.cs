using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models
{
    /// <summary>
    /// Modelo para la tabla PermisosEIncapacidadesSAP
    /// Almacena tanto registros provenientes de SAP como registros manuales
    /// </summary>
    [Table("PermisosEIncapacidadesSAP")]
    public class PermisosEIncapacidadesSAP
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        [Required]
        public int Nomina { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Posicion { get; set; } = string.Empty;

        [Required]
        public DateOnly Desde { get; set; }

        [Required]
        public DateOnly Hasta { get; set; }

        /// <summary>
        /// Código SAP de ausencia (2380, 1331, 1100, 2310, 2381, 2396, 2394, 2123, 1315)
        /// </summary>
        [Required]
        public int ClAbPre { get; set; }  // Cambiar de string a int

        /// <summary>
        /// Descripción de la clase de absentismo
        /// </summary>
        [MaxLength(200)]
        public string? ClaseAbsentismo { get; set; } = string.Empty;

        /// <summary>
        /// Número de días del permiso/incapacidad
        /// </summary>
        public double? Dias { get; set; }

        /// <summary>
        /// Días naturales
        /// </summary>
        public double? DiaNat { get; set; }

        /// <summary>
        /// Observaciones adicionales
        /// </summary>
        [MaxLength(500)]
        public string? Observaciones { get; set; }

        /// <summary>
        /// Indica si el registro fue creado manualmente (true) o proviene de SAP (false)
        /// </summary>
        public bool EsRegistroManual { get; set; } = false;

        /// <summary>
        /// Fecha en que se registró el permiso/incapacidad en el sistema
        /// </summary>
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        /// <summary>
        /// ID del usuario que registró el permiso/incapacidad (solo para registros manuales)
        /// </summary>
        public int? UsuarioRegistraId { get; set; }

        /// <summary>
        /// Relación con el usuario que registró (opcional)
        /// </summary>
        [ForeignKey("UsuarioRegistraId")]
        public User? UsuarioRegistra { get; set; }

        /// <summary>
        /// Estado: Aprobado, Pendiente, Rechazado
        /// </summary>
        [MaxLength(20)]
        public string? EstadoSolicitud { get; set; } = "Aprobado";

        /// <summary>
        /// ID del delegado sindical que solicitó (solo para solicitudes)
        /// </summary>
        public int? DelegadoSolicitanteId { get; set; }

        //[ForeignKey("DelegadoSolicitanteId")]
        //public User? DelegadoSolicitante { get; set; }

        /// <summary>
        /// ID del jefe que aprobó/rechazó
        /// </summary>
        public int? JefeAprobadorId { get; set; }

        //[ForeignKey("JefeAprobadorId")]
        //public User? JefeAprobador { get; set; }

        /// <summary>
        /// Motivo de rechazo
        /// </summary>
        [MaxLength(500)]
        public string? MotivoRechazo { get; set; }

        /// <summary>
        /// Fecha de la solicitud
        /// </summary>
        public DateTime? FechaSolicitud { get; set; }

        /// <summary>
        /// Fecha de respuesta (aprobación/rechazo)
        /// </summary>
        public DateTime? FechaRespuesta { get; set; }
    }
}