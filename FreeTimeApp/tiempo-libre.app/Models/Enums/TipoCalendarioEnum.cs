namespace tiempo_libre.Models.Enums;

public enum TipoCalendarioEnum
{
    // Turnos de trabajo
    Turno1 = 1,
    Turno2 = 2,
    Turno3 = 3,
    
    // Descanso
    Descanso = 4,
    
    // Permisos
    PermisoConGoce = 5,
    PermisoDefuncion = 6,
    PermisoSinGoce = 7,
    
    // Vacaciones
    Vacacion = 8,
    VacacionAsignada = 9,
    
    // Incapacidades
    IncapacidadEnfermedadGeneral = 10,
    IncapacidadAccidenteTrabajo = 11,
    IncapacidadPorMaternidad = 12,
    IncapacidadProbableRiesgoTrabajo = 13,
    
    // Otros
    Suspension = 14,
    PCGPorPaternidad = 15
}

public static class TipoCalendarioExtensions
{
    public static string ToShortString(this TipoCalendarioEnum tipo)
    {
        return tipo switch
        {
            TipoCalendarioEnum.Turno1 => "1",
            TipoCalendarioEnum.Turno2 => "2",
            TipoCalendarioEnum.Turno3 => "3",
            TipoCalendarioEnum.Descanso => "D",
            TipoCalendarioEnum.PermisoConGoce => "P",
            TipoCalendarioEnum.PermisoDefuncion => "PD",
            TipoCalendarioEnum.PermisoSinGoce => "G",
            TipoCalendarioEnum.Vacacion => "V",
            TipoCalendarioEnum.VacacionAsignada => "VA",
            TipoCalendarioEnum.IncapacidadEnfermedadGeneral => "E",
            TipoCalendarioEnum.IncapacidadAccidenteTrabajo => "A",
            TipoCalendarioEnum.IncapacidadPorMaternidad => "M",
            TipoCalendarioEnum.IncapacidadProbableRiesgoTrabajo => "R",
            TipoCalendarioEnum.Suspension => "S",
            TipoCalendarioEnum.PCGPorPaternidad => "PP",
            _ => ""
        };
    }
    
    public static string ToDescription(this TipoCalendarioEnum tipo)
    {
        return tipo switch
        {
            TipoCalendarioEnum.Turno1 => "Turno de trabajo",
            TipoCalendarioEnum.Turno2 => "Turno de trabajo",
            TipoCalendarioEnum.Turno3 => "Turno de trabajo",
            TipoCalendarioEnum.Descanso => "Descanso",
            TipoCalendarioEnum.PermisoConGoce => "Permiso con Goce",
            TipoCalendarioEnum.PermisoDefuncion => "Permiso Defunción",
            TipoCalendarioEnum.PermisoSinGoce => "Permiso sin Goce",
            TipoCalendarioEnum.Vacacion => "Vacación",
            TipoCalendarioEnum.VacacionAsignada => "Vacación asignada",
            TipoCalendarioEnum.IncapacidadEnfermedadGeneral => "Inc. Enfermedad General",
            TipoCalendarioEnum.IncapacidadAccidenteTrabajo => "Inc. Accidente de Trabajo",
            TipoCalendarioEnum.IncapacidadPorMaternidad => "Inc. por Maternidad",
            TipoCalendarioEnum.IncapacidadProbableRiesgoTrabajo => "Inc. Pble. Riesgo Trabajo",
            TipoCalendarioEnum.Suspension => "Suspensión",
            TipoCalendarioEnum.PCGPorPaternidad => "PCG por Paternidad",
            _ => ""
        };
    }
    
    public static TipoCalendarioEnum FromShortString(string shortString)
    {
        return shortString switch
        {
            "1" => TipoCalendarioEnum.Turno1,
            "2" => TipoCalendarioEnum.Turno2,
            "3" => TipoCalendarioEnum.Turno3,
            "D" => TipoCalendarioEnum.Descanso,
            "P" => TipoCalendarioEnum.PermisoConGoce,
            "PD" => TipoCalendarioEnum.PermisoDefuncion,
            "G" => TipoCalendarioEnum.PermisoSinGoce,
            "V" => TipoCalendarioEnum.Vacacion,
            "VA" => TipoCalendarioEnum.VacacionAsignada,
            "E" => TipoCalendarioEnum.IncapacidadEnfermedadGeneral,
            "A" => TipoCalendarioEnum.IncapacidadAccidenteTrabajo,
            "M" => TipoCalendarioEnum.IncapacidadPorMaternidad,
            "R" => TipoCalendarioEnum.IncapacidadProbableRiesgoTrabajo,
            "S" => TipoCalendarioEnum.Suspension,
            "PP" => TipoCalendarioEnum.PCGPorPaternidad,
            _ => TipoCalendarioEnum.Descanso
        };
    }
}
