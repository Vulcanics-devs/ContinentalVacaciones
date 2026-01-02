import { httpClient } from "@/services/httpClient";
import type { ApiResponse } from "@/interfaces/Api.interface";

export interface CreateDiaInhabilRequest {
  fechaInicial: string; // Format: "YYYY-MM-DD"
  fechaFinal: string;   // Format: "YYYY-MM-DD"
  detalles: string;
  tipoActividadDelDia: number; // 0: IncidenciaOPermiso, 1: InhabilPorLey, 2: InhabilPorContinental
}

export interface CreateDiaInhabilResponse {
  success: boolean;
  data: number[]; // Lista de IDs de los registros creados
  errorMsg: string;
}

export interface DiaInhabil {
  id: number;
  fecha: string;
  fechaInicial: string;
  fechaFinal: string;
  detalles: string;
  tipoActividadDelDia: number;
  createdAt?: string;
  updatedAt?: string;
}

export interface GetDiasInhabilesResponse {
  success: boolean;
  data: DiaInhabil[];
  errorMsg: string;
}

export interface DeleteDiaInhabilResponse {
  success: boolean;
  data: number;
  errorMsg: string;
}

export interface GetDiasInhabilesParams {
  fechaInicio?: string; // Format: "YYYY-MM-DD"
  fechaFin?: string;    // Format: "YYYY-MM-DD"
  tipoDiaInhabil?: number; // 0, 1, or 2
}

export const TipoActividadDelDia = {
  INCIDENCIA_O_PERMISO: 0,
  INHABIL_POR_LEY: 1,
  INHABIL_POR_CONTINENTAL: 2
} as const;

export type TipoActividadDelDia = typeof TipoActividadDelDia[keyof typeof TipoActividadDelDia];

export const diasInhabilesService = {
  async createDiasInhabiles(request: CreateDiaInhabilRequest): Promise<CreateDiaInhabilResponse> {
    try {
      const response = await httpClient.post<ApiResponse<CreateDiaInhabilResponse>>(
        "/api/DiasInhabiles",
        request
      );

      // Extract data from API response
      const responseData = response;
      if (!responseData) {
        throw new Error("Invalid response from server");
      }

      return responseData as unknown as CreateDiaInhabilResponse;
    } catch (error) {
      console.error("Error in diasInhabilesService.createDiasInhabiles:", error);
      throw error;
    }
  },

  async getDiasInhabiles(params?: GetDiasInhabilesParams): Promise<GetDiasInhabilesResponse> {
    try {
      // Build query string from parameters
      const queryParams = new URLSearchParams();
      
      if (params?.fechaInicio) {
        queryParams.append('fechaInicio', params.fechaInicio);
      }
      if (params?.fechaFin) {
        queryParams.append('fechaFin', params.fechaFin);
      }
      if (params?.tipoDiaInhabil !== undefined) {
        queryParams.append('tipoDiaInhabil', params.tipoDiaInhabil.toString());
      }

      const queryString = queryParams.toString();
      const url = queryString ? `/api/DiasInhabiles?${queryString}` : '/api/DiasInhabiles';

      const response = await httpClient.get<ApiResponse<GetDiasInhabilesResponse>>(url);

      // Extract data from API response
      const responseData = response;
      if (!responseData) {
        throw new Error("Invalid response from server");
      }

      return responseData as unknown as GetDiasInhabilesResponse;
    } catch (error) {
      console.error("Error in diasInhabilesService.getDiasInhabiles:", error);
      throw error;
    }
  },

  async deleteDiaInhabil(id: number): Promise<DeleteDiaInhabilResponse> {
    try {
      const response = await httpClient.delete<ApiResponse<DeleteDiaInhabilResponse>>(`/api/DiasInhabiles/${id}`);
      
      // Extract data from API response
      const responseData = response;
      if (!responseData) {
        throw new Error("Invalid response from server");
      }

      return responseData as unknown as DeleteDiaInhabilResponse;
    } catch (error) {
      console.error("Error in diasInhabilesService.deleteDiaInhabil:", error);
      throw error;
    }
  }
};
