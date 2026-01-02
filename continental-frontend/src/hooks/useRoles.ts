import { useState, useEffect } from 'react';
import { roleService } from '@/services/roleService';
import type { Role } from '@/interfaces/Api.interface';

interface UseRolesReturn {
  roles: Role[];
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}

export const useRoles = (): UseRolesReturn => {
  const [roles, setRoles] = useState<Role[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchRoles = async () => {
    try {
      setLoading(true);
      setError(null);
      const rolesData = await roleService.getRoles();
      setRoles(rolesData);
    } catch (err) {
      console.error('Error fetching roles:', err);
      setError(err instanceof Error ? err.message : 'Error al cargar roles');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchRoles();
  }, []);

  return {
    roles,
    loading,
    error,
    refetch: fetchRoles
  };
};
