import React from "react";
import { PieChart } from "@/components/ui/PieChart";
import type { EstadisticasEmpleados as EstadisticasEmpleadosType } from "@/interfaces/Api.interface";

interface EstadisticasEmpleadosProps {
  estadisticas: EstadisticasEmpleadosType;
}

export const EstadisticasEmpleados: React.FC<EstadisticasEmpleadosProps> = ({
  estadisticas,
}) => {
  const porcentajePendiente =
    100 -
    estadisticas.porcentajeCompletado -
    estadisticas.porcentajeReservado -
    estadisticas.porcentajeNoRespondio;

  const segments = [
    {
      value: estadisticas.porcentajeCompletado,
      color: "#10b981",
      label: "Completado",
    },
    {
      value: estadisticas.porcentajeReservado,
      color: "#fbbf24",
      label: "Reservado",
    },
    {
      value: estadisticas.porcentajeNoRespondio,
      color: "#ef4444",
      label: "No respondió",
    },
    {
      value: porcentajePendiente,
      color: "#6b7280",
      label: "Pendientes",
    },
  ].filter(segment => segment.value > 0);

  return (
    <div className="bg-white p-6 rounded-lg border border-gray-200">
      <h3 className="text-md font-medium text-gray-700 mb-4">
        Estado de Empleados
      </h3>

      <div className="flex flex-col items-center">
        <div className="mb-4">
          <PieChart
            segments={segments}
            size={192}
            centerContent={
              <span className="text-xs font-bold text-center">
                {estadisticas.totalEmpleadosAsignados}
                <br />
                total
              </span>
            }
            showLegend={false}
          />
        </div>

        <div className="w-full space-y-2">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <div
                className="w-3 h-3 rounded"
                style={{ backgroundColor: "#10b981" }}
              />
              <span className="text-sm">Completado</span>
            </div>
            <span className="text-sm font-medium">
              {estadisticas.porcentajeCompletado.toFixed(1)}%
            </span>
          </div>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <div
                className="w-3 h-3 rounded"
                style={{ backgroundColor: "#fbbf24" }}
              />
              <span className="text-sm">Reservado</span>
            </div>
            <span className="text-sm font-medium">
              {estadisticas.porcentajeReservado.toFixed(1)}%
            </span>
          </div>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <div
                className="w-3 h-3 rounded"
                style={{ backgroundColor: "#ef4444" }}
              />
              <span className="text-sm">No respondió</span>
            </div>
            <span className="text-sm font-medium">
              {estadisticas.porcentajeNoRespondio.toFixed(1)}%
            </span>
          </div>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <div
                className="w-3 h-3 rounded"
                style={{ backgroundColor: "#6b7280" }}
              />
              <span className="text-sm">Pendientes</span>
            </div>
            <span className="text-sm font-medium">
              {porcentajePendiente.toFixed(1)}%
            </span>
          </div>
        </div>
      </div>
    </div>
  );
};