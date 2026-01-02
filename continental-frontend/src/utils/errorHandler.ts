/**
 * Error Handler Utilities
 * Centralized error handling and user-friendly error messages
 */

import { logger } from './logger';
import type { ApiError } from '@/interfaces/Api.interface';

export interface ErrorInfo {
  message: string;
  code?: string;
  details?: any;
}

export class AppError extends Error {
  public code?: string;
  public details?: any;
  public isOperational: boolean;

  constructor(message: string, code?: string, details?: any, isOperational = true) {
    super(message);
    this.name = 'AppError';
    this.code = code;
    this.details = details;
    this.isOperational = isOperational;

    // Maintains proper stack trace for where our error was thrown
    if (Error.captureStackTrace) {
      Error.captureStackTrace(this, AppError);
    }
  }
}

export const errorMessages = {
  // Authentication errors
  INVALID_CREDENTIALS: 'Credenciales inválidas. Verifica tu correo/nómina y contraseña.',
  ACCOUNT_LOCKED: 'Tu cuenta ha sido bloqueada temporalmente. Intenta más tarde.',
  TOKEN_EXPIRED: 'Tu sesión ha expirado. Por favor, inicia sesión nuevamente.',
  UNAUTHORIZED: 'No tienes permisos para acceder a este recurso.',
  
  // Network errors
  NETWORK_ERROR: 'Error de conexión. Verifica tu conexión a internet.',
  SERVER_ERROR: 'Error del servidor. Intenta nuevamente más tarde.',
  TIMEOUT_ERROR: 'La solicitud tardó demasiado. Intenta nuevamente.',
  
  // Validation errors
  VALIDATION_ERROR: 'Los datos ingresados no son válidos.',
  REQUIRED_FIELD: 'Este campo es requerido.',
  INVALID_EMAIL: 'El formato del correo electrónico no es válido.',
  INVALID_PASSWORD: 'La contraseña debe tener al menos 6 caracteres.',
  
  // Generic errors
  UNKNOWN_ERROR: 'Ha ocurrido un error inesperado. Intenta nuevamente.',
  NOT_FOUND: 'El recurso solicitado no fue encontrado.',
  FORBIDDEN: 'No tienes permisos para realizar esta acción.',
};

export const getErrorMessage = (error: any): string => {
  // If it's already a user-friendly message, return it
  if (typeof error === 'string') {
    return error;
  }

  // Handle API errors
  if (error && typeof error === 'object') {
    // Check for API error structure
    if (error.message) {
      return error.message;
    }

    // Check for specific error codes
    if (error.code) {
      const message = errorMessages[error.code as keyof typeof errorMessages];
      if (message) {
        return message;
      }
    }

    // Check for HTTP status codes
    if (error.status) {
      switch (error.status) {
        case 400:
          return errorMessages.VALIDATION_ERROR;
        case 401:
          return errorMessages.INVALID_CREDENTIALS;
        case 403:
          return errorMessages.FORBIDDEN;
        case 404:
          return errorMessages.NOT_FOUND;
        case 408:
          return errorMessages.TIMEOUT_ERROR;
        case 500:
        case 502:
        case 503:
        case 504:
          return errorMessages.SERVER_ERROR;
        default:
          return errorMessages.UNKNOWN_ERROR;
      }
    }
  }

  // Handle network errors
  if (error instanceof TypeError && error.message.includes('fetch')) {
    return errorMessages.NETWORK_ERROR;
  }

  // Handle timeout errors
  if (error.name === 'AbortError' || error.message?.includes('timeout')) {
    return errorMessages.TIMEOUT_ERROR;
  }

  // Default fallback
  return errorMessages.UNKNOWN_ERROR;
};

export const handleError = (error: any, context?: string): ErrorInfo => {
  const message = getErrorMessage(error);
  const errorInfo: ErrorInfo = {
    message,
    code: error?.code,
    details: error?.details || error,
  };

  // Log the error
  logger.error(`Error in ${context || 'unknown context'}`, error);

  return errorInfo;
};

export const isOperationalError = (error: any): boolean => {
  if (error instanceof AppError) {
    return error.isOperational;
  }

  // Consider API errors as operational
  if (error && typeof error === 'object' && error.status) {
    return true;
  }

  return false;
};

export const createApiError = (response: any): ApiError => {
  return {
    message: response.message || 'API Error',
    status: response.status || 500,
    code: response.code,
    details: response,
  };
};

export default {
  AppError,
  errorMessages,
  getErrorMessage,
  handleError,
  isOperationalError,
  createApiError,
};