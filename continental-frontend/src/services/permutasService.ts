import { httpClient } from '@/services/httpClient';
import type { ApiResponse, SolicitudPermutaRequest, SolicitudPermutaResponse } from '@/interfaces/Api.interface';

export type { SolicitudPermutaRequest as PermutaRequest, SolicitudPermutaResponse as PermutaResponse };

export const permutasService = {
    async solicitarPermuta(payload: SolicitudPermutaRequest): Promise<SolicitudPermutaResponse> {
        const resp = await httpClient.post<SolicitudPermutaResponse>(
            '/api/permutas/solicitar',
            payload
        );

        if (!resp.success || !resp.data) {
            throw new Error(resp.errorMsg || 'No se pudo procesar la solicitud de permuta');
        }

        return resp.data;
    }
};