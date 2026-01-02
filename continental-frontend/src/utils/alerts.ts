/**
 * Alert utilities for the application
 * Provides consistent alert/notification functionality
 */

export interface AlertOptions {
  title?: string;
  message: string;
  type?: 'success' | 'error' | 'warning' | 'info';
  duration?: number;
}

class AlertManager {
  private static instance: AlertManager;
  private alertContainer: HTMLElement | null = null;

  private constructor() {
    this.createAlertContainer();
  }

  public static getInstance(): AlertManager {
    if (!AlertManager.instance) {
      AlertManager.instance = new AlertManager();
    }
    return AlertManager.instance;
  }

  private createAlertContainer() {
    if (typeof window === 'undefined') return;
    
    this.alertContainer = document.createElement('div');
    this.alertContainer.id = 'alert-container';
    this.alertContainer.style.cssText = `
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 9999;
      max-width: 400px;
      pointer-events: none;
    `;
    document.body.appendChild(this.alertContainer);
  }

  private getAlertStyles(type: string): string {
    const baseStyles = `
      margin-bottom: 10px;
      padding: 16px;
      border-radius: 8px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      font-size: 14px;
      line-height: 1.4;
      pointer-events: auto;
      animation: slideIn 0.3s ease-out;
      transition: all 0.3s ease;
    `;

    const typeStyles = {
      success: 'background: #10b981; color: white;',
      error: 'background: #ef4444; color: white;',
      warning: 'background: #f59e0b; color: white;',
      info: 'background: #3b82f6; color: white;'
    };

    return baseStyles + (typeStyles[type as keyof typeof typeStyles] || typeStyles.info);
  }

  public showAlert(options: AlertOptions): void {
    if (!this.alertContainer) return;

    const { title, message, type = 'info', duration = 4000 } = options;

    const alertElement = document.createElement('div');
    alertElement.style.cssText = this.getAlertStyles(type);
    
    const titleHtml = title ? `<div style="font-weight: 600; margin-bottom: 4px;">${title}</div>` : '';
    alertElement.innerHTML = `${titleHtml}<div>${message}</div>`;

    // Add animation styles
    const style = document.createElement('style');
    style.textContent = `
      @keyframes slideIn {
        from {
          transform: translateX(100%);
          opacity: 0;
        }
        to {
          transform: translateX(0);
          opacity: 1;
        }
      }
      @keyframes slideOut {
        from {
          transform: translateX(0);
          opacity: 1;
        }
        to {
          transform: translateX(100%);
          opacity: 0;
        }
      }
    `;
    if (!document.head.querySelector('#alert-animations')) {
      style.id = 'alert-animations';
      document.head.appendChild(style);
    }

    this.alertContainer.appendChild(alertElement);

    // Auto remove after duration
    setTimeout(() => {
      alertElement.style.animation = 'slideOut 0.3s ease-in';
      setTimeout(() => {
        if (alertElement.parentNode) {
          alertElement.parentNode.removeChild(alertElement);
        }
      }, 3000);
    }, duration);

    // Click to dismiss
    alertElement.addEventListener('click', () => {
      alertElement.style.animation = 'slideOut 0.3s ease-in';
      setTimeout(() => {
        if (alertElement.parentNode) {
          alertElement.parentNode.removeChild(alertElement);
        }
      }, 3000);
    });
  }
}

// Convenience functions
export const showAlert = (options: AlertOptions) => {
  AlertManager.getInstance().showAlert(options);
};

export const showSuccess = (message: string, title?: string) => {
  showAlert({ message, title, type: 'success' });
};

export const showError = (message: string, title?: string) => {
  showAlert({ message, title, type: 'error' });
};

export const showWarning = (message: string, title?: string) => {
  showAlert({ message, title, type: 'info' });
};

export const showInfo = (message: string, title?: string) => {
  showAlert({ message, title, type: 'info' });
};
