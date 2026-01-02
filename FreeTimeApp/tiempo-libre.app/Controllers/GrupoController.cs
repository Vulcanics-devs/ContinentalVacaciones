using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using tiempo_libre.Models;
using tiempo_libre.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
// ...existing using statements...
[Authorize] // Acceso sólo para usuarios autenticados
public class GrupoController : ControllerBase
{
    private readonly FreeTimeDbContext _db;
    public GrupoController(FreeTimeDbContext db)
    {
        _db = db;
    }

    // PUT: api/Grupo/{id}/Lider
    [HttpPut("{id}/Lider")]
    [Authorize(Roles = "SuperUsuario")]
    public async Task<IActionResult> AsignarLider(int id, [FromBody] int liderId)
    {
        var grupo = await _db.Grupos.FindAsync(id);
        if (grupo == null)
        {
            return NotFound(new ApiResponse<Grupo>(false, null, "Grupo no encontrado"));
        }
        var lider = await _db.Users.FindAsync(liderId);
        if (lider == null)
        {
            return BadRequest(new ApiResponse<Grupo>(false, null, "El líder especificado no existe"));
        }
        grupo.LiderId = liderId;
        await _db.SaveChangesAsync();
        return Ok(new ApiResponse<Grupo>(true, grupo));
    }

    // PUT: api/Grupo/{id}/LiderSuplente
    [HttpPut("{id}/LiderSuplente")]
    [Authorize(Roles = "SuperUsuario")]
    public async Task<IActionResult> AsignarLiderSuplente(int id, [FromBody] int liderSuplenteId)
    {
        var grupo = await _db.Grupos.FindAsync(id);
        if (grupo == null)
        {
            return NotFound(new ApiResponse<Grupo>(false, null, "Grupo no encontrado"));
        }
        var liderSuplente = await _db.Users.FindAsync(liderSuplenteId);
        if (liderSuplente == null)
        {
            return BadRequest(new ApiResponse<Grupo>(false, null, "El líder suplente especificado no existe"));
        }
        grupo.LiderSuplenteId = liderSuplenteId;
        await _db.SaveChangesAsync();
        return Ok(new ApiResponse<Grupo>(true, grupo));
    }

    // PUT: api/Grupo/{id}/Turno
    [HttpPut("{id}/Turno")]
    [Authorize(Roles = "SuperUsuario")]
    public async Task<IActionResult> UpdateTurno(int id, [FromBody] GrupoUpdateTurnoRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<Grupo>(false, null, "Datos inválidos"));
        }

        var grupo = await _db.Grupos.FindAsync(id);
        if (grupo == null)
        {
            return NotFound(new ApiResponse<Grupo>(false, null, "Grupo no encontrado"));
        }

        grupo.PersonasPorTurno = request.PersonasPorTurno;
        grupo.DuracionDeturno = request.DuracionDeturno;

        await _db.SaveChangesAsync();
        return Ok(new ApiResponse<Grupo>(true, grupo));
    }

    // POST: api/Grupo
    [HttpPost]
    [Authorize(Roles = "SuperUsuario")]
    public async Task<IActionResult> Create([FromBody] Grupo? grupo)
    {
        if (grupo == null)
        {
            return BadRequest(new ApiResponse<Grupo>(false, null, "El grupo es requerido"));
        }
        // Validar campos requeridos
        if (string.IsNullOrWhiteSpace(grupo.IdentificadorSAP) || grupo.PersonasPorTurno <= 0 || grupo.DuracionDeturno <= 0)
        {
            return BadRequest(new ApiResponse<Grupo>(false, null, "Datos requeridos faltantes o inválidos"));
        }
        // Validar que el Lider exista
        if (grupo.LiderId == null || !await _db.Users.AnyAsync(u => u.Id == grupo.LiderId))
        {
            return BadRequest(new ApiResponse<Grupo>(false, null, "El líder especificado no existe"));
        }
        // Validar que el LiderSuplente exista si se recibe
        if (grupo.LiderSuplenteId != null && !await _db.Users.AnyAsync(u => u.Id == grupo.LiderSuplenteId))
        {
            return BadRequest(new ApiResponse<Grupo>(false, null, "El líder suplente especificado no existe"));
        }
        _db.Grupos.Add(grupo);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Detail), new { id = grupo.GrupoId }, new ApiResponse<Grupo>(true, grupo));
    }

    // GET: api/Grupo/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Detail(int id)
    {
        var grupo = await _db.Grupos
            .Include(g => g.Area)
            .FirstOrDefaultAsync(g => g.GrupoId == id);
        if (grupo == null)
        {
            return NotFound(new ApiResponse<Grupo>(false, null, "Grupo no encontrado"));
        }
        var dto = new GrupoDetail(
            grupo.GrupoId,
            grupo.Rol,
            grupo.AreaId,
            grupo.Area.UnidadOrganizativaSap,
            grupo.Area.NombreGeneral,
            grupo.IdentificadorSAP,
            grupo.PersonasPorTurno,
            grupo.DuracionDeturno,
            grupo.LiderId,
            grupo.LiderSuplenteId
        );
        return Ok(new ApiResponse<GrupoDetail>(true, dto));
    }

    // GET: api/Grupo
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var grupos = await _db.Grupos
            .Include(g => g.Area)
            .Select(g => new GrupoDetail(
            g.GrupoId,
            g.Rol,
            g.AreaId,
            g.Area.UnidadOrganizativaSap,
            g.Area.NombreGeneral,
            g.IdentificadorSAP,
            g.PersonasPorTurno,
            g.DuracionDeturno,
            g.LiderId,
            g.LiderSuplenteId
        )).ToListAsync();
        return Ok(new ApiResponse<List<GrupoDetail>>(true, grupos));
    }

    // GET: api/Grupo/Area/{areaId}
    [HttpGet("Area/{areaId}")]
    public async Task<IActionResult> ListByArea(int areaId)
    {
        var grupos = await _db.Grupos
            .Include(g => g.Area)
            .Where(g => g.AreaId == areaId)
            .Select(g => new GrupoDetail(
                g.GrupoId,
                g.Rol,
                g.AreaId,
                g.Area.UnidadOrganizativaSap,
                g.Area.NombreGeneral,
                g.IdentificadorSAP,
                g.PersonasPorTurno,
                g.DuracionDeturno,
                g.LiderId,
                g.LiderSuplenteId
            )).ToListAsync();
        return Ok(new ApiResponse<List<GrupoDetail>>(true, grupos));
    }
}

public class GrupoDetail
{
    public int GrupoId { get; set; }
    public string Rol { get; set; }
    public int AreaId { get; set; }
    public string AreaUnidadOrganizativaSap { get; set; }
    public string AreaNombre { get; set; }
    public string IdentificadorSAP { get; set; }
    public int PersonasPorTurno { get; set; }
    public int DuracionDeturno { get; set; }
    public int? LiderId { get; set; }
    public int? LiderSuplenteId { get; set; }

    public GrupoDetail(int grupoId, string rol, int areaId, string areaUnidadOrganizativaSap, string areaNombre, string identificadorSAP, int personasPorTurno, int duracionDeturno, int? liderId, int? liderSuplenteId)
    {
        GrupoId = grupoId;
        Rol = rol;
        AreaId = areaId;
        AreaUnidadOrganizativaSap = areaUnidadOrganizativaSap;
        AreaNombre = areaNombre;
        IdentificadorSAP = identificadorSAP;
        PersonasPorTurno = personasPorTurno;
        DuracionDeturno = duracionDeturno;
        LiderId = liderId;
        LiderSuplenteId = liderSuplenteId;
    }
}
