import { httpClient } from './httpClient';
import type { ApiResponse, Role } from '@/interfaces/Api.interface';

class RoleService {
  /**
   * Fetch all roles from the API
   */
  async getRoles(): Promise<Role[]> {
    try {
      const response = await httpClient.get<ApiResponse<Role[]>>('/api/Rol');
      
      if (response?.success && response?.data) {
        return response.data as unknown as Role[];
      } else {
        throw new Error(response?.errorMsg || 'Error al obtener roles');
      }
    } catch (error) {
      console.error('Error fetching roles:', error);
      throw error;
    }
  }
}

export const roleService = new RoleService();
