/**
 * =============================================================================
 * AUSENCIAS INTERFACES
 * =============================================================================
 * 
 * @description
 * Interfaces para el manejo de datos de ausencias y cálculos de porcentajes
 * de disponibilidad del personal por grupo y área.
 * 
 * @author Vulcanics Dev Team
 * @created 2025
 * =============================================================================
 */

export interface EmpleadoAusente {
    empleadoId: number;
    nombreCompleto: string;
    nomina?: number;
    tipoAusencia: string;
    tipoVacacion?: string; // Tipo específico de vacación (Anual, Automatica, etc.)
    maquina?: string; // Máquina asignada al empleado
    motivo?: string; // Motivo adicional (para compatibilidad)
}

export interface EmpleadoDisponible {
    empleadoId: number;
    nombreCompleto: string;
    nomina?: number;
    rol?: string;
}

export interface AusenciasPorGrupo {
    grupoId: number;
    nombreGrupo: string;
    areaId: number;
    nombreArea: string;
    manningRequerido: number;
    personalTotal: number;
    personalNoDisponible: number;
    personalDisponible: number;
    porcentajeDisponible: number;
    porcentajeAusencia: number;
    porcentajeMaximoPermitido: number;
    excedeLimite: boolean;
    puedeReservar: boolean;
    empleadosAusentes: EmpleadoAusente[];
    empleadosDisponibles?: EmpleadoDisponible[];
}

export interface AusenciasPorFecha {
    fecha: string; // Format: YYYY-MM-DD
    ausenciasPorGrupo: AusenciasPorGrupo[];
}

export interface AusenciasResponse {
    success: boolean;
    data: AusenciasPorFecha[];
    message?: string;
}

export interface CalcularAusenciasRequest {
    fechaInicio: string; // Format: YYYY-MM-DD
    fechaFin?: string; // Format: YYYY-MM-DD, optional
    grupoId?: number; // Optional
    areaId?: number; // Optional
}

export interface AusenciasFilters {
    fechaInicio: Date;
    fechaFin?: Date;
    grupoIds?: number[];
    areaId?: number;
    view: 'monthly' | 'weekly' | 'daily';
}
