using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using tiempo_libre.DTOs;
using tiempo_libre.Models;

namespace tiempo_libre.Services
{
	public class SincronizacionRolesService
	{
		private readonly FreeTimeDbContext _context;
		private readonly ILogger<SincronizacionRolesService> _logger;

		public SincronizacionRolesService(FreeTimeDbContext context, ILogger<SincronizacionRolesService> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task<int> SincronizarRolesDesdeRegla()
		{
			int registrosActualizados = 0;

			var rolesEmpleadosSAP = await _context.RolesEmpleadosSAP
				.Where(r => !string.IsNullOrEmpty(r.Regla))
				.ToListAsync();

			foreach (var rolSAP in rolesEmpleadosSAP)
			{
				// Actualizar en tabla Empleados
				var empleado = await _context.Empleados
					.FirstOrDefaultAsync(e => e.Nomina == rolSAP.Nomina);

				if (empleado != null && empleado.Rol != rolSAP.Regla)
				{
					empleado.Rol = rolSAP.Regla;
					registrosActualizados++;
				}

				// Actualizar en tabla Users si existe
				var user = await _context.Users
					.FirstOrDefaultAsync(u => u.Nomina == rolSAP.Nomina);

				// Nota: Users no tiene campo Rol directo, solo tiene relación con tabla Roles
				// Si necesitas actualizar algo específico en Users, agrégalo aquí
			}

			await _context.SaveChangesAsync();
			_logger.LogInformation($"Sincronización completada. {registrosActualizados} registros actualizados.");

			return registrosActualizados;
		}
	}
}