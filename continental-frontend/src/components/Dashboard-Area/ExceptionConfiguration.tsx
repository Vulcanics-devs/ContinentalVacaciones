import React, { useState, useEffect } from 'react';
import { Plus, Edit2, Trash2, Save, X } from 'lucide-react';
import { Button } from '../ui/button';
import { useVacationConfig } from '@/hooks/useVacationConfig';
import { excepcionesService } from '@/services/excepcionesService';
import type { ExcepcionPorcentaje } from '@/interfaces/Api.interface';
import type { Grupo } from '@/interfaces/Grupo.interface';
import { toast } from 'sonner';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';

interface ExceptionConfigurationProps {
  currentDate: Date;
  currentAreaGroups?: Grupo[];
}

interface ExceptionFormData {
  grupoId: number;
  fecha: string;
  porcentajeMaximoPermitido: number;
  motivo: string;
}

export const ExceptionConfiguration: React.FC<ExceptionConfigurationProps> = ({
  currentDate,
  currentAreaGroups = []
}) => {
  const { config } = useVacationConfig();
  const [excepciones, setExcepciones] = useState<ExcepcionPorcentaje[]>([]);
  const [loading, setLoading] = useState(false);
  const [showForm, setShowForm] = useState(false);
  const [editingException, setEditingException] = useState<ExcepcionPorcentaje | null>(null);
  const [formData, setFormData] = useState<ExceptionFormData>({
    grupoId: 0,
    fecha: format(currentDate, 'yyyy-MM-dd'),
    porcentajeMaximoPermitido: config?.porcentajeAusenciaMaximo || 4.5,
    motivo: ''
  });

  // Cargar excepciones al montar el componente y cuando cambie la fecha
  useEffect(() => {
    console.log('🔍 ExceptionConfiguration - currentAreaGroups:', currentAreaGroups);
    loadExcepciones();
  }, [currentDate, currentAreaGroups]);

  // Actualizar fecha del formulario cuando cambie currentDate
  useEffect(() => {
    setFormData(prev => ({
      ...prev,
      fecha: format(currentDate, 'yyyy-MM-dd')
    }));
  }, [currentDate]);

  const loadExcepciones = async () => {
    if (!currentAreaGroups.length) return;

    setLoading(true);
    try {
      const fechaStr = format(currentDate, 'yyyy-MM-dd');
      // Cargar excepciones para todos los grupos del área en la fecha actual
      const allExcepciones: ExcepcionPorcentaje[] = [];
      
      for (const grupo of currentAreaGroups) {
        try {
          const grupoExcepciones = await excepcionesService.getExcepciones(
            grupo.grupoId,
            fechaStr,
            fechaStr
          );
          allExcepciones.push(...grupoExcepciones);
        } catch (error) {
          console.warn(`Error loading exceptions for group ${grupo.grupoId}:`, error);
        }
      }

      setExcepciones(allExcepciones);
    } catch (error) {
      console.error('Error loading exceptions:', error);
      toast.error('Error al cargar excepciones');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateException = async () => {
    if (!formData.grupoId || formData.porcentajeMaximoPermitido <= 0) {
      toast.error('Por favor complete todos los campos requeridos');
      return;
    }

    try {
      const newException = await excepcionesService.createExcepcion({
        grupoId: formData.grupoId,
        fecha: formData.fecha,
        porcentajeMaximoPermitido: formData.porcentajeMaximoPermitido,
        motivo: formData.motivo || undefined
      });

      setExcepciones(prev => [...prev, newException]);
      toast.success('Excepción creada correctamente');
      resetForm();
    } catch (error: any) {
      console.error('Error creating exception:', error);
      toast.error(error.message || 'Error al crear excepción');
    }
  };

  const handleUpdateException = async () => {
    if (!editingException) return;

    try {
      const updatedException = await excepcionesService.updateExcepcion(
        editingException.id,
        {
          grupoId: formData.grupoId,
          fecha: formData.fecha,
          porcentajeMaximoPermitido: formData.porcentajeMaximoPermitido,
          motivo: formData.motivo || undefined
        }
      );

      setExcepciones(prev => 
        prev.map(exc => exc.id === editingException.id ? updatedException : exc)
      );
      toast.success('Excepción actualizada correctamente');
      resetForm();
    } catch (error: any) {
      console.error('Error updating exception:', error);
      toast.error(error.message || 'Error al actualizar excepción');
    }
  };

  const handleDeleteException = async (excepcionId: number) => {
    if (!confirm('¿Está seguro de que desea eliminar esta excepción?')) return;

    try {
      const result = await excepcionesService.deleteExcepcion(excepcionId);
      setExcepciones(prev => prev.filter(exc => exc.id !== excepcionId));
      toast.success(result.mensaje || 'Excepción eliminada correctamente');
    } catch (error: any) {
      console.error('Error deleting exception:', error);
      toast.error(error.message || 'Error al eliminar excepción');
    }
  };

  const startEdit = (excepcion: ExcepcionPorcentaje) => {
    setEditingException(excepcion);
    setFormData({
      grupoId: excepcion.grupoId,
      fecha: excepcion.fecha,
      porcentajeMaximoPermitido: excepcion.porcentajeMaximoPermitido,
      motivo: excepcion.motivo || ''
    });
    setShowForm(true);
  };

  const resetForm = () => {
    setShowForm(false);
    setEditingException(null);
    setFormData({
      grupoId: 0,
      fecha: format(currentDate, 'yyyy-MM-dd'),
      porcentajeMaximoPermitido: config?.porcentajeAusenciaMaximo || 4.5,
      motivo: ''
    });
  };

  return (
    <div className="bg-white border border-gray-200 p-4 rounded-lg">
      <div className="text-sm font-semibold text-gray-900 mb-3">
        Configuración de Excepciones
      </div>
      
      {/* Header con información del día y porcentaje base */}
      <div className="space-y-3 mb-4">
        <div className="text-center">
          <div className="text-lg font-semibold text-gray-700 mb-1">
            % Base permitido: {config?.porcentajeAusenciaMaximo?.toFixed(1) || '4.5'}%
          </div>
          <div className="text-xs text-gray-500 mb-3">
            {format(currentDate, "d 'de' MMMM 'de' yyyy", { locale: es })}
          </div>
        </div>

        {/* Botón para agregar nueva excepción */}
        <div className="flex justify-center">
          <Button
            onClick={() => setShowForm(true)}
            className="flex items-center gap-2 text-sm"
            variant="outline"
          >
            <Plus size={16} />
            Nueva Excepción
          </Button>
        </div>
      </div>

      {/* Formulario de creación/edición */}
      {showForm && (
        <div className="border border-gray-300 rounded-lg p-3 mb-4 bg-gray-50">
          <div className="space-y-3">
            <div>
              <label className="text-xs font-medium text-gray-600 block mb-1">
                Grupo
              </label>
              <select
                value={formData.grupoId}
                onChange={(e) => setFormData(prev => ({ ...prev, grupoId: parseInt(e.target.value) }))}
                className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:outline-none focus:border-blue-500"
              >
                <option value={0}>Seleccionar grupo</option>
                {currentAreaGroups.map(grupo => (
                  <option key={grupo.grupoId} value={grupo.grupoId}>
                    {grupo.rol}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="text-xs font-medium text-gray-600 block mb-1">
                Fecha
              </label>
              <input
                type="date"
                value={formData.fecha}
                onChange={(e) => setFormData(prev => ({ ...prev, fecha: e.target.value }))}
                className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:outline-none focus:border-blue-500"
              />
            </div>

            <div>
              <label className="text-xs font-medium text-gray-600 block mb-1">
                Porcentaje Máximo (%)
              </label>
              <input
                type="number"
                value={formData.porcentajeMaximoPermitido}
                onChange={(e) => setFormData(prev => ({ ...prev, porcentajeMaximoPermitido: parseFloat(e.target.value) }))}
                className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                step="0.1"
                min="0.1"
                max="100"
              />
            </div>

            <div>
              <label className="text-xs font-medium text-gray-600 block mb-1">
                Motivo (opcional)
              </label>
              <textarea
                value={formData.motivo}
                onChange={(e) => setFormData(prev => ({ ...prev, motivo: e.target.value }))}
                className="w-full px-2 py-1 text-sm border border-gray-300 rounded focus:outline-none focus:border-blue-500"
                rows={2}
                maxLength={200}
                placeholder="Descripción del motivo de la excepción"
              />
            </div>

            <div className="flex gap-2">
              <Button
                onClick={editingException ? handleUpdateException : handleCreateException}
                className="flex-1 text-sm py-1"
                style={{ backgroundColor: 'var(--color-continental-yellow)' }}
              >
                <Save size={14} className="mr-1" />
                {editingException ? 'Actualizar' : 'Crear'}
              </Button>
              <Button
                onClick={resetForm}
                variant="outline"
                className="flex-1 text-sm py-1"
              >
                <X size={14} className="mr-1" />
                Cancelar
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Tabla de excepciones por grupo */}
      <div className="space-y-2">
        <div className="text-xs font-medium text-gray-600 mb-2">
          Excepciones para {format(currentDate, "d 'de' MMMM", { locale: es })}
        </div>
        
        {loading ? (
          <div className="text-center py-4 text-sm text-gray-500">
            Cargando excepciones...
          </div>
        ) : (
          <div className="space-y-1">
            {currentAreaGroups.map(grupo => {
              const excepcion = excepciones.find(exc => exc.grupoId === grupo.grupoId);
              const porcentajeActual = excepcion?.porcentajeMaximoPermitido || config?.porcentajeAusenciaMaximo || 4.5;
              
              return (
                <div
                  key={grupo.grupoId}
                  className="flex items-center justify-between py-2 px-3 bg-gray-50 rounded border"
                >
                  <div className="flex-1">
                    <div className="text-sm font-medium text-gray-800">
                      {grupo.rol}
                    </div>
                    <div className="text-xs text-gray-500">
                      {excepcion ? `${porcentajeActual.toFixed(1)}% (Excepción)` : `${porcentajeActual.toFixed(1)}% (Base)`}
                    </div>
                    {excepcion?.motivo && (
                      <div className="text-xs text-gray-400 mt-1">
                        {excepcion.motivo}
                      </div>
                    )}
                  </div>
                  
                  <div className="flex gap-1">
                    {excepcion ? (
                      <>
                        <button
                          onClick={() => startEdit(excepcion)}
                          className="p-1 text-blue-600 hover:text-blue-700"
                          title="Editar excepción"
                        >
                          <Edit2 size={14} />
                        </button>
                        <button
                          onClick={() => handleDeleteException(excepcion.id)}
                          className="p-1 text-red-600 hover:text-red-700"
                          title="Eliminar excepción"
                        >
                          <Trash2 size={14} />
                        </button>
                      </>
                    ) : (
                      <button
                        onClick={() => {
                          setFormData(prev => ({ ...prev, grupoId: grupo.grupoId }));
                          setShowForm(true);
                        }}
                        className="p-1 text-green-600 hover:text-green-700"
                        title="Crear excepción"
                      >
                        <Plus size={14} />
                      </button>
                    )}
                  </div>
                </div>
              );
            })}
            
            {currentAreaGroups.length === 0 && (
              <div className="text-center py-4 text-sm text-gray-500">
                No hay grupos disponibles en el área
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
