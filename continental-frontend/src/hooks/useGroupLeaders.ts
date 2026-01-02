import { useState, useEffect, useCallback } from 'react';
import { userService } from '../services/userService';
import type { User } from '../interfaces/User.interface';
import { UserRole } from '../interfaces/User.interface';

interface UseGroupLeadersState {
  leaders: User[];
  loading: boolean;
  error: string | null;
}

interface UseGroupLeadersReturn extends UseGroupLeadersState {
  fetchLeaders: () => Promise<void>;
  refetch: () => Promise<void>;
}

export const useGroupLeaders = (): UseGroupLeadersReturn => {
  const [state, setState] = useState<UseGroupLeadersState>({
    leaders: [],
    loading: false,
    error: null,
  });

  const fetchLeaders = useCallback(async () => {
    setState(prev => ({ ...prev, loading: true, error: null }));

    try {
      const leaders = await userService.getUsersByRole(UserRole.LEADER);
      
      setState(prev => ({
        ...prev,
        leaders,
        loading: false,
        error: null,
      }));
    } catch (error) {
      setState(prev => ({
        ...prev,
        loading: false,
        error: error instanceof Error ? error.message : 'Error fetching group leaders',
      }));
    }
  }, []);

  const refetch = useCallback(() => {
    return fetchLeaders();
  }, [fetchLeaders]);

  useEffect(() => {
    fetchLeaders();
  }, [fetchLeaders]);

  return {
    ...state,
    fetchLeaders,
    refetch,
  };
};
