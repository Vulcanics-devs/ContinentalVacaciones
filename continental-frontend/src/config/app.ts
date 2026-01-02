/**
 * Application Configuration
 * Centralized configuration for the entire application
 */

import { env } from './env';

export const appConfig = {
  // Application Info
  name: env.APP_NAME,
  version: env.APP_VERSION,
  
  // Environment
  isDevelopment: env.NODE_ENV === 'development',
  isProduction: env.NODE_ENV === 'production',
  isTest: env.NODE_ENV === 'test',
  
  // API Configuration
  api: {
    baseURL: env.API_BASE_URL,
    timeout: env.API_TIMEOUT,
    endpoints: {
      auth: {
        login: '/auth/login',
        logout: '/auth/logout',
        refresh: '/auth/refresh',
        forgotPassword: '/auth/forgot-password',
        resetPassword: '/auth/reset-password',
        changePassword: '/auth/change-password',
      },
      users: {
        profile: '/users/profile',
        list: '/users',
        create: '/users',
        update: '/users/:id',
        delete: '/users/:id',
      },
      admin: {
        dashboard: '/admin/dashboard',
        reports: '/admin/reports',
        settings: '/admin/settings',
      },
      area: {
        dashboard: '/area/dashboard',
        employees: '/area/employees',
        reports: '/area/reports',
      },
      employees: {
        dashboard: '/employees/dashboard',
        vacations: '/employees/vacations',
        profile: '/employees/profile',
      },
    },
  },
  
  // Debug Configuration
  debug: {
    enabled: env.DEBUG_MODE,
    logLevel: env.LOG_LEVEL,
    showApiLogs: env.DEBUG_MODE && env.NODE_ENV === 'development',
    showRouterLogs: env.DEBUG_MODE && env.NODE_ENV === 'development',
  },
  
  // UI Configuration
  ui: {
    theme: {
      colors: {
        primary: 'var(--color-continental-yellow)',
        secondary: 'var(--color-continental-gray-1)',
        background: 'var(--color-continental-bg)',
        surface: 'var(--color-continental-white)',
        error: 'var(--color-continental-red)',
        success: 'var(--color-continental-green)',
      },
    },
    animations: {
      duration: {
        fast: '150ms',
        normal: '300ms',
        slow: '500ms',
      },
    },
  },
  
  // Security Configuration
  security: {
    tokenKey: 'auth_token',
    userKey: 'user',
    sessionTimeout: 3600000, // 1 hour in milliseconds
    maxLoginAttempts: 5,
    lockoutDuration: 900000, // 15 minutes in milliseconds
  },
  
  // Routes Configuration
  routes: {
    public: {
      home: '/',
      login: '/login',
      loginEmployee: '/login-vacaciones',
    },
    protected: {
      admin: '/admin',
      area: '/area',
      employees: '/empleados',
    },
  },
  
  // Feature Flags
  features: {
    enableRefreshToken: true,
    enableRememberMe: false,
    enableMultiLanguage: false,
    enableDarkMode: false,
    enableNotifications: true,
  },
};

export default appConfig;