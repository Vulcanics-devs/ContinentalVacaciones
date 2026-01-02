using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using tiempo_libre.Models.Enums;

namespace tiempo_libre.Models
{
    public class Notificaciones
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public TiposDeNotificacionEnum TipoDeNotificacion { get; set; }

        public int? IdUsuarioReceptor { get; set; }
        [ForeignKey("IdUsuarioReceptor")]
        public User? UsuarioReceptor { get; set; }

        public int? IdUsuarioEmisor { get; set; }
        [ForeignKey("IdUsuarioEmisor")]
        public User? UsuarioEmisor { get; set; }

        /// <summary>
        /// Nombre completo de quien realizó el cambio/acción
        /// Se guarda el nombre aunque no tenga referencia a User
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string NombreEmisor { get; set; } = null!;

        public int? IdSolicitud { get; set; }
        // FK a SolicitudCambioDiasFestivosPorDescanso o ReprogramacionesDeVacaciones (relación polimórfica, solo referencia por id)

        [Required]
        [MaxLength(150)]
        public string Titulo { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string Mensaje { get; set; } = null!;

        /// <summary>
        /// Fecha y hora de la acción que generó la notificación
        /// </summary>
        [Required]
        public DateTime FechaAccion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Estatus de la notificación (NoLeída, Leída, Archivada)
        /// </summary>
        [Required]
        public EstatusNotificacionEnum Estatus { get; set; } = EstatusNotificacionEnum.NoLeida;

        /// <summary>
        /// ID del área relacionada (si aplica)
        /// </summary>
        public int? AreaId { get; set; }
        [ForeignKey("AreaId")]
        public Area? Area { get; set; }

        /// <summary>
        /// ID del grupo relacionado (si aplica)
        /// </summary>
        public int? GrupoId { get; set; }
        [ForeignKey("GrupoId")]
        public Grupo? Grupo { get; set; }

        /// <summary>
        /// Tipo específico de movimiento dentro de la categoría
        /// Ej: "Vacaciones Programadas", "Intercambio Aprobado", etc.
        /// </summary>
        [MaxLength(100)]
        public string? TipoMovimiento { get; set; }

        /// <summary>
        /// Metadatos adicionales en formato JSON (opcional)
        /// Para almacenar información específica del contexto
        /// </summary>
        [MaxLength(1000)]
        public string? MetadatosJson { get; set; }
    }
}
