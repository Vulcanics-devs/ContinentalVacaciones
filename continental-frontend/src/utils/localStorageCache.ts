/**
 * Local Storage Cache Utils
 * Utilities for persisting cache data in localStorage
 */

interface CacheMetadata {
  timestamp: number;
  ttl: number; // time to live in minutes
}

interface LocalCacheEntry<T = any> {
  data: T;
  metadata: CacheMetadata;
}

export class LocalStorageCache {
  private static readonly PREFIX = 'empleados_cache_';

  static set<T>(key: string, data: T, ttlMinutes: number = 10): void {
    try {
      const entry: LocalCacheEntry<T> = {
        data,
        metadata: {
          timestamp: Date.now(),
          ttl: ttlMinutes
        }
      };

      localStorage.setItem(
        this.PREFIX + key, 
        JSON.stringify(entry)
      );
    } catch (error) {
      console.warn('Failed to save to localStorage:', error);
    }
  }

  static get<T>(key: string): T | null {
    try {
      const stored = localStorage.getItem(this.PREFIX + key);
      if (!stored) return null;

      const entry: LocalCacheEntry<T> = JSON.parse(stored);
      
      // Check if expired
      const now = Date.now();
      const ageInMinutes = (now - entry.metadata.timestamp) / (1000 * 60);
      
      if (ageInMinutes > entry.metadata.ttl) {
        this.remove(key);
        return null;
      }

      return entry.data;
    } catch (error) {
      console.warn('Failed to read from localStorage:', error);
      this.remove(key);
      return null;
    }
  }

  static remove(key: string): void {
    try {
      localStorage.removeItem(this.PREFIX + key);
    } catch (error) {
      console.warn('Failed to remove from localStorage:', error);
    }
  }

  static clear(): void {
    try {
      const keys = Object.keys(localStorage);
      keys.forEach(key => {
        if (key.startsWith(this.PREFIX)) {
          localStorage.removeItem(key);
        }
      });
    } catch (error) {
      console.warn('Failed to clear localStorage cache:', error);
    }
  }

  static getValidKeys(): string[] {
    try {
      const keys = Object.keys(localStorage);
      const validKeys: string[] = [];
      
      keys.forEach(key => {
        if (key.startsWith(this.PREFIX)) {
          const cacheKey = key.replace(this.PREFIX, '');
          if (this.get(cacheKey) !== null) {
            validKeys.push(cacheKey);
          }
        }
      });
      
      return validKeys;
    } catch (error) {
      console.warn('Failed to get valid keys from localStorage:', error);
      return [];
    }
  }
}
