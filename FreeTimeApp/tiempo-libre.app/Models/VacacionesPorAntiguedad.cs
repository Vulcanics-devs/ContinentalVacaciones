using System.ComponentModel.DataAnnotations;

namespace tiempo_libre.Models;

public class VacacionesPorAntiguedad
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int AntiguedadEnAniosRangoInicial { get; set; }

    public int? AntiguedadEnAniosRangoFinal { get; set; }

    [Required]
    public int TotalDiasDeVacaciones { get; set; }

    [Required]
    public int DiasAsignadosPorContinental { get; set; } = 12;

    [Required]
    public int DiasParaAsignarAutomaticamente { get; set; } = 0;

    [Required]
    public int DiasPorEscogerPorEmpleado { get; set; } = 0;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}