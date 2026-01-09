using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models
{
    /// <summary>
    /// Modelo para solicitudes de permisos creadas por delegados sindicales
    /// Requieren aprobación del jefe de área
    /// </summary>
    [Table("SolicitudesPermisos")]
    public class SolicitudPermiso
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NominaEmpleado { get; set; }

        [Required]
        [MaxLength(200)]
        public string NombreEmpleado { get; set; } = string.Empty;

        /// <summary>
        /// Código SAP de ausencia
        /// </summary>
        [Required]
        public int ClAbPre { get; set; }

        [Required]
        public DateOnly FechaInicio { get; set; }
        [Required]
        public DateOnly FechaFin { get; set; }

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        /// <summary>
        /// Estado: Pendiente, Aprobada, Rechazada
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Estado { get; set; } = "Pendiente";

        /// <summary>
        /// Motivo de rechazo (si aplica)
        /// </summary>
        [MaxLength(500)]
        public string? MotivoRechazo { get; set; }

        [Required]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        public DateTime? FechaRespuesta { get; set; }

        /// <summary>
        /// ID del delegado sindical que crea la solicitud
        /// </summary>
        [Required]
        public int DelegadoSindicalizadoId { get; set; }

        [ForeignKey("DelegadoSindicalizadoId")]
        public User DelegadoSindicalizado { get; set; } = null!;

        /// <summary>
        /// ID del jefe de área que debe aprobar
        /// </summary>
        public int? JefeAreaId { get; set; }

        [ForeignKey("JefeAreaId")]
        public User? JefeArea { get; set; }

        /// <summary>
        /// ID del área del empleado
        /// </summary>
        public int? AreaId { get; set; }

        [ForeignKey("AreaId")]
        public Area? Area { get; set; }

        /// <summary>
        /// ID del grupo del empleado
        /// </summary>
        public int? GrupoId { get; set; }

        [ForeignKey("GrupoId")]
        public Grupo? Grupo { get; set; }

        /// <summary>
        /// ID del registro creado en PermisosEIncapacidadesSAP después de aprobación
        /// </summary>
        public string? PermisoCreado { get; set; } // JSON: {Nomina, Desde, Hasta, ClAbPre}
    }
}