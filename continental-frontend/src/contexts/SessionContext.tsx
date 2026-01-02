/**
 * Session Management Context
 * Handles session expiration events and provides global session state
 */

import React, { createContext, useContext, useEffect, useState, useCallback, useRef, type ReactNode } from 'react';
import { authService } from '@/services/authService';
import { logger } from '@/utils/logger';

interface SessionContextType {
  isSessionExpired: boolean;
  showSessionExpiredDialog: boolean;
  timeUntilExpiration: number | null; // minutes until expiration
  handleSessionExpired: () => void;
  dismissSessionExpired: () => void;
  refreshSession: () => Promise<void>;
}

const SessionContext = createContext<SessionContextType | undefined>(undefined);

interface SessionProviderProps {
  children: ReactNode;
}

export const SessionProvider: React.FC<SessionProviderProps> = ({ children }) => {
  const [isSessionExpired, setIsSessionExpired] = useState(false);
  const [showSessionExpiredDialog, setShowSessionExpiredDialog] = useState(false);
  const [timeUntilExpiration, setTimeUntilExpiration] = useState<number | null>(null);
  const intervalRef = useRef<NodeJS.Timeout | null>(null);

  // Calculate time until expiration in minutes
  const calculateTimeUntilExpiration = useCallback((): number | null => {
    const expiration = authService.getTokenExpiration();
    if (!expiration) return null;

    const expirationDate = new Date(expiration);
    const now = new Date();
    const diffMs = expirationDate.getTime() - now.getTime();
    const diffMinutes = Math.floor(diffMs / (1000 * 60));

    return diffMinutes > 0 ? diffMinutes : 0;
  }, []);

  // Check session expiration every minute
  const checkSessionExpiration = useCallback(() => {
    // Don't check session expiration on login pages
    const currentPath = window.location.pathname;
    if (currentPath === '/login' || currentPath === '/login-vacaciones') {
      return;
    }

    const minutesLeft = calculateTimeUntilExpiration();
    setTimeUntilExpiration(minutesLeft);

    if (minutesLeft === null) {
      // No expiration data available
      return;
    }

    if (minutesLeft <= 0) {
      // Session has expired
      logger.authAction('Session has expired');
      setIsSessionExpired(true);
      setShowSessionExpiredDialog(true);
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
    } else if (minutesLeft <= 5 && !showSessionExpiredDialog) {
      // Show warning 5 minutes before expiration
      logger.authAction('Session expiring soon', { minutesLeft });
      setShowSessionExpiredDialog(true);
    }
  }, [calculateTimeUntilExpiration, showSessionExpiredDialog]);

  useEffect(() => {
    // Don't check session expiration on login pages
    const currentPath = window.location.pathname;
    if (currentPath === '/login' || currentPath === '/login-vacaciones') {
      return;
    }

    // Initial check
    checkSessionExpiration();

    // Set up interval to check every minute
    intervalRef.current = setInterval(checkSessionExpiration, 60000);

    // Listen for session expired events from httpClient (for 401 errors when refresh fails)
    const handleSessionExpired = () => {
      // Don't show session expired dialog on login pages
      const currentPath = window.location.pathname;
      if (currentPath === '/login' || currentPath === '/login-vacaciones') {
        return;
      }
      
      logger.authAction('Session expired event received from httpClient - refresh failed');
      setIsSessionExpired(true);
      setShowSessionExpiredDialog(true);
      // Clear the interval since session is definitely expired
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
        intervalRef.current = null;
      }
    };

    window.addEventListener('session-expired', handleSessionExpired);

    return () => {
      if (intervalRef.current) {
        clearInterval(intervalRef.current);
      }
      window.removeEventListener('session-expired', handleSessionExpired);
    };
  }, [checkSessionExpiration]);

  const handleSessionExpiredAction = () => {
    setIsSessionExpired(true);
    setShowSessionExpiredDialog(true);
  };

  const dismissSessionExpired = () => {
    setShowSessionExpiredDialog(false);
    setIsSessionExpired(false);
    setTimeUntilExpiration(null);
    
    // Clear the interval
    if (intervalRef.current) {
      clearInterval(intervalRef.current);
      intervalRef.current = null;
    }
    
    // Determine which login page to redirect to based on current route
    const currentPath = window.location.pathname;
    const loginUrl = currentPath.startsWith('/empleados') ? '/login-vacaciones' : '/login';
    
    // Redirect to appropriate login after dismissing
    setTimeout(() => {
      window.location.href = loginUrl;
    }, 100);
  };

  const refreshSession = async () => {
    try {
      await authService.refreshToken();
      setIsSessionExpired(false);
      setShowSessionExpiredDialog(false);
      
      // Restart the expiration check with new token
      checkSessionExpiration();
      
      logger.authAction('Session refreshed successfully');
    } catch (error) {
      logger.error('Failed to refresh session', error, 'SESSION_CONTEXT');
      // If refresh fails, treat as session expired
      handleSessionExpiredAction();
    }
  };

  const contextValue: SessionContextType = {
    isSessionExpired,
    showSessionExpiredDialog,
    timeUntilExpiration,
    handleSessionExpired: handleSessionExpiredAction,
    dismissSessionExpired,
    refreshSession,
  };

  return (
    <SessionContext.Provider value={contextValue}>
      {children}
    </SessionContext.Provider>
  );
};

export const useSession = (): SessionContextType => {
  const context = useContext(SessionContext);
  if (context === undefined) {
    throw new Error('useSession must be used within a SessionProvider');
  }
  return context;
};
