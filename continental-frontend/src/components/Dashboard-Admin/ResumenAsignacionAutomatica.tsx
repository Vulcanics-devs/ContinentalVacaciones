/**
 * Componente para mostrar el resumen de la asignación automática de vacaciones
 * Organiza los resultados por áreas y grupos
 */

import type { AsignacionAutomaticaResponse } from '@/interfaces/Api.interface';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import { Calendar, CheckCircle, Users, XCircle } from 'lucide-react';

interface ResumenAsignacionAutomaticaProps {
  resultados: AsignacionAutomaticaResponse;
}



export const ResumenAsignacionAutomatica = ({ resultados }: ResumenAsignacionAutomaticaProps) => {

  return (
    <div className="space-y-6">
      {/* Resumen General */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div className="bg-blue-50 p-4 rounded-lg border border-blue-200">
          <div className="flex items-center gap-2 mb-2">
            <Users className="w-5 h-5 text-blue-600" />
            <span className="text-sm font-medium text-blue-800">Total Procesados</span>
          </div>
          <p className="text-2xl font-bold text-blue-900">{resultados.totalEmpleadosProcesados}</p>
        </div>

        <div className="bg-green-50 p-4 rounded-lg border border-green-200">
          <div className="flex items-center gap-2 mb-2">
            <CheckCircle className="w-5 h-5 text-green-600" />
            <span className="text-sm font-medium text-green-800">Asignados</span>
          </div>
          <p className="text-2xl font-bold text-green-900">{resultados.totalEmpleadosAsignados}</p>
        </div>

        <div className="bg-red-50 p-4 rounded-lg border border-red-200">
          <div className="flex items-center gap-2 mb-2">
            <XCircle className="w-5 h-5 text-red-600" />
            <span className="text-sm font-medium text-red-800">Fallidos</span>
          </div>
          <p className="text-2xl font-bold text-red-900">
            {resultados.totalEmpleadosProcesados - resultados.totalEmpleadosAsignados}
          </p>
        </div>

        <div className="bg-purple-50 p-4 rounded-lg border border-purple-200">
          <div className="flex items-center gap-2 mb-2">
            <Calendar className="w-5 h-5 text-purple-600" />
            <span className="text-sm font-medium text-purple-800">Días Asignados</span>
          </div>
          <p className="text-2xl font-bold text-purple-900">{resultados.totalDiasAsignados}</p>
        </div>
      </div>

      {/* Información de Procesamiento */}
      <div className="bg-gray-50 p-4 rounded-lg border">
        <h4 className="font-medium text-gray-900 mb-2">Información del Procesamiento</h4>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm text-gray-600">
          <div>
            <span className="font-medium">Año de asignación:</span> {resultados.anioAsignacion}
          </div>
          <div>
            <span className="font-medium">Fecha de procesamiento:</span> {' '}
            {format(new Date(resultados.fechaProcesamiento), "d 'de' MMMM 'de' yyyy 'a las' HH:mm", { locale: es })}
          </div>
        </div>
      </div>

      
    </div>
  );
};
