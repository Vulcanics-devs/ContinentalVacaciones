import { httpClient } from './httpClient';
import type { VacationConfig, ApiResponse } from '@/interfaces/Api.interface';

/**
 * Vacation Configuration Service
 * Handles API calls related to vacation configuration
 */
class VacationConfigService {
  private readonly baseUrl = '/api/configuracion-vacaciones';

  /**
   * Get current vacation configuration
   * @returns Promise<VacationConfig>
   */
  async getVacationConfig(): Promise<VacationConfig> {
    try {
      const response = await httpClient.get<ApiResponse<VacationConfig>>(this.baseUrl);
      
      if (!response.data) {
        throw new Error('Error al obtener la configuración de vacaciones');
      }
      
      return response.data as unknown as VacationConfig;
    } catch (error) {
      console.error('Error fetching vacation configuration:', error);
      throw error;
    }
  }
}

export const vacationConfigService = new VacationConfigService();
