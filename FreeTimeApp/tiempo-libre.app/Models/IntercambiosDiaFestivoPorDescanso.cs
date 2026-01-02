using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using tiempo_libre.Models.Enums;

namespace tiempo_libre.Models
{
    public class IntercambiosDiaFestivoPorDescanso
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

        [ForeignKey("JefeAutoriza")]
        public int? IdJefeAutoriza { get; set; }
        public virtual User JefeAutoriza { get; set; }

        [ForeignKey("UsuarioComiteSindicalSolicitante")]
        public int? IdUsuarioComiteSindicalSolicitante { get; set; }
        public virtual User UsuarioComiteSindicalSolicitante { get; set; }

        [Required]
        [ForeignKey("Area")]
        public int IdArea { get; set; }
        public virtual Area Area { get; set; }

        [Required]
        [ForeignKey("Grupo")]
        public int IdGrupo { get; set; }
        public virtual Grupo Grupo { get; set; }

        [Required]
        [ForeignKey("EmpleadoSindicalizadoSolicitante")]
        public int IdEmpleadoSindicalizadoSolicitante { get; set; }
        public virtual User EmpleadoSindicalizadoSolicitante { get; set; }

        [Required]
        public int NominaEmpleadoSindical { get; set; }

        [Required]
        [ForeignKey("DiaFestivoTrabajado")]
        public int IdDiaFestivoTrabajado { get; set; }
        public virtual DiasFestivosTrabajados DiaFestivoTrabajado { get; set; }

        [Required]
        public DateOnly FechaDiaFestivoTrabajado { get; set; }

        [Required]
        public DateOnly FechaDiaDescansoQueTomara { get; set; }

        [MaxLength(250)]
        public string? Justificacion { get; set; }

        [Required]
        public EstatusIntercambioDiaFestivoEnum Estatus { get; set; } = EstatusIntercambioDiaFestivoEnum.Pendiente;

        [ForeignKey("CalPorEmpDiaDescansoQueTomara")]
        public int? IdDiaCalEmpDescansoQueTomara { get; set; }
        public virtual DiasCalendarioEmpleado DiaCalEmpDescansoQueTomara { get; set; }

        [Required]
        public TiposDeCambiosEnum TiposDeCambiosEnum { get; set; } = TiposDeCambiosEnum.IntercambioDiaFestivo;

        [Required]
        public bool CambiosAplicados { get; set; } = false;

        [Required]
        [ForeignKey("PeriodoDeProgramacionAnual")]
        public int IdPeriodoDeProgramacionAnual { get; set; }
        public virtual ProgramacionesAnuales PeriodoDeProgramacionAnual { get; set; }

        [Required]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
        public DateTime? Updated_At { get; set; }
    }
}
