import { useState, useEffect, useCallback } from 'react';
import { diasInhabilesService, type DiaInhabil, type GetDiasInhabilesParams } from '../services/diasInhabilesService';

interface UseDiasInhabilesState {
  diasInhabiles: DiaInhabil[];
  loading: boolean;
  error: string | null;
}

interface UseDiasInhabilesReturn extends UseDiasInhabilesState {
  fetchDiasInhabiles: (params?: GetDiasInhabilesParams) => Promise<void>;
  refetch: () => Promise<void>;
}

export const useDiasInhabiles = (initialParams?: GetDiasInhabilesParams): UseDiasInhabilesReturn => {
  const [state, setState] = useState<UseDiasInhabilesState>({
    diasInhabiles: [],
    loading: false,
    error: null,
  });

  const fetchDiasInhabiles = useCallback(async (params?: GetDiasInhabilesParams) => {
    setState(prev => ({ ...prev, loading: true, error: null }));

    try {
      const response = await diasInhabilesService.getDiasInhabiles(params);
      console.log({response})
      if (response.success) {
        setState(prev => ({
          ...prev,
          diasInhabiles: response.data,
          loading: false,
          error: null,
        }));
      } else {
        setState(prev => ({
          ...prev,
          loading: false,
          error: response.errorMsg || 'Error fetching días inhábiles',
        }));
      }
    } catch (error) {
      setState(prev => ({
        ...prev,
        loading: false,
        error: error instanceof Error ? error.message : 'Error fetching días inhábiles',
      }));
    }
  }, []);

  const refetch = useCallback(() => {
    return fetchDiasInhabiles(initialParams);
  }, [fetchDiasInhabiles, initialParams]);

  useEffect(() => {
    fetchDiasInhabiles(initialParams);
  }, [fetchDiasInhabiles, initialParams]);

  return {
    ...state,
    fetchDiasInhabiles,
    refetch,
  };
};
