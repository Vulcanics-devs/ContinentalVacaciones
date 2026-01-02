import { useEffect, useMemo, useState } from "react";
import { Button } from "@/components/ui/button";
import { DateRangeInput } from "@/components/ui/date-range-input";
import { Card, CardContent } from "@/components/ui/card";
import { downloadConstanciaAntiguedadPDF } from "@/services/pdfService";
import { toast } from "sonner";
import { empleadosService } from "@/services/empleadosService";
import { solicitudesService } from "@/services/solicitudesService";
import { areasService } from "@/services/areasService";
import { useVacationConfig } from "@/hooks/useVacationConfig";
import type { Area } from "@/interfaces/Areas.interface";
import { PeriodOptions } from "@/interfaces/Calendar.interface";
import { exportarReprogramacionesExcel } from "@/utils/reprogramacionesExcel";
import { ReporteAprobaciones } from "./ReporteAprobaciones";
import { reportesService } from "@/services/reportesService";
import { vacacionesService } from "@/services/vacacionesService";
import { generarExcelVacacionesAsignadasEmpresa } from "@/utils/vacacionesAsignadasEmpresaExcel";
import type { EmpleadoDetalle } from "@/interfaces/Api.interface";
import {
  Download,
  Palmtree,
  Calendar,
  RefreshCw,
  FileText,
  Award,
  ShieldCheck,
  ShieldAlert,
  FileSpreadsheet
} from "lucide-react";

const MIN_REPROGRAMACION_DATE = "2026-01-01";

const calculateAntiguedadAlCierre = (fechaIngreso: string | null | undefined, targetYear: number): number => {
  if (!fechaIngreso) return 0;
  const normalized = fechaIngreso.includes("T") ? fechaIngreso : `${fechaIngreso}T00:00:00`;
  const ingreso = new Date(normalized);
  if (Number.isNaN(ingreso.getTime())) return 0;
  const referenceDate = new Date(targetYear, 11, 31);
  let years = referenceDate.getFullYear() - ingreso.getFullYear();
  const monthDiff = referenceDate.getMonth() - ingreso.getMonth();
  if (monthDiff < 0 || (monthDiff === 0 && referenceDate.getDate() < ingreso.getDate())) {
    years -= 1;
  }
  return Math.max(years, 0);
};

const formatFechaMMDDYYYY = (fecha: string | undefined | null): string => {
  if (!fecha) return "";
  const normalized = fecha.includes("T") ? fecha : `${fecha}T00:00:00`;
  const date = new Date(normalized);
  if (Number.isNaN(date.getTime())) return "";
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  const year = date.getFullYear();
  return `${month}/${day}/${year}`;
};

export const Reportes = () => {
  const [startDate, setStartDate] = useState<string>("");
  const [endDate, setEndDate] = useState<string>("");
  const [areas, setAreas] = useState<Area[]>([]);
  const [selectedArea, setSelectedArea] = useState<number | "all">("all");
  const [loadingGeneral, setLoadingGeneral] = useState(false);
  const [loadingFiltered, setLoadingFiltered] = useState(false);
  const { currentPeriod, loading: configLoading, config } = useVacationConfig();
  const isReprogramming = currentPeriod === PeriodOptions.reprogramming;
  const [selectedYear, setSelectedYear] = useState<string>("");

  useEffect(() => {
    if (config?.anioVigente && !selectedYear) {
      setSelectedYear(config.anioVigente.toString());
    }
  }, [config, selectedYear]);

  useEffect(() => {
    const loadAreas = async () => {
      try {
        const fetched = await areasService.getAreas();
        const ordered = [...fetched].sort((a, b) => (a.areaId || 0) - (b.areaId || 0));
        setAreas(ordered);
      } catch (error) {
        console.error("Error cargando areas", error);
        toast.error("No se pudieron cargar las areas");
      }
    };
    loadAreas();
  }, []);

  const selectedAreaName = useMemo(() => {
    if (selectedArea === "all") return "Todas";
    const found = areas.find((a) => a.areaId === selectedArea);
    return found?.nombreGeneral || `Área ${selectedArea}`;
  }, [areas, selectedArea]);

  const otherReportCards = [
    {
      id: "vacaciones",
      icon: Palmtree,
      title: "Reporte de Vacaciones",
      subtitle: "Reporte con los empleados en vacaciones."
    },
    {
      id: "constancia",
      icon: Award,
      title: "Constancia de Antiguedad",
      subtitle: "Constancia de antiguedad y vacaciones adicionales para empleados del área."
    },
    {
      id: "vacaciones-area",
      icon: FileSpreadsheet,
      title: "Vacaciones Programadas por Área",
      subtitle: "Exporta todas las vacaciones programadas agrupadas por área en formato Excel."
    }
  ];

  const ensureReprogramming = (): boolean => {
    if (!isReprogramming) {
      toast.error("Disponible solo durante el periodo de reprogramacion.");
      return false;
    }
    return true;
  };

  const handleDownloadConstancia = async () => {
    if (!selectedYear) {
      toast.error("Selecciona el año para generar la constancia de antiguedad");
      return;
    }

    const areaId = selectedArea === "all" ? undefined : selectedArea;
    let loadingToast: string | number | undefined;

    const runWithTimeout = async <T,>(promise: Promise<T>, ms: number, label: string): Promise<T> =>
      new Promise((resolve, reject) => {
        const timer = setTimeout(() => reject(new Error(`Tiempo de espera agotado (${label})`)), ms);
        promise
          .then((res) => {
            clearTimeout(timer);
            resolve(res);
          })
          .catch((err) => {
            clearTimeout(timer);
            reject(err);
          });
      });

    try {
      loadingToast = toast.loading("Generando PDF de Constancia de Antiguedad...");

      const empleadosResponse = await runWithTimeout(
        empleadosService.getEmpleadosSindicalizados({
          AreaId: areaId,
          Page: 1,
          PageSize: 1000
        }),
        60000,
        "empleados sindicalizados"
      );

      const empleadosLista = empleadosResponse.usuarios || [];
      const empleadosPorNomina = new Map(empleadosLista.map((emp) => [String(emp.nomina), emp]));
      const nominasFiltradas = new Set(
        empleadosLista
          .filter((emp) => !selectedArea || selectedArea === "all" || emp.area?.areaId === selectedArea)
          .map((emp) => String(emp.nomina))
      );

      const vacacionesAsignadas = await runWithTimeout(
        vacacionesService.getVacacionesAsignadas(
          {
            areaId: areaId,
            anio: parseInt(selectedYear),
            incluirDetalleEmpleado: true,
            incluirResumenPorGrupo: false,
            incluirResumenPorArea: false
          },
          { timeout: 120000 }
        ),
        120000,
        "vacaciones asignadas"
      );

      const detalleFiltrado = (vacacionesAsignadas.empleadosDetalle || []).filter((empleado) =>
        nominasFiltradas.size > 0 ? nominasFiltradas.has(String(empleado.nomina)) : true
      );

      if (!detalleFiltrado || detalleFiltrado.length === 0) {
        toast.error("No se encontraron empleados con vacaciones asignadas para los criterios seleccionados");
        return;
      }

      const empleadosData = detalleFiltrado.map((empleado: EmpleadoDetalle) => {
        const resumen = empleado.resumen;
        const vacaciones = empleado.vacaciones || [];

        const diasVacacionesCorresponden =
          (resumen?.diasAsignadosAutomaticamente || 0) + (resumen?.diasProgramables || 0);
        const diasAdicionales = resumen?.diasProgramables || 0;

        const diasProgramados = vacaciones
          .filter((v: any) => v.estadoVacacion === "Activa")
          .map((v: any) => {
            const [year, month, day] = v.fechaVacacion.split("-");
            const fechaFormateada = `${month}/${day}/${year}`;
            return {
              de: fechaFormateada,
              al: fechaFormateada,
              dias: 1,
              tipoVacacion: v.tipoVacacion
            };
          });

        const totalVacacionesAsignadas = vacaciones.filter((v: any) => v.estadoVacacion === "Activa").length;
        const totalAutomaticas = vacaciones.filter(
          (v: any) => v.tipoVacacion === "Automatica" && v.estadoVacacion === "Activa"
        ).length;
        const totalAnuales = vacaciones.filter((v: any) => v.tipoVacacion === "Anual" && v.estadoVacacion === "Activa")
          .length;

        const totalProgramados = totalVacacionesAsignadas;
        const porProgramar = (empleado.totalVacaciones || 0) - totalAutomaticas - totalAnuales;
        const totalGozados = 0;
        const porGozar = empleado.totalVacaciones || 0;

        const empleadoInfo = empleado.nomina ? empleadosPorNomina.get(String(empleado.nomina)) : undefined;
        const fechaIngresoRaw = empleadoInfo?.fechaIngreso || (empleado as any).fechaIngreso || "";
        const fechaIngreso = fechaIngresoRaw ? formatFechaMMDDYYYY(fechaIngresoRaw) : "";
        const antiguedadAnios = calculateAntiguedadAlCierre(fechaIngresoRaw, parseInt(selectedYear));

        return {
          nomina: empleado.nomina || "N/A",
          nombre: empleado.nombreCompleto || "N/A",
          fechaIngreso: fechaIngreso || "",
          antiguedadAnios,
          diasVacacionesCorresponden,
          diasAdicionales,
          diasProgramados,
          diasGozados: [],
          totalProgramados,
          porProgramar: Math.max(0, porProgramar),
          totalGozados,
          porGozar
        };
      });

      const pdfData = {
        empleados: empleadosData,
        area: selectedAreaName,
        grupos: selectedArea === "all" ? ["Todas"] : [selectedAreaName],
        periodo: {
          inicio: `01/01/${selectedYear}`,
          fin: `12/31/${selectedYear}`
        },
        targetYear: parseInt(selectedYear)
      };

      await downloadConstanciaAntiguedadPDF(pdfData);
      toast.success(`PDF de Constancia de Antiguedad generado para ${empleadosData.length} empleado(s)`);
    } catch (error) {
      console.error("Error generating PDF (Dashboard-Area):", error);
      toast.error(error instanceof Error ? error.message : "Error al generar el PDF de Constancia de Antiguedad");
    } finally {
      if (loadingToast !== undefined) {
        toast.dismiss(loadingToast);
      } else {
        toast.dismiss();
      }
    }
  };

  const validateFiltered = (): boolean => {
    if (!startDate || !endDate) {
      toast.error("Captura el rango de fechas (inicio y fin).");
      return false;
    }
    if (new Date(startDate) < new Date(MIN_REPROGRAMACION_DATE)) {
      toast.error("El calendario de reprogramaciones comienza en 2026.");
      return false;
    }
    if (new Date(startDate) > new Date(endDate)) {
      toast.error("La fecha de inicio no puede ser mayor a la fecha fin.");
      return false;
    }
    if (selectedArea === "all") {
      toast.error("Selecciona el área para filtrar.");
      return false;
    }
    return true;
  };

  const handleReprogGeneral = async () => {
    if (!ensureReprogramming()) return;
    setLoadingGeneral(true);
    try {
      const response = await solicitudesService.getSolicitudesList({
        areaId: selectedArea === "all" ? undefined : selectedArea,
        fechaDesde: undefined,
        fechaHasta: undefined
      });

      const solicitudes = response?.solicitudes || [];
      if (solicitudes.length === 0) {
        toast.info("No hay reprogramaciones con los filtros seleccionados.");
        return;
      }

      exportarReprogramacionesExcel(solicitudes, {
        titulo: "Reporte general de reprogramaciones",
        tipo: "general",
        area: selectedAreaName,
        fechaDesde: undefined,
        fechaHasta: undefined
      });
      toast.success("Reporte general descargado.");
    } catch (error: any) {
      console.error("Error al descargar reprogramaciones generales", error);
      toast.error(error?.message || "No se pudo generar el reporte general de reprogramaciones.");
    } finally {
      setLoadingGeneral(false);
    }
  };

  const handleReprogFiltrado = async () => {
    if (!ensureReprogramming()) return;
    if (!validateFiltered()) return;
    setLoadingFiltered(true);
    try {
      const response = await solicitudesService.getSolicitudesList({
        areaId: selectedArea === "all" ? undefined : selectedArea
      });

      const solicitudes = response?.solicitudes || [];
      const start = new Date(startDate);
      const end = new Date(endDate);
      const isInRange = (value?: string | null) => {
        if (!value) return false;
        const current = new Date(value);
        if (Number.isNaN(current.getTime())) return false;
        return current >= start && current <= end;
      };

      const filteredByDates = solicitudes.filter((s) => isInRange(s.fechaOriginal) || isInRange(s.fechaNueva));

      if (filteredByDates.length === 0) {
        toast.info("No se encontraron reprogramaciones usando fecha original o nueva en ese rango.");
        return;
      }

      exportarReprogramacionesExcel(filteredByDates, {
        titulo: "Reprogramaciones por área y fecha",
        tipo: "hu57",
        area: selectedAreaName,
        fechaDesde: startDate,
        fechaHasta: endDate
      });
      toast.success("Reporte filtrado descargado.");
    } catch (error: any) {
      console.error("Error al descargar reporte filtrado", error);
      toast.error(error?.message || "No se pudo generar el reporte filtrado.");
    } finally {
      setLoadingFiltered(false);
    }
  };

  const handleDownloadVacacionesAsignadas = async () => {
    try {
      if (!selectedYear) {
        toast.error("Selecciona el año para generar el reporte");
        return;
      }

      const loadingToast = toast.loading("Generando listado de vacaciones asignadas por la empresa...");

      const data = await reportesService.obtenerVacacionesEmpresa({
        anio: parseInt(selectedYear),
        areaId: selectedArea === "all" ? undefined : selectedArea,
        grupoId: undefined
      });

      toast.dismiss(loadingToast);

      if (!data.vacaciones.length) {
        toast.info("No hay vacaciones asignadas por la empresa con los filtros seleccionados");
        return;
      }

      generarExcelVacacionesAsignadasEmpresa(data);
      toast.success(`Reporte descargado con ${data.totalVacaciones} vacaciones asignadas por la empresa`);
    } catch (error) {
      console.error("Error al descargar vacaciones asignadas por la empresa:", error);
      toast.dismiss();
      toast.error(
        error instanceof Error ? error.message : "No se pudo generar el reporte de vacaciones asignadas por la empresa"
      );
    }
  };

  return (
    <div className="p-6 bg-white min-h-screen">
      <div className="max-w-7xl mx-auto space-y-8">
        <div className="space-y-2">
          <div className="text-[25px] font-bold text-continental-black text-left">Descargar Reportes</div>
          <p className="text-[16px] font-medium text-continental-black text-left">
            Accede y descarga los reportes más relevantes
          </p>
        </div>

        <div className="rounded-xl border border-gray-200 p-4 bg-gray-50">
          <div className="flex items-center gap-3 mb-3">
            {isReprogramming ? (
              <span className="inline-flex items-center gap-2 text-sm text-green-700 bg-green-50 border border-green-200 rounded-full px-3 py-1">
                <ShieldCheck className="w-4 h-4" /> Periodo de reprogramación activo
              </span>
            ) : (
              <span className="inline-flex items-center gap-2 text-sm text-amber-700 bg-amber-50 border border-amber-200 rounded-full px-3 py-1">
                <ShieldAlert className="w-4 h-4" /> Los reportes de reprogramación solo se habilitan en reprogramación
              </span>
            )}
          </div>

          <DateRangeInput
            startDate={startDate}
            endDate={endDate}
            onStartDateChange={setStartDate}
            onEndDateChange={setEndDate}
            minDate={MIN_REPROGRAMACION_DATE}
            title="Periodo del reporte (mínimo 2026) - opcional"
            startLabel="Fecha inicio (opcional)"
            endLabel="Fecha fin (opcional)"
          />

          <div className="mt-4 flex flex-col md:flex-row gap-3">
            <div className="flex flex-col">
              <label className="text-sm font-semibold text-gray-700">Área</label>
              <select
                className="border border-gray-300 rounded-lg px-3 py-2 text-sm mt-1"
                value={selectedArea}
                onChange={(e) => setSelectedArea(e.target.value === "all" ? "all" : parseInt(e.target.value, 10))}
                disabled={configLoading}
              >
                <option value="all">Todas</option>
                {areas.map((area) => (
                  <option key={area.areaId} value={area.areaId}>
                    {area.nombreGeneral}
                  </option>
                ))}
              </select>
            </div>
          </div>
        </div>

        <div className="space-y-4">
          <h2 className="text-base font-bold text-continental-black text-left">Control de Aprobaciones en Días Llenos</h2>
          <p className="text-sm text-gray-600">
            Reporta solicitudes aprobadas (automáticas o por jefe) en días con alta ocupación por vacaciones o permisos.
          </p>
          <ReporteAprobaciones selectedArea={selectedArea} selectedAreaName={selectedAreaName} />
        </div>

        <div className="space-y-4">
          <h2 className="text-base font-bold text-continental-black text-left">
            Reportes de reprogramaciones (solo en reprogramación)
          </h2>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Card className="rounded-xl border-gray-300">
              <CardContent className="p-6">
                <div className="flex items-start justify-between">
                  <div className="flex items-start gap-4">
                    <div className="flex-shrink-0">
                      <Calendar size={48} className="text-continental-black" />
                    </div>
                    <div className="space-y-2">
                      <h3 className="font-semibold text-continental-black">General de reprogramaciones</h3>
                      <p className="text-sm text-gray-600">Todas las reprogramaciones (por área opcional).</p>
                      <div className="flex items-center gap-2 text-xs text-gray-500">
                        <RefreshCw size={14} /> Incluye estatus y fechas originales/nuevas.
                      </div>
                    </div>
                  </div>
                  <Button
                    onClick={handleReprogGeneral}
                    variant="continental"
                    className="flex items-center gap-2"
                    disabled={loadingGeneral || !isReprogramming}
                  >
                    {loadingGeneral ? (
                      "Generando..."
                    ) : (
                      <>
                        <Download size={16} /> Descargar
                      </>
                    )}
                  </Button>
                </div>
              </CardContent>
            </Card>

            <Card className="rounded-xl border-gray-300">
              <CardContent className="p-6">
                <div className="flex items-start justify-between">
                  <div className="flex items-start gap-4">
                    <div className="flex-shrink-0">
                      <Calendar size={48} className="text-continental-black" />
                    </div>
                    <div className="space-y-2">
                      <h3 className="font-semibold text-continental-black">Reprogramaciones por área y fecha</h3>
                      <p className="text-sm text-gray-600">
                        Filtra por área y rango de fechas (a partir de 2026) usando fechas originales o nuevas.
                      </p>
                      <div className="flex items-center gap-2 text-xs text-gray-500">
                        <FileText size={14} /> Reporte listo para análisis y consulta.
                      </div>
                    </div>
                  </div>
                  <Button
                    onClick={handleReprogFiltrado}
                    variant="continental"
                    className="flex items-center gap-2"
                    disabled={loadingFiltered || !isReprogramming}
                  >
                    {loadingFiltered ? (
                      "Generando..."
                    ) : (
                      <>
                        <Download size={16} /> Descargar
                      </>
                    )}
                  </Button>
                </div>
              </CardContent>
            </Card>
          </div>
        </div>

        <div className="space-y-4">
          <h2 className="text-base font-bold text-continental-black text-left">Otros reportes</h2>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {otherReportCards.map((report) => (
              <Card key={report.id} className="rounded-xl border-gray-300">
                <CardContent className="p-6">
                  <div className="flex items-start justify-between">
                    <div className="flex items-start gap-4">
                      <div className="flex-shrink-0">
                        <report.icon size={48} className="text-continental-black" />
                      </div>

                      <div className="space-y-2">
                        <h3 className="font-semibold text-continental-black">{report.title}</h3>
                        <p className="text-sm text-gray-600">{report.subtitle}</p>
                        <div className="flex items-center gap-2">
                          <FileText size={16} className="text-gray-400" />
                        </div>
                      </div>
                    </div>

                    <Button
                      onClick={async () => {
                        if (report.id === "constancia") {
                          handleDownloadConstancia();
                        } else if (report.id === "vacaciones") {
                          handleDownloadVacacionesAsignadas();
                        } else if (report.id === "vacaciones-area") {
                            try {
                              const loadingToast = toast.loading("Generando reporte de vacaciones por area...");
                              const yearToExport = selectedYear ? parseInt(selectedYear) : undefined;
                              const areaIdToExport = selectedArea === "all" ? undefined : (selectedArea as number);
                              await reportesService.exportarVacacionesPorArea(yearToExport, areaIdToExport);
                              toast.dismiss(loadingToast);
                              toast.success("Reporte de vacaciones por area descargado exitosamente");
                            } catch (error) {
                              console.error("Error al descargar reporte de vacaciones por area:", error);
                              toast.dismiss();
                              toast.error(
                                error instanceof Error ? error.message : "No se pudo descargar el reporte de vacaciones por area"
                              );
                            }
                        } else {
                          toast.info("En construccion");
                        }
                      }}
                      variant="continental"
                      className="flex items-center gap-2"
                    >
                      <Download size={16} />
                      Descargar
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};
