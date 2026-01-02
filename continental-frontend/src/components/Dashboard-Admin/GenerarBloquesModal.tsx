/**
 * Modal para Generar Bloques de Reservación
 * Muestra simulación y permite confirmar la generación de bloques
 */

import { useState } from 'react';
import { Button } from "@/components/ui/button";
import { Loader2, CheckCircle, XCircle, X, Calendar, AlertTriangle } from 'lucide-react';
import { BloquesReservacionService } from '@/services/bloquesReservacionService';
import type { GenerarBloquesRequest, GenerarBloquesResponse, ResumenPorGrupoBloque } from '@/interfaces/Api.interface';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';

interface GenerarBloquesModalProps {
  isOpen: boolean;
  onClose: () => void;
  onNotification: (type: 'success' | 'info' | 'warning' | 'error', title: string, message?: string) => void;
  onSuccess?: () => void;
  fechaInicio: string;
  anioVigente: number;
}

type ModalStep = 'confirmation' | 'processing' | 'simulation' | 'finalizing';

export const GenerarBloquesModal = ({ 
  isOpen, 
  onClose, 
  onNotification,
  onSuccess,
  fechaInicio,
  anioVigente
}: GenerarBloquesModalProps) => {
  const [step, setStep] = useState<ModalStep>('confirmation');
  const [isProcessing, setIsProcessing] = useState(false);
  const [simulationResults, setSimulationResults] = useState<GenerarBloquesResponse | null>(null);
  const [isFinalizing, setIsFinalizing] = useState(false);

  const handleClose = () => {
    if (isProcessing || isFinalizing) {
      onNotification('warning', 'Proceso en curso', 'No se puede cerrar el modal mientras se está procesando. Por favor espere.');
      return;
    }
    
    setStep('confirmation');
    setSimulationResults(null);
    onClose();
  };

  const handleConfirmarSimulacion = async () => {
    if (!fechaInicio) {
      onNotification('error', 'Fecha requerida', 'Por favor seleccione una fecha de inicio antes de continuar.');
      return;
    }

    try {
      setIsProcessing(true);
      setStep('processing');

      const request: GenerarBloquesRequest = {
        fechaInicioGeneracion: fechaInicio + 'T07:00:00',
        anioObjetivo: anioVigente,
        grupoIds: [],
        soloSimulacion: true
      };

      const response = await BloquesReservacionService.generarBloques(request);
      setSimulationResults(response);
      setStep('simulation');
      
      onNotification('info', 'Simulación Completada', 
        `Se generaron ${response.totalBloqueGenerados} bloques para ${response.totalGruposProcesados} grupos.`);
    } catch (error) {
      console.error('Error en simulación:', error);
      
      let errorMessage = 'Ocurrió un error durante la simulación. Por favor intente nuevamente.';
      if (error instanceof Error && error.message) {
        errorMessage = error.message;
      }
      
      onNotification('error', 'Error en Simulación', errorMessage);
      setStep('confirmation');
    } finally {
      setIsProcessing(false);
    }
  };

  const handleAprobarGeneracion = async () => {
    if (!simulationResults) return;

    try {
      setIsFinalizing(true);
      setStep('finalizing');

      const request: GenerarBloquesRequest = {
        fechaInicioGeneracion: fechaInicio + 'T07:00:00',
        anioObjetivo: anioVigente,
        grupoIds: [],
        soloSimulacion: false
      };

      const response = await BloquesReservacionService.generarBloques(request);
      
      onNotification('success', 'Bloques Generados', 
        `Se generaron exitosamente ${response.totalBloqueGenerados} bloques para ${response.totalEmpleadosAsignados} empleados.`);
      
      onSuccess?.();
      handleClose();
    } catch (error) {
      console.error('Error al aprobar generación:', error);
      
      let errorMessage = 'Ocurrió un error al generar los bloques finales. Por favor intente nuevamente.';
      if (error instanceof Error && error.message) {
        errorMessage = error.message;
      }
      
      onNotification('error', 'Error al Generar Bloques', errorMessage);
    } finally {
      setIsFinalizing(false);
    }
  };

  const handleRechazarGeneracion = () => {
    onNotification('info', 'Generación Cancelada', 'La generación de bloques ha sido cancelada.');
    handleClose();
  };

  // Agrupar resultados por área
  const agruparPorAreas = () => {
    if (!simulationResults) return [];
    
    const areasMap = new Map();
    
    simulationResults.resumenPorGrupo.forEach(grupo => {
      const areaName = grupo.nombreArea;
      if (!areasMap.has(areaName)) {
        areasMap.set(areaName, {
          nombreArea: areaName,
          grupos: [],
          totalGrupos: 0,
          totalBloques: 0,
          totalEmpleados: 0
        });
      }
      
      const area = areasMap.get(areaName);
      area.grupos.push(grupo);
      area.totalGrupos++;
      area.totalBloques += grupo.totalBloques;
      area.totalEmpleados += grupo.totalEmpleados;
    });
    
    return Array.from(areasMap.values());
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Overlay */}
      <div className="fixed inset-0 bg-black/50 backdrop-blur-sm" />
      
      {/* Modal */}
      <div className="relative bg-white rounded-lg shadow-xl max-w-4xl w-full mx-4 max-h-[90vh] overflow-hidden">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b">
          <h2 className="text-xl font-semibold text-gray-900">
            {step === 'confirmation' && 'Confirmar Generación de Bloques'}
            {step === 'processing' && 'Generando Simulación...'}
            {step === 'simulation' && 'Resultados de Simulación'}
            {step === 'finalizing' && 'Generando Bloques Finales...'}
          </h2>
          <button
            onClick={handleClose}
            disabled={isProcessing || isFinalizing}
            className="text-gray-400 hover:text-gray-600 disabled:opacity-50"
          >
            <X className="w-6 h-6" />
          </button>
        </div>

        {/* Content */}
        <div className="p-6 overflow-y-auto max-h-[calc(90vh-140px)]">
          {/* Confirmation Step */}
          {step === 'confirmation' && (
            <div className="space-y-6">
              <div className="flex items-center gap-4">
                <div className="flex-shrink-0">
                  <Calendar className="w-12 h-12 text-blue-500" />
                </div>
                <div>
                  <h3 className="text-lg font-medium text-gray-900 mb-2">
                    ¿Deseas generar los bloques de reservación?
                  </h3>
                  <p className="text-sm text-gray-600">
                    Se ejecutará una simulación primero para revisar los resultados antes de la generación final.
                  </p>
                </div>
              </div>

              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <h4 className="font-medium text-blue-800 mb-2">Parámetros de generación:</h4>
                <ul className="text-sm text-blue-700 space-y-1">
                  <li>• <strong>Fecha de inicio:</strong> {fechaInicio ? format(new Date(fechaInicio + 'T07:00:00'), "d 'de' MMMM 'de' yyyy 'a las' HH:mm", { locale: es }) : 'No seleccionada'}</li>
                  <li>• <strong>Año objetivo:</strong> {anioVigente}</li>
                  <li>• <strong>Grupos:</strong> Todos los grupos configurados</li>
                </ul>
              </div>

              <div className="flex justify-end gap-3">
                <Button onClick={handleClose} variant="outline">
                  Cancelar
                </Button>
                <Button 
                  onClick={handleConfirmarSimulacion}
                  variant="continental"
                  disabled={!fechaInicio}
                >
                  Ejecutar Simulación
                </Button>
              </div>
            </div>
          )}

          {/* Processing Step */}
          {step === 'processing' && (
            <div className="flex flex-col items-center justify-center py-12 space-y-4">
              <Loader2 className="w-16 h-16 text-blue-500 animate-spin" />
              <h3 className="text-lg font-medium text-gray-900">Ejecutando Simulación</h3>
              <p className="text-sm text-gray-600 text-center max-w-md">
                Estamos generando una simulación de los bloques de reservación. Esto puede tomar unos minutos...
              </p>
            </div>
          )}

          {/* Simulation Results Step */}
          {step === 'simulation' && simulationResults && (
            <div className="space-y-6">
              {/* Resumen general */}
              <div className="bg-green-50 border border-green-200 rounded-lg p-6">
                <div className="flex items-center gap-3 mb-4">
                  <CheckCircle className="w-6 h-6 text-green-600" />
                  <h3 className="font-medium text-green-800">Simulación Completada</h3>
                </div>
                
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <div className="text-center">
                    <div className="text-2xl font-bold text-green-900">{simulationResults.totalGruposProcesados}</div>
                    <div className="text-sm text-green-700">Grupos Procesados</div>
                  </div>
                  <div className="text-center">
                    <div className="text-2xl font-bold text-blue-900">{simulationResults.totalBloqueGenerados}</div>
                    <div className="text-sm text-blue-700">Bloques Generados</div>
                  </div>
                  <div className="text-center">
                    <div className="text-2xl font-bold text-purple-900">{simulationResults.totalEmpleadosAsignados}</div>
                    <div className="text-sm text-purple-700">Empleados Asignados</div>
                  </div>
                </div>
              </div>

              {/* Advertencias y errores */}
              {(simulationResults.advertencias.length > 0 || simulationResults.errores.length > 0) && (
                <div className="space-y-3">
                  {simulationResults.errores.length > 0 && (
                    <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                      <div className="flex items-center gap-2 mb-2">
                        <XCircle className="w-5 h-5 text-red-500" />
                        <h4 className="font-medium text-red-800">Errores encontrados:</h4>
                      </div>
                      <ul className="text-sm text-red-700 space-y-1">
                        {simulationResults.errores.map((error: string, index: number) => (
                          <li key={index}>• {error}</li>
                        ))}
                      </ul>
                    </div>
                  )}
                  
                  {simulationResults.advertencias.length > 0 && (
                    <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
                      <div className="flex items-center gap-2 mb-2">
                        <AlertTriangle className="w-5 h-5 text-yellow-500" />
                        <h4 className="font-medium text-yellow-800">Advertencias:</h4>
                      </div>
                      <ul className="text-sm text-yellow-700 space-y-1">
                        {simulationResults.advertencias.map((advertencia: string, index: number) => (
                          <li key={index}>• {advertencia}</li>
                        ))}
                      </ul>
                    </div>
                  )}
                </div>
              )}

              {/* Resumen por áreas */}
              <div>
                <h4 className="font-medium text-gray-900 mb-3">Resumen por Áreas</h4>
                <div className="space-y-3 max-h-60 overflow-y-auto">
                  {agruparPorAreas().map(area => (
                    <div key={area.nombreArea} className="bg-gray-50 border rounded-lg p-4">
                      <div className="flex items-center justify-between mb-2">
                        <h5 className="font-medium text-gray-800">{area.nombreArea}</h5>
                        <div className="text-sm text-gray-600">
                          {area.totalGrupos} grupos • {area.totalBloques} bloques • {area.totalEmpleados} empleados
                        </div>
                      </div>
                      
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-2 text-xs text-gray-600">
                        {area.grupos.slice(0, 4).map((grupo: ResumenPorGrupoBloque) => (
                          <div key={grupo.grupoId} className="flex justify-between">
                            <span>{grupo.nombreGrupo}</span>
                            <span>{grupo.totalBloques} bloques</span>
                          </div>
                        ))}
                        {area.grupos.length > 4 && (
                          <div className="col-span-full text-center text-gray-500">
                            ... y {area.grupos.length - 4} grupos más
                          </div>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              </div>

              {/* Botones de acción */}
              <div className="flex justify-end gap-3 pt-4 border-t">
                <Button onClick={handleRechazarGeneracion} variant="outline">
                  Rechazar
                </Button>
                <Button 
                  onClick={handleAprobarGeneracion}
                  variant="continental"
                  disabled={!simulationResults.generacionExitosa}
                >
                  Aprobar y Generar
                </Button>
              </div>
            </div>
          )}

          {/* Finalizing Step */}
          {step === 'finalizing' && (
            <div className="flex flex-col items-center justify-center py-12 space-y-4">
              <Loader2 className="w-16 h-16 text-green-500 animate-spin" />
              <h3 className="text-lg font-medium text-gray-900">Generando Bloques Finales</h3>
              <p className="text-sm text-gray-600 text-center max-w-md">
                Estamos creando los bloques de reservación definitivos. Por favor espere...
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
