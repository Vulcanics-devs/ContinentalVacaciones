import { useState } from "react";
import { ChevronDown, LogOut, User, Bell, UserPlus } from "lucide-react";
import useAuth from "@/hooks/useAuth";
import { useNotifications } from "@/hooks/useNotifications";
import { useNavigate, useLocation } from "react-router-dom";
import { UserRole } from "@/interfaces/User.interface";

// Función para formatear el rol del usuario
const formatRole = (role: string): string => {
  const roleMap: Record<string, string> = {
    'admin': 'Administrador',
    'super_admin': 'Super Administrador',
    'area_admin': 'Administrador de Área',
    'leader': 'Líder',
    'industrial': 'Industrial',
    'union_representative': 'Representante Sindical',
    'unionized': 'Sindicalizado'
  };
  return roleMap[role] || role;
};

export const NavbarUser = () => {
  const { logout, user, hasAnyRole } = useAuth();
  const { unreadCount } = useNotifications();
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  // Función para navegar a notificaciones
  const handleNotificationsClick = () => {
    navigate('/notificaciones');
    setIsDropdownOpen(false);
  };

  // Función para navegar a suplente
  const handleSuplenteClick = () => {
    navigate('/suplente');
    setIsDropdownOpen(false);
  };

  // Verificar si estamos en la vista de notificaciones
  const isNotificationsActive = location.pathname === '/notificaciones';

  // Verificar si estamos en la vista de suplente
  const isSuplenteActive = location.pathname === '/suplente';

  const handleLogout = async () => {
    try {
      await logout();
      setIsDropdownOpen(false);
      //detectar si el usuario es sindicalizado
      if ((user as any)?.roles.includes(UserRole.UNIONIZED) || (user as any)?.roles.includes(UserRole.UNION_REPRESENTATIVE)) {
        navigate('/login-vacaciones', { replace: true });
      } else {
        navigate('/login', { replace: true });
      }
    } catch (error) {
      console.error('Error al cerrar sesión:', error);
    }
  };

  if (!user) {
    return null;
  }

  return (
    <div className="relative">
      <button
        onClick={() => setIsDropdownOpen(!isDropdownOpen)}
        className="flex items-center gap-2 px-4 py-2 rounded-lg hover:bg-continental-gray-4 transition-colors cursor-pointer"
      >
        <div className="flex items-center gap-2">
          {hasAnyRole([UserRole.SUPER_ADMIN, UserRole.ADMIN, UserRole.INDUSTRIAL, UserRole.AREA_ADMIN, UserRole.LEADER]) && (
            <>
              {/* Indicador de notificaciones no leídas */}
              {unreadCount > 0 && (
                <div className="relative">
                  <Bell className="w-5 h-5 text-continental-yellow" />
                  <div className="absolute -top-1 -right-1 w-2 h-2 bg-red-500 text-white text-xs rounded-full flex items-center justify-center">
                    
                  </div>
                </div>
              )}
            </>
          )}

          <User className="w-5 h-5 text-continental-gray-1" />
          <div className="text-left">
            <div className="text-sm font-medium text-continental-black">
              {user.username}
            </div>
            <div className="text-xs text-continental-gray-1">
              {formatRole((user as any).roles[0])}
            </div>
          </div>
        </div>
        <ChevronDown className={`w-4 h-4 text-continental-gray-1 transition-transform ${isDropdownOpen ? 'rotate-180' : ''
          }`} />
      </button>

      {/* Dropdown Menu */}
      {isDropdownOpen && (
        <>
          {/* Overlay para cerrar el dropdown */}
          <div
            className="fixed inset-0 z-10"
            onClick={() => setIsDropdownOpen(false)}
          />

          {/* Dropdown Content */}
          <div className="absolute right-0 mt-2 w-56 bg-white rounded-lg shadow-lg border border-continental-gray-3 z-20">
            <div className="p-3 border-b border-continental-gray-3 mb-2">
              <div className="text-sm font-medium text-continental-black">
                {user.username}
              </div>
              <div className="text-xs text-continental-gray-1">
                {formatRole((user as any).roles[0])}
              </div>
            </div>

            {hasAnyRole([UserRole.SUPER_ADMIN, UserRole.ADMIN, UserRole.INDUSTRIAL, UserRole.AREA_ADMIN, UserRole.LEADER]) && (
              <div className="p-1">
                <button
                  onClick={handleNotificationsClick}
                  className={`w-full flex items-center justify-center gap-2 px-3 py-2 text-sm rounded-md border-1 cursor-pointer transition-colors ${isNotificationsActive
                      ? 'bg-continental-yellow'
                      : 'text-continental-black hover:bg-continental-gray-4'
                    }`}
                >
                  <div className="relative">
                    <Bell className="w-4 h-4" />
                    {unreadCount > 0 && (
                      <div className="absolute -top-1 -right-1 w-3 h-3 bg-red-500 text-white text-xs rounded-full flex items-center justify-center text-[10px]">
                        {unreadCount > 9 ? '9+' : unreadCount}
                      </div>
                    )}
                  </div>
                  Notificaciones
                </button>
              </div>
            )}

            {/* Mostrar Suplente solo para SUPER_ADMIN, INDUSTRIAL y AREA_ADMIN */}
            {hasAnyRole([UserRole.SUPER_ADMIN, UserRole.ADMIN, UserRole.INDUSTRIAL, UserRole.AREA_ADMIN]) && (
              <div className="p-1">
                <button
                  onClick={handleSuplenteClick}
                  className={`w-full flex items-center justify-center gap-2 px-3 py-2 text-sm rounded-md border-1 cursor-pointer transition-colors ${isSuplenteActive
                      ? 'bg-continental-yellow'
                      : 'text-continental-black hover:bg-continental-gray-4'
                    }`}
                >
                  <UserPlus className="w-4 h-4" />
                  Suplente
                </button>
              </div>
            )}

            <div className="p-1">
              <button
                onClick={handleLogout}
                className="w-full flex items-center justify-center gap-2 px-3 py-2 text-sm text-continental-red hover:bg-continental-gray-4 rounded-md transition-colors cursor-pointer"
              >
                <LogOut className="w-4 h-4" />
                Cerrar Sesión
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  );
};