using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using tiempo_libre.Models;
using tiempo_libre.DTOs;
using tiempo_libre.Models.Enums;
using tiempo_libre.Logic;

namespace tiempo_libre.Services
{
    public class GeneraReservacionTurnosService
    {
        private readonly FreeTimeDbContext _db;
        private readonly ILogger<GeneraReservacionTurnosService> _logger;

        public GeneraReservacionTurnosService(FreeTimeDbContext db, ILogger<GeneraReservacionTurnosService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public virtual async Task<ApiResponse<string>> EjecutarAsync(AsignacionDeVacacionesRequest request, int userId)
        {
            if (request.FechaInicio >= request.FechaFinal)
            {
                return new ApiResponse<string>(false, null, "La FechaInicio debe ser menor a la FechaFinal.");
            }
            if (request.FechaInicioReservaciones >= request.FechaInicio)
            {
                return new ApiResponse<string>(false, null, "La FechaInicioReservaciones debe ser menor a la FechaInicio.");
            }

            try
            {
                var anio = request.FechaInicio.Year;
                var programacion = await _db.ProgramacionesAnuales
                    .FirstOrDefaultAsync(p => p.Anio == anio);

                if (programacion == null)
                {
                    programacion = new ProgramacionesAnuales
                    {
                        IdSuperUser = userId,
                        Anio = anio,
                        FechaInicia = request.FechaInicio,
                        FechaTermina = request.FechaFinal,
                        FechaInicioReservaTurnos = request.FechaInicioReservaciones,
                        Estatus = EstatusProgramacionAnualEnum.EnProceso,
                        BorradoLogico = false
                    };
                    _db.ProgramacionesAnuales.Add(programacion);
                }
                else if (programacion.Estatus == EstatusProgramacionAnualEnum.Cerrada)
                {
                    programacion.Estatus = EstatusProgramacionAnualEnum.EnProceso;
                    programacion.FechaInicia = request.FechaInicio;
                    programacion.FechaTermina = request.FechaFinal;
                    programacion.FechaInicioReservaTurnos = request.FechaInicioReservaciones;
                    _db.ProgramacionesAnuales.Update(programacion);
                }
                else if (programacion.Estatus == EstatusProgramacionAnualEnum.EnProceso)
                {
                    programacion.FechaInicia = request.FechaInicio;
                    programacion.FechaTermina = request.FechaFinal;
                    programacion.FechaInicioReservaTurnos = request.FechaInicioReservaciones;
                    _db.ProgramacionesAnuales.Update(programacion);
                }
                else if (programacion.Estatus == EstatusProgramacionAnualEnum.Pendiente)
                {
                    programacion.Estatus = EstatusProgramacionAnualEnum.EnProceso;
                    programacion.FechaInicia = request.FechaInicio;
                    programacion.FechaTermina = request.FechaFinal;
                    programacion.FechaInicioReservaTurnos = request.FechaInicioReservaciones;
                    _db.ProgramacionesAnuales.Update(programacion);
                }

                await _db.SaveChangesAsync();

                // Ejecutar la generación de calendarios de forma asíncrona
                _ = Task.Run(async () =>
                {
                    var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                    var logger = loggerFactory.CreateLogger<EmployeesCalendarsGenerator>();
                    var generator = new EmployeesCalendarsGenerator(_db, logger, request.FechaInicio, request.FechaFinal);
                    await generator.GenerateEmployeesCalendarsAsync(request.FechaInicio, request.FechaFinal);
                });

                return new ApiResponse<string>(true, "La generación de calendarios y reservaciones se ha iniciado correctamente.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ejecutar la generación de reservaciones y calendarios.");
                return new ApiResponse<string>(false, null, "Ocurrió un error al ejecutar la operación.");
            }
        }
    }
}
