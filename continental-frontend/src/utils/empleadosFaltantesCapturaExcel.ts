import * as XLSX from 'xlsx';
import { saveAs } from 'file-saver';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import type { EmpleadosFaltantesCapturaResponse } from '@/interfaces/Api.interface';

export const generarExcelEmpleadosFaltantesCaptura = (data: EmpleadosFaltantesCapturaResponse): void => {
  const workbook = XLSX.utils.book_new();

  const resumen = [
    { Concepto: 'Año', Valor: data.anio },
    { Concepto: 'Total empleados', Valor: data.totalEmpleados },
    { Concepto: 'En bloque cola', Valor: data.totalCriticos },
    {
      Concepto: 'Fecha del reporte',
      Valor: format(new Date(data.fechaReporte), "dd/MM/yyyy HH:mm", { locale: es })
    }
  ];

  const resumenSheet = XLSX.utils.json_to_sheet(resumen);
  resumenSheet['!cols'] = [{ wch: 28 }, { wch: 35 }];
  XLSX.utils.book_append_sheet(workbook, resumenSheet, 'Resumen');

  const detalle = data.empleados
    .slice()
    .sort((a, b) => {
      const area = a.nombreArea.localeCompare(b.nombreArea);
      if (area !== 0) return area;
      const grupo = a.nombreGrupo.localeCompare(b.nombreGrupo);
      if (grupo !== 0) return grupo;
      return a.numeroBloque - b.numeroBloque;
    })
    .map((empleado) => ({
      'ID Empleado': empleado.empleadoId,
      'Nomina': empleado.nomina,
      'Nombre': empleado.nombreCompleto,
      'Area': empleado.nombreArea,
      'Grupo': empleado.nombreGrupo,
      'Bloque': empleado.numeroBloque,
      'En Bloque Cola': empleado.esBloqueCola ? 'Si' : 'No',
      'Fecha Limite Bloque': format(new Date(empleado.fechaLimiteBloque), "dd/MM/yyyy HH:mm", { locale: es }),
      'Fecha Asignacion': format(new Date(empleado.fechaAsignacion), "dd/MM/yyyy HH:mm", { locale: es }),
      'Maquina': empleado.maquina ?? '',
      'Observaciones': empleado.observaciones ?? '',
      'Requiere Accion': empleado.requiereAccionUrgente ? 'Si' : 'No',
    }));

  const detalleSheet = XLSX.utils.json_to_sheet(detalle);
  detalleSheet['!cols'] = [
    { wch: 10 }, // ID
    { wch: 12 }, // Nomina
    { wch: 32 }, // Nombre
    { wch: 22 }, // Area
    { wch: 18 }, // Grupo
    { wch: 8 },  // Bloque
    { wch: 14 }, // Cola
    { wch: 22 }, // Fecha limite
    { wch: 22 }, // Fecha asignacion
    { wch: 15 }, // Maquina
    { wch: 40 }, // Observaciones
    { wch: 15 }, // Requiere accion
  ];
  XLSX.utils.book_append_sheet(workbook, detalleSheet, 'Faltantes');

  const buffer = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
  const blob = new Blob([buffer], {
    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
  });

  const fileName = `Empleados_Faltantes_Vacaciones_${data.anio}_${format(new Date(), 'yyyyMMdd_HHmmss')}.xlsx`;
  saveAs(blob, fileName);
};
