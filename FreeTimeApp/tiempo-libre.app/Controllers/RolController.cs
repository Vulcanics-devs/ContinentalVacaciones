using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using tiempo_libre.Models;
using tiempo_libre.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace tiempo_libre.app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Solo usuarios autenticados
    public class RolController : ControllerBase
    {
        private readonly FreeTimeDbContext _dbContext;

        public RolController(FreeTimeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // EP: Listar todos los roles
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _dbContext.Roles
                .Select(r => new RolListDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Abreviation = r.Abreviation
                })
                .ToListAsync();
            return Ok(new ApiResponse<List<RolListDto>>(true, roles));
        }

        // EP: Editar un rol
        [HttpPatch("{id}")]
        [Authorize(Roles = "SuperUsuario")]
        public async Task<IActionResult> EditRol(int id, [FromBody] EditRolRequestDto request)
        {
            var rol = await _dbContext.Roles.FindAsync(id);
            if (rol == null)
                return NotFound(new ApiResponse<object>(false, null, "Rol no encontrado"));

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Description) || string.IsNullOrWhiteSpace(request.Abreviation))
                return BadRequest(new ApiResponse<object>(false, null, "Todos los campos son requeridos"));
            
            var rolWithSameName = await _dbContext.Roles
                .FirstOrDefaultAsync(r => r.Name == request.Name && r.Id != id);
            if (rolWithSameName != null)
                return Conflict(new ApiResponse<object>(false, null, "Ya existe un rol con ese nombre"));

            rol.Name = request.Name;
            rol.Description = request.Description;
            rol.Abreviation = request.Abreviation;
            await _dbContext.SaveChangesAsync();

            var updatedRol = new RolListDto
            {
                Id = rol.Id,
                Name = rol.Name,
                Description = rol.Description,
                Abreviation = rol.Abreviation
            };
            return Ok(new ApiResponse<RolListDto>(true, updatedRol));
        }
    }
}
