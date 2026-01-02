using System.ComponentModel.DataAnnotations;

namespace tiempo_libre.DTOs;

public class AreaIngenieroAssignRequest
{
    [Required(ErrorMessage = "El ID del área es requerido")]
    public int AreaId { get; set; }

    [Required(ErrorMessage = "El ID del ingeniero es requerido")]
    public int IngenieroId { get; set; }
}
