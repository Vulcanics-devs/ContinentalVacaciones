import * as XLSX from 'xlsx';
import { saveAs } from 'file-saver';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import type { WeeklyRoleEntry } from '@/services/rolesService';

export interface WeeklyRolesSheetInput {
    areaId?: number;
    areaName: string;
    groupName: string;
    grupoId: number;
    weekStart: Date;
    weekDays: Date[];
    roles: WeeklyRoleEntry[];
    employees: { id: number; nomina: string; fullName: string }[];
}

interface WeeklyRolesExportMeta {
    areaFilterName?: string;
    generatedFrom?: string;
}

/**
 * Genera un Excel con una hoja por Grupo (incluye nombre de Area y semana)
 */
export const exportWeeklyRolesExcel = (
    inputs: WeeklyRolesSheetInput[],
    meta?: WeeklyRolesExportMeta
): void => {
    const workbook = XLSX.utils.book_new();

    // Agrupar por grupo para evitar nombres de hoja duplicados y permitir varias semanas del mismo grupo
    const grouped = new Map<number, WeeklyRolesSheetInput[]>();
    inputs.forEach((input) => {
        const key = input.grupoId;
        if (!grouped.has(key)) grouped.set(key, []);
        grouped.get(key)!.push(input);
    });

    grouped.forEach((groupInputs, grupoId) => {
        // Ordenar por fecha de semana por consistencia
        groupInputs.sort((a, b) => a.weekStart.getTime() - b.weekStart.getTime());

        const areaName = groupInputs[0]?.areaName || "";
        const groupName = groupInputs[0]?.groupName || `Grupo ${grupoId}`;
        const rows: Record<string, string>[] = [];

        // Encabezado
        const header: Record<string, string> = {
            Area: "Area",
            Grupo: "Grupo",
            Nomina: "Nomina",
            Empleado: "Empleado",
            Semana: "Semana",
        };
        // Usar los días de la primera semana para los encabezados D0..D6 (todas las semanas tienen 7 días)
        const headerDays = groupInputs[0]?.weekDays || [];
        headerDays.forEach((day, idx) => {
            header[`D${idx}`] = `${format(day, "EEE", { locale: es })} ${format(day, "dd/MM", { locale: es })}`;
        });
        rows.push(header);

        groupInputs.forEach((group, idx) => {
            const weekLabel = `${format(group.weekDays[0], "dd MMM", { locale: es })} - ${format(
                group.weekDays[6],
                "dd MMM yyyy",
                { locale: es }
            )}`;

            // Fila separadora opcional entre semanas
            if (idx > 0) {
                rows.push({});
            }

            group.employees.forEach((emp) => {
                const row: Record<string, string> = {
                    Area: areaName,
                    Grupo: groupName,
                    Nomina: emp.nomina,
                    Empleado: emp.fullName,
                    Semana: weekLabel,
                };

                group.weekDays.forEach((day, dIdx) => {
                    const dateStr = format(day, "yyyy-MM-dd");
                    const entry = group.roles.find((s) => s.empleado.id === emp.id && s.fecha === dateStr);
                    row[`D${dIdx}`] = entry?.codigoTurno || "";
                });

                rows.push(row);
            });
        });

        const sheet = XLSX.utils.json_to_sheet(rows, { skipHeader: true });
        sheet["!cols"] = [
            { wch: 20 }, // Area
            { wch: 20 }, // Grupo
            { wch: 12 }, // Nomina
            { wch: 30 }, // Empleado
            { wch: 22 }, // Semana
            { wch: 18 },
            { wch: 18 },
            { wch: 18 },
            { wch: 18 },
            { wch: 18 },
            { wch: 18 },
            { wch: 18 },
        ];
        const sheetName = `${groupName}`.slice(0, 31) || `Grupo_${grupoId}`;
        XLSX.utils.book_append_sheet(workbook, sheet, sheetName);
    });

    const fileName = `RolesSemanales_${meta?.areaFilterName || 'todas'}_${format(new Date(), 'yyyyMMdd_HHmmss')}.xlsx`;
    const buffer = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
    const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
    saveAs(blob, fileName);
};

export default exportWeeklyRolesExcel;
