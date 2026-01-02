using System.ComponentModel.DataAnnotations;

namespace tiempo_libre.DTOs;

public class GrupoUpdateTurnoRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "PersonasPorTurno debe ser mayor a 0")]
    public int PersonasPorTurno { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "DuracionDeturno debe ser mayor a 0")]
    public int DuracionDeturno { get; set; }
}
