import { Navigate } from 'react-router-dom'
import type { ReactNode } from 'react'
import type { UserRole } from '@/interfaces/User.interface'
import { UserRole as UserRoleEnum } from '@/interfaces/User.interface'
import { authService } from '@/services/authService'
import { logger } from '@/utils/logger'
import { toast } from 'sonner'

interface ProtectedRouteProps {
  children: ReactNode
  requiredRole: UserRole[]
}

const ProtectedRoute = ({ children, requiredRole }: ProtectedRouteProps) => {
  const isAuthenticated = authService.isAuthenticated()
  const user = authService.getCurrentUser()
  const logout = authService.logout

  console.log({user, isAuthenticated})

  // Log access attempt
  logger.debug('Protected route access attempt', {
    isAuthenticated,
    userRoles: user?.roles,
    requiredRoles: requiredRole,
    path: window.location.pathname
  });

  if (!isAuthenticated) {
    logger.warn('Unauthenticated access attempt, redirecting to login');
    
    // Redirect to appropriate login based on required role
    if (requiredRole.includes(UserRoleEnum.UNION_REPRESENTATIVE) || requiredRole.includes(UserRoleEnum.UNIONIZED)) {
      return <Navigate to="/login-vacaciones" replace />
    }
    // Admin and Area users use the same login
    return <Navigate to="/login" replace />
  }

  if (requiredRole && !authService.hasAnyRole(requiredRole)) {
    logger.warn('Insufficient permissions', {
      userRoles: user?.roles,
      requiredRoles: requiredRole
    });
    toast.error('No tienes permisos para acceder a esta ruta');
    logout();
    // Redirect to appropriate login based on required role
    if (requiredRole.includes(UserRoleEnum.UNION_REPRESENTATIVE) || requiredRole.includes(UserRoleEnum.UNIONIZED)) {
      return <Navigate to="/login-vacaciones" replace />
    }
    // Admin and Area users use the same login
    return <Navigate to="/login" replace />
  }

  logger.debug('Protected route access granted');
  return children
}

export default ProtectedRoute
