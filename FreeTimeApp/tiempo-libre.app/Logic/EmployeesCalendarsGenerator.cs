
using System;
using System.Linq;
using System.Threading.Tasks;
using tiempo_libre.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using tiempo_libre.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using tiempo_libre.Logic;



public class EmployeesCalendarsGenerator
{
	private readonly FreeTimeDbContext _freeTimeDb;
	private readonly Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator> _logger;
	private readonly CalculosSobreManning calculosSobreManning;

	private readonly List<DiasInhabiles> _todoDiasInabiles;
	private readonly List<IncidenciaOPermiso> _todosPermisosEIncapacidades;

    public EmployeesCalendarsGenerator(FreeTimeDbContext freeTimeDb, Microsoft.Extensions.Logging.ILogger<EmployeesCalendarsGenerator> logger, DateTime fechaInicio, DateTime fechaFinal, CalculosSobreManning? calculadoraMannning = null)
    {
        _freeTimeDb = freeTimeDb;
        _logger = logger;

        this._todoDiasInabiles = this.GetDiasInhabilesDentroDelRango(fechaInicio, fechaFinal);
        _logger.LogInformation("Días inhábiles encontrados: {Count}", _todoDiasInabiles.Count);

        this._todosPermisosEIncapacidades = this.GetPermisosEincapacidadesDentroDelRango(fechaInicio, fechaFinal);
        _logger.LogInformation("Permisos e Incapacidades encontrados: {Count}", _todosPermisosEIncapacidades.Count);
        this.calculosSobreManning = calculadoraMannning ?? new CalculosSobreManning(freeTimeDb);
    }

    public async Task<int> GenerateEmployeesCalendarsAsync(DateTime fechaInicio, DateTime fechaFinal)
	{
		_logger.LogInformation("Inicio de generación de calendarios para los Usuario de Rol 'Empleados Sindicalizados'...");
		this.CalculaAntiguedadParaTodosEmpleados();

		var areas = this._freeTimeDb.Areas.OrderBy(a => a.AreaId).ToList();
		_logger.LogInformation("Áreas encontradas: {Count}", areas.Count);

		foreach (var area in areas)
		{
			var usersInArea = this.GetUsersByArea(area);
			this.GeneraCalendarioPara(usersInArea, fechaInicio, fechaFinal);
			this.AsignaVacacionesAEmpleadosPorArea(area);
		}


		await _freeTimeDb.SaveChangesAsync();
		_logger.LogInformation("Finalizada la generación de calendarios para los Usuario de Rol 'Empleados Sindicalizados'.");
		return 0;
	}

	#region Métodos Privados para la Asignación de Vacaciones

	public void AsignaVacacionesAEmpleadosPorArea(Area area)
	{
		var empleados = new List<User>();
		var grupos = this._freeTimeDb.Grupos.Where(g => g.AreaId == area.AreaId).OrderBy(g => g.GrupoId).ToList();
		_logger.LogInformation("Grupos encontrados en el área {AreaId} - {AreaName}: {Count}", area.AreaId, area.NombreGeneral, grupos.Count);

		foreach (var grupo in grupos)
		{
			empleados = this.DameEmpleadosSindicalizadosConVacacionesPorAsignarOrdenadosPorAntiguedadYNominaPorGrupo(grupo.GrupoId);
			this.AsignaVacacionesAEmpleadosSiCorrespondeIterandoSobreLosDiasDeLaProgramacionAnualActual(empleados);
		}
	}

	public void AsignaVacacionesAEmpleadosSiCorrespondeIterandoSobreLosDiasDeLaProgramacionAnualActual(List<User> empleadosSindicalizados)
	{
		if (empleadosSindicalizados == null || empleadosSindicalizados.Count == 0)
		{
			_logger.LogWarning("No hay empleados sindicalizados con vacaciones asignadas para procesar.");
			return;
		}

		// Obtener la programación anual actual
		var programacionAnualActual = this._freeTimeDb.ProgramacionesAnuales
			.FirstOrDefault(p => p.Estatus == EstatusProgramacionAnualEnum.EnProceso);

		if (programacionAnualActual == null)
		{
			_logger.LogWarning("No hay una Programación Anual activa. No se pueden asignar vacaciones automáticamente.");
			return;
		}

		_logger.LogInformation("Empleados sindicalizados con vacaciones asignadas encontrados: {Count}", empleadosSindicalizados.Count);

		var indEmpleadoDelGrupo = 0;
		var diasRecorridos = 0;
		var fechaInicioDeAsignacion = programacionAnualActual.FechaInicia.AddDays(7); // Start on day 8 (day 1 + 7 days)
		var fechaFinalDeAsignacion = programacionAnualActual.FechaTermina.AddDays(-21);
		var iteraciones = 0;
		while (indEmpleadoDelGrupo < empleadosSindicalizados.Count)
		{
			for (int indDia = 0; indDia <= (fechaFinalDeAsignacion - fechaInicioDeAsignacion).Days; indDia++)
			{
				var fechaObjetivo = fechaInicioDeAsignacion.AddDays(indDia);

				// Skip weekends (Saturday = 6, Sunday = 0)
				if (fechaObjetivo.DayOfWeek == DayOfWeek.Saturday || fechaObjetivo.DayOfWeek == DayOfWeek.Sunday)
				{
					_logger.LogInformation("Skipping weekend day: {Fecha}", fechaObjetivo.ToShortDateString());
					continue;
				}

				_logger.LogInformation("Procesando fecha: {Fecha}", fechaObjetivo.ToShortDateString());
				try
				{
					diasRecorridos = this.AsignaVacacionesIterandoSobreLosDiasHastaAsginarTodos(empleadosSindicalizados[indEmpleadoDelGrupo], fechaObjetivo, fechaFinalDeAsignacion);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Error al asignar vacaciones al empleado {UserId} - {UserName}.", empleadosSindicalizados[indEmpleadoDelGrupo].Id, empleadosSindicalizados[indEmpleadoDelGrupo].FullName);
				}
				indEmpleadoDelGrupo++;
				if (diasRecorridos > 0)
				{
					_logger.LogInformation("Se asignaron {DiasAsignados} días de vacaciones al empleado {UserId} - {UserName}.", diasRecorridos, empleadosSindicalizados[indEmpleadoDelGrupo-1].Id, empleadosSindicalizados[indEmpleadoDelGrupo-1].FullName);
					indDia += (diasRecorridos - 1); // Saltar los días ya asignados (-1 porque el for incrementa indDia)
				}
				diasRecorridos = 0;
				if (indEmpleadoDelGrupo >= empleadosSindicalizados.Count)
				{
					break;
				}
			}
			if (indEmpleadoDelGrupo < empleadosSindicalizados.Count)
			{
				_logger.LogInformation("Se han recorrido todos los días del rango de asignación, pero aún quedan {EmpleadosRestantes} empleados con vacaciones por asignar. Se reiniciará el ciclo de días.", empleadosSindicalizados.Count - indEmpleadoDelGrupo);
			}
			iteraciones++;
			if (iteraciones > empleadosSindicalizados.Count)
			{
				_logger.LogWarning("Se han realizado más de 10 iteraciones sin completar la asignación de vacaciones. Se detendrá el proceso para evitar un ciclo infinito.");
				break;
			}
		}
	}

	public virtual int AsignaVacacionesIterandoSobreLosDiasHastaAsginarTodos(User empleadoSindicalizado, DateTime fechaInicio, DateTime fechaFinal)
	{
		if (empleadoSindicalizado == null)
		{
			_logger.LogWarning("El empleado sindicalizado es nulo. No se pueden asignar vacaciones.");
			return 0;
		}

		var diasRecorridos = 0;
		var diasAsignados = 0;
		var fechaActual = fechaInicio;
		var diasDeVacaciones = empleadoSindicalizado.VacacionesPorAntiguedad != null ? empleadoSindicalizado.VacacionesPorAntiguedad.DiasParaAsignarAutomaticamente : 0;

		if (diasDeVacaciones <= 0)
		{
			_logger.LogWarning("El empleado {UserId} - {UserName} no tiene días de vacaciones para asignar.", empleadoSindicalizado.Id, empleadoSindicalizado.FullName);
			return diasRecorridos;
		}

		_logger.LogInformation("Iniciando asignación de {DiasDeVacaciones} días de vacaciones para el empleado {UserId} - {UserName} desde {FechaInicio} hasta {FechaFinal}.", diasDeVacaciones, empleadoSindicalizado.Id, empleadoSindicalizado.FullName, fechaInicio.ToShortDateString(), fechaFinal.ToShortDateString());

		while (diasAsignados < diasDeVacaciones && fechaActual <= fechaFinal)
		{
			// Validar el Porcentaje de Ausencia antes de asignar el día de vacaciones
			var diaCalendarioDelEmpleado = this.DameDiaDeCalendarioParaElEmpleadoEnLaFecha(empleadoSindicalizado.Id, fechaActual);
			if (diaCalendarioDelEmpleado == null)
			{
				_logger.LogWarning("No se encontró el día de calendario para el empleado {UserId} - {UserName} en la fecha {Fecha}. Se omitirá esta fecha.", empleadoSindicalizado.Id, empleadoSindicalizado.FullName, fechaActual.ToShortDateString());
				fechaActual = fechaActual.AddDays(1);
				diasRecorridos++;
				continue;
			}
			if (diaCalendarioDelEmpleado.TipoActividadDelDia != TipoActividadDelDiaEnum.Laboral)
			{
				_logger.LogInformation("El día {Fecha} para el empleado {UserId} - {UserName} no es un día laboral (es {TipoActividad}), por lo que no se asignarán vacaciones.", fechaActual.ToShortDateString(), empleadoSindicalizado.Id, empleadoSindicalizado.FullName, diaCalendarioDelEmpleado.TipoActividadDelDia);
				fechaActual = fechaActual.AddDays(1);
				diasRecorridos++;
				continue;
			}
			if (!this.calculosSobreManning.ElPorcentajeDeAusenciaEstaDentroDelRango(empleadoSindicalizado.GrupoId ?? 0, fechaActual))
			{
				_logger.LogWarning("No se puede asignar el día {Fecha} de vacaciones al empleado {UserId} - {UserName} porque el porcentaje de ausencia para el grupo {GrupoId} excede el máximo permitido.", fechaActual.ToShortDateString(), empleadoSindicalizado.Id, empleadoSindicalizado.FullName, empleadoSindicalizado.GrupoId);
				fechaActual = fechaActual.AddDays(1);
				diasRecorridos++;
				continue;
			}
			try
			{
				if (diaCalendarioDelEmpleado.TipoActividadDelDia == TipoActividadDelDiaEnum.Laboral &&
					this.CambiaDiaCalendarioEmpleadoDeLaboralAVacaciones(diaCalendarioDelEmpleado, empleadoSindicalizado))
				{
					_logger.LogInformation("Día {Fecha} asignado como día de vacaciones para el empleado {UserId} - {UserName}.", fechaActual.ToShortDateString(), empleadoSindicalizado.Id, empleadoSindicalizado.FullName);
					// Asignar el día de vacaciones
					diasRecorridos++;
					diasAsignados++;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al cambiar día {Fecha} de vacaciones al empleado {UserId} - {UserName}.", fechaActual.ToShortDateString(), empleadoSindicalizado.Id, empleadoSindicalizado.FullName);
			}
			fechaActual = fechaActual.AddDays(1);
		}
		return diasRecorridos;
	}

	public virtual bool CambiaDiaCalendarioEmpleadoDeLaboralAVacaciones(DiasCalendarioEmpleado diaCalendario, User empleado)
	{
		try
		{
			if (diaCalendario.TipoActividadDelDia != TipoActividadDelDiaEnum.Laboral)
			{
				_logger.LogWarning("El día {Fecha} para el empleado {UserId} - {UserName} no es un día laboral (es {TipoActividad}), por lo que no se puede cambiar a vacaciones.", diaCalendario.FechaDelDia.ToShortDateString(), diaCalendario.IdUsuarioEmpleadoSindicalizado, diaCalendario.UsuarioEmpleadoSindicalizado?.FullName, diaCalendario.TipoActividadDelDia);
				return false;
			}

			var diaDeVacaciones = new Vacaciones
			{
				NominaEmpleado = diaCalendario.NominaEmpleado,
				Fecha = diaCalendario.FechaDelDia,
				AnioFecha = diaCalendario.AnioFecha,
				MesFecha = diaCalendario.MesFecha,
				DiaFecha = diaCalendario.DiaFecha,
				TurnoCubria = diaCalendario.TurnoXRolSemanalXRegla?.Turno ?? TurnosEnum.Matutino,
				ActividadDelDia = TipoActividadDelDiaEnum.VacacionesAutoAsignadasPorApp,
				IdGrupo = diaCalendario.IdGrupo,
				IdTurnoXRolSemanalXRegla = diaCalendario.IdTurnoXRolSemanalXRegla,
				AsignadaPorJefe = true,
				CreatedAt = DateOnly.FromDateTime(DateTime.Now),
				IdUsuarioEmpleadoSindicalizado = diaCalendario.IdUsuarioEmpleadoSindicalizado,
				UsuarioEmpleadoSindicalizado = diaCalendario.UsuarioEmpleadoSindicalizado,
				Grupo = diaCalendario.Grupo,
				TurnoXRolSemanalXRegla = diaCalendario.TurnoXRolSemanalXRegla
			};
			_freeTimeDb.Vacaciones.Add(diaDeVacaciones);

			diaCalendario.TipoActividadDelDia = TipoActividadDelDiaEnum.VacacionesAutoAsignadasPorApp;
			diaCalendario.EsDiaDeVacaciones = true;
			diaCalendario.EsDiaLaboral = false;
			diaCalendario.UpdatedAt = DateTime.UtcNow;
			diaCalendario.IdVacaciones = diaDeVacaciones.Id;

			_freeTimeDb.DiasCalendarioEmpleado.Update(diaCalendario);

			_logger.LogInformation("Se cambió el día {Fecha} del empleado {UserId} - {UserName} de laboral a vacaciones.", diaCalendario.FechaDelDia.ToShortDateString(), diaCalendario.IdUsuarioEmpleadoSindicalizado, diaCalendario.UsuarioEmpleadoSindicalizado?.FullName);
			empleado.DiasDeVacacionesAsignados = (empleado.DiasDeVacacionesAsignados ?? 0) + 1;
			_freeTimeDb.Users.Update(empleado);
			_logger.LogInformation("Se actualizó el contador de días de vacaciones asignados para el empleado {UserId} - {UserName}. Días asignados: {DiasAsignados}.", empleado.Id, empleado.FullName, empleado.DiasDeVacacionesAsignados);

			_freeTimeDb.SaveChanges();
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error al cambiar el día {Fecha} del empleado {UserId} - {UserName} de laboral a vacaciones.", diaCalendario.FechaDelDia.ToShortDateString(), diaCalendario.IdUsuarioEmpleadoSindicalizado, diaCalendario.UsuarioEmpleadoSindicalizado?.FullName);
			return false;
		}
	}

	public DiasCalendarioEmpleado? DameDiaDeCalendarioParaElEmpleadoEnLaFecha(int idUsuarioEmpleadoSindicalizado, DateTime fecha)
	{
		var Anio = fecha.Year;
		var Mes = fecha.Month;
		var Dia = fecha.Day;

		var diaCalendario = this._freeTimeDb.DiasCalendarioEmpleado
			.Include(d => d.Vacaciones)
			.Include(d => d.IncidenciaOPermiso)
			.Include(d => d.DiaInhabil)
			.FirstOrDefault(d => d.IdUsuarioEmpleadoSindicalizado == idUsuarioEmpleadoSindicalizado && d.AnioFecha == Anio && d.MesFecha == Mes && d.DiaFecha == Dia);

		return diaCalendario;
	}

	public List<User> DameEmpleadosSindicalizadosConVacacionesPorAsignarOrdenadosPorAntiguedadYNominaPorGrupo(int GrupoId)
	{
		_logger.LogInformation("Obteniendo empleados sindicalizados con vacaciones asignadas, ordenados por antigüedad y nómina...");
		var empleados = _freeTimeDb.Users
			.Include(u => u.VacacionesPorAntiguedad)
			.Where(u => u.Roles.Any(r => r.Id == (int)RolEnum.Empleado_Sindicalizado) && u.GrupoId == GrupoId && u.VacacionesPorAntiguedad != null && u.VacacionesPorAntiguedad.DiasParaAsignarAutomaticamente > 0)
			.OrderByDescending(u => u.AntiguedadEnDias.HasValue ? u.AntiguedadEnDias.Value : int.MinValue) // Empleados sin antigüedad al final
			.ThenBy(u => u.Nomina)
			.ToList();
		_logger.LogInformation("Empleados encontrados: {Count}", empleados.Count);
		return empleados;
	}

	#endregion

	public void GeneraCalendarioPara(List<User> users, DateTime fechaInicio, DateTime fechaFinal)
	{
		_logger.LogInformation("Generando calendarios para {Count} usuarios en el área...", users.Count);

		var diasInabilesPorContinental = this._todoDiasInabiles.Where(d => d.TipoActividadDelDia == TipoActividadDelDiaEnum.InhabilPorContinental).ToList();
		_logger.LogInformation("Días inhábiles por Continental encontrados: {Count}", diasInabilesPorContinental.Count);

		var diasInabilesPorLey = this._todoDiasInabiles.Where(d => d.TipoActividadDelDia == TipoActividadDelDiaEnum.InhabilPorLey).ToList();
		_logger.LogInformation("Días inhábiles por Ley encontrados: {Count}", diasInabilesPorLey.Count);

		foreach (var user in users)
		{
			GeneraCalendarioPara(user, fechaInicio, fechaFinal);
		}
	}

	public void GeneraCalendarioPara(User user, DateTime fechaInicio, DateTime fechaFinal)
	{
		_logger.LogInformation("Generando calendario para el usuario: {UserId} - {UserName}", user.Id, user.FullName);
		if (user.Roles == null || !user.Roles.Any(r => r.Id == (int)RolEnum.Empleado_Sindicalizado))
		{
			_logger.LogWarning("El usuario {UserId} - {UserName} no tiene el rol 'Empleado Sindicalizado', se omitirá la generación de calendario.", user.Id, user.FullName);
			return;
		}

		/*if (user.VacacionesPorAntiguedad == null)
		{
			_logger.LogWarning("El usuario {UserId} - {UserName} no tiene una regla de VacacionesPorAntiguedad asignada, se omitirá la generación de calendario.", user.Id, user.FullName);
			return;
		}*/

		var datosInicialesTemp = this.ObtenDatosDeInicioPara(user);
		if (datosInicialesTemp == null)
		{
			_logger.LogWarning("No se pudieron obtener los datos iniciales para el usuario {UserId} - {UserName}, se omitirá la generación de calendario.", user.Id, user.FullName);
			return;
		}

		var permisosEIncapacidadesDelEmpleado = this.GetIncidenciaOPermisoDentroDelRangoPorEmpleado(user.Nomina ?? 0, fechaInicio, fechaFinal);
		_logger.LogInformation("Permisos e Incapacidades encontrados para el usuario {UserId} - {UserName}: {Count}", user.Id, user.FullName, permisosEIncapacidadesDelEmpleado.Count);

		var diasInabilesPorContinental = this._todoDiasInabiles.Where(d => d.TipoActividadDelDia == TipoActividadDelDiaEnum.InhabilPorContinental).ToList();
		var diasInabilesPorLey = this._todoDiasInabiles.Where(d => d.TipoActividadDelDia == TipoActividadDelDiaEnum.InhabilPorLey).ToList();

		foreach (var dia in Enumerable.Range(0, (fechaFinal - fechaInicio).Days + 1))
		{
			var fechaObjetivo = fechaInicio.AddDays(dia);
			var fechaObjetivoDateOnly = DateOnly.FromDateTime(fechaObjetivo);

			var diaInhabilPorContinental = diasInabilesPorContinental.FirstOrDefault(d => d.Fecha == fechaObjetivoDateOnly);
			var diaInhabilPorLey = diasInabilesPorLey.FirstOrDefault(d => d.Fecha == fechaObjetivoDateOnly);
			var permisoOIncapacidad = permisosEIncapacidadesDelEmpleado.FirstOrDefault(p => p.Fecha == fechaObjetivoDateOnly);
			var diaDeVacaciones = this._freeTimeDb.Vacaciones.FirstOrDefault(v => v.NominaEmpleado == user.Nomina && v.Fecha == fechaObjetivoDateOnly);

			var nuevoTurnoRolSemanal = this.ObtenTurnoParaLaFecha(fechaInicio, fechaObjetivo, datosInicialesTemp.Regla, datosInicialesTemp.turnoRolSemanalInicial);
			if (nuevoTurnoRolSemanal == null && diaInhabilPorContinental != null &&
									diaInhabilPorLey != null &&
									permisoOIncapacidad != null)
			{
				_logger.LogWarning("No se pudo obtener el turno para la fecha {Fecha} del usuario {UserId} - {UserName}, se omitirá ese día en el calendario.", fechaObjetivo.ToShortDateString(), user.Id, user.FullName);
				continue;
			}
			var diaDeCalendarioEmpleado = this.CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos(datosInicialesTemp, fechaObjetivo, nuevoTurnoRolSemanal,
																									diaInhabilPorContinental, diaInhabilPorLey,
																									permisoOIncapacidad, diaDeVacaciones);
			this._freeTimeDb.DiasCalendarioEmpleado.Add(diaDeCalendarioEmpleado);
			_logger.LogInformation("Usuario: {UserId}, Fecha: {Fecha}, ActividadDelDia: {ActividadDelDia}, Turno: {Turno}, IdVacaciones: {IdVacaciones}, IdIncidenciaOPermiso: {IdIncidenciaOPermiso}, IdDiaInhabil: {IdDiaInhabil}",
									user.Id, fechaObjetivo.ToShortDateString(), diaDeCalendarioEmpleado.TipoActividadDelDia, diaDeCalendarioEmpleado.Turno,
									diaDeCalendarioEmpleado.IdVacaciones, diaDeCalendarioEmpleado.IdIncidenciaOPermiso, diaDeCalendarioEmpleado.IdDiaInhabil);
			if (diaDeCalendarioEmpleado != null)
			{
				this._freeTimeDb.SaveChanges();
			}
		}
		this._freeTimeDb.Users.Update(user);
		this._freeTimeDb.SaveChanges();
		if (datosInicialesTemp.VacacionesPorAntiguedad != null)
			_logger.LogInformation("Usuario: {UserId}, Días de Vacaciones a Asignar: {DiasParaAsignar}", user.Id, datosInicialesTemp.VacacionesPorAntiguedad.DiasParaAsignarAutomaticamente);
		else
			_logger.LogInformation("Usuario: {UserId}, No tiene VacacionesPorAntiguedad asignada.", user.Id);
	}

	public DiasCalendarioEmpleado CreateDiaParaElEmpleadoBasadoEnDiasInhabilesOTurnos(DatosTempEmpleadoSindicalizado datosEmpleado,
																						DateTime fecha,
																						TurnoXRolSemanalXRegla? turnoRolSemanal,
																						DiasInhabiles? diaInhabilPorContinental,
																						DiasInhabiles? diaInhabilPorLey,
																						IncidenciaOPermiso? permisoOIncapacidad,
																						Vacaciones? diaDeVacaciones)
	{
		var user = datosEmpleado.User;
		var tipoActividadDelDia = TipoActividadDelDiaEnum.Laboral;
		var turnoLaboral = TurnosEnum.Descanso;

		// Prioridad: InhabilPorLey > InhabilPorContinental > IncidenciaOPermiso > Vacaciones > TurnoXRolSemanalXRegla
		if (diaInhabilPorLey != null)
		{
			tipoActividadDelDia = TipoActividadDelDiaEnum.InhabilPorLey;
			datosEmpleado.agregaDiaDeInhabilPorLey(diaInhabilPorLey);
		}
		else if (diaInhabilPorContinental != null)
		{
			tipoActividadDelDia = TipoActividadDelDiaEnum.InhabilPorContinental;
			datosEmpleado.agregaDiaDeInhabilPorContinental(diaInhabilPorContinental);
		}
		else if (permisoOIncapacidad != null)
		{
			tipoActividadDelDia = TipoActividadDelDiaEnum.IncidenciaOPermiso;
			datosEmpleado.agregaPermisoOIncapacidad(permisoOIncapacidad);
		}
		else if (diaDeVacaciones != null)
		{
			tipoActividadDelDia = diaDeVacaciones.ActividadDelDia;
		}
		else if (turnoRolSemanal != null)
		{
			tipoActividadDelDia = turnoRolSemanal.ActividadDelDia;
			turnoLaboral = turnoRolSemanal.Turno ?? TurnosEnum.Descanso;
		}

		var diaDeCalendario = new DiasCalendarioEmpleado
		{
			CreatedAt = DateTime.Now,
			FechaDelDia = DateOnly.FromDateTime(fecha),
			AnioFecha = fecha.Year,
			MesFecha = fecha.Month,
			DiaFecha = fecha.Day,
			DiaDeLaSemana = (DiasDeLaSemanaEnum)fecha.DayOfWeek,
			TipoActividadDelDia = tipoActividadDelDia,
			Turno = turnoLaboral,
			TipoDeIncedencia = permisoOIncapacidad?.TiposDeIncedencia,
			IdProgramacionAnual = datosEmpleado.CalendarioEmpleado.IdProgramacionAnual ?? 0,
			IdCalendarioEmpleado = datosEmpleado.CalendarioEmpleado.Id,
			IdArea = user.AreaId ?? 0,
			IdGrupo = user.GrupoId ?? 0,
			IdRegla = turnoRolSemanal?.IdRegla ?? 0,
			IdRolSemanal = turnoRolSemanal?.IdRolSemanal ?? 0,
			IdTurnoXRolSemanalXRegla = turnoRolSemanal?.Id ?? 0,
			IdUsuarioEmpleadoSindicalizado = user.Id,
			NominaEmpleado = user.Nomina ?? 0,

			IdVacaciones = diaDeVacaciones?.Id,
			IdIncidenciaOPermiso = permisoOIncapacidad?.Id,
			IdDiaInhabil = diaInhabilPorLey?.Id ?? diaInhabilPorContinental?.Id ?? 0,
			DetallesDiaInhabil = diaInhabilPorLey?.Detalles ?? diaInhabilPorContinental?.Detalles,

			IdIntercambioDiaFestivoPorDescanso = null, // Los intermcambios se manejan en otro proceso
			IdReprogramacionDeVacaciones = null, // Las reprogramaciones se manejan en otro proceso

			EsDiaFestivo = tipoActividadDelDia == TipoActividadDelDiaEnum.InhabilPorLey,
			EsDiaDeDescanso = tipoActividadDelDia == TipoActividadDelDiaEnum.DescansoSemanal,
			EsDiaLaboral = tipoActividadDelDia == TipoActividadDelDiaEnum.Laboral,
			EsDiaInhabil = tipoActividadDelDia == TipoActividadDelDiaEnum.InhabilPorLey,
			EsDiaDeVacaciones = tipoActividadDelDia == TipoActividadDelDiaEnum.VacacionesAutoAsignadasPorApp || tipoActividadDelDia == TipoActividadDelDiaEnum.VacacionesSeleccionadasPorEmp || tipoActividadDelDia == TipoActividadDelDiaEnum.InhabilPorContinental,
			EsDiaDePermiso = tipoActividadDelDia == TipoActividadDelDiaEnum.IncidenciaOPermiso,

			EsDiaReprogramado = false,
			EsDiaIntercambiado = false,

			UsuarioEmpleadoSindicalizado = user,
			Regla = turnoRolSemanal?.Regla ?? null!,
			Grupo = datosEmpleado.Grupo ?? null!,
			TurnoXRolSemanalXRegla = turnoRolSemanal ?? null!,
			Vacaciones = diaDeVacaciones,
			IncidenciaOPermiso = permisoOIncapacidad
		};

		return diaDeCalendario;
	}

	public List<DiasInhabiles> GetDiasInhabilesDentroDelRango(DateTime fechaInicio, DateTime fechaFinal)
	{
		var fechaInicialDateOnly = DateOnly.FromDateTime(fechaInicio);
		var fechaFinalDateOnly = DateOnly.FromDateTime(fechaFinal);
		return this._freeTimeDb.DiasInhabiles
			.Where(d => d.Fecha >= fechaInicialDateOnly && d.Fecha <= fechaFinalDateOnly)
			.ToList();
	}

	public List<IncidenciaOPermiso> GetPermisosEincapacidadesDentroDelRango(DateTime fechaInicio, DateTime fechaFinal)
	{
		var fechaInicialDateOnly = DateOnly.FromDateTime(fechaInicio);
		var fechaFinalDateOnly = DateOnly.FromDateTime(fechaFinal);
		return this._freeTimeDb.IncidenciasOPermisos
			.Where(p => p.Fecha >= fechaInicialDateOnly && p.Fecha <= fechaFinalDateOnly)
			.ToList();
	}

	public List<IncidenciaOPermiso> GetIncidenciaOPermisoDentroDelRangoPorEmpleado(int nomina, DateTime fechaInicio, DateTime fechaFinal)
	{
		var fechaInicialDateOnly = DateOnly.FromDateTime(fechaInicio);
		var fechaFinalDateOnly = DateOnly.FromDateTime(fechaFinal);
		return this._freeTimeDb.IncidenciasOPermisos
			.Where(p => p.Fecha >= fechaInicialDateOnly && p.Fecha <= fechaFinalDateOnly && p.NominaEmpleado == nomina)
			.ToList();
	}

	public DatosTempEmpleadoSindicalizado? ObtenDatosDeInicioPara(User user)
	{
		var userNomina = user.Nomina ?? 0;
		var grupo = this._freeTimeDb.Grupos.FirstOrDefault(g => g.GrupoId == user.GrupoId);
		var rolInicial = this._freeTimeDb.RolesInicialesPorEmpleado
			.FirstOrDefault(r => r.Nomina == userNomina);

		if (rolInicial == null)
		{
			_logger.LogWarning("El usuario {UserId} - {UserName} no tiene un Rol Inicial asignado en RolesInicialesPorEmpleado, se omitirá la generación de calendario.", user.Id, user.FullName);
			return null;
		}

		var rolSemanalInicial = this._freeTimeDb.RolesSemanales.FirstOrDefault(r => r.Rol == rolInicial.RolSemanal);
		if (rolSemanalInicial == null)
		{
			_logger.LogWarning("El usuario {UserId} - {UserName} no tiene una regla de Rol Semanal asignada, se omitirá la generación de calendario.", user.Id, user.FullName);
			return null;
		}

		var diaDeLaSemanaFechaInicial = (DiasDeLaSemanaEnum)rolInicial.Fecha.DayOfWeek;
		var turno = this._freeTimeDb.TurnosXRolSemanalXRegla
					.FirstOrDefault(t => t.IdRegla == rolSemanalInicial.Regla.Id &&
											t.IdRolSemanal == rolSemanalInicial.RolSemanalId &&
											t.DiaDeLaSemana == diaDeLaSemanaFechaInicial);
		if (turno == null)
		{
			_logger.LogWarning("El usuario {UserId} - {UserName} no existe un Turno en TurnosXRolSemanalXRegla que coincida con los datos iniciales del usuario, se omitirá la generación de calendario.", user.Id, user.FullName);
			return null;
		}

		int diasDisponibles = user.VacacionesPorAntiguedad != null ? user.VacacionesPorAntiguedad.TotalDiasDeVacaciones - user.VacacionesPorAntiguedad.DiasAsignadosPorContinental : 0;
		if (diasDisponibles < 0) diasDisponibles = 0;

		user.DiasDeVacacionesAsignados = diasDisponibles;
		this._freeTimeDb.Users.Update(user);

		_logger.LogInformation("Datos de inicio obtenidos para el usuario {UserId} - {UserName}.", user.Id, user.FullName);
		_logger.LogInformation("Días de vacaciones asignados: {DiasVacacionesAsignados}.", user.DiasDeVacacionesAsignados);
		_logger.LogInformation("Días de vacaciones disponibles: {DiasVacacionesDisponibles}.", diasDisponibles);

		var nextYear = DateTime.Now.Year + 1;
		// Buscar una ProgramacionAnual que esté en estatus Pendiente o EnProceso
		// y que sea del año actual o del siguiente.
		// Si existe, asignarla al CalendarioEmpleado que se va a crear.
		var programacionAnual = this._freeTimeDb.ProgramacionesAnuales.FirstOrDefault(p => (p.Anio == nextYear - 1 || p.Anio == nextYear) && p.BorradoLogico == false &&
																							(p.Estatus == EstatusProgramacionAnualEnum.Pendiente ||
																							p.Estatus == EstatusProgramacionAnualEnum.EnProceso));
		if (programacionAnual != null)
		{
			_logger.LogInformation("Se encontró una ProgramacionAnual en estatus {Estatus} para el año {Anio}, se asignará al CalendarioEmpleado.", programacionAnual.Estatus, programacionAnual.Anio);
		}
		else
		{
			_logger.LogInformation("No se encontró una ProgramacionAnual en estatus Pendiente o EnProceso para el año actual o siguiente.");
		}

		var calendarioEmpleado = new CalendarioEmpleado
		{
			CreatedAt = DateTime.Now,
			IdRegla = rolSemanalInicial.Regla.Id,
			IdArea = user.AreaId ?? 0,
			IdGrupo = user.GrupoId ?? 0,
			IdProgramacionAnual = programacionAnual?.Id ?? 0,
			IdUsuarioEmpleadoSindicalizado = user.Id,
			NominaEmpleado = user.Nomina ?? 0,
			IdRolSemanalIniciaGeneracionDeCalendario = rolSemanalInicial.RolSemanalId,
			FechaInicioGeneracionDeCalendario = DateOnly.FromDateTime(programacionAnual.FechaInicia),
			IdVacacionesPorAntiguedad = user.VacacionesPorAntiguedad?.Id,
			UsuarioEmpleadoSindicalizado = user,
		};
		this._freeTimeDb.CalendarioEmpleados.Add(calendarioEmpleado);
		this._freeTimeDb.SaveChanges();

		return new DatosTempEmpleadoSindicalizado(user, calendarioEmpleado, grupo!, rolSemanalInicial.Regla, rolSemanalInicial, turno, user.VacacionesPorAntiguedad);
	}

	public TurnoXRolSemanalXRegla? ObtenTurnoParaLaFecha(DateTime fechaInicial, DateTime fechaObjetivo, Regla regla, TurnoXRolSemanalXRegla turnoRolSemanalInicial)
	{
		var diasDiferencia = (fechaObjetivo - fechaInicial).Days;
		if (diasDiferencia == 0)
		{
			return turnoRolSemanalInicial;
		}
		else if (diasDiferencia < 0)
		{
			_logger.LogWarning("La fecha objetivo {FechaObjetivo} es anterior a la fecha inicial {FechaInicial}, no se podrá obtener el turno.", fechaObjetivo.ToShortDateString(), fechaInicial.ToShortDateString());
			return null;
		}
		if (regla.TurnosXRolSemanalXRegla.Count == 0)
		{
			_logger.LogWarning("La regla {ReglaId} no tiene turnos asignados en TurnosXRolSemanalXRegla, no se podrá obtener el turno para la fecha {Fecha}.", regla.Id, fechaObjetivo.ToShortDateString());
			return null;
		}

		var indiceTurno = (turnoRolSemanalInicial.IndicePorRegla + diasDiferencia) % regla.TurnosXRolSemanalXRegla.Count;
		var diaDeLaSemana = (DiasDeLaSemanaEnum)fechaObjetivo.DayOfWeek;
		var turno = this._freeTimeDb.TurnosXRolSemanalXRegla
			.FirstOrDefault(txrsxr => txrsxr.IdRegla == regla.Id && txrsxr.IndicePorRegla == indiceTurno && txrsxr.DiaDeLaSemana == diaDeLaSemana);
		return turno;
	}

	public List<User> GetUsersByArea(Area area)
	{
		return this._freeTimeDb.Users.Where(u => u.AreaId == area.AreaId)
		.OrderByDescending(u => u.AntiguedadEnAnios).ThenBy(u => u.Nomina)
		.ToList();
	}

	public void CalculaAntiguedadParaTodosEmpleados()
	{
		_logger.LogInformation("Buscando usuarios con rol 'Empleados Sindicalizados'...");
		var users = _freeTimeDb.Users.Where(u => u.Roles.Any(r => r.Id == (int)RolEnum.Empleado_Sindicalizado)).ToList();
		_logger.LogInformation("Usuarios/Empleados sindicalizados encontrados: {Count}", users.Count);
		_logger.LogInformation("Calculando antigüedad por cada empleado y asignandole sus VacacionesPorAntiguedad correspondiente...");
		foreach (var user in users)
		{
			// La antigüedad se calcula como la diferencia entre la fecha actual y la fecha de ingreso del empleado en años.
			// Si la fecha de ingreso es null, se asume una antigüedad de 0 años.
			if (!user.FechaIngreso.HasValue)
			{
				_logger.LogWarning("El usuario {UserId} no tiene fecha de ingreso, se asumirá antigüedad de 0 años.", user.Id);
				user.AntiguedadEnAnios = 0;
				user.AntiguedadEnDias = 0;
				user.VacacionesPorAntiguedad = null;
				_freeTimeDb.Users.Update(user);
				continue;
			}

			var antiguedad = DateTime.Now.Date - user.FechaIngreso.Value.ToDateTime(new TimeOnly(0, 0));
			user.AntiguedadEnDias = antiguedad.Days;
			// Buscar la regla de VacacionesPorAntiguedad correspondiente a la antigüedad del empleado
			var vacacionesPorAntiguedad = this.BuscarVacacionesPorAntiguedad(antiguedad);
			if (vacacionesPorAntiguedad == null)
			{
				_logger.LogWarning("No se encontró una regla de VacacionesPorAntiguedad para el usuario {UserId} - {UserName} con antigüedad de {Antiguedad} días.", user.Id, user.FullName, antiguedad.Days);
			}
			user.VacacionesPorAntiguedadId = vacacionesPorAntiguedad?.Id;
			user.VacacionesPorAntiguedad = vacacionesPorAntiguedad;
			user.AntiguedadEnAnios = (int)(antiguedad.Days / 365.25);
			_logger.LogInformation("Empleado: {UserId}, Antigüedad: {Antiguedad}, Vacaciones por Antigüedad: {VacacionesPorAntiguedad}", user.Id, antiguedad.Days, vacacionesPorAntiguedad);
			_freeTimeDb.Users.Update(user);
			_freeTimeDb.SaveChanges();
		}
		_logger.LogInformation("Antigüedad calculada y VacacionesPorAntiguedad asignadas para todos los empleados sindicalizados.");
	}

	public VacacionesPorAntiguedad? BuscarVacacionesPorAntiguedad(TimeSpan antiguedad)
	{
		var antiguedadEnAnios = (int)(antiguedad.Days / 365.25);
		var vacacionesPorAntiguedad = _freeTimeDb.VacacionesPorAntiguedad.ToList();
		return _freeTimeDb.VacacionesPorAntiguedad
			.FirstOrDefault(v => (v.AntiguedadEnAniosRangoFinal == null && antiguedadEnAnios == v.AntiguedadEnAniosRangoInicial) ||
								(v.AntiguedadEnAniosRangoFinal != null && antiguedadEnAnios >= v.AntiguedadEnAniosRangoInicial && antiguedadEnAnios <= v.AntiguedadEnAniosRangoFinal));
	}
}


public class DatosTempEmpleadoSindicalizado
{
	public User User { get; set; }
	public Grupo Grupo { get; set; }
	public Regla Regla { get; set; }
	public RolSemanal RolInicial { get; set; }
	public CalendarioEmpleado CalendarioEmpleado { get; set; }
	public TurnoXRolSemanalXRegla turnoRolSemanalInicial { get; set; }
	public VacacionesPorAntiguedad VacacionesPorAntiguedad { get; set; } = null!;
	public List<DiasInhabiles> DiasInhabilesPorContinental { get; set; } = new List<DiasInhabiles>();
	public List<DiasInhabiles> DiasInhabilesPorLey { get; set; } = new List<DiasInhabiles>();
	public List<IncidenciaOPermiso> PermisosEIncapacidades { get; set; } = new List<IncidenciaOPermiso>();
	public List<Vacaciones> DiasDeVacacionesAsignadasAutomaticamente { get; set; } = new List<Vacaciones>();
	public List<Vacaciones> DiasDeVacacionesAsignadasPorEmpleado { get; set; } = new List<Vacaciones>();
	public int AntiguedadEnDias { get; set; }
	public int AntiguedadEnAnios { get; set; }

	// Dias de vacaciones que le faltan por asignar al empleado
	// Basado en la regla de VacacionesPorAntiguedad
	public static int DiasDisponiblesParaAsignarPorApp { get; set; }

	// Dias de vacaciones que ya le ha asignado Continental
	// Basado en la regla de VacacionesPorAntiguedad
	public int DiasAsignadosPorContinental { get; set; }
	public int DiasDisponiblesEmpleadoElija { get; set; }

	public DatosTempEmpleadoSindicalizado(User user, CalendarioEmpleado calendarioEmpleado, Grupo grupo, Regla regla, RolSemanal rolInicial,
											TurnoXRolSemanalXRegla turnoInicial, VacacionesPorAntiguedad vacacionesPorAntiguedad)
	{
		User = user;
		CalendarioEmpleado = calendarioEmpleado;
		Grupo = grupo;
		Regla = regla;
		RolInicial = rolInicial;
		turnoRolSemanalInicial = turnoInicial;
		VacacionesPorAntiguedad = vacacionesPorAntiguedad;
		AntiguedadEnDias = user.AntiguedadEnDias ?? 0;
		AntiguedadEnAnios = user.AntiguedadEnAnios ?? 0;

		int diasDisponibles = user.VacacionesPorAntiguedad != null ? user.VacacionesPorAntiguedad.TotalDiasDeVacaciones - user.VacacionesPorAntiguedad.DiasAsignadosPorContinental : 0;
		if (diasDisponibles < 0) diasDisponibles = 0;
		DiasDisponiblesParaAsignarPorApp = user.VacacionesPorAntiguedad?.DiasParaAsignarAutomaticamente ?? 0;

		DiasAsignadosPorContinental = 0;
		DiasDisponiblesEmpleadoElija = 0;
	}

	public int agregaDiaDeInhabilPorContinental(DiasInhabiles diaInhabil)
	{
		if (diaInhabil.TipoActividadDelDia != TipoActividadDelDiaEnum.InhabilPorContinental)
		{
			throw new ArgumentException("El día inhabil no es de tipo 'InhabilPorContinental'.");
		}
		this.DiasInhabilesPorContinental.Add(diaInhabil);
		this.DiasAsignadosPorContinental++;
		return this.DiasInhabilesPorContinental.Count;
	}

	public int dameCuentaDiasInhabilesPorContinental()
	{
		return this.DiasInhabilesPorContinental.Count;
	}

	public int agregaDiaDeInhabilPorLey(DiasInhabiles diaInhabil)
	{
		if (diaInhabil.TipoActividadDelDia != TipoActividadDelDiaEnum.InhabilPorLey)
		{
			throw new ArgumentException("El día inhabil no es de tipo 'InhabilPorLey'.");
		}
		this.DiasInhabilesPorLey.Add(diaInhabil);
		return this.DiasInhabilesPorLey.Count;
	}

	public int agregaPermisoOIncapacidad(IncidenciaOPermiso permisoOIncapacidad)
	{
		this.PermisosEIncapacidades.Add(permisoOIncapacidad);
		return this.PermisosEIncapacidades.Count;
	}

	public int agregaDiaDeVacacionesAsignadasAutomaticamente(Vacaciones diaDeVacaciones)
	{
		if (this.DiasDeVacacionesAsignadasAutomaticamente.Count >= DiasDisponiblesParaAsignarPorApp)
		{
			throw new InvalidOperationException("No hay días disponibles para asignar automáticamente.");
		}
		this.DiasDeVacacionesAsignadasAutomaticamente.Add(diaDeVacaciones);
		return this.DiasDeVacacionesAsignadasAutomaticamente.Count;
	}

	public bool tieneDiasDeVacacionesDisponiblesParaAsignarAutomaticamente()
	{
		return this.DiasDeVacacionesAsignadasAutomaticamente.Count < DiasDisponiblesParaAsignarPorApp;
	}

	public int dameDiasDeVacacionesAsignadosAutomaticamente()
	{
		return this.DiasDeVacacionesAsignadasAutomaticamente.Count;
	}

	public bool puedeElEmpleadoEscogerDiasDeVacaciones()
	{
		var totalDiasDeVacaciones = this.VacacionesPorAntiguedad.TotalDiasDeVacaciones;
		var diasAsignanAutomaticamente = this.VacacionesPorAntiguedad.DiasParaAsignarAutomaticamente;
		var diasDisponibles = totalDiasDeVacaciones - (this.DiasAsignadosPorContinental + diasAsignanAutomaticamente);
		return diasDisponibles > 0;
	}

}