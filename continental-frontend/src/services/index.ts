/**
 * Services Index
 * Centralized export for all services
 */

export { authService } from './authService';
export { httpClient } from './httpClient';
export { CalendarService } from './calendarService';
export { vacationConfigService } from './vacationConfigService';

// Re-export types for convenience
export type { AuthState } from './authService';
export type {
  ApiResponse,
  ApiError,
  LoginRequest,
  LoginResponse,
  RequestConfig,
} from '@/interfaces/Api.interface';