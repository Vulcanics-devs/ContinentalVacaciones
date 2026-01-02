using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using tiempo_libre.Models.Enums;

namespace tiempo_libre.Models
{
    public class SolicitudIntercambiosOReprogramacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("JefeArea")]
        public int IdJefeArea { get; set; }
        public virtual User JefeArea { get; set; }

        [Required]
        [ForeignKey("LiderGrupo")]
        public int IdLiderGrupo { get; set; }
        public virtual User LiderGrupo { get; set; }

        [ForeignKey("UsuarioComiteSindicalSolicitante")]
        public int? IdUsuarioComiteSindicalSolicitante { get; set; }
        public virtual User UsuarioComiteSindicalSolicitante { get; set; }

        [Required]
        [ForeignKey("EmpleadoSindicalizadoSolicitante")]
        public int IdEmpleadoSindicalizadoSolicitante { get; set; }
        public virtual User EmpleadoSindicalizadoSolicitante { get; set; }

        [ForeignKey("JefeLiderAutoriza")]
        public int? IdJefeLiderAutoriza { get; set; }
        public virtual User JefeLiderAutoriza { get; set; }

        [Required]
        [ForeignKey("Area")]
        public int IdArea { get; set; }
        public virtual Area Area { get; set; }

        [Required]
        [ForeignKey("Grupo")]
        public int IdGrupo { get; set; }
        public virtual Grupo Grupo { get; set; }

        [ForeignKey("IntercambiosDiaFestivoPorDescanso")]
        public int? IdIntercambiosDiaFestivoPorDescanso { get; set; }
        public virtual IntercambiosDiaFestivoPorDescanso IntercambiosDiaFestivoPorDescanso { get; set; }

        [ForeignKey("ReprogramacionesDeVacaciones")]
        public int? IdReprogramacionesDeVacaciones { get; set; }
        public virtual ReprogramacionesDeVacaciones ReprogramacionesDeVacaciones { get; set; }

        [Required]
        public TiposDeCambiosEnum TipoDeSolicitud { get; set; }

        [Required]
        public int NominaEmpleadoSindical { get; set; }

        [MaxLength(250)]
        public string? Justificacion { get; set; }

        [Required]
        public EstatusSolicitudEnum Estatus { get; set; } = EstatusSolicitudEnum.Pendiente;

        public DateTime? FechaRespuesta { get; set; }

        [Required]
        [ForeignKey("PeriodoDeProgramacionAnual")]
        public int IdPeriodoDeProgramacionAnual { get; set; }
        public virtual ProgramacionesAnuales PeriodoDeProgramacionAnual { get; set; }

        [Required]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
        public DateTime? Updated_At { get; set; }
    }
}
