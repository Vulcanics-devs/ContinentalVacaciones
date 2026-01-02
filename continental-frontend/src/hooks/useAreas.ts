import { useState, useEffect, useCallback } from 'react';
import type { Area } from '@/interfaces/Areas.interface';
import { areasService } from '@/services/areasService';

interface UseAreasReturn {
  areas: Area[];
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
  getAreaById: (areaId: number) => Promise<Area>;
  loadingAreaDetails: boolean;
}

export const useAreas = (): UseAreasReturn => {
  const [areas, setAreas] = useState<Area[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [loadingAreaDetails, setLoadingAreaDetails] = useState(false);

  const fetchAreas = async () => {
    try {
      setLoading(true);
      setError(null);
      const areasData = await areasService.getAreas();
      setAreas(areasData);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error al cargar las áreas';
      setError(errorMessage);
      console.error('Error fetching areas:', err);
    } finally {
      setLoading(false);
    }
  };

  const getAreaById = useCallback(async (areaId: number): Promise<Area> => {
    try {
      setLoadingAreaDetails(true);
      const areaDetails = await areasService.getAreaById(areaId);
      return areaDetails;
    } catch (error) {
      console.error('Error fetching area details:', error);
      throw error;
    } finally {
      setLoadingAreaDetails(false);
    }
  }, []);

  useEffect(() => {
    fetchAreas();
  }, []);

  return {
    areas,
    loading,
    error,
    refetch: fetchAreas,
    getAreaById,
    loadingAreaDetails
  };
};
