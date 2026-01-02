import * as XLSX from 'xlsx';
import { saveAs } from 'file-saver';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import type { EmpleadoSinAsignacion, EmpleadosSinAsignacionResponse } from '@/services/asignacionAutomaticaService';

interface ExcelEmpleadoRow {
  'ID Empleado': number;
  'Nómina': string;
  'Nombre Completo': string;
  'Área': string;
  'Grupo': string;
  'Fecha Ingreso': string;
  'Antigüedad (años)': number;
  'Días Correspondientes': number;
  'Días Asignados Automáticamente': number;
  'Días Programables Anual': number;
  'Días Ya Asignados': number;
  'Tiene Turnos Disponibles': string;
  'Motivo No Asignación': string;
}

/**
 * Genera y descarga un archivo Excel con los empleados sin asignación automática
 * @param data - Respuesta con empleados sin asignación
 */
export const generarExcelEmpleadosSinAsignacion = (data: EmpleadosSinAsignacionResponse): void => {
  try {
    // Crear un nuevo libro de Excel
    const workbook = XLSX.utils.book_new();

    // Crear hoja de resumen
    const resumenData = [
      { 'Concepto': 'Año', 'Valor': data.anio },
      { 'Concepto': 'Total de Empleados Sin Asignación', 'Valor': data.totalEmpleadosSinAsignacion },
      { 'Concepto': 'Sin Turnos Disponibles', 'Valor': data.resumenMotivos.sinTurnosDisponibles },
      { 'Concepto': 'Días Insuficientes', 'Valor': data.resumenMotivos.diasInsuficientes },
      { 'Concepto': 'Error en Procesamiento', 'Valor': data.resumenMotivos.errorProcesamiento },
      { 'Concepto': 'Otros Motivos', 'Valor': data.resumenMotivos.otrosMotivos },
      { 'Concepto': 'Fecha del Reporte', 'Valor': format(new Date(data.fechaReporte), "dd/MM/yyyy HH:mm", { locale: es }) }
    ];

    const resumenWorksheet = XLSX.utils.json_to_sheet(resumenData);
    resumenWorksheet['!cols'] = [{ wch: 30 }, { wch: 40 }];
    XLSX.utils.book_append_sheet(workbook, resumenWorksheet, 'Resumen');

    // Agrupar empleados por área
    const empleadosPorArea: Record<string, EmpleadoSinAsignacion[]> = {};
    data.empleados.forEach(empleado => {
      const areaKey = empleado.nombreArea || 'Sin Área';
      if (!empleadosPorArea[areaKey]) {
        empleadosPorArea[areaKey] = [];
      }
      empleadosPorArea[areaKey].push(empleado);
    });

    // Crear hoja con todos los empleados
    const todosEmpleados: ExcelEmpleadoRow[] = data.empleados.map(empleado => ({
      'ID Empleado': empleado.empleadoId,
      'Nómina': empleado.nomina,
      'Nombre Completo': empleado.nombreCompleto,
      'Área': empleado.nombreArea || 'Sin Área',
      'Grupo': empleado.nombreGrupo || 'Sin Grupo',
      'Fecha Ingreso': empleado.fechaIngreso
        ? format(new Date(empleado.fechaIngreso), "dd/MM/yyyy", { locale: es })
        : '',
      'Antigüedad (años)': empleado.antiguedadAnios,
      'Días Correspondientes': empleado.diasCorrespondientes,
      'Días Asignados Automáticamente': empleado.diasAsignadosAutomaticamente,
      'Días Programables Anual': empleado.diasProgramablesAnual,
      'Días Ya Asignados': empleado.diasYaAsignados,
      'Tiene Turnos Disponibles': empleado.tieneTurnosDisponibles ? 'Sí' : 'No',
      'Motivo No Asignación': empleado.motivoNoAsignacion
    }));

    // Ordenar por área y luego por nombre
    todosEmpleados.sort((a, b) => {
      const areaComparison = a['Área'].localeCompare(b['Área']);
      if (areaComparison !== 0) return areaComparison;
      return a['Nombre Completo'].localeCompare(b['Nombre Completo']);
    });

    const todosWorksheet = XLSX.utils.json_to_sheet(todosEmpleados);

    // Ajustar ancho de columnas
    todosWorksheet['!cols'] = [
      { wch: 10 },  // ID Empleado
      { wch: 12 },  // Nómina
      { wch: 35 },  // Nombre Completo
      { wch: 20 },  // Área
      { wch: 15 },  // Grupo
      { wch: 12 },  // Fecha Ingreso
      { wch: 12 },  // Antigüedad
      { wch: 18 },  // Días Correspondientes
      { wch: 25 },  // Días Asignados Automáticamente
      { wch: 20 },  // Días Programables Anual
      { wch: 15 },  // Días Ya Asignados
      { wch: 18 },  // Tiene Turnos Disponibles
      { wch: 40 },  // Motivo No Asignación
    ];

    XLSX.utils.book_append_sheet(workbook, todosWorksheet, 'Todos los Empleados');

    // Crear una hoja por cada área (si hay más de una)
    if (Object.keys(empleadosPorArea).length > 1) {
      Object.entries(empleadosPorArea).forEach(([nombreArea, empleados]) => {
        const areaRows: ExcelEmpleadoRow[] = empleados.map(empleado => ({
          'ID Empleado': empleado.empleadoId,
          'Nómina': empleado.nomina,
          'Nombre Completo': empleado.nombreCompleto,
          'Área': empleado.nombreArea || 'Sin Área',
          'Grupo': empleado.nombreGrupo || 'Sin Grupo',
          'Fecha Ingreso': empleado.fechaIngreso
            ? format(new Date(empleado.fechaIngreso), "dd/MM/yyyy", { locale: es })
            : '',
          'Antigüedad (años)': empleado.antiguedadAnios,
          'Días Correspondientes': empleado.diasCorrespondientes,
          'Días Asignados Automáticamente': empleado.diasAsignadosAutomaticamente,
          'Días Programables Anual': empleado.diasProgramablesAnual,
          'Días Ya Asignados': empleado.diasYaAsignados,
          'Tiene Turnos Disponibles': empleado.tieneTurnosDisponibles ? 'Sí' : 'No',
          'Motivo No Asignación': empleado.motivoNoAsignacion
        }));

        // Ordenar por nombre dentro del área
        areaRows.sort((a, b) => a['Nombre Completo'].localeCompare(b['Nombre Completo']));

        const areaWorksheet = XLSX.utils.json_to_sheet(areaRows);

        // Ajustar ancho de columnas
        areaWorksheet['!cols'] = [
          { wch: 10 },  // ID Empleado
          { wch: 12 },  // Nómina
          { wch: 35 },  // Nombre Completo
          { wch: 20 },  // Área
          { wch: 15 },  // Grupo
          { wch: 12 },  // Fecha Ingreso
          { wch: 12 },  // Antigüedad
          { wch: 18 },  // Días Correspondientes
          { wch: 25 },  // Días Asignados Automáticamente
          { wch: 20 },  // Días Programables Anual
          { wch: 15 },  // Días Ya Asignados
          { wch: 18 },  // Tiene Turnos Disponibles
          { wch: 40 },  // Motivo No Asignación
        ];

        // Truncar el nombre del área si es muy largo para el nombre de la hoja
        let sheetName = nombreArea;
        if (sheetName.length > 31) {
          sheetName = sheetName.substring(0, 28) + '...';
        }

        XLSX.utils.book_append_sheet(workbook, areaWorksheet, sheetName);
      });
    }

    // Generar el archivo Excel
    const excelBuffer = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
    const blob = new Blob([excelBuffer], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    });

    // Descargar el archivo
    const fileName = `Empleados_Sin_Asignacion_${data.anio}_${format(new Date(), 'yyyyMMdd_HHmmss')}.xlsx`;
    saveAs(blob, fileName);

    console.log(`Archivo Excel generado: ${fileName}`);
  } catch (error) {
    console.error('Error al generar el archivo Excel:', error);
    throw new Error('No se pudo generar el archivo Excel. Por favor, intente nuevamente.');
  }
};