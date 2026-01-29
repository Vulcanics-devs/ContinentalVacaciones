import { httpClient } from '@/services/httpClient';

export interface PermutaListItem {
    id: number;
    empleadoOrigenNombre: string;
    empleadoDestinoNombre: string;
    fechaPermuta: string;
    turnoEmpleadoOrigen: string;
    turnoEmpleadoDestino: string;
    motivo: string;
    solicitadoPorNombre: string;
    fechaSolicitud: string;
    estadoSolicitud: 'Pendiente' | 'Aprobada' | 'Rechazada';
    jefeAprobadorNombre?: string;
    fechaRespuesta?: string;
    motivoRechazo?: string;
}

export interface ResponderPermutaRequest {
    aprobar: boolean;
    motivoRechazo?: string;
}

export interface PermutasListResponse {
    permutas: PermutaListItem[];
    total: number;
}

export const permutasListService = {
    async obtenerPermutas(anio: number): Promise<PermutasListResponse> {
        const resp = await httpClient.get<PermutasListResponse>(
            `/api/permutas/listado?anio=${anio}`
        );
        
        if (!resp.success || !resp.data) {
            throw new Error(resp.errorMsg || 'Error al obtener permutas');
        }
        
        return resp.data;
    },

    async exportarExcel(anio: number): Promise<Blob> {
        const response = await fetch(
            `${import.meta.env.VITE_API_URL}/api/permutas/exportar-excel?anio=${anio}`,
            {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${localStorage.getItem('token')}`
                }
            }
        );

        if (!response.ok) {
            throw new Error('Error al exportar Excel');
        }

        return await response.blob();
    },

    async responderPermuta(permutaId: number, aprobar: boolean, motivoRechazo?: string): Promise<void> {
        const resp = await httpClient.post<void>(
            `/api/permutas/responder/${permutaId}`,
            { aprobar, motivoRechazo }
        );

        if (!resp.success) {
            throw new Error(resp.errorMsg || 'Error al responder permuta');
        }
    }
};