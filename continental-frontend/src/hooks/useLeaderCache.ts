import { useState, useCallback, useRef } from 'react';
import { userService } from '@/services/userService';
import type { User } from '@/interfaces/User.interface';

// Cache global para líderes
const leaderCache = new Map<number, User>();
const pendingRequests = new Map<number, Promise<User>>();

export const useLeaderCache = () => {
  const [loading, setLoading] = useState(false);
  const abortControllerRef = useRef<AbortController | null>(null);

  // Función para obtener múltiples líderes de forma optimizada
  const getLeadersBatch = useCallback(async (leaderIds: number[]): Promise<Map<number, User>> => {
    // Filtrar IDs que ya están en cache
    const uncachedIds = leaderIds.filter(id => !leaderCache.has(id) && !pendingRequests.has(id));
    
    if (uncachedIds.length === 0) {
      // Todos los líderes ya están en cache
      const result = new Map<number, User>();
      leaderIds.forEach(id => {
        const leader = leaderCache.get(id);
        if (leader) {
          result.set(id, leader);
        }
      });
      return result;
    }

    // Cancelar request anterior si existe
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }

    abortControllerRef.current = new AbortController();
    setLoading(true);

    try {
      // Crear promesas para los IDs no cacheados
      const promises = uncachedIds.map(async (id) => {
        // Evitar requests duplicados
        if (pendingRequests.has(id)) {
          return pendingRequests.get(id)!;
        }

        const promise = userService.getUserById(id);
        pendingRequests.set(id, promise);
        
        try {
          const user = await promise;
          leaderCache.set(id, user);
          pendingRequests.delete(id);
          return user;
        } catch (error) {
          pendingRequests.delete(id);
          throw error;
        }
      });

      // Ejecutar todas las promesas en paralelo
      await Promise.allSettled(promises);

      // Construir resultado final
      const result = new Map<number, User>();
      leaderIds.forEach(id => {
        const leader = leaderCache.get(id);
        if (leader) {
          result.set(id, leader);
        }
      });

      return result;
    } catch (error) {
      console.error('Error loading leaders batch:', error);
      throw error;
    } finally {
      setLoading(false);
      abortControllerRef.current = null;
    }
  }, []);

  // Función para obtener un líder individual (usa cache si está disponible)
  const getLeader = useCallback(async (leaderId: number): Promise<User | null> => {
    if (leaderCache.has(leaderId)) {
      return leaderCache.get(leaderId)!;
    }

    if (pendingRequests.has(leaderId)) {
      return await pendingRequests.get(leaderId)!;
    }

    try {
      const promise = userService.getUserById(leaderId);
      pendingRequests.set(leaderId, promise);
      
      const user = await promise;
      leaderCache.set(leaderId, user);
      pendingRequests.delete(leaderId);
      
      return user;
    } catch (error) {
      pendingRequests.delete(leaderId);
      console.error(`Error loading leader ${leaderId}:`, error);
      return null;
    }
  }, []);

  // Función para limpiar cache (útil para testing o refresh)
  const clearCache = useCallback(() => {
    leaderCache.clear();
    pendingRequests.clear();
  }, []);

  // Función para formatear nombre del líder
  const formatLeaderName = useCallback((fullName: string): string => {
    if (!fullName) return '';
    const names = fullName.trim().split(' ');
    return names.slice(0, 2).join(' ');
  }, []);

  return {
    getLeadersBatch,
    getLeader,
    clearCache,
    formatLeaderName,
    loading
  };
};
