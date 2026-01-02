/**
 * =============================================================================
 * UTILIDAD PARA EXPORTAR REPORTE DE APROBACIONES EN DÍAS LLENOS
 * =============================================================================
 * 
 * @description
 * Genera archivos Excel con reportes de solicitudes aprobadas en días que ya
 * tienen vacaciones o permisos programados. Permite distinguir entre 
 * aprobaciones automáticas y manuales.
 * 
 * @author Vulcanics Dev Team
 * @created 2025-12-10
 * =============================================================================
 */

import * as XLSX from 'xlsx';
import { saveAs } from 'file-saver';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import type { Solicitud } from '@/interfaces/Solicitudes.interface';

export interface SolicitudAprobacionData {
    id: number;
    nominaEmpleado: string;
    nombreEmpleado: string;
    areaEmpleado: string;
    grupoEmpleado: string;
    fechaOriginal: string;
    fechaNueva: string;
    fechaSolicitud: string;
    fechaAprobacion: string;
    estadoSolicitud: string;
    motivo: string;
    tipoAprobacion: 'Automática' | 'Manual';
    aprobadoPor: string;
    porcentajeCalculado: number;
    conflictosDetectados: string[];
}

interface OpcionesReporte {
    titulo: string;
    mesAnio?: string;
    tipoFiltro: 'automaticas' | 'manuales' | 'todas';
    area?: string;
}

/**
 * Formatea una fecha ISO a formato dd/MM/yyyy
 */
const formatDate = (fecha?: string | null): string => {
    if (!fecha || fecha === '0001-01-01') return 'No especificada';
    try {
        return format(new Date(fecha), 'dd/MM/yyyy', { locale: es });
    } catch {
        return fecha || 'No especificada';
    }
};

/**
 * Formatea una fecha con hora
 */
const formatDateHora = (fecha?: string | null): string => {
    if (!fecha || fecha === '0001-01-01') return 'No especificada';
    try {
        return format(new Date(fecha), 'dd/MM/yyyy HH:mm', { locale: es });
    } catch {
        return fecha || 'No especificada';
    }
};

/**
 * Agrupa solicitudes por mes y día
 */
const agruparPorMesYDia = (solicitudes: SolicitudAprobacionData[]): any[] => {
    const grupos: Record<string, Record<string, { automaticas: number; manuales: number }>> = {};

    solicitudes.forEach(sol => {
        const fecha = new Date(sol.fechaNueva);
        const mes = format(fecha, 'MMMM', { locale: es });
        const dia = fecha.getDate();

        if (!grupos[mes]) {
            grupos[mes] = {};
        }

        if (!grupos[mes][dia]) {
            grupos[mes][dia] = { automaticas: 0, manuales: 0 };
        }

        if (sol.tipoAprobacion === 'Automática') {
            grupos[mes][dia].automaticas++;
        } else {
            grupos[mes][dia].manuales++;
        }
    });

    // Convertir a array para Excel
    const resultado: any[] = [];
    Object.entries(grupos).forEach(([mes, dias]) => {
        resultado.push({ Mes: mes, Dia: '', Automaticas: '', Manuales: '' });
        Object.entries(dias).forEach(([dia, conteos]) => {
            resultado.push({
                Mes: '',
                Dia: dia,
                'Aprobadas por dia disponible': conteos.automaticas,
                'Aprobadas por jefe de area': conteos.manuales,
            });
        });
    });

    return resultado;
};

/**
 * Exporta el reporte de aprobaciones a Excel
 */
export const exportarAprobacionesExcel = (
    solicitudes: SolicitudAprobacionData[],
    opciones: OpcionesReporte
): void => {
    const workbook = XLSX.utils.book_new();

    // Hoja 1: Resumen por Mes y Día (formato solicitado)
    const resumenPorDia = agruparPorMesYDia(solicitudes);
    const resumenSheet = XLSX.utils.json_to_sheet(resumenPorDia);
    resumenSheet['!cols'] = [
        { wch: 20 },  // Mes
        { wch: 10 },  // Día
        { wch: 30 },  // Aprobadas por día disponible
        { wch: 30 },  // Aprobadas por jefe
    ];
    XLSX.utils.book_append_sheet(workbook, resumenSheet, 'Resumen por Dia');

    // Hoja 2: Detalle completo
    const detailRows = solicitudes.map((sol) => ({
        'ID': sol.id,
        'Nómina': sol.nominaEmpleado,
        'Nombre Empleado': sol.nombreEmpleado,
        'Área': sol.areaEmpleado,
        'Grupo': sol.grupoEmpleado,
        'Fecha Original': formatDate(sol.fechaOriginal),
        'Fecha Nueva': formatDate(sol.fechaNueva),
        'Tipo de Aprobación': sol.tipoAprobacion,
        'Aprobado Por': sol.aprobadoPor,
        'Fecha de Solicitud': formatDateHora(sol.fechaSolicitud),
        'Fecha de Aprobación': formatDateHora(sol.fechaAprobacion),
        'Estado': sol.estadoSolicitud,
        'Porcentaje': `${sol.porcentajeCalculado.toFixed(1)}%`,
        'Conflictos': sol.conflictosDetectados.join(', ') || 'Ninguno',
        'Motivo': sol.motivo || 'Sin motivo',
    }));

    const detailSheet = XLSX.utils.json_to_sheet(detailRows);
    detailSheet['!cols'] = [
        { wch: 8 },   // ID
        { wch: 10 },  // Nómina
        { wch: 30 },  // Nombre
        { wch: 20 },  // Área
        { wch: 15 },  // Grupo
        { wch: 16 },  // Fecha Original
        { wch: 16 },  // Fecha Nueva
        { wch: 18 },  // Tipo Aprobación
        { wch: 25 },  // Aprobado Por
        { wch: 18 },  // Fecha Solicitud
        { wch: 18 },  // Fecha Aprobación
        { wch: 12 },  // Estado
        { wch: 12 },  // Porcentaje
        { wch: 40 },  // Conflictos
        { wch: 30 },  // Motivo
    ];
    XLSX.utils.book_append_sheet(workbook, detailSheet, 'Detalle');

    // Hoja 3: Resumen General
    const resumenData = [
        { Concepto: 'Título', Valor: opciones.titulo },
        { Concepto: 'Área', Valor: opciones.area || 'Todas las áreas' },
        { Concepto: 'Periodo', Valor: opciones.mesAnio || 'Todos los periodos' },
        { Concepto: 'Total solicitudes', Valor: solicitudes.length },
        { Concepto: 'Aprobaciones Automáticas', Valor: solicitudes.filter(s => s.tipoAprobacion === 'Automática').length },
        { Concepto: 'Aprobaciones por Jefe', Valor: solicitudes.filter(s => s.tipoAprobacion === 'Manual').length },
        { Concepto: 'Generado el', Valor: format(new Date(), 'dd/MM/yyyy HH:mm', { locale: es }) },
    ];

    const estadisticasSheet = XLSX.utils.json_to_sheet(resumenData);
    estadisticasSheet['!cols'] = [{ wch: 30 }, { wch: 40 }];
    XLSX.utils.book_append_sheet(workbook, estadisticasSheet, 'Estadisticas');

    // Generar archivo
    const buffer = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
    const blob = new Blob([buffer], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    });

    const areaTexto = opciones.area
        ? `_${opciones.area.replace(/\s+/g, '_')}`
        : '_TodasAreas';

    const timestamp = format(new Date(), 'yyyyMMdd_HHmmss');
    const nombreArchivo = `Aprobaciones_DiasLlenos${areaTexto}_${timestamp}.xlsx`;

    saveAs(blob, nombreArchivo);
};

/**
 * Transforma una solicitud en datos para el reporte de aprobaciones
 */
export const transformarSolicitudAAprobacion = (
    solicitud: Solicitud
): SolicitudAprobacionData => {
    // Determinar si es aprobación automática o manual
    const solicitadoPor = solicitud.solicitadoPor?.toLowerCase() || '';
    const esAutomatica =
        solicitadoPor.includes('sistema') ||
        solicitadoPor.includes('automático') ||
        solicitadoPor.includes('automatico') ||
        solicitadoPor === 'system';

    // Detectar conflictos basados en el porcentaje calculado
    const conflictos: string[] = [];

    if ((solicitud.porcentajeCalculado || 0) > 5) {
        conflictos.push("Rebasado");
    } else {
        // Si NO rebasó el 5%, entonces solo marcamos "Manual" si aplica
        const solicitadoPor = solicitud.solicitadoPor?.toLowerCase() || '';
        const esManual = !(
            solicitadoPor.includes("sistema") ||
            solicitadoPor.includes("automático") ||
            solicitadoPor.includes("automatico") ||
            solicitadoPor === "system"
        );

        if (esManual) conflictos.push("Ninguno");
    }

    // Agregar tipo de solicitud como conflicto si es relevante
    if (solicitud.motivo?.toLowerCase().includes('vacacion')) {
        conflictos.push('Vacaciones programadas');
    }
    if (solicitud.motivo?.toLowerCase().includes('permiso')) {
        conflictos.push('Permiso programado');
    }

    return {
        id: solicitud.id,
        nominaEmpleado: solicitud.nominaEmpleado,
        nombreEmpleado: solicitud.nombreEmpleado,
        areaEmpleado: solicitud.areaEmpleado,
        grupoEmpleado: solicitud.grupoEmpleado,
        fechaOriginal: solicitud.fechaOriginal,
        fechaNueva: solicitud.fechaNueva,
        fechaSolicitud: solicitud.fechaSolicitud,
        fechaAprobacion: solicitud.fechaAprobacion || solicitud.fechaSolicitud,
        estadoSolicitud: solicitud.estadoSolicitud,
        motivo: solicitud.motivo || '',
        tipoAprobacion: esAutomatica ? 'Automática' : 'Manual',
        aprobadoPor: solicitud.solicitadoPor || 'Sistema',
        porcentajeCalculado: solicitud.porcentajeCalculado || 0,
        conflictosDetectados: conflictos,
    };
};