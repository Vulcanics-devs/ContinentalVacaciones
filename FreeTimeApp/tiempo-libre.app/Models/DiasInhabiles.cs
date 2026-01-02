using System.ComponentModel.DataAnnotations;
using tiempo_libre.Models.Enums;

namespace tiempo_libre.Models;

public class DiasInhabiles
{
    [Key]
    public int Id { get; set; }
    public DateOnly Fecha { get; set; }
    public DateOnly FechaInicial { get; set; }
    public DateOnly FechaFinal { get; set; }
    public int AnioFechaInicial { get; set; } // INDICE
    public int MesFechaInicial { get; set; } // INDICE
    public int DiaFechaInicial { get; set; } // INDICE
    public int AnioFechaFinal { get; set; } // INDICE
    public int MesFechaFinal { get; set; } // INDICE
    public int DiaFechaFinal { get; set; } // INDICE
    [MaxLength(250)]
    public string Detalles { get; set; } = string.Empty;
    public TipoActividadDelDiaEnum TipoActividadDelDia { get; set; }
}
