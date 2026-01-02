
using System;
using System.Linq;
using System.Threading.Tasks;
using tiempo_libre.Models;
// PasswordHasher es global, no requiere using especial
using Microsoft.EntityFrameworkCore;

public class GenericUsersGenerator
{
	private readonly FreeTimeDbContext _freeTimeDb;
	private readonly Microsoft.Extensions.Logging.ILogger<GenericUsersGenerator> _logger;

	public GenericUsersGenerator(FreeTimeDbContext freeTimeDb, Microsoft.Extensions.Logging.ILogger<GenericUsersGenerator> logger)
	{
		_freeTimeDb = freeTimeDb;
		_logger = logger;
	}

	public async Task<int> GenerateUsersFromEmpleadosAsync()
	{
		_logger.LogInformation("Inicio de generación de usuarios desde empleados");
		var empleados = await _freeTimeDb.Empleados.ToListAsync();
		_logger.LogInformation("Empleados encontrados: {Count}", empleados.Count);
		var rol = await _freeTimeDb.Roles.FirstOrDefaultAsync(r => r.Id == 2);
		if (rol == null)
		{
			_logger.LogError("Rol de Empleado Sindicalizado no encontrado");
			throw new Exception("Rol de Empleado Sindicalizado no encontrado");
		}

		int createdCount = 0;
		foreach (var empleado in empleados)
		{
			var nominaStr = empleado.Nomina.ToString();
			if (await _freeTimeDb.Users.AnyAsync(u => u.Username == nominaStr))
			{
				_logger.LogInformation("Usuario ya existe para la nómina: {Nomina}", nominaStr);
				continue;
			}

			var salt = Guid.NewGuid().ToString();
			var hash = PasswordHasher.HashPassword(nominaStr, salt);

			var fechaIngreso = empleado.FechaAlta.HasValue ? empleado.FechaAlta.Value : (DateOnly?)null;

			var nombreAreaUpper = empleado.UnidadOrganizativa?.ToUpper() ?? "";
			var encargadoUpper = empleado.EncargadoRegistro?.ToUpper() ?? "";
			
			// Buscar área por UnidadOrganizativaSap y EncargadoRegistro
			var area = await _freeTimeDb.Areas.FirstOrDefaultAsync(a => 
				a.UnidadOrganizativaSap.ToUpper() == nombreAreaUpper && 
				(a.EncargadoRegistro == null || a.EncargadoRegistro.ToUpper() == encargadoUpper));
			
			// Si no se encuentra el área, se registra un warning y se continúa con el siguiente empleado
			if (area == null)
			{
				_logger.LogWarning("No se encontró área para el empleado: {Nomina} - {Nombre} - UnidadOrganizativa: {UnidadOrganizativa} - EncargadoRegistro: {EncargadoRegistro}", 
					nominaStr, empleado.Nombre, empleado.UnidadOrganizativa, empleado.EncargadoRegistro);
				continue;
			}

			var nombreRolUpper = empleado.Rol?.ToUpper() ?? "";
			var grupo = await _freeTimeDb.Grupos.FirstOrDefaultAsync(g => g.Rol.ToUpper() == nombreRolUpper && g.AreaId == area.AreaId);

			if (grupo == null)
			{
				_logger.LogWarning("No se encontró grupo para el empleado: {Nomina} - {Nombre}", nominaStr, empleado.Nombre);
				continue;
			}

			var user = new User
			{
				Username = nominaStr,
				PasswordSalt = salt,
				PasswordHash = hash,
				FullName = empleado.Nombre,
				Roles = new List<Rol> { rol },
				AreaId = area?.AreaId ?? 0,
				GrupoId = grupo?.GrupoId ?? 0,
				FechaIngreso = fechaIngreso,
				Nomina = empleado.Nomina,
				CentroCoste = empleado.CentroCoste,
				Posicion = empleado.Posicion
			};
			_freeTimeDb.Users.Add(user);
			createdCount++;
			_logger.LogInformation("Usuario generado para nómina: {Nomina}", nominaStr);
		}
		await _freeTimeDb.SaveChangesAsync();
		_logger.LogInformation("Usuarios generados: {CreatedCount}", createdCount);
		return createdCount;
	}
}
