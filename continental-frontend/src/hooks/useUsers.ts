import { useState, useEffect, useCallback } from 'react';
import { userService } from '../services/userService';
import type { User, UsersListResponse } from '../interfaces/User.interface';

interface UseUsersState {
  users: User[];
  currentPage: number;
  pageSize: number;
  totalUsers: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  loading: boolean;
  error: string | null;
}

interface UseUsersReturn extends UseUsersState {
  fetchUsers: (page?: number, pageSize?: number) => Promise<void>;
  refetch: () => Promise<void>;
  setPage: (page: number) => void;
  setPageSize: (pageSize: number) => void;
}

export const useUsers = (initialPage: number = 1, initialPageSize: number = 10): UseUsersReturn => {
  const [state, setState] = useState<UseUsersState>({
    users: [],
    currentPage: initialPage,
    pageSize: initialPageSize,
    totalUsers: 0,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false,
    loading: false,
    error: null,
  });

  const fetchUsers = useCallback(async (page?: number, pageSize?: number) => {
    const targetPage = page ?? state.currentPage;
    const targetPageSize = pageSize ?? state.pageSize;

    setState(prev => ({ ...prev, loading: true, error: null }));

    try {
      const response: UsersListResponse = await userService.getUserList(targetPage, targetPageSize);
      
      setState(prev => ({
        ...prev,
        users: response.users,
        currentPage: response.currentPage,
        pageSize: response.pageSize,
        totalUsers: response.totalUsers,
        totalPages: response.totalPages,
        hasNextPage: response.hasNextPage,
        hasPreviousPage: response.hasPreviousPage,
        loading: false,
        error: null,
      }));
    } catch (error) {
      setState(prev => ({
        ...prev,
        loading: false,
        error: error instanceof Error ? error.message : 'Error fetching users',
      }));
    }
  }, [state.currentPage, state.pageSize]);

  const refetch = useCallback(() => {
    return fetchUsers();
  }, [fetchUsers]);

  const setPage = useCallback((page: number) => {
    setState(prev => ({ ...prev, currentPage: page }));
    fetchUsers(page);
  }, [fetchUsers]);

  const setPageSize = useCallback((pageSize: number) => {
    setState(prev => ({ ...prev, pageSize, currentPage: 1 }));
    fetchUsers(1, pageSize);
  }, [fetchUsers]);

  useEffect(() => {
    fetchUsers();
  }, []);

  return {
    ...state,
    fetchUsers,
    refetch,
    setPage,
    setPageSize,
  };
};
