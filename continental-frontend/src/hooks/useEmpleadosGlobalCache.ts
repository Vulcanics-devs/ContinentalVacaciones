/**
 * useEmpleadosGlobalCache Hook
 * Uses global cache to persist data across navigation
 */

import { useState, useEffect, useCallback, useRef } from 'react';
import { globalEmpleadosCache } from '@/utils/globalEmpleadosCache';
import { empleadosService } from '@/services/empleadosService';
import { logger } from '@/utils/logger';
import type { 
  EmpleadosSindicalizadosRequest, 
  PaginatedEmpleadosResponse, 
  UsuarioInfoDto 
} from '@/interfaces/Api.interface';

interface UseEmpleadosGlobalCacheState {
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

interface UseEmpleadosGlobalCacheReturn extends UseEmpleadosGlobalCacheState {
  fetchEmpleados: (request?: EmpleadosSindicalizadosRequest) => Promise<void>;
  refetch: () => Promise<void>;
  setPage: (page: number) => void;
  setPageSize: (pageSize: number) => void;
  setFilters: (areaId?: number, grupoId?: number) => void;
  resetFilters: () => void;
  invalidateCache: (areaId?: number) => void;
  clearAllCache: () => void;
}

export const useEmpleadosGlobalCache = (
  initialRequest: EmpleadosSindicalizadosRequest = {}
): UseEmpleadosGlobalCacheReturn => {
  
  const [state, setState] = useState<UseEmpleadosGlobalCacheState>({
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
  const pendingRequestsRef = useRef<Map<string, Promise<PaginatedEmpleadosResponse>>>(new Map());

  const generateRequestKey = useCallback((request: EmpleadosSindicalizadosRequest): string => {
    const { AreaId, GrupoId, Page = 1, PageSize = 25 } = request;
    return `${AreaId || 'all'}_${GrupoId || 'all'}_${Page}_${PageSize}`;
  }, []);

  const fetchEmpleados = useCallback(async (request?: EmpleadosSindicalizadosRequest) => {
    const requestToUse = request || currentRequestRef.current;
    const requestKey = generateRequestKey(requestToUse);
    
    // Check if there's a pending request for this exact query
    const pendingRequest = pendingRequestsRef.current.get(requestKey);
    if (pendingRequest) {
      logger.debug('Reusing pending request for empleados', requestToUse);
      try {
        const response = await pendingRequest;
        updateStateFromResponse(response, true);
        return;
      } catch (error) {
        // If pending request fails, continue with normal flow
      }
    }
    
    setState(prev => ({ ...prev, loading: true, error: null }));
    
    // Check global cache first
    const cachedData = globalEmpleadosCache.get(requestToUse);
    if (cachedData) {
      logger.debug('Using global cached empleados data', requestToUse);
      updateStateFromResponse(cachedData, true);
      setState(prev => ({ ...prev, loading: false }));
      
      if (request) {
        currentRequestRef.current = request;
      }
      return;
    }
    
    // Create and store the API request promise
    const requestPromise = (async (): Promise<PaginatedEmpleadosResponse> => {
      try {
        logger.debug('Fetching fresh empleados data (global cache)', requestToUse);
        const response = await empleadosService.getEmpleadosSindicalizados(requestToUse);
        
        // Store in global cache
        globalEmpleadosCache.set(requestToUse, response);
        
        return response;
      } finally {
        // Clean up pending request
        pendingRequestsRef.current.delete(requestKey);
      }
    })();
    
    pendingRequestsRef.current.set(requestKey, requestPromise);
    
    try {
      const response = await requestPromise;
      updateStateFromResponse(response, false);

      if (request) {
        currentRequestRef.current = request;
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Error al cargar empleados sindicalizados';
      logger.error('Error in useEmpleadosGlobalCache', error, 'HOOK');
      
      setState(prev => ({
        ...prev,
        loading: false,
        error: errorMessage,
        empleados: [],
        isFromCache: false,
      }));
    }
  }, [generateRequestKey]);

  const updateStateFromResponse = useCallback((response: PaginatedEmpleadosResponse, fromCache: boolean) => {
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
      isFromCache: fromCache,
    }));
  }, []);

  const refetch = useCallback(async () => {
    setState(prev => ({ ...prev, loading: true, error: null }));
    try {
      const sync = await empleadosService.syncEmpleadosSindicalizados();
      if (sync) {
        // Clear cache and fetch fresh data
        globalEmpleadosCache.clear();
        await fetchEmpleados();
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Error al sincronizar empleados';
      setState(prev => ({ ...prev, loading: false, error: errorMessage }));
    }
  }, [fetchEmpleados]);

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

  const invalidateCache = useCallback((areaId?: number) => {
    globalEmpleadosCache.invalidateByArea(areaId);
    logger.debug('Cache invalidated from hook', { areaId });
  }, []);

  const clearAllCache = useCallback(() => {
    globalEmpleadosCache.clear();
    pendingRequestsRef.current.clear();
    logger.debug('All cache cleared from hook');
  }, []);

  // Only fetch on initial mount if we don't have data and haven't initialized yet
  useEffect(() => {
    if (!hasInitializedRef.current) {
      hasInitializedRef.current = true;
      
      // Check if we have cached data first
      const cachedData = globalEmpleadosCache.get(initialRequest);
      if (cachedData) {
        logger.debug('Initializing with cached data', initialRequest);
        updateStateFromResponse(cachedData, true);
        currentRequestRef.current = initialRequest;
      } else if (state.empleados.length === 0 && !state.loading) {
        logger.debug('No cached data, fetching fresh', initialRequest);
        fetchEmpleados(initialRequest);
      }
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
    invalidateCache,
    clearAllCache,
  };
};
