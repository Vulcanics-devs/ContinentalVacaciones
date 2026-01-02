using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace tiempo_libre.Services
{
    public class ActualizacionEstadosBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ActualizacionEstadosBackgroundService> _logger;

        // Ejecutar cada 6 horas
        private readonly TimeSpan _periodo = TimeSpan.FromHours(6);

        public ActualizacionEstadosBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ActualizacionEstadosBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de actualización de estados de bloques iniciado. Ejecutará cada {Periodo} horas",
                _periodo.TotalHours);

            // Ejecutar inmediatamente al inicio
            await EjecutarActualizacionAsync();

            // Luego ejecutar periódicamente
            using var timer = new PeriodicTimer(_periodo);

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await EjecutarActualizacionAsync();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Servicio de actualización de estados cancelado");
            }
        }

        private async Task EjecutarActualizacionAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando actualización automática programada de estados de bloques");

                using var scope = _serviceScopeFactory.CreateScope();
                var estadosService = scope.ServiceProvider.GetRequiredService<EstadosBloquesService>();

                await estadosService.ActualizarEstadosAutomaticamenteAsync();

                _logger.LogInformation("Actualización automática de estados completada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la actualización automática programada de estados");
                // No propagar la excepción para que el servicio continúe funcionando
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deteniendo servicio de actualización de estados de bloques");
            await base.StopAsync(cancellationToken);
        }
    }
}