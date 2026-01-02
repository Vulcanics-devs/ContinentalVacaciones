/**
 * useAuth Hook
 * Custom hook for authentication state management
 */

import { useState, useEffect } from 'react';
import { authService, type AuthState } from '@/services/authService';
import { logger } from '@/utils/logger';
import type { User, UserRole } from '@/interfaces/User.interface';
import type { LoginRequest } from '@/interfaces/Api.interface';

export const useAuth = () => {
  const [authState, setAuthState] = useState<AuthState>(() => authService.getAuthState());

  useEffect(() => {
    // Initialize auth state
    const currentState = authService.getAuthState();
    setAuthState(currentState);

    // Listen for storage changes (for multi-tab support)
    const handleStorageChange = (e: StorageEvent) => {
      if (e.key === 'user' || e.key === 'auth_token') {
        const newState = authService.getAuthState();
        setAuthState(newState);
        logger.debug('Auth state updated from storage change');
      }
    };

    window.addEventListener('storage', handleStorageChange);
    return () => window.removeEventListener('storage', handleStorageChange);
  }, []);

  const login = async (credentials: LoginRequest): Promise<User> => {
    setAuthState(prev => ({ ...prev, isLoading: true }));
    
    try {
      const user = await authService.login(credentials);
      const newState = authService.getAuthState();
      setAuthState(newState);
      return user as unknown as User;
    } catch (error) {
      setAuthState(prev => ({ ...prev, isLoading: false }));
      throw error;
    }
  };

  const logout = async (): Promise<void> => {
    setAuthState(prev => ({ ...prev, isLoading: true }));
    
    try {
      await authService.logout();
      setAuthState({
        user: null,
        token: null,
        expiration: null,
        isAuthenticated: false,
        isLoading: false,
      });
    } catch (error) {
      logger.error('Logout error in hook', error);
      // Still clear state even if logout fails
      setAuthState({
        user: null,
        token: null,
        expiration: null,
        isAuthenticated: false,
        isLoading: false,
      });
    }
  };

  const refreshToken = async (): Promise<string> => {
    try {
      const newToken = await authService.refreshToken();
      const newState = authService.getAuthState();
      setAuthState(newState);
      return newToken;
    } catch (error) {
      // If refresh fails, clear auth state
      setAuthState({
        user: null,
        token: null,
        expiration: null,
        isAuthenticated: false,
        isLoading: false,
      });
      throw error;
    }
  };

  const hasRole = (role: UserRole): boolean => {
    return authService.hasRole(role);
  };

  const hasAnyRole = (roles: UserRole[]): boolean => {
    return authService.hasAnyRole(roles);
  };

  return {
    // State
    user: authState.user,
    token: authState.token,
    isAuthenticated: authState.isAuthenticated,
    isLoading: authState.isLoading,
    
    // Actions
    login,
    logout,
    refreshToken,
    
    // Utilities
    hasRole,
    hasAnyRole,
  };
};

export default useAuth;