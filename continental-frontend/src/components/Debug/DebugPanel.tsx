/**
 * Debug Panel Component
 * Shows debug information and allows testing of services
 * Only visible in development mode
 */

import { useState } from 'react';
import { Button } from '../ui/button';
import { logger } from '@/utils/logger';
import { authService } from '@/services/authService';
import { env } from '@/config/env';

export const DebugPanel = () => {
  const [isVisible, setIsVisible] = useState(false);

  // Only show in development mode
  if (!env.DEBUG_MODE || env.NODE_ENV === 'production') {
    return null;
  }

  const testLogger = () => {
    logger.debug('Debug test message', { test: 'data' });
    logger.info('Info test message', { test: 'data' });
    logger.warn('Warning test message', { test: 'data' });
    logger.error('Error test message', new Error('Test error'));
  };

  const testAuthService = () => {
    const authState = authService.getAuthState();
    logger.info('Current auth state', authState);
  };

  const clearStorage = () => {
    localStorage.clear();
    logger.info('Storage cleared');
    window.location.reload();
  };

  if (!isVisible) {
    return (
      <div className="fixed bottom-4 right-4 z-50">
        <Button
          onClick={() => setIsVisible(true)}
          className="bg-red-500 hover:bg-red-600 text-white text-xs px-2 py-1 h-8"
        >
          Debug
        </Button>
      </div>
    );
  }

  return (
    <div className="fixed bottom-4 right-4 z-50 bg-white border border-gray-300 rounded-lg shadow-lg p-4 w-80">
      <div className="flex justify-between items-center mb-3">
        <h3 className="font-bold text-sm">Debug Panel</h3>
        <Button
          onClick={() => setIsVisible(false)}
          className="text-xs px-2 py-1 h-6"
          variant="outline"
        >
          ×
        </Button>
      </div>
      
      <div className="space-y-2">
        <div className="text-xs text-gray-600">
          <div>Environment: {env.NODE_ENV}</div>
          <div>Debug Mode: {env.DEBUG_MODE ? 'ON' : 'OFF'}</div>
          <div>Log Level: {env.LOG_LEVEL}</div>
          <div>API URL: {env.API_BASE_URL}</div>
        </div>
        
        <div className="space-y-1">
          <Button
            onClick={testLogger}
            className="w-full text-xs h-8"
            variant="outline"
          >
            Test Logger
          </Button>
          
          <Button
            onClick={testAuthService}
            className="w-full text-xs h-8"
            variant="outline"
          >
            Check Auth State
          </Button>
          
          <Button
            onClick={clearStorage}
            className="w-full text-xs h-8 bg-red-500 hover:bg-red-600 text-white"
          >
            Clear Storage
          </Button>
        </div>
        
        <div className="text-xs text-gray-500 mt-2">
          Check browser console for logs
        </div>
      </div>
    </div>
  );
};