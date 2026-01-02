/**
 * Servicio para la gestión de programación anual
 * Maneja operaciones de reversión completa y resúmenes
 */

import { httpClient } from './httpClient';
import type { ApiResponse, ResumenReversionResponse, RevertirCompletoResponse } from '@/interfaces/Api.interface';

export class ProgramacionAnualService {
  /**
   * Obtiene un resumen detallado de todo lo que sería eliminado antes de ejecutar la reversión
   * @param anio - Año a consultar
   * @returns Promise<ResumenReversionResponse>
   */
  static async obtenerResumenReversion(anio: number): Promise<ResumenReversionResponse> {
    try {
      console.log('🔍 Obteniendo resumen de reversión para año:', anio);
      
      const response: ApiResponse<ResumenReversionResponse> = await httpClient.get(
        `/api/ProgramacionAnual/resumen-reversion?anio=${anio}`
      );

      console.log('📊 Resumen de reversión obtenido:', response);

      if (!response.success) {
        throw new Error(response.errorMsg || 'Error al obtener el resumen de reversión');
      }

      return response.data!;
    } catch (error) {
      console.error('❌ Error al obtener resumen de reversión:', error);
      throw error;
    }
  }

  /**
   * Elimina COMPLETAMENTE toda la programación anual del año especificado
   * @param anio - Año a revertir
   * @param confirmar - Debe ser true para ejecutar la operación
   * @returns Promise<RevertirCompletoResponse>
   */
  static async revertirCompleto(anio: number, confirmar: boolean = true): Promise<RevertirCompletoResponse> {
    try {
      console.log('🔄 Ejecutando reversión completa para año:', anio, 'confirmar:', confirmar);
      
      // Usar timeout de 10 minutos para esta operación crítica
      const response: ApiResponse<RevertirCompletoResponse> = await httpClient.delete(
        `/api/ProgramacionAnual/revertir-completo?anio=${anio}&confirmar=${confirmar}`,
        { timeout: 600000 } // 10 minutos
      );

      console.log('🔍 Respuesta de reversión completa:', response);

      if (!response.success) {
        throw new Error(response.errorMsg || 'Error al revertir la programación anual');
      }

      const result = response.data!;
      console.log('✅ Reversión completa exitosa:', result);
      return result;
    } catch (error) {
      console.error('❌ Error en reversión completa:', error);
      throw error;
    }
  }
}
