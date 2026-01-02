using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace tiempo_libre.DTOs
{
    /// <summary>
    /// Request para asignar vacaciones manualmente a un empleado
    /// </summary>
    public class AsignacionManualRequest
    {
        [Required(ErrorMessage = "El ID del empleado es requerido")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "Las fechas de vacaciones son requeridas")]
        public List<DateOnly> FechasVacaciones { get; set; } = new();

        [Required(ErrorMessage = "El tipo de vacación es requerido")]
        [MaxLength(50)]
        public string TipoVacacion { get; set; } = "Manual"; // 'Anual', 'Reprogramacion', 'Automatica', 'Manual', 'Compensatoria', 'Extraordinaria'

        [MaxLength(30)]
        public string OrigenAsignacion { get; set; } = "Manual"; // 'Manual', 'Automatica', 'Sistema'

        [MaxLength(20)]
        public string EstadoVacacion { get; set; } = "Activa"; // 'Activa', 'Intercambiada', 'Cancelada'

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        [MaxLength(200)]
        public string? MotivoAsignacion { get; set; }

        // Para casos especiales
        public bool IgnorarRestricciones { get; set; } = true; // Permite asignar sin validar porcentajes, días disponibles, etc.

        public bool NotificarEmpleado { get; set; } = true; // Si se debe notificar al empleado

        public int? BloqueId { get; set; } // Si está relacionado con un bloque específico

        // Para tracking
        public string? OrigenSolicitud { get; set; } // 'NoRespondio', 'Ajuste', 'Correcion', 'Especial'
    }

    /// <summary>
    /// Request para asignar vacaciones en lote a múltiples empleados
    /// </summary>
    public class AsignacionManualLoteRequest
    {
        [Required(ErrorMessage = "Los IDs de empleados son requeridos")]
        public List<int> EmpleadosIds { get; set; } = new();

        [Required(ErrorMessage = "Las fechas de vacaciones son requeridas")]
        public List<DateOnly> FechasVacaciones { get; set; } = new();

        [Required(ErrorMessage = "El tipo de vacación es requerido")]
        [MaxLength(50)]
        public string TipoVacacion { get; set; } = "Manual";

        [MaxLength(30)]
        public string OrigenAsignacion { get; set; } = "Manual";

        [MaxLength(20)]
        public string EstadoVacacion { get; set; } = "Activa";

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        [MaxLength(200)]
        public string? MotivoAsignacion { get; set; }

        public bool IgnorarRestricciones { get; set; } = true;

        public bool NotificarEmpleados { get; set; } = true;

        public int? BloqueId { get; set; }

        public string? OrigenSolicitud { get; set; }
    }

    /// <summary>
    /// Response de asignación manual individual
    /// </summary>
    public class AsignacionManualResponse
    {
        public bool Exitoso { get; set; }
        public int EmpleadoId { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public List<int> VacacionesAsignadasIds { get; set; } = new();
        public List<DateOnly> FechasAsignadas { get; set; } = new();
        public int TotalDiasAsignados { get; set; }
        public string TipoVacacion { get; set; } = string.Empty;
        public string? Mensaje { get; set; }
        public List<string> Advertencias { get; set; } = new();
        public DateTime FechaAsignacion { get; set; }
        public string UsuarioAsigno { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response de asignación manual en lote
    /// </summary>
    public class AsignacionManualLoteResponse
    {
        public int TotalEmpleados { get; set; }
        public int AsignacionesExitosas { get; set; }
        public int AsignacionesFallidas { get; set; }
        public List<AsignacionManualResponse> Detalles { get; set; } = new();
        public List<string> ErroresGenerales { get; set; } = new();
        public DateTime FechaEjecucion { get; set; }
        public string UsuarioEjecuto { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para validación previa de asignación
    /// </summary>
    public class ValidacionAsignacionDto
    {
        public int EmpleadoId { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public List<DateOnly> FechasDisponibles { get; set; } = new();
        public List<DateOnly> FechasNoDisponibles { get; set; } = new();
        public List<ConflictoAsignacionDto> Conflictos { get; set; } = new();
        public int DiasDisponiblesRestantes { get; set; }
        public bool PuedeAsignar { get; set; }
    }

    public class ConflictoAsignacionDto
    {
        public DateOnly Fecha { get; set; }
        public string TipoConflicto { get; set; } = string.Empty; // 'YaAsignada', 'Incapacidad', 'DiaDescanso', 'Festivo'
        public string Descripcion { get; set; } = string.Empty;
    }
}