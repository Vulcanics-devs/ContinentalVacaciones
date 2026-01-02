using System.ComponentModel.DataAnnotations;
using tiempo_libre.Models.Enums;

namespace tiempo_libre.Models;

public class TurnoXRolSemanalXRegla
{
    [Key]
    public int Id { get; set; }
    public int IndicePorRegla { get; set; } // INDICE
    public int IdRegla { get; set; }
    public int IdRolSemanal { get; set; }
    public DiasDeLaSemanaEnum DiaDeLaSemana { get; set; }
    public TipoActividadDelDiaEnum ActividadDelDia { get; set; }
    public TurnosEnum? Turno { get; set; }
    public virtual Regla Regla { get; set; } = null!;
    public virtual RolSemanal RolSemanal { get; set; } = null!;

    public override string ToString()
    {
        return $"Regla: {IdRegla}, RolSemanal: {IdRolSemanal}, DiaDeLaSemana: {DiaDeLaSemana}, ActividadDelDia: {ActividadDelDia}, Turno: {Turno}";
    }
}