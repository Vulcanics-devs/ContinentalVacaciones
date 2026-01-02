import { useState, useEffect, useCallback } from 'react';
import { vacationConfigService } from '../services/vacationConfigService';
import { ApiPeriodMapping, type Period, type ApiPeriod } from '../interfaces/Calendar.interface';
import type { VacationConfig } from '../interfaces/Api.interface';

interface UseVacationConfigState {
  config: VacationConfig | null;
  currentPeriod: Period;
  loading: boolean;
  error: string | null;
}

interface UseVacationConfigReturn extends UseVacationConfigState {
  fetchConfig: () => Promise<void>;
  refetch: () => Promise<void>;
}

/**
 * Hook para manejar la configuración de vacaciones y el período actual
 * Obtiene la configuración desde la API y mapea el período actual
 */
export const useVacationConfig = (): UseVacationConfigReturn => {
  const [state, setState] = useState<UseVacationConfigState>({
    config: null,
    currentPeriod: 'annual', // Default fallback
    loading: false,
    error: null,
  });

  const fetchConfig = useCallback(async () => {
    setState(prev => ({ ...prev, loading: true, error: null }));

    try {
      const config = await vacationConfigService.getVacationConfig();
      
      // Mapear el período de la API al período local
      const apiPeriod = config.periodoActual as ApiPeriod;
      const mappedPeriod = ApiPeriodMapping[apiPeriod] || 'annual';
      
      setState(prev => ({
        ...prev,
        config,
        currentPeriod: mappedPeriod,
        loading: false,
        error: null,
      }));
    } catch (error) {
      setState(prev => ({
        ...prev,
        loading: false,
        error: error instanceof Error ? error.message : 'Error al obtener la configuración de vacaciones',
      }));
    }
  }, []);

  const refetch = useCallback(() => {
    return fetchConfig();
  }, [fetchConfig]);

  // Cargar configuración al montar el componente
  useEffect(() => {
    fetchConfig();
  }, [fetchConfig]);

  return {
    ...state,
    fetchConfig,
    refetch,
  };
};
