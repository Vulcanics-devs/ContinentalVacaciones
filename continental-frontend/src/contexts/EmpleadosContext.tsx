/**
 * Empleados Cache Context
 * Manages caching for empleados sindicalizados to reduce API calls
 */

import React, { createContext, useContext, useCallback, useRef, type ReactNode } from 'react';
import { empleadosService } from '@/services/empleadosService';
import { logger } from '@/utils/logger';
import { globalEmpleadosCache } from '@/utils/globalEmpleadosCache';
import type { 
  EmpleadosSindicalizadosRequest, 
  PaginatedEmpleadosResponse 
} from '@/interfaces/Api.interface';

interface CacheEntry {
  data: PaginatedEmpleadosResponse;
  timestamp: number;
  request: EmpleadosSindicalizadosRequest;
}

interface EmpleadosContextType {
  getEmpleados: (request: EmpleadosSindicalizadosRequest) => Promise<PaginatedEmpleadosResponse>;
  invalidateCache: (areaId?: number) => void;
  clearAllCache: () => void;
  syncEmpleados: () => Promise<{ created: number }>;
}

const EmpleadosContext = createContext<EmpleadosContextType | undefined>(undefined);

interface EmpleadosProviderProps {
  children: ReactNode;
  cacheTTL?: number; // Cache time-to-live in minutes (default: 5 minutes)
}

export const EmpleadosProvider: React.FC<EmpleadosProviderProps> = ({ 
  children, 
  cacheTTL = 10 // Increased default cache time to 10 minutes
}) => {
  const cacheRef = useRef<Map<string, CacheEntry>>(new Map());
  const loadingRef = useRef<Set<string>>(new Set());
  const pendingRequestsRef = useRef<Map<string, Promise<PaginatedEmpleadosResponse>>>(new Map());

  // Generate cache key from request parameters
  const generateCacheKey = useCallback((request: EmpleadosSindicalizadosRequest): string => {
    const { AreaId, GrupoId, Page = 1, PageSize = 25 } = request;
    return `${AreaId || 'all'}-${GrupoId || 'all'}-${Page}-${PageSize}`;
  }, []);

  // Check if cache entry is still valid
  const isCacheValid = useCallback((entry: CacheEntry): boolean => {
    const now = Date.now();
    const ageInMinutes = (now - entry.timestamp) / (1000 * 60);
    return ageInMinutes < cacheTTL;
  }, [cacheTTL]);

  // Get empleados with caching
  const getEmpleados = useCallback(async (
    request: EmpleadosSindicalizadosRequest
  ): Promise<PaginatedEmpleadosResponse> => {
    const cacheKey = generateCacheKey(request);
    const cache = cacheRef.current;
    const pendingRequests = pendingRequestsRef.current;
    const loading = loadingRef.current;
    
    // Check global cache first
    const globalCachedData = globalEmpleadosCache.get(request);
    if (globalCachedData) {
      logger.debug('Using global cached empleados data', { cacheKey, request });
      return globalCachedData;
    }

    // Check if we have a valid cached entry in memory
    const cachedEntry = cache.get(cacheKey);
    if (cachedEntry && isCacheValid(cachedEntry)) {
      logger.debug('Using memory cached empleados data', { cacheKey, request });
      return cachedEntry.data;
    }

    // Check if this request is already in progress
    if (loading.has(cacheKey)) {
      const existingPromise = pendingRequests.get(cacheKey);
      if (existingPromise) {
        logger.debug('Reusing pending request', { cacheKey });
        return existingPromise;
      }
    }

    // Mark this request as in progress and create the promise
    loading.add(cacheKey);
    
    const requestPromise = (async (): Promise<PaginatedEmpleadosResponse> => {
      try {
        logger.debug('Fetching fresh empleados data', { cacheKey, request });
        const response = await empleadosService.getEmpleadosSindicalizados(request);
        
        // Cache the response in both memory and global cache
        const cacheEntry: CacheEntry = {
          data: response,
          timestamp: Date.now(),
          request
        };
        cache.set(cacheKey, cacheEntry);
        
        // Store in global cache for persistence across navigation
        globalEmpleadosCache.set(request, response);
        
        logger.debug('Cached empleados data', { cacheKey, totalUsers: response.totalUsers });
        
        return response;
      } catch (error) {
        logger.error('Error fetching empleados', error, 'EMPLEADOS_CONTEXT');
        throw error;
      } finally {
        // Clean up
        loading.delete(cacheKey);
        pendingRequests.delete(cacheKey);
      }
    })();

    // Store the promise so other calls can reuse it
    pendingRequests.set(cacheKey, requestPromise);
    
    return requestPromise;
  }, [generateCacheKey, isCacheValid]);

  // Invalidate cache for specific area or all
  const invalidateCache = useCallback((areaId?: number) => {
    const cache = cacheRef.current;
    
    if (areaId) {
      // Remove cache entries for specific area from memory
      const keysToDelete: string[] = [];
      cache.forEach((entry, key) => {
        if (entry.request.AreaId === areaId || key.startsWith(`${areaId}-`)) {
          keysToDelete.push(key);
        }
      });
      keysToDelete.forEach(key => cache.delete(key));
      
      // Also invalidate global cache
      globalEmpleadosCache.invalidateByArea(areaId);
      
      logger.debug('Invalidated cache for area', { areaId, deletedKeys: keysToDelete.length });
    } else {
      // Clear all cache
      cache.clear();
      globalEmpleadosCache.clear();
      logger.debug('Invalidated all empleados cache');
    }
  }, []);

  // Clear all cache
  const clearAllCache = useCallback(() => {
    cacheRef.current.clear();
    loadingRef.current.clear();
    pendingRequestsRef.current.clear();
    globalEmpleadosCache.clear();
    logger.debug('Cleared all empleados cache');
  }, []);

  // Sync empleados and invalidate cache
  const syncEmpleados = useCallback(async (): Promise<{ created: number }> => {
    try {
      logger.debug('Syncing empleados sindicalizados');
      const result = await empleadosService.syncEmpleadosSindicalizados();
      
      // Clear all cache after sync
      clearAllCache();
      
      logger.debug('Empleados sync completed, cache cleared', result);
      return result;
    } catch (error) {
      logger.error('Error syncing empleados', error, 'EMPLEADOS_CONTEXT');
      throw error;
    }
  }, [clearAllCache]);

  const contextValue: EmpleadosContextType = {
    getEmpleados,
    invalidateCache,
    clearAllCache,
    syncEmpleados,
  };

  return (
    <EmpleadosContext.Provider value={contextValue}>
      {children}
    </EmpleadosContext.Provider>
  );
};

export const useEmpleadosCache = (): EmpleadosContextType => {
  const context = useContext(EmpleadosContext);
  if (context === undefined) {
    throw new Error('useEmpleadosCache must be used within an EmpleadosProvider');
  }
  return context;
};
