/**
 * Session Expired Dialog Component
 * Shows when user session has expired and needs to re-authenticate
 */

import React from 'react';
import { AlertTriangle, RefreshCw, LogOut } from 'lucide-react';
import { useSession } from '@/contexts/SessionContext';

const SessionExpiredDialog: React.FC = () => {
  const { showSessionExpiredDialog, timeUntilExpiration, dismissSessionExpired, refreshSession } = useSession();

  if (!showSessionExpiredDialog) return null;

  const handleRefresh = async () => {
    try {
      await refreshSession();
    } catch (error) {
      // If refresh fails, dialog will stay open and user can choose to logout
      console.error('Failed to refresh session:', error);
    }
  };

  const handleGoToLogin = () => {
    // Clear all session data before redirecting
    localStorage.removeItem('auth_token');
    localStorage.removeItem('user');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('token_expiration');
    
    // Dismiss the dialog
    dismissSessionExpired();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
    <div className="absolute inset-0 bg-black/50 backdrop-blur-sm z-40 flex items-center justify-center">
    </div>
      <div className="bg-white rounded-lg shadow-xl p-6 max-w-md w-full mx-4 z-50">
        <div className="flex items-center mb-4">
          <AlertTriangle className="h-6 w-6 text-yellow-500 mr-3" />
          <h2 className="text-lg font-semibold text-gray-900">
            {timeUntilExpiration && timeUntilExpiration > 0 ? 'Sesión por Expirar' : 'Sesión Expirada'}
          </h2>
        </div>
        
        <div className="text-gray-600 mb-6">
          {timeUntilExpiration && timeUntilExpiration > 0 ? (
            <>
              <p className="mb-2">
                Tu sesión expirará en <span className="font-semibold text-red-600">{timeUntilExpiration} minuto{timeUntilExpiration !== 1 ? 's' : ''}</span>.
              </p>
              <p>
                ¿Deseas renovar tu sesión para continuar trabajando?
              </p>
            </>
          ) : (
            <p>
              Tu sesión ha expirado por seguridad. Por favor, inicia sesión nuevamente para continuar.
            </p>
          )}
        </div>
        
        <div className="flex flex-col sm:flex-row gap-3">
          {timeUntilExpiration && timeUntilExpiration > 0 ? (
            // Show both buttons when session is about to expire
            <>
              <button
                onClick={handleRefresh}
                className="flex cursor-pointer items-center justify-center px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors"
              >
                <RefreshCw className="h-4 w-4 mr-2" />
                Continuar Sesión
              </button>
              
              <button
                onClick={handleGoToLogin}
                className="flex cursor-pointer items-center justify-center px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 transition-colors"
              >
                <LogOut className="h-4 w-4 mr-2" />
                Iniciar Sesión
              </button>
            </>
          ) : (
            // Show only "Go to Login" button when session has expired
            <button
              onClick={handleGoToLogin}
              className="flex cursor-pointer items-center justify-center px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors w-full"
            >
              <LogOut className="h-4 w-4 mr-2" />
              Ir a Iniciar Sesión
            </button>
          )}
        </div>
      </div>
    </div>
  );
};

export default SessionExpiredDialog;
