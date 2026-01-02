import { useState } from "react";
import { Button } from "@/components/ui/button";
import { AlertTriangle, X } from "lucide-react";
import { areasService } from "@/services/areasService";
import type { Grupo } from "@/interfaces/Areas.interface";
import { useNavigate } from "react-router-dom";

interface GroupConfigValidatorProps {
  onValidationSuccess: () => void;
  onNotification: (type: 'success' | 'info' | 'warning' | 'error', title: string, message?: string) => void;
  children: React.ReactNode; // El botón que activará la validación
}

export const GroupConfigValidator = ({ 
  onValidationSuccess, 
  onNotification, 
  children 
}: GroupConfigValidatorProps) => {
  const [showConfigModal, setShowConfigModal] = useState(false);
  const [groupsWithoutConfig, setGroupsWithoutConfig] = useState<Grupo[]>([]);
  const [isValidating, setIsValidating] = useState(false);
  const navigate = useNavigate();

  const validateGroupsConfiguration = async () => {
    if (isValidating) return;
    
    setIsValidating(true);
    try {
      // Verificar si hay grupos sin "personasPorTurno" o "duracionDeturno"
      const groups = await areasService.getGroups();
      const missingConfigGroups = groups.filter(group => 
        !group.personasPorTurno || 
        group.personasPorTurno === 0 || 
        !group.duracionDeturno || 
        group.duracionDeturno === 0
      );
      
      if (missingConfigGroups.length > 0) {
        // Mostrar modal con grupos sin configuración
        setGroupsWithoutConfig(missingConfigGroups);
        setShowConfigModal(true);
      } else {
        // Todos los grupos tienen configuración completa
        onValidationSuccess();
      }
    } catch (error) {
      console.error('Error al verificar configuración de grupos:', error);
      onNotification('error', 'Error', 'No se pudo verificar la configuración de los grupos.');
    } finally {
      setIsValidating(false);
    }
  };

  return (
    <>
      {/* Clonar el children y agregar el onClick */}
      <div onClick={validateGroupsConfiguration} className="cursor-pointer w-fit">
        {children}
      </div>

      {/* Modal de configuración incompleta */}
      {showConfigModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg shadow-xl max-w-2xl w-full mx-4 max-h-[80vh] overflow-hidden">
            {/* Header del modal */}
            <div className="flex items-center justify-between p-6 border-b border-gray-200">
              <div className="flex items-center gap-3">
                <AlertTriangle className="text-red-500" size={24} />
                <h2 className="text-xl font-semibold text-gray-900">
                  Configuración Incompleta
                </h2>
              </div>
              <button
                onClick={() => setShowConfigModal(false)}
                className="text-gray-400 hover:text-gray-600 transition-colors"
              >
                <X size={24} />
              </button>
            </div>

            {/* Contenido del modal */}
            <div className="p-6">
              <p className="text-gray-700 mb-4">
                Los siguientes grupos no cuentan con configuración de bloques: 
                <strong> personas por bloque</strong> y <strong>duración del bloque</strong>.
              </p>
              
              <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
                <p className="text-sm text-red-700 mb-3 font-medium">
                  Grupos que requieren configuración:
                </p>
                <div className="space-y-2 max-h-60 overflow-y-auto">
                  {groupsWithoutConfig.map((group, index) => (
                    <div key={group.grupoId || index} className="flex items-start gap-2 text-sm">
                      <span className="text-red-500 mt-1">•</span>
                      <div className="flex-1">
                        <span className="font-medium">Área:</span> {group.areaNombre || 'Sin área'} - 
                        <span className="font-medium ml-2">Grupo:</span> {group.rol}
                        <div className="text-xs text-gray-600 mt-1">
                          Personas por turno: {group.personasPorTurno || 0} | 
                          Duración del turno: {group.duracionDeturno || 0}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>

              <p className="text-sm text-gray-600">
                Para continuar, es necesario configurar estos grupos 
                en la sección de <strong>Áreas y Grupos</strong>.
              </p>
            </div>

            {/* Footer del modal */}
            <div className="flex justify-end gap-3 p-6 border-t border-gray-200 bg-gray-50">
              <Button
                onClick={() => setShowConfigModal(false)}
                variant="outline"
                className="px-6"
              >
                Cerrar
              </Button>
              <Button
                onClick={() => {
                  navigate('/admin/areas');
                  setShowConfigModal(false);
                }}
                variant="continental"
                className="px-6"
              >
                Ir a Configuración
              </Button>
            </div>
          </div>
        </div>
      )}
    </>
  );
};
