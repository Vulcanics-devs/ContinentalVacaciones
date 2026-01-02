/**
 * Utilidad para generar Excel de empleados que no respondieron
 */

import * as XLSX from 'xlsx';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import type { EmpleadosNoRespondieronResponse, EmpleadoNoRespondio } from '@/interfaces/Api.interface';

/**
 * Genera y descarga un archivo Excel con los empleados que no respondieron
 * @param data - Datos de empleados que no respondieron
 */
export const generarExcelEmpleadosNoRespondieron = (data: EmpleadosNoRespondieronResponse) => {
  try {
    // Crear un nuevo workbook
    const workbook = XLSX.utils.book_new();

    // Hoja 1: Resumen
    const resumenData = [
      ['REPORTE DE EMPLEADOS QUE NO RESPONDIERON'],
      [''],
      ['Año:', data.anio],
      ['Fecha del reporte:', format(new Date(data.fechaReporte), "d 'de' MMMM 'de' yyyy 'a las' HH:mm", { locale: es })],
      [''],
      ['RESUMEN GENERAL'],
      ['Total empleados que no respondieron:', data.totalEmpleadosNoRespondio],
      ['Empleados en bloques regulares:', data.empleadosEnBloquesRegulares],
      ['Empleados en bloque cola (CRÍTICO):', data.empleadosEnBloqueCola],
      [''],
      ['INTERPRETACIÓN:'],
      ['• Bloques regulares: Empleados transferidos al bloque cola por no reservar'],
      ['• Bloque cola: Empleados que NO reservaron ni en el bloque cola (REQUIERE ACCIÓN URGENTE)'],
    ];

    const resumenSheet = XLSX.utils.aoa_to_sheet(resumenData);
    
    // Aplicar estilos al resumen
    resumenSheet['!cols'] = [
      { width: 40 },
      { width: 20 }
    ];

    XLSX.utils.book_append_sheet(workbook, resumenSheet, 'Resumen');

    // Hoja 2: Detalle de empleados
    if (data.empleados && data.empleados.length > 0) {
      const empleadosParaExcel = data.empleados.map((empleado: EmpleadoNoRespondio) => ({
        'ID Empleado': empleado.empleadoId,
        'Nombre Completo': empleado.nombreCompleto,
        'Nómina': empleado.nomina,
        'Máquina': empleado.maquina || 'N/A',
        'Área': empleado.nombreArea,
        'Grupo': empleado.nombreGrupo,
        'Número de Bloque': empleado.numeroBloque,
        'Tipo de Bloque': empleado.esBloqueCola ? 'BLOQUE COLA' : 'Bloque Regular',
        'Estado Crítico': empleado.requiereAccionUrgente ? 'SÍ - URGENTE' : 'No',
        'Fecha Límite': format(new Date(empleado.fechaLimiteBloque), "d/MM/yyyy HH:mm", { locale: es }),
        'Fecha Asignación': format(new Date(empleado.fechaAsignacion), "d/MM/yyyy HH:mm", { locale: es }),
        'Observaciones': empleado.observaciones,
      }));

      const empleadosSheet = XLSX.utils.json_to_sheet(empleadosParaExcel);
      
      // Configurar anchos de columna
      empleadosSheet['!cols'] = [
        { width: 12 }, // ID Empleado
        { width: 25 }, // Nombre Completo
        { width: 12 }, // Nómina
        { width: 15 }, // Máquina
        { width: 20 }, // Área
        { width: 25 }, // Grupo
        { width: 15 }, // Número de Bloque
        { width: 18 }, // Tipo de Bloque
        { width: 15 }, // Estado Crítico
        { width: 18 }, // Fecha Límite
        { width: 18 }, // Fecha Asignación
        { width: 50 }, // Observaciones
      ];

      XLSX.utils.book_append_sheet(workbook, empleadosSheet, 'Empleados No Respondieron');
    }

    // Hoja 3: Empleados críticos (solo bloque cola)
    const empleadosCriticos = data.empleados.filter(emp => emp.esBloqueCola);
    if (empleadosCriticos.length > 0) {
      const criticosParaExcel = empleadosCriticos.map((empleado: EmpleadoNoRespondio) => ({
        'ID Empleado': empleado.empleadoId,
        'Nombre Completo': empleado.nombreCompleto,
        'Nómina': empleado.nomina,
        'Máquina': empleado.maquina || 'N/A',
        'Área': empleado.nombreArea,
        'Grupo': empleado.nombreGrupo,
        'Fecha Límite Vencida': format(new Date(empleado.fechaLimiteBloque), "d/MM/yyyy HH:mm", { locale: es }),
        'Días Vencidos': Math.ceil((new Date().getTime() - new Date(empleado.fechaLimiteBloque).getTime()) / (1000 * 60 * 60 * 24)),
        'Observaciones': empleado.observaciones,
        'Acción Requerida': 'INTERVENCIÓN MANUAL INMEDIATA'
      }));

      const criticosSheet = XLSX.utils.json_to_sheet(criticosParaExcel);
      
      // Configurar anchos de columna para críticos
      criticosSheet['!cols'] = [
        { width: 12 }, // ID Empleado
        { width: 25 }, // Nombre Completo
        { width: 12 }, // Nómina
        { width: 15 }, // Máquina
        { width: 20 }, // Área
        { width: 25 }, // Grupo
        { width: 18 }, // Fecha Límite Vencida
        { width: 15 }, // Días Vencidos
        { width: 50 }, // Observaciones
        { width: 30 }, // Acción Requerida
      ];

      XLSX.utils.book_append_sheet(workbook, criticosSheet, 'CRÍTICOS - Bloque Cola');
    }

    // Generar nombre del archivo
    const fechaActual = format(new Date(), 'yyyy-MM-dd_HH-mm');
    const nombreArchivo = `Empleados_No_Respondieron_${data.anio}_${fechaActual}.xlsx`;

    // Descargar el archivo
    XLSX.writeFile(workbook, nombreArchivo);

    console.log(`Excel generado exitosamente: ${nombreArchivo}`);
    return nombreArchivo;
  } catch (error) {
    console.error('Error al generar Excel de empleados que no respondieron:', error);
    throw new Error('No se pudo generar el archivo Excel');
  }
};

/**
 * Genera estadísticas rápidas para mostrar en UI
 * @param data - Datos de empleados que no respondieron
 * @returns Objeto con estadísticas resumidas
 */
export const generarEstadisticasEmpleadosNoRespondieron = (data: EmpleadosNoRespondieronResponse) => {
  const empleadosPorArea = data.empleados.reduce((acc, empleado) => {
    const area = empleado.nombreArea;
    if (!acc[area]) {
      acc[area] = { total: 0, criticos: 0, regulares: 0 };
    }
    acc[area].total++;
    if (empleado.esBloqueCola) {
      acc[area].criticos++;
    } else {
      acc[area].regulares++;
    }
    return acc;
  }, {} as Record<string, { total: number; criticos: number; regulares: number }>);

  const empleadosPorGrupo = data.empleados.reduce((acc, empleado) => {
    const grupo = empleado.nombreGrupo;
    if (!acc[grupo]) {
      acc[grupo] = { total: 0, criticos: 0, regulares: 0 };
    }
    acc[grupo].total++;
    if (empleado.esBloqueCola) {
      acc[grupo].criticos++;
    } else {
      acc[grupo].regulares++;
    }
    return acc;
  }, {} as Record<string, { total: number; criticos: number; regulares: number }>);

  return {
    resumenGeneral: {
      total: data.totalEmpleadosNoRespondio,
      regulares: data.empleadosEnBloquesRegulares,
      criticos: data.empleadosEnBloqueCola,
      porcentajeCriticos: data.totalEmpleadosNoRespondio > 0 
        ? Math.round((data.empleadosEnBloqueCola / data.totalEmpleadosNoRespondio) * 100) 
        : 0
    },
    porArea: empleadosPorArea,
    porGrupo: empleadosPorGrupo,
    fechaReporte: data.fechaReporte
  };
};
