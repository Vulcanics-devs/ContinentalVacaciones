import { useState, useEffect } from 'react';

interface NotificationData {
  unreadCount: number;
  notifications: Array<{
    id: number;
    title: string;
    message: string;
    isRead: boolean;
    timestamp: string;
  }>;
}

export const useNotifications = () => {
  const [notificationData, setNotificationData] = useState<NotificationData>({
    unreadCount: 0,
    notifications: []
  });

  useEffect(() => {
    // Simular carga de notificaciones desde una API
    // En una aplicación real, esto sería una llamada al backend
    const mockNotifications = [
      {
        id: 1,
        title: "Nueva solicitud de vacaciones",
        message: "Juan Pérez ha solicitado vacaciones del 15-20 de septiembre",
        isRead: false,
        timestamp: "2025-08-22 10:30"
      },
      {
        id: 2,
        title: "Cambio de porcentaje",
        message: "Nancy López modificó porcentajes en Octubre",
        isRead: false,
        timestamp: "2025-08-22 09:15"
      },
      {
        id: 3,
        title: "Solicitud aprobada",
        message: "La solicitud de Francisco Hernández ha sido aprobada",
        isRead: true,
        timestamp: "2025-08-21 16:45"
      },
      {
        id: 4,
        title: "Nuevo empleado agregado",
        message: "Miguel Martinez ha sido agregado al sistema",
        isRead: false,
        timestamp: "2025-08-21 14:20"
      }
    ];

    const unreadCount = mockNotifications.filter(n => !n.isRead).length;
    
    setNotificationData({
      unreadCount,
      notifications: mockNotifications
    });
  }, []);

  const markAsRead = (notificationId: number) => {
    setNotificationData(prev => ({
      ...prev,
      notifications: prev.notifications.map(n => 
        n.id === notificationId ? { ...n, isRead: true } : n
      ),
      unreadCount: prev.unreadCount - (prev.notifications.find(n => n.id === notificationId && !n.isRead) ? 1 : 0)
    }));
  };

  const markAllAsRead = () => {
    setNotificationData(prev => ({
      ...prev,
      notifications: prev.notifications.map(n => ({ ...n, isRead: true })),
      unreadCount: 0
    }));
  };

  return {
    unreadCount: notificationData.unreadCount,
    notifications: notificationData.notifications,
    markAsRead,
    markAllAsRead
  };
};
