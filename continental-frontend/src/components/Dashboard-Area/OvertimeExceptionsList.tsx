import React from 'react';
import { Clock } from 'lucide-react';
import type { ExcepcionPorcentaje } from '@/interfaces/Api.interface';

interface OvertimeExceptionsListProps {
    excepciones: ExcepcionPorcentaje[];
    empleadoNombre: string;
    grupoId?: number;
    loading?: boolean;
}

export const OvertimeExceptionsList: React.FC<OvertimeExceptionsListProps> = ({
    excepciones,
    empleadoNombre,
    grupoId,
    loading = false
}) => {
    const excepcionesDelGrupo = excepciones.filter(exc => exc.grupoId === grupoId);

    if (loading) {
        return (
            <div className="bg-white rounded-lg shadow p-4">
                <h3 className="text-lg font-semibold mb-3 flex items-center gap-2">
                    <Clock size={20} className="text-orange-500" />
                    Tiempo Extra Programado
                </h3>
                <p className="text-gray-500 text-sm">Cargando...</p>
            </div>
        );
    }

    if (excepcionesDelGrupo.length === 0) {
        return (
            <div className="bg-white rounded-lg shadow p-4">
                <h3 className="text-lg font-semibold mb-3 flex items-center gap-2">
                    <Clock size={20} className="text-orange-500" />
                    Tiempo Extra Programado
                </h3>
                <p className="text-gray-500 text-sm">
                    No hay excepciones de tiempo extra programadas para {empleadoNombre}
                </p>
            </div>
        );
    }

    return (
        <div className="bg-white rounded-lg shadow p-4">
            <h3 className="text-lg font-semibold mb-3 flex items-center gap-2">
                <Clock size={20} className="text-orange-500" />
                Tiempo Extra Programado ({excepcionesDelGrupo.length})
            </h3>

            <div className="space-y-2 max-h-60 overflow-y-auto">
                {excepcionesDelGrupo.map(exc => (
                    <div
                        key={exc.id}
                        className="border border-orange-200 rounded-lg p-3 bg-orange-50"
                    >
                        <div className="flex justify-between items-start mb-2">
                            <span className="text-sm font-semibold text-gray-800">
                                {new Date(exc.fecha + 'T00:00:00').toLocaleDateString('es-MX', {
                                    weekday: 'long',
                                    year: 'numeric',
                                    month: 'long',
                                    day: 'numeric'
                                })}
                            </span>
                            <span className="text-sm font-bold text-orange-600">
                                {exc.porcentajeMaximoPermitido}%
                            </span>
                        </div>
                        {exc.motivo && (
                            <p className="text-xs text-gray-600 mt-1">
                                Motivo: {exc.motivo}
                            </p>
                        )}
                    </div>
                ))}
            </div>
        </div>
    );
};