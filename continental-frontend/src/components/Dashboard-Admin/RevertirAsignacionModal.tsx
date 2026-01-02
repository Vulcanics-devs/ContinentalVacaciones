/**
 * Modal de Confirmación para Revertir Asignación Automática
 * Componente reutilizable para confirmar la reversión de asignaciones automáticas
 */

import { Button } from "@/components/ui/button";
import { Trash2, AlertTriangle, X } from "lucide-react";
import type { ResumenAsignacionAutomaticaResponse } from "@/interfaces/Api.interface";

interface RevertirAsignacionModalProps {
  isOpen: boolean;
  isReverting: boolean;
  anioVigente: number;
  resumenAsignacion: ResumenAsignacionAutomaticaResponse | null;
  onConfirmar: () => void;
  onCancelar: () => void;
}

export const RevertirAsignacionModal = ({
  isOpen,
  isReverting,
  anioVigente,
  resumenAsignacion,
  onConfirmar,
  onCancelar
}: RevertirAsignacionModalProps) => {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Overlay */}
      <div 
        className="fixed inset-0 bg-black/50 backdrop-blur-sm" 
        onClick={onCancelar}
      />
      
      {/* Modal */}
      <div className="relative bg-white rounded-lg shadow-xl max-w-md w-full mx-4">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b">
          <h2 className="text-xl font-semibold text-gray-900">
            Confirmar Reversión
          </h2>
          <button
            onClick={onCancelar}
            disabled={isReverting}
            className="text-gray-400 hover:text-gray-600 disabled:opacity-50"
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        {/* Content */}
        <div className="p-6">
          <div className="flex items-center gap-4 mb-4">
            <div className="flex-shrink-0">
              <AlertTriangle className="w-12 h-12 text-red-500" />
            </div>
            <div>
              <h3 className="text-lg font-medium text-gray-900 mb-2">
                ¿Estás seguro de revertir la asignación?
              </h3>
              <p className="text-sm text-gray-600">
                Esta acción eliminará todas las vacaciones asignadas automáticamente para el año {anioVigente}.
              </p>
            </div>
          </div>

          {resumenAsignacion && (
            <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-4">
              <h4 className="font-medium text-red-800 mb-2">Se eliminarán:</h4>
              <ul className="text-sm text-red-700 space-y-1">
                <li>• {resumenAsignacion.totalVacacionesAsignadas} días de vacaciones</li>
                <li>• Asignaciones de {resumenAsignacion.estadisticas.empleadosConAsignacion} empleados</li>
                <li>• Toda la programación automática del año {anioVigente}</li>
              </ul>
            </div>
          )}

          <p className="text-sm text-gray-500 mb-6">
            <strong>Nota:</strong> Esta acción no se puede deshacer. Tendrás que ejecutar nuevamente la programación automática si deseas asignar vacaciones.
          </p>

          {/* Buttons */}
          <div className="flex justify-end gap-3">
            <Button
              onClick={onCancelar}
              variant="outline"
              disabled={isReverting}
              className="px-6"
            >
              Cancelar
            </Button>
            <Button
              onClick={onConfirmar}
              variant="outline"
              disabled={isReverting}
              className="px-6 bg-red-600 text-white border-red-600 hover:bg-red-700"
            >
              {isReverting ? (
                <>
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                  Revirtiendo...
                </>
              ) : (
                <>
                  <Trash2 className="w-4 h-4 mr-2" />
                  Confirmar Reversión
                </>
              )}
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};
