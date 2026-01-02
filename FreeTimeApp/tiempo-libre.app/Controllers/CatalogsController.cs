using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using tiempo_libre.Models.Enums;
using tiempo_libre.Models;
using System.Linq;

namespace tiempo_libre.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CatalogsController : ControllerBase
{
    [HttpGet("tipos-actividad")]
    public IActionResult GetTiposActividadDelDia()
    {
        var tipos = ((TipoActividadDelDiaEnum[])System.Enum.GetValues(typeof(TipoActividadDelDiaEnum)))
            .Select(e => new
            {
                Value = (int)e,
                Name = string.Concat(e.ToString().Select((c, i) => i > 0 && char.IsUpper(c) ? " " + c : c.ToString()))
            })
            .ToList();
        var response = new ApiResponse<object>(true, tipos, "Tipos de actividad del día obtenidos correctamente");
        return Ok(response);
    }

    [HttpGet("turnos")]
    public IActionResult GetTurnos()
    {
        var turnos = ((TurnosEnum[])System.Enum.GetValues(typeof(TurnosEnum)))
            .Select(e => new
            {
                Value = (int)e,
                Name = string.Concat(e.ToString().Select((c, i) => i > 0 && char.IsUpper(c) ? " " + c : c.ToString()))
            })
            .ToList();
        var response = new ApiResponse<object>(true, turnos, "Turnos obtenidos correctamente");
        return Ok(response);
    }

    [HttpGet("tipos-incidencia")]
    public IActionResult GetTiposDeIncidencia()
    {
        var tipos = ((TiposDeIncidenciasEnum[])System.Enum.GetValues(typeof(TiposDeIncidenciasEnum)))
            .Select(e => new
            {
                Value = (int)e,
                Name = string.Concat(e.ToString().Select((c, i) => i > 0 && char.IsUpper(c) ? " " + c : c.ToString()))
            })
            .ToList();
        var response = new ApiResponse<object>(true, tipos, "Tipos de incidencia obtenidos correctamente");
        return Ok(response);
    }
}
