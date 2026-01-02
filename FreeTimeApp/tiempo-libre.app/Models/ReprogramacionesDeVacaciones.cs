using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using tiempo_libre.Models.Enums;

namespace tiempo_libre.Models
{
    public class ReprogramacionesDeVacaciones
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("EmpleadoSindicalizadoSolicitante")]
        public int IdEmpleadoSindicalizadoSolicitante { get; set; }
        public virtual User EmpleadoSindicalizadoSolicitante { get; set; }

        [ForeignKey("UsuarioComiteSindicalSolicitante")]
        public int? IdUsuarioComiteSindicalSolicitante { get; set; }
        public virtual User UsuarioComiteSindicalSolicitante { get; set; }

        [Required]
        public int NominaEmpleadoSindical { get; set; }

        [Required]
        [ForeignKey("PeriodoDeProgramacionAnual")]
        public int IdPeriodoDeProgramacionAnual { get; set; }
        public virtual ProgramacionesAnuales PeriodoDeProgramacionAnual { get; set; }

        [MaxLength(250)]
        public string? Detalles { get; set; }

        [Required]
        public DateOnly FechaDiasDeVacacionOriginal { get; set; }

        [Required]
        public DateOnly FechaDiasDeVacacionReprogramada { get; set; }

        [Required]
        [ForeignKey("DiaCalEmpCambiar")]
        public int IdDiaCalEmpCambiar { get; set; }
        public virtual DiasCalendarioEmpleado DiaCalEmpCambiar { get; set; }

        [Required]
        [ForeignKey("DiaCalEmpNuevo")]
        public int IdDiaCalEmpNuevo { get; set; }
        public virtual DiasCalendarioEmpleado DiaCalEmpNuevo { get; set; }

        [Required]
        public EstatusReprogramacionDeVacacionesEnum Estatus { get; set; } = EstatusReprogramacionDeVacacionesEnum.Pendiente;

        [Required]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
        public DateTime? Updated_At { get; set; }
    }
}
