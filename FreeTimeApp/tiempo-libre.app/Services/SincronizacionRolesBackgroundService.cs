using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using tiempo_libre.DTOs;
using tiempo_libre.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace tiempo_libre.Services
{
    public class SincronizacionRolesBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SincronizacionRolesBackgroundService> _logger;
        private readonly TimeSpan _intervalo = TimeSpan.FromHours(6); // Se ejecuta cada 6 horas

        public SincronizacionRolesBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<SincronizacionRolesBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de sincronización de roles iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SincronizarRoles();
                    _logger.LogInformation($"Próxima sincronización en {_intervalo.TotalHours} horas");
                    await Task.Delay(_intervalo, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el servicio de sincronización de roles");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        private async Task SincronizarRoles()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<FreeTimeDbContext>();

                try
                {
                    int registrosActualizados = 0;

                    var rolesEmpleadosSAP = await context.RolesEmpleadosSAP
                        .Where(r => !string.IsNullOrEmpty(r.Regla))
                        .ToListAsync();

                    foreach (var rolSAP in rolesEmpleadosSAP)
                    {
                        var empleado = await context.Empleados
                            .FirstOrDefaultAsync(e => e.Nomina == rolSAP.Nomina);

                        if (empleado != null && empleado.Rol != rolSAP.Regla)
                        {
                            empleado.Rol = rolSAP.Regla;
                            registrosActualizados++;
                        }
                    }

                    if (registrosActualizados > 0)
                    {
                        await context.SaveChangesAsync();
                        _logger.LogInformation($"Sincronización completada. {registrosActualizados} roles actualizados.");
                    }
                    else
                    {
                        _logger.LogInformation("Sincronización completada. No hay cambios que aplicar.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al sincronizar roles desde RolesEmpleadosSAP");
                }
            }
        }
    }
}