/**
 * Debug Utils for Empleados Cache
 * Utilities to debug cache behavior
 */

import { globalEmpleadosCache } from './globalEmpleadosCache';

export class EmpleadosDebugger {
  private static logLevel: 'off' | 'basic' | 'verbose' = 'off';

  static setLogLevel(level: 'off' | 'basic' | 'verbose') {
    this.logLevel = level;
  }

  static logCacheStats() {
    if (this.logLevel === 'off') return;
    
    const stats = globalEmpleadosCache.getStats();
    console.group('📊 Cache Stats');
    console.log('Total entries:', stats.totalEntries);
    console.log('Valid entries:', stats.validEntries);
    console.log('Expired entries:', stats.expiredEntries);
    console.log('Cache TTL:', stats.cacheTTL, 'minutes');
    console.groupEnd();
  }

  static logNavigationEvent(componentName: string, action: string, request?: any) {
    if (this.logLevel === 'off') return;
    
    const timestamp = new Date().toISOString().split('T')[1].split('.')[0];
    console.log(`🧭 [${timestamp}] ${componentName} - ${action}`, request || '');
    
    if (this.logLevel === 'verbose') {
      this.logCacheStats();
    }
  }

  static logApiCall(request: any, fromCache: boolean) {
    if (this.logLevel === 'off') return;
    
    const timestamp = new Date().toISOString().split('T')[1].split('.')[0];
    const source = fromCache ? '💾 CACHE' : '🌐 API';
    console.log(`${source} [${timestamp}]`, request);
  }

  static logComponentMount(componentName: string, initialRequest?: any) {
    if (this.logLevel === 'off') return;
    
    console.group(`🏗️ ${componentName} MOUNT`);
    console.log('Initial request:', initialRequest);
    this.logCacheStats();
    console.groupEnd();
  }

  static logComponentUnmount(componentName: string) {
    if (this.logLevel === 'off') return;
    
    console.log(`🗑️ ${componentName} UNMOUNT`);
  }
}

// Auto-enable debugging in development
if (import.meta.env.DEV) {
  EmpleadosDebugger.setLogLevel('off');
  
  // Make debugger available globally for manual debugging
  (window as any).debugEmpleados = {
    stats: () => EmpleadosDebugger.logCacheStats(),
    clear: () => globalEmpleadosCache.clear(),
    setLogLevel: (level: 'off' | 'basic' | 'verbose') => EmpleadosDebugger.setLogLevel(level),
    cache: globalEmpleadosCache
  };

}

export const debugEmpleados = EmpleadosDebugger;
