/**
 * useEmpleadosSindicalizados Hook
 * Custom hook for managing unionized employees state and operations
 * Now uses caching through EmpleadosContext to reduce API calls
 */

import { useState, useEffect, useCallback } from 'react';
import { useEmpleadosCache } from '@/contexts/EmpleadosContext';
import { logger } from '@/utils/logger';
import type { 
  EmpleadosSindicalizadosRequest, 
  PaginatedEmpleadosResponse, 
  UsuarioInfoDto 
} from '@/interfaces/Api.interface';

interface UseEmpleadosSindicalizadosState {
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
}

interface UseEmpleadosSindicalizadosReturn extends UseEmpleadosSindicalizadosState {
  fetchEmpleados: (request?: EmpleadosSindicalizadosRequest) => Promise<void>;
  refetch: () => Promise<void>;
  setPage: (page: number) => void;
  setPageSize: (pageSize: number) => void;
  setFilters: (areaId?: number, grupoId?: number) => void;
}

export const useEmpleadosSindicalizados = (
  initialRequest: EmpleadosSindicalizadosRequest = {}
): UseEmpleadosSindicalizadosReturn => {
  const { getEmpleados, syncEmpleados } = useEmpleadosCache();
  
  const [state, setState] = useState<UseEmpleadosSindicalizadosState>({
    empleados: [],
    loading: false,
    error: null,
    currentPage: 1,
    pageSize: 25,
    totalUsers: 0,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false,
    filteredByArea: initialRequest.AreaId,
    filteredByGrupo: initialRequest.GrupoId,
  });

  const [currentRequest, setCurrentRequest] = useState<EmpleadosSindicalizadosRequest>(initialRequest);

  const fetchEmpleados = useCallback(async (request?: EmpleadosSindicalizadosRequest) => {
    const requestToUse = request || currentRequest;
    
    setState(prev => ({ ...prev, loading: true, error: null }));
    
    try {
      logger.debug('Fetching empleados sindicalizados', requestToUse);
      
      const response: PaginatedEmpleadosResponse = await getEmpleados(requestToUse);
      
      console.log('response', response);
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
        setCurrentRequest(request);
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Error al cargar empleados sindicalizados';
      logger.error('Error in useEmpleadosSindicalizados', error, 'HOOK');
      
      setState(prev => ({
        ...prev,
        loading: false,
        error: errorMessage,
        empleados: [],
      }));
    }
  }, [currentRequest, getEmpleados]);

  const refetch = useCallback(async () => {
    setState(prev => ({ ...prev, loading: true, error: null }));
    const sync = await syncEmpleados();
    if (sync) {
      fetchEmpleados();
    }
    setState(prev => ({ ...prev, loading: false, error: null }));
    
  }, [fetchEmpleados, syncEmpleados]);

  const setPage = useCallback((page: number) => {
    const newRequest = { ...currentRequest, Page: page };
    fetchEmpleados(newRequest);
  }, [currentRequest, fetchEmpleados]);

  const setPageSize = useCallback((pageSize: number) => {
    const newRequest = { ...currentRequest, PageSize: pageSize, Page: 1 };
    fetchEmpleados(newRequest);
  }, [currentRequest, fetchEmpleados]);

  const setFilters = useCallback((areaId?: number, grupoId?: number) => {
    const newRequest = { 
      ...currentRequest, 
      AreaId: areaId, 
      GrupoId: grupoId, 
      Page: 1 // Reset to first page when filters change
    };
    fetchEmpleados(newRequest);
  }, [currentRequest, fetchEmpleados]);

  // Initial fetch on mount - only if we don't have cached data
  useEffect(() => {
    // Only fetch if we don't have any data yet
    if (state.empleados.length === 0 && !state.loading && !state.error) {
      fetchEmpleados(initialRequest);
    }
  }, []); // Only run once on mount

  return {
    ...state,
    fetchEmpleados,
    refetch,
    setPage,
    setPageSize,
    setFilters,
  };
};
