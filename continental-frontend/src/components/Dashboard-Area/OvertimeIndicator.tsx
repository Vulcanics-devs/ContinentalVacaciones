import React from 'react';
import { Clock } from 'lucide-react';
import type { ExcepcionPorcentaje } from '@/interfaces/Api.interface';

interface OvertimeIndicatorProps {
    fecha: string;
    excepciones: ExcepcionPorcentaje[];
    grupoId?: number;
}

export const OvertimeIndicator: React.FC<OvertimeIndicatorProps> = ({
    fecha,
    excepciones,
    grupoId
}) => {
    const excepcionDelDia = excepciones.find(
        exc => exc.fecha === fecha && exc.grupoId === grupoId
    );

    if (!excepcionDelDia) return null;

    return (
        <div
            className="absolute top-1 left-1 bg-orange-500 text-white rounded-full p-1 z-10"
            title={`Tiempo Extra: ${excepcionDelDia.porcentajeMaximoPermitido}%${excepcionDelDia.motivo ? ` - ${excepcionDelDia.motivo}` : ''}`}
        >
            <Clock size={12} />
        </div>
    );
};