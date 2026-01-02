/**
 * useEmpleadosOptimized Hook
 * Optimized version that minimizes API calls through better caching strategy
 */

import { useState, useEffect, useCallback, useRef } from 'react';
import { useEmpleadosCache } from '@/contexts/EmpleadosContext';
import { logger } from '@/utils/logger';
import type { 
  EmpleadosSindicalizadosRequest, 
  PaginatedEmpleadosResponse, 
  UsuarioInfoDto 
} from '@/interfaces/Api.interface';

interface UseEmpleadosOptimizedState {
  empleados: UsuarioInfoDto[];
  loading: boolean;
  error: string | null;
  currentPage: number;
  pageSize: number;
  totalUsers: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  filteredByArea?: number;
  filteredByGrupo?: number;
  isFromCache?: boolean;
}

interface UseEmpleadosOptimizedReturn extends UseEmpleadosOptimizedState {
  fetchEmpleados: (request?: EmpleadosSindicalizadosRequest) => Promise<void>;
  refetch: () => Promise<void>;
  setPage: (page: number) => void;
  setPageSize: (pageSize: number) => void;
  setFilters: (areaId?: number, grupoId?: number) => void;
  resetFilters: () => void;
}

export const useEmpleadosOptimized = (
  initialRequest: EmpleadosSindicalizadosRequest = {}
): UseEmpleadosOptimizedReturn => {
  const { getEmpleados, syncEmpleados } = useEmpleadosCache();
  
  const [state, setState] = useState<UseEmpleadosOptimizedState>({
    empleados: [],
    loading: false,
    error: null,
    currentPage: initialRequest.Page || 1,
    pageSize: initialRequest.PageSize || 25,
    totalUsers: 0,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false,
    filteredByArea: initialRequest.AreaId,
    filteredByGrupo: initialRequest.GrupoId,
    isFromCache: false,
  });

  const currentRequestRef = useRef<EmpleadosSindicalizadosRequest>(initialRequest);
  const hasInitializedRef = useRef(false);

  const fetchEmpleados = useCallback(async (request?: EmpleadosSindicalizadosRequest) => {
    const requestToUse = request || currentRequestRef.current;
    
    setState(prev => ({ ...prev, loading: true, error: null }));
    
    try {
      logger.debug('Fetching empleados sindicalizados (optimized)', requestToUse);
      
      const response: PaginatedEmpleadosResponse = await getEmpleados(requestToUse);
      
      setState(prev => ({
        ...prev,
        empleados: response.usuarios,
        currentPage: response.currentPage,
        pageSize: response.pageSize,
        totalUsers: response.totalUsers,
        totalPages: response.totalPages,
        hasNextPage: response.hasNextPage,
        hasPreviousPage: response.hasPreviousPage,
        filteredByArea: response.filteredByArea,
        filteredByGrupo: response.filteredByGrupo,
        loading: false,
        error: null,
      }));

      if (request) {
        currentRequestRef.current = request;
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Error al cargar empleados sindicalizados';
      logger.error('Error in useEmpleadosOptimized', error, 'HOOK');
      
      setState(prev => ({
        ...prev,
        loading: false,
        error: errorMessage,
        empleados: [],
      }));
    }
  }, [getEmpleados]);

  const refetch = useCallback(async () => {
    setState(prev => ({ ...prev, loading: true, error: null }));
    try {
      const sync = await syncEmpleados();
      if (sync) {
        await fetchEmpleados();
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Error al sincronizar empleados';
      setState(prev => ({ ...prev, loading: false, error: errorMessage }));
    }
  }, [fetchEmpleados, syncEmpleados]);

  const setPage = useCallback((page: number) => {
    const newRequest = { ...currentRequestRef.current, Page: page };
    fetchEmpleados(newRequest);
  }, [fetchEmpleados]);

  const setPageSize = useCallback((pageSize: number) => {
    const newRequest = { ...currentRequestRef.current, PageSize: pageSize, Page: 1 };
    fetchEmpleados(newRequest);
  }, [fetchEmpleados]);

  const setFilters = useCallback((areaId?: number, grupoId?: number) => {
    const newRequest = { 
      ...currentRequestRef.current, 
      AreaId: areaId, 
      GrupoId: grupoId, 
      Page: 1 // Reset to first page when filters change
    };
    fetchEmpleados(newRequest);
  }, [fetchEmpleados]);

  const resetFilters = useCallback(() => {
    const newRequest = { 
      Page: 1,
      PageSize: currentRequestRef.current.PageSize || 25
    };
    fetchEmpleados(newRequest);
  }, [fetchEmpleados]);

  // Only fetch on initial mount if we don't have data and haven't initialized yet
  useEffect(() => {
    if (!hasInitializedRef.current && state.empleados.length === 0 && !state.loading) {
      hasInitializedRef.current = true;
      fetchEmpleados(initialRequest);
    }
  }, []); // Empty dependency array - only runs once

  return {
    ...state,
    fetchEmpleados,
    refetch,
    setPage,
    setPageSize,
    setFilters,
    resetFilters,
  };
};
