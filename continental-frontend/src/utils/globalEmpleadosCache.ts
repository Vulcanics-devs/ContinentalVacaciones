/**
 * Global Empleados Cache Store
 * Persistent cache that survives component unmounting/mounting
 */

import { LocalStorageCache } from '@/utils/localStorageCache';
import { logger } from '@/utils/logger';
import type { 
  EmpleadosSindicalizadosRequest, 
  PaginatedEmpleadosResponse 
} from '@/interfaces/Api.interface';

export interface GlobalCacheEntry {
  data: PaginatedEmpleadosResponse;
  timestamp: number;
  request: EmpleadosSindicalizadosRequest;
}

export class GlobalEmpleadosCache {
  private static instance: GlobalEmpleadosCache;
  private memoryCache = new Map<string, GlobalCacheEntry>();
  private cacheTTL = 15; // 15 minutes default
  private readonly CACHE_VERSION = 'v1.0';

  private constructor() {
    this.initializeFromStorage();
  }

  static getInstance(): GlobalEmpleadosCache {
    if (!GlobalEmpleadosCache.instance) {
      GlobalEmpleadosCache.instance = new GlobalEmpleadosCache();
    }
    return GlobalEmpleadosCache.instance;
  }

  private generateCacheKey(request: EmpleadosSindicalizadosRequest): string {
    // Soportar claves compuestas cuando se consultan múltiples áreas desde el hook
    const anyReq = request as any;
    const composite = anyReq?._allAreas as string | undefined;
    const { AreaId, GrupoId, Page = 1, PageSize = 25 } = request;
    // Si viene _allAreas, úsalo para formar una clave única estable
    if (composite) {
      return `${this.CACHE_VERSION}_ALL(${composite})_${GrupoId || 'all'}_${Page}_${PageSize}`;
    }
    return `${this.CACHE_VERSION}_${AreaId || 'all'}_${GrupoId || 'all'}_${Page}_${PageSize}`;
  }

  private isCacheValid(entry: GlobalCacheEntry): boolean {
    const now = Date.now();
    const ageInMinutes = (now - entry.timestamp) / (1000 * 60);
    return ageInMinutes < this.cacheTTL;
  }

  private initializeFromStorage(): void {
    try {
      // Load valid cache entries from localStorage
      const validKeys = LocalStorageCache.getValidKeys();
      validKeys.forEach(key => {
        if (key.startsWith(this.CACHE_VERSION)) {
          const data = LocalStorageCache.get<PaginatedEmpleadosResponse>(key);
          if (data) {
            // Reconstruct the request from the key
            const [, areaId, grupoId, page, pageSize] = key.split('_');
            const request: EmpleadosSindicalizadosRequest = {
              AreaId: areaId === 'all' ? undefined : parseInt(areaId),
              GrupoId: grupoId === 'all' ? undefined : parseInt(grupoId),
              Page: parseInt(page),
              PageSize: parseInt(pageSize)
            };

            this.memoryCache.set(key, {
              data,
              timestamp: Date.now(), // Use current time since we don't store original timestamp
              request
            });
          }
        }
      });

    } catch (error) {
      logger.error('Error initializing cache from storage', error, 'GLOBAL_CACHE');
    }
  }

  has(request: EmpleadosSindicalizadosRequest): boolean {
    const key = this.generateCacheKey(request);
    const entry = this.memoryCache.get(key);
    return entry ? this.isCacheValid(entry) : false;
  }

  get(request: EmpleadosSindicalizadosRequest): PaginatedEmpleadosResponse | null {
    const key = this.generateCacheKey(request);
    const entry = this.memoryCache.get(key);
    
    if (!entry) return null;
    
    if (!this.isCacheValid(entry)) {
      this.delete(request);
      return null;
    }

    return entry.data;
  }

  set(request: EmpleadosSindicalizadosRequest, data: PaginatedEmpleadosResponse): void {
    const key = this.generateCacheKey(request);
    const entry: GlobalCacheEntry = {
      data,
      timestamp: Date.now(),
      request
    };

    // Store in memory
    this.memoryCache.set(key, entry);
    
    // Store in localStorage (skip giant composite keys to avoid size bloat)
    if (!key.includes('ALL(')) {
      LocalStorageCache.set(key, data, this.cacheTTL);
    }
    
  }

  delete(request: EmpleadosSindicalizadosRequest): void {
    const key = this.generateCacheKey(request);
    this.memoryCache.delete(key);
    LocalStorageCache.remove(key);
    logger.debug('Cache entry deleted', { key });
  }

  invalidateByArea(areaId?: number): void {
    const keysToDelete: string[] = [];
    
    this.memoryCache.forEach((entry, key) => {
      if (areaId) {
        // Delete entries for specific area
        if (entry.request.AreaId === areaId || key.includes(`_${areaId}_`) || key.includes('ALL(')) {
          keysToDelete.push(key);
        }
      } else {
        // Delete all entries
        keysToDelete.push(key);
      }
    });

    keysToDelete.forEach(key => {
      this.memoryCache.delete(key);
      LocalStorageCache.remove(key);
    });

    logger.debug('Cache invalidated', { 
      areaId, 
      deletedEntries: keysToDelete.length 
    });
  }

  clear(): void {
    this.memoryCache.clear();
    LocalStorageCache.clear();
    logger.debug('All cache cleared');
  }

  getStats() {
    const totalEntries = this.memoryCache.size;
    const validEntries = Array.from(this.memoryCache.values()).filter(entry => 
      this.isCacheValid(entry)
    ).length;

    return {
      totalEntries,
      validEntries,
      expiredEntries: totalEntries - validEntries,
      cacheTTL: this.cacheTTL
    };
  }

  setCacheTTL(minutes: number): void {
    this.cacheTTL = minutes;
  }
}

// Export singleton instance
export const globalEmpleadosCache = GlobalEmpleadosCache.getInstance();
