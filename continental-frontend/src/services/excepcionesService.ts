import { httpClient } from "@/services/httpClient";
import type { 
  ApiResponse, 
  ExcepcionPorcentaje, 
  CreateExcepcionRequest, 
  UpdateExcepcionRequest,
  DeleteExcepcionResponse 
} from "@/interfaces/Api.interface";

export const excepcionesService = {
  /**
   * Obtener excepciones de porcentaje con filtros opcionales
   * @param grupoId - ID del grupo (opcional)
   * @param fechaInicio - Fecha de inicio del rango (opcional)
   * @param fechaFin - Fecha de fin del rango (opcional)
   */
  async getExcepciones(
    grupoId?: number,
    fechaInicio?: string,
    fechaFin?: string
  ): Promise<ExcepcionPorcentaje[]> {
    try {
      const params = new URLSearchParams();
      if (grupoId) params.append('grupoId', grupoId.toString());
      if (fechaInicio) params.append('fechaInicio', fechaInicio);
      if (fechaFin) params.append('fechaFin', fechaFin);

      const queryString = params.toString();
      const url = `/api/configuracion-vacaciones/excepciones${queryString ? `?${queryString}` : ''}`;

      const response = await httpClient.get<ApiResponse<ExcepcionPorcentaje[]>>(url);

      // Extract data from API response
      const excepcionesData = response.data || response;
      if (!excepcionesData) {
        throw new Error("Invalid response from server");
      }

      return excepcionesData as unknown as ExcepcionPorcentaje[];
    } catch (error) {
      console.error("Error in excepcionesService.getExcepciones:", error);
      throw error;
    }
  },

  /**
   * Crear una nueva excepción de porcentaje
   * @param excepcionData - Datos de la excepción a crear
   */
  async createExcepcion(excepcionData: CreateExcepcionRequest): Promise<ExcepcionPorcentaje> {
    try {
      const response = await httpClient.post<ApiResponse<ExcepcionPorcentaje>>(
        `/api/configuracion-vacaciones/excepciones`,
        excepcionData
      );

      // Extract data from API response
      const createdExcepcion = response.data || response;
      if (!createdExcepcion) {
        throw new Error("Invalid response from server");
      }

      return createdExcepcion as unknown as ExcepcionPorcentaje;
    } catch (error: any) {
      console.error("Error in excepcionesService.createExcepcion:", error);
      
      // Manejar errores específicos de la API
      if (error.response?.status === 400) {
        throw new Error(error.response?.data?.errorMsg || 'Error en los datos de la excepción');
      }
      
      throw error;
    }
  },

  /**
   * Actualizar una excepción existente
   * @param excepcionId - ID de la excepción a actualizar
   * @param excepcionData - Nuevos datos de la excepción
   */
  async updateExcepcion(
    excepcionId: number, 
    excepcionData: UpdateExcepcionRequest
  ): Promise<ExcepcionPorcentaje> {
    try {
      const response = await httpClient.put<ApiResponse<ExcepcionPorcentaje>>(
        `/api/configuracion-vacaciones/excepciones/${excepcionId}`,
        excepcionData
      );

      // Extract data from API response
      const updatedExcepcion = response.data || response;
      if (!updatedExcepcion) {
        throw new Error("Invalid response from server");
      }

      return updatedExcepcion as unknown as ExcepcionPorcentaje;
    } catch (error: any) {
      console.error("Error in excepcionesService.updateExcepcion:", error);
      
      // Manejar errores específicos de la API
      if (error.response?.status === 404) {
        throw new Error(`Excepción con ID ${excepcionId} no encontrada`);
      } else if (error.response?.status === 400) {
        throw new Error(error.response?.data?.errorMsg || 'Error en los datos de la excepción');
      }
      
      throw error;
    }
  },

  /**
   * Eliminar una excepción de porcentaje
   * @param excepcionId - ID de la excepción a eliminar
   */
  async deleteExcepcion(excepcionId: number): Promise<DeleteExcepcionResponse> {
    try {
      const response = await httpClient.delete<ApiResponse<DeleteExcepcionResponse>>(
        `/api/configuracion-vacaciones/excepciones/${excepcionId}`
      );

      // Extract data from API response
      const deleteResult = response.data || response;
      if (!deleteResult) {
        throw new Error("Invalid response from server");
      }

      return deleteResult as unknown as DeleteExcepcionResponse;
    } catch (error: any) {
      console.error("Error in excepcionesService.deleteExcepcion:", error);
      
      // Manejar errores específicos de la API
      if (error.response?.status === 404) {
        throw new Error(`Excepción con ID ${excepcionId} no encontrada`);
      } else if (error.response?.status === 400) {
        throw new Error(error.response?.data?.errorMsg || 'No se puede eliminar la excepción');
      }
      
      throw error;
    }
  }
};
