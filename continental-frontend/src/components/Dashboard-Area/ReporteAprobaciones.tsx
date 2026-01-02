/**
 * =============================================================================
 * REPORTE DE APROBACIONES EN DÍAS LLENOS
 * =============================================================================
 * 
 * @description
 * Componente con filtro por MES para generar reportes de aprobaciones
 * automáticas vs manuales en días llenos.
 * 
 * @used_in
 * - src/components/Dashboard-Area/Reportes.tsx
 * 
 * @user_roles
 * - Jefe de Área
 * 
 * @author Vulcanics Dev Team
 * @created 2025-12-10
 * =============================================================================
 */

import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";
import { solicitudesService } from "@/services/solicitudesService";
import {
    exportarAprobacionesExcel,
    transformarSolicitudAAprobacion,
    type SolicitudAprobacionData
} from "@/utils/aprobacionesExcel";
import type { Solicitud } from "@/interfaces/Solicitudes.interface";
//import { SolicitudReprogramacion } from "@/interfaces/Api.interface";
import {
    Download,
    TrendingUp,
    Clock,
    CheckCircle,
    AlertCircle,
    Calendar as CalendarIcon,
} from "lucide-react";

const MESES = [
    { value: 1, label: "Enero" },
    { value: 2, label: "Febrero" },
    { value: 3, label: "Marzo" },
    { value: 4, label: "Abril" },
    { value: 5, label: "Mayo" },
    { value: 6, label: "Junio" },
    { value: 7, label: "Julio" },
    { value: 8, label: "Agosto" },
    { value: 9, label: "Septiembre" },
    { value: 10, label: "Octubre" },
    { value: 11, label: "Noviembre" },
    { value: 12, label: "Diciembre" },
];

const AÑOS = [2024, 2025, 2026, 2027, 2028, 2029, 2030];

// ✅ SIN FILTRO DE PORCENTAJE
const PORCENTAJE_DIA_LLENO = 0;

interface ReporteAprobacionesProps {
    selectedArea: number | "all";
    selectedAreaName: string;
}

export const ReporteAprobaciones = ({
    selectedArea,
    selectedAreaName
}: ReporteAprobacionesProps) => {
    const [loading, setLoading] = useState(false);
    const [selectedMes, setSelectedMes] = useState<number | "all">("all");
    const [selectedAnio, setSelectedAnio] = useState<number>(new Date().getFullYear());
    const [previewData, setPreviewData] = useState<{
        total: number;
        automaticas: number;
        manuales: number;
    } | null>(null);

    // Filtrar solicitud por mes/año
    const filtrarPorPeriodo = (solicitud: Solicitud): boolean => {
        if (selectedMes === "all") return true;

        const fechaAprobacion = solicitud.fechaAprobacion || solicitud.fechaSolicitud;
        if (!fechaAprobacion || fechaAprobacion === '0001-01-01') return false;

        try {
            const fecha = new Date(fechaAprobacion);
            return fecha.getMonth() + 1 === selectedMes && fecha.getFullYear() === selectedAnio;
        } catch {
            return false;
        }
    };

    // Filtrar por días llenos
    const esDiaLleno = (solicitud: Solicitud): boolean => {
        if (PORCENTAJE_DIA_LLENO === 0) return true;
        return (solicitud.porcentajeCalculado || 0) >= PORCENTAJE_DIA_LLENO;
    };

    // Previsualizar cantidad de registros
    const previsualizarDatos = async () => {
        try {
            const response = await solicitudesService.getSolicitudesList({
                areaId: selectedArea === "all" ? undefined : selectedArea,
                estadoSolicitud: "Aprobada",
            });

            const solicitudes = response?.solicitudes || [];

            // ✅ Aplicar filtros
            const filtradas = solicitudes
                .filter(filtrarPorPeriodo)
                .filter(esDiaLleno);

            // ✅ Debug detallado
            console.log('📊 Reporte Aprobaciones - Preview:', {
                totalAprobadas: solicitudes.length,
                filtradas: filtradas.length,
                mes: selectedMes === "all" ? "Todos" : MESES.find(m => m.value === selectedMes)?.label,
                anio: selectedAnio,
                area: selectedArea === "all" ? "Todas" : selectedAreaName,
                primerasSolicitudes: solicitudes.slice(0, 3).map(s => ({
                    id: s.id,
                    empleado: s.nombreEmpleado,
                    fechaAprobacion: s.fechaAprobacion,
                    estado: s.estadoSolicitud
                }))
            });

            // Contar automáticas vs manuales
            const automaticas = filtradas.filter(sol => {
                const solicitadoPor = sol.solicitadoPor?.toLowerCase() || '';
                return solicitadoPor.includes("sistema") ||
                    solicitadoPor.includes("automático") ||
                    solicitadoPor.includes("automatico") ||
                    solicitadoPor === "system";
            }).length;

            const manuales = filtradas.length - automaticas;

            setPreviewData({
                total: filtradas.length,
                automaticas,
                manuales,
            });

        } catch (error) {
            console.error("Error al previsualizar datos", error);
            setPreviewData({ total: 0, automaticas: 0, manuales: 0 });
        }
    };

    useEffect(() => {
        previsualizarDatos();
    }, [selectedArea, selectedMes, selectedAnio]);

    const handleGenerarReporte = async () => {
        setLoading(true);
        try {
            // Obtener solicitudes aprobadas
            const response = await solicitudesService.getSolicitudesList({
                areaId: selectedArea === "all" ? undefined : selectedArea,
                estadoSolicitud: "Aprobada",
            });

            let solicitudes = response?.solicitudes || [];

            console.log('📊 Total solicitudes aprobadas antes de filtrar:', solicitudes.length);

            // ✅ APLICAR FILTROS
            solicitudes = solicitudes
                .filter(filtrarPorPeriodo)
                .filter(esDiaLleno);

            console.log('📊 Solicitudes después de filtrar:', solicitudes.length);

            if (solicitudes.length === 0) {
                toast.info("No se encontraron solicitudes aprobadas con los filtros seleccionados.");
                return;
            }

            // Transformar datos para el reporte
            const datosReporte: SolicitudAprobacionData[] = solicitudes.map(transformarSolicitudAAprobacion);

            // Generar nombre del periodo
            const mesTexto = selectedMes === "all"
                ? `Todos los meses de ${selectedAnio}`
                : `${MESES.find(m => m.value === selectedMes)?.label} ${selectedAnio}`;

            // Generar Excel
            exportarAprobacionesExcel(datosReporte, {
                titulo: "Reporte de Aprobaciones en Dias Llenos",
                mesAnio: mesTexto,
                tipoFiltro: "todas",
                area: selectedAreaName,
            });

            toast.success(`Reporte generado con ${datosReporte.length} registro(s).`);

        } catch (error: any) {
            console.error("Error al generar reporte:", error);
            toast.error(error?.message || "No se pudo generar el reporte.");
        } finally {
            setLoading(false);
        }
    };

    const mesTexto = selectedMes === "all"
        ? "Todos los meses"
        : MESES.find(m => m.value === selectedMes)?.label || "";

    return (
        <div className="rounded-lg border border-gray-200 p-4 bg-white space-y-4">
            <div className="flex items-start gap-3">
                <div className="flex-shrink-0 mt-1">
                    <CheckCircle size={20} className="text-green-600" />
                </div>
                <div className="flex-1">
                    <h4 className="font-semibold text-gray-900 mb-1">
                        Aprobaciones en Días Llenos
                    </h4>
                    <p className="text-sm text-gray-600 mb-3">
                        Solicitudes aprobadas (automáticas o por jefe) en el periodo seleccionado.
                    </p>

                    {/* Filtros de Mes y Año */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-3 mb-3">
                        <div className="flex flex-col">
                            <label className="text-sm font-medium text-gray-700 mb-1">
                                Año
                            </label>
                            <select
                                className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-continental-yellow focus:border-transparent"
                                value={selectedAnio}
                                onChange={(e) => setSelectedAnio(parseInt(e.target.value, 10))}
                            >
                                {AÑOS.map((anio) => (
                                    <option key={anio} value={anio}>
                                        {anio}
                                    </option>
                                ))}
                            </select>
                        </div>

                        <div className="flex flex-col">
                            <label className="text-sm font-medium text-gray-700 mb-1">
                                Mes
                            </label>
                            <select
                                className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-continental-yellow focus:border-transparent"
                                value={selectedMes}
                                onChange={(e) =>
                                    setSelectedMes(e.target.value === "all" ? "all" : parseInt(e.target.value, 10))
                                }
                            >
                                <option value="all">Todos los meses</option>
                                {MESES.map((mes) => (
                                    <option key={mes.value} value={mes.value}>
                                        {mes.label}
                                    </option>
                                ))}
                            </select>
                        </div>
                    </div>

                    {/* Vista previa de datos */}
                    {previewData !== null && (
                        <div className="flex flex-wrap items-center gap-4 p-3 bg-gray-50 rounded-lg mb-3">
                            <div className="flex items-center gap-2 text-sm">
                                <CalendarIcon size={16} className="text-gray-500" />
                                <span className="text-gray-600">Periodo:</span>
                                <span className="font-semibold text-gray-900">
                                    {mesTexto} {selectedAnio}
                                </span>
                            </div>
                            <div className="flex items-center gap-2 text-sm">
                                <TrendingUp size={16} className="text-gray-500" />
                                <span className="text-gray-600">Total:</span>
                                <span className="font-semibold text-gray-900">{previewData.total}</span>
                            </div>
                            <div className="flex items-center gap-2 text-sm">
                                <Clock size={16} className="text-blue-500" />
                                <span className="text-gray-600">Automáticas:</span>
                                <span className="font-semibold text-blue-600">{previewData.automaticas}</span>
                            </div>
                            <div className="flex items-center gap-2 text-sm">
                                <CheckCircle size={16} className="text-green-500" />
                                <span className="text-gray-600">Por Jefe:</span>
                                <span className="font-semibold text-green-600">{previewData.manuales}</span>
                            </div>
                        </div>
                    )}

                    {/* Mensaje informativo si no hay datos */}
                    {previewData && previewData.total === 0 && (
                        <div className="flex items-start gap-2 p-3 bg-amber-50 border border-amber-200 rounded-lg text-sm mb-3">
                            <AlertCircle size={16} className="text-amber-600 mt-0.5" />
                            <div className="text-amber-800">
                                No se encontraron solicitudes aprobadas en <strong>{mesTexto} {selectedAnio}</strong>.
                                <br />
                                <span className="text-xs text-amber-600">
                                    Intenta cambiar el mes, año o área. Revisa la consola para ver datos disponibles.
                                </span>
                            </div>
                        </div>
                    )}

                    {/* Botón de descarga */}
                    <Button
                        onClick={handleGenerarReporte}
                        variant="continental"
                        size="sm"
                        className="flex items-center gap-2"
                        disabled={loading || !previewData || previewData.total === 0}
                    >
                        {loading ? (
                            "Generando..."
                        ) : (
                            <>
                                <Download size={16} />
                                Descargar Excel ({previewData?.total || 0} registros)
                            </>
                        )}
                    </Button>
                </div>
            </div>
        </div>
    );
};