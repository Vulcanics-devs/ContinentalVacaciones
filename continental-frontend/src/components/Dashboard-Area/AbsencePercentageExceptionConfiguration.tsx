import React, { useState, useEffect } from 'react';
import { Plus, Edit2, Trash2, Save, X } from 'lucide-react';
import { Button } from '../ui/button';
import { useVacationConfig } from '@/hooks/useVacationConfig';
import { excepcionesService } from '@/services/excepcionesService';
import type { ExcepcionPorcentaje } from '@/interfaces/Api.interface';
import type { Grupo } from '@/interfaces/Grupo.interface';
import { toast } from 'sonner';
import { format, startOfMonth, endOfMonth } from 'date-fns';
import { es } from 'date-fns/locale';

interface AbsencePercentageExceptionConfigurationProps {
  currentDate: Date;
  currentAreaGroups?: Grupo[];
}

interface ExceptionFormData {
  grupoId: number;
  fecha: string;
  porcentajeMaximoPermitido: number;
  motivo: string;
}

export const AbsencePercentageExceptionConfiguration: React.FC<AbsencePercentageExceptionConfigurationProps> = ({
  currentDate,
  currentAreaGroups = []
}) => {
  const { config } = useVacationConfig();
  const [excepciones, setExcepciones] = useState<ExcepcionPorcentaje[]>([]);
  const [loading, setLoading] = useState(false);
  const [showForm, setShowForm] = useState(false);
  const [editingException, setEditingException] = useState<ExcepcionPorcentaje | null>(null);
  const [selectedGrupo, setSelectedGrupo] = useState<number | null>(null);
  const [formData, setFormData] = useState<ExceptionFormData>({
    grupoId: 0,
    fecha: format(currentDate, 'yyyy-MM-dd'),
    porcentajeMaximoPermitido: config?.porcentajeAusenciaMaximo || 4.5,
    motivo: ''
  });

  const porcentajeBase = config?.porcentajeAusenciaMaximo || 4.5;

  // Cargar excepciones del mes actual para todos los grupos
  useEffect(() => {
    if (currentAreaGroups.length > 0) {
      loadExcepciones();
    }
  }, [currentDate, currentAreaGroups]);

  // Actualizar formulario cuando cambia la fecha
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
      const monthStart = startOfMonth(currentDate);
      const monthEnd = endOfMonth(currentDate);
      const fechaInicio = format(monthStart, 'yyyy-MM-dd');
      const fechaFin = format(monthEnd, 'yyyy-MM-dd');

      // Cargar excepciones para todos los grupos del área en el mes actual
      const allExcepciones: ExcepcionPorcentaje[] = [];

      for (const grupo of currentAreaGroups) {
        try {
          const grupoExcepciones = await excepcionesService.getExcepciones(
            grupo.grupoId,
            fechaInicio,
            fechaFin
          );
          allExcepciones.push(...grupoExcepciones);
        } catch (error) {
          console.warn(`Error loading exceptions for group ${grupo.grupoId}:`, error);
        }
      }

      setExcepciones(allExcepciones);
      console.log('📋 Excepciones de % Ausencia cargadas:', allExcepciones);
    } catch (error) {
      console.error('Error loading percentage exceptions:', error);
      toast.error('Error al cargar excepciones de % ausencia');
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
      toast.success('Excepción de % ausencia creada correctamente');
      resetForm();
    } catch (error: any) {
      console.error('Error creating percentage exception:', error);
      toast.error(error.message || 'Error al crear excepción de % ausencia');
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
      toast.success('Excepción de % ausencia actualizada correctamente');
      resetForm();
    } catch (error: any) {
      console.error('Error updating percentage exception:', error);
      toast.error(error.message || 'Error al actualizar excepción de % ausencia');
    }
  };

  const handleDeleteException = async (excepcionId: number) => {
    if (!confirm('¿Está seguro de que desea eliminar esta excepción de % ausencia?')) return;

    try {
      await excepcionesService.deleteExcepcion(excepcionId);
      setExcepciones(prev => prev.filter(exc => exc.id !== excepcionId));
      toast.success('Excepción de % ausencia eliminada correctamente');
    } catch (error: any) {
      console.error('Error deleting percentage exception:', error);
      toast.error(error.message || 'Error al eliminar excepción de % ausencia');
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
    setSelectedGrupo(null);
    setFormData({
      grupoId: 0,
      fecha: format(currentDate, 'yyyy-MM-dd'),
      porcentajeMaximoPermitido: porcentajeBase,
      motivo: ''
    });
  };

  const getExcepcionesParaGrupo = (grupoId: number) => {
    return excepciones.filter(exc => exc.grupoId === grupoId);
  };

  const handleGrupoClick = (grupoId: number) => {
    setSelectedGrupo(selectedGrupo === grupoId ? null : grupoId);
  };

  if (currentAreaGroups.length === 0) {
    return (
      <div className="bg-white border border-gray-200 p-4 rounded-lg">
        <div className="text-sm font-semibold text-gray-900 mb-3">
          Excepciones de % Ausencia
        </div>
        <div className="text-center py-4 text-sm text-gray-500">
          No hay grupos disponibles en el área
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white border border-gray-200 p-4 rounded-lg">
      <div className="text-sm font-semibold text-gray-900 mb-3">
        Excepciones de % Ausencia
      </div>

      {/* Header con información del mes actual */}
      <div className="space-y-3 mb-4">
        <div className="text-center">
          <div className="text-lg font-semibold text-gray-700 mb-1">
            % Base: {porcentajeBase.toFixed(1)}%
          </div>
          <div className="text-xs text-gray-500 mb-3">
            {format(currentDate, "MMMM 'de' yyyy", { locale: es })}
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
              <div className="text-xs text-gray-500 mt-1">
                % base: {porcentajeBase.toFixed(1)}%
              </div>
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

      {/* Lista de grupos con excepciones */}
      <div className="space-y-2">
        <div className="text-xs font-medium text-gray-600 mb-2">
          Excepciones por Grupo
        </div>

        {loading ? (
          <div className="text-center py-4 text-sm text-gray-500">
            Cargando excepciones...
          </div>
        ) : (
          <div className="space-y-2">
            {currentAreaGroups.map(grupo => {
              const grupoExcepciones = getExcepcionesParaGrupo(grupo.grupoId);
              const isExpanded = selectedGrupo === grupo.grupoId;

              return (
                <div key={grupo.grupoId} className="border border-gray-200 rounded">
                  {/* Grupo header */}
                  <div
                    onClick={() => handleGrupoClick(grupo.grupoId)}
                    className={`flex items-center justify-between py-2 px-3 cursor-pointer hover:bg-gray-50 ${
                      grupoExcepciones.length > 0 ? 'bg-orange-50' : 'bg-gray-50'
                    }`}
                  >
                    <div className="flex-1">
                      <div className="text-sm font-medium text-gray-800">
                        {grupo.rol}
                      </div>
                      <div className="text-xs text-gray-500">
                        {grupoExcepciones.length} excepcion(es)
                      </div>
                    </div>
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        setFormData(prev => ({
                          ...prev,
                          grupoId: grupo.grupoId,
                          fecha: format(currentDate, 'yyyy-MM-dd')
                        }));
                        setShowForm(true);
                      }}
                      className="p-1 text-green-600 hover:text-green-700"
                      title="Crear excepción"
                    >
                      <Plus size={14} />
                    </button>
                  </div>

                  {/* Grupo excepciones detail (expandible) */}
                  {isExpanded && (
                    <div className="border-t border-gray-200 p-2 bg-white max-h-48 overflow-y-auto">
                      {grupoExcepciones.length > 0 ? (
                        <div className="space-y-1">
                          {grupoExcepciones.map(excepcion => {
                            const excepcionDate = new Date(excepcion.fecha + 'T00:00:00');
                            return (
                              <div
                                key={excepcion.id}
                                className="flex items-center justify-between py-1 px-2 bg-gray-50 rounded text-xs"
                              >
                                <div className="flex-1">
                                  <div className="font-medium text-gray-800">
                                    {format(excepcionDate, "d 'de' MMMM", { locale: es })}
                                  </div>
                                  <div className="text-gray-600">
                                    {excepcion.porcentajeMaximoPermitido.toFixed(1)}%
                                  </div>
                                  {excepcion.motivo && (
                                    <div className="text-gray-400 truncate">
                                      {excepcion.motivo}
                                    </div>
                                  )}
                                </div>
                                <div className="flex gap-1">
                                  <button
                                    onClick={() => startEdit(excepcion)}
                                    className="p-1 text-blue-600 hover:text-blue-700"
                                    title="Editar excepción"
                                  >
                                    <Edit2 size={12} />
                                  </button>
                                  <button
                                    onClick={() => handleDeleteException(excepcion.id)}
                                    className="p-1 text-red-600 hover:text-red-700"
                                    title="Eliminar excepción"
                                  >
                                    <Trash2 size={12} />
                                  </button>
                                </div>
                              </div>
                            );
                          })}
                        </div>
                      ) : (
                        <div className="text-center py-2 text-xs text-gray-500">
                          No hay excepciones para este grupo
                        </div>
                      )}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
};
