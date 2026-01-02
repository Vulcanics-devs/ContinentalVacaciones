import { useState, useEffect, useCallback } from 'react';
import { userService } from '@/services/userService';
import { UserRole, type User } from '@/interfaces/User.interface';
import { logger } from '@/utils/logger';

export const useAreaIngenieros = (areaId: number | null) => {
  const [assignedIngenieros, setAssignedIngenieros] = useState<User[]>([]);
  const [availableIngenieros, setAvailableIngenieros] = useState<User[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchAssignedIngenieros = useCallback(async () => {
    if (!areaId) return;
    
    try {
      setLoading(true);
      const ingenieros = await userService.getIngenierosByAreaId(areaId);
      setAssignedIngenieros(ingenieros);
    } catch (err) {
      logger.error('Error fetching assigned ingenieros', err, 'USE_AREA_INGENIEROS');
      setError('Error al cargar ingenieros asignados');
    } finally {
      setLoading(false);
    }
  }, [areaId]);

  const fetchAvailableIngenieros = useCallback(async () => {
    try {
      setLoading(true);
      const allIngenieros = await userService.getUsersByRole(UserRole.INDUSTRIAL);
      setAvailableIngenieros(allIngenieros);
    } catch (err) {
      logger.error('Error fetching available ingenieros', err, 'USE_AREA_INGENIEROS');
      setError('Error al cargar ingenieros disponibles');
    } finally {
      setLoading(false);
    }
  }, []);

  const assignIngeniero = useCallback(async (ingenieroId: number) => {
    if (!areaId) return;

    try {
      await userService.assignIngenieroToArea(areaId, ingenieroId);
      await fetchAssignedIngenieros();
      logger.info('Ingeniero assigned successfully', { areaId, ingenieroId });
    } catch (err) {
      logger.error('Error assigning ingeniero', err, 'USE_AREA_INGENIEROS');
      setError('Error al asignar ingeniero');
      throw err;
    }
  }, [areaId, fetchAssignedIngenieros]);

  const unassignIngeniero = useCallback(async (ingenieroId: number) => {
    if (!areaId) return;

    try {
      await userService.unassignIngenieroFromArea(areaId, ingenieroId);
      await fetchAssignedIngenieros();
      logger.info('Ingeniero unassigned successfully', { areaId, ingenieroId });
    } catch (err) {
      logger.error('Error unassigning ingeniero', err, 'USE_AREA_INGENIEROS');
      setError('Error al desasignar ingeniero');
      throw err;
    }
  }, [areaId, fetchAssignedIngenieros]);

  useEffect(() => {
    fetchAvailableIngenieros();
  }, [fetchAvailableIngenieros]);

  useEffect(() => {
    console.log(`🔄 useAreaIngenieros effect triggered for areaId: ${areaId}`);
    if (areaId) {
      console.log(`📞 Calling fetchAssignedIngenieros for area ${areaId}`);
      fetchAssignedIngenieros();
    } else {
      console.log('❌ No areaId provided, skipping fetchAssignedIngenieros');
      setAssignedIngenieros([]);
    }
  }, [areaId, fetchAssignedIngenieros]);

  return {
    assignedIngenieros,
    availableIngenieros,
    loading,
    error,
    assignIngeniero,
    unassignIngeniero,
    refetch: fetchAssignedIngenieros,
  };
};
