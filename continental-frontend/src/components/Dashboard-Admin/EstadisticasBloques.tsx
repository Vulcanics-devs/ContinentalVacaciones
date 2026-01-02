import React from "react";
import { PieChart } from "@/components/ui/PieChart";

interface EstadisticasBloquesProps {
  totalBloques: number;
  bloquesCompletados: number;
}

export const EstadisticasBloques: React.FC<EstadisticasBloquesProps> = ({
  totalBloques,
  bloquesCompletados,
}) => {
  const bloquesPendientes = totalBloques - bloquesCompletados;
  const porcentajeCompletado = totalBloques > 0
    ? Math.round((bloquesCompletados / totalBloques) * 100)
    : 0;

  const segments = [
    {
      value: bloquesCompletados,
      color: "#10b981",
      label: "Completados",
    },
    {
      value: bloquesPendientes,
      color: "#3b82f6",
      label: "Pendientes",
    },
  ];

  return (
    <div className="bg-white p-6 rounded-lg border border-gray-200">
      <h3 className="text-md font-medium text-gray-700 mb-4">
        Estado de Bloques
      </h3>

      <div className="flex flex-col items-center">
        <div className="mb-4">
          <PieChart
            segments={segments}
            size={192}
            centerContent={
              <span className="text-sm font-bold">{porcentajeCompletado}%</span>
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
              <span className="text-sm">Completados</span>
            </div>
            <span className="text-sm font-medium">
              {bloquesCompletados} / {totalBloques}
            </span>
          </div>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <div
                className="w-3 h-3 rounded"
                style={{ backgroundColor: "#3b82f6" }}
              />
              <span className="text-sm">Pendientes</span>
            </div>
            <span className="text-sm font-medium">{bloquesPendientes}</span>
          </div>
        </div>
      </div>
    </div>
  );
};