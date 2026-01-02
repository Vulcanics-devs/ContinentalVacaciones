/**
 * Logger Utility
 * Provides structured logging with different levels and debug mode support
 */

import { env } from '@/config/env';

export type LogLevel = 'debug' | 'info' | 'warn' | 'error';

interface LogEntry {
  level: LogLevel;
  message: string;
  data?: any;
  timestamp: string;
  context?: string;
}

class Logger {
  private isDebugMode: boolean;
  private logLevel: LogLevel;

  constructor() {
    this.isDebugMode = env.DEBUG_MODE;
    this.logLevel = env.LOG_LEVEL;
  }

  private shouldLog(level: LogLevel): boolean {
    if (!this.isDebugMode && env.NODE_ENV === 'production') {
      return level === 'error';
    }

    const levels: Record<LogLevel, number> = {
      debug: 0,
      info: 1,
      warn: 2,
      error: 3,
    };

    return levels[level] >= levels[this.logLevel];
  }

  private formatMessage(entry: LogEntry): string {
    const prefix = `[${entry.timestamp}] [${entry.level.toUpperCase()}]`;
    const context = entry.context ? ` [${entry.context}]` : '';
    return `${prefix}${context} ${entry.message}`;
  }

  private createLogEntry(level: LogLevel, message: string, data?: any, context?: string): LogEntry {
    return {
      level,
      message,
      data,
      context,
      timestamp: new Date().toISOString(),
    };
  }

  private log(entry: LogEntry): void {
    if (!this.shouldLog(entry.level)) return;

    const formattedMessage = this.formatMessage(entry);
    const consoleMethod = entry.level === 'debug' ? 'log' : entry.level;

    if (entry.data) {
      console[consoleMethod](formattedMessage, entry.data);
    } else {
      console[consoleMethod](formattedMessage);
    }
  }

  debug(message: string, data?: any, context?: string): void {
    this.log(this.createLogEntry('debug', message, data, context));
  }

  info(message: string, data?: any, context?: string): void {
    this.log(this.createLogEntry('info', message, data, context));
  }

  warn(message: string, data?: any, context?: string): void {
    this.log(this.createLogEntry('warn', message, data, context));
  }

  error(message: string, error?: any, context?: string): void {
    this.log(this.createLogEntry('error', message, error, context));
  }

  // Utility methods for common scenarios
  apiRequest(method: string, url: string, data?: any): void {
    this.debug(`API Request: ${method} ${url}`, data, 'API');
  }

  apiResponse(method: string, url: string, status: number, data?: any): void {
    this.debug(`API Response: ${method} ${url} - ${status}`, data, 'API');
  }

  apiError(method: string, url: string, error: any): void {
    this.error(`API Error: ${method} ${url}`, error, 'API');
  }

  authAction(action: string, data?: any): void {
    this.info(`Auth: ${action}`, data, 'AUTH');
  }

  routeChange(from: string, to: string): void {
    this.debug(`Route change: ${from} -> ${to}`, undefined, 'ROUTER');
  }
}

export const logger = new Logger();
export default logger;