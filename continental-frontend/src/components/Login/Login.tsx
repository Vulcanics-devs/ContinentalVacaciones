import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Input } from "../ui/input";
import { Label } from "../ui/label";
import { Eye, EyeOff } from "lucide-react";
import { Button } from "../ui/button";
import { logger } from "@/utils/logger";
import { showSuccess } from "@/utils/alerts";
import { authService } from "@/services/authService";
import Logo from "../../assets/Logo.webp";
import { UserRole } from "@/interfaces/User.interface";
import { FirstTimePasswordReset } from "./FirstTimePasswordReset";

interface Credentials {
  username: string;
  password: string;
}

export const Login = () => {
  const [isVisible, setIsVisible] = useState(false);
  const [credentials, setCredentials] = useState<Credentials>({ username: '', password: '' });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string>('');
  const [showFirstTimeReset, setShowFirstTimeReset] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement | HTMLButtonElement>) => {
    e.preventDefault();
    setError('');

    // Basic validation
    if (!credentials.username || !credentials.password) {
      setError('Por favor, completa todos los campos');
      return;
    }

    setIsLoading(true);

    try {
      const response = await authService.login({
        username: credentials.username,
        password: credentials.password,
      });
      console.log('🔍 Login response for admin:', {response});
      console.log('🔍 ultimoInicioSesion value:', response?.ultimoInicioSesion);
      console.log('🔍 ultimoInicioSesion type:', typeof response?.ultimoInicioSesion);

      // Check if this is the user's first login (ultimoInicioSesion is null or undefined)
      if (response && typeof response === 'object' && 'ultimoInicioSesion' in response && (response.ultimoInicioSesion === null || response.ultimoInicioSesion === undefined)) {
        logger.info('First time login detected, showing password reset', { username: credentials.username });
        setShowFirstTimeReset(true);
        return;
      } else {
        console.log('🔍 First time login NOT detected. ultimoInicioSesion:', response?.ultimoInicioSesion);
      }

      logger.info('Login successful, redirecting user', { user: response.user });

        // Check user roles and redirect accordingly
        const userRoles = response.user.roles || [];
        logger.info('User roles after login', { roles: userRoles });
        
        const hasRole = (role: string) => userRoles.some((r: string | { name: string }) => 
          typeof r === 'string' ? r === role : r.name === role
        );
      const redirectPath = hasRole(UserRole.SUPER_ADMIN) ? '/admin/areas' : '/area';
      navigate(redirectPath);
    } catch (err: unknown) {
      logger.error('Login failed', err);
      const errorMessage = err instanceof Error ? err.message : 'Error al iniciar sesión. Intenta nuevamente.';
      setError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  const toggleVisibility = () => setIsVisible((prev) => !prev);

  const handleFirstTimePasswordReset = async (password: string) => {
    try {
      // For first-time password reset, we need to use a temporary password
      // Since we don't have the current password, we'll use the change-password endpoint
      // with a special handling for first-time users
      const response = await authService.changePassword({
        CurrentPassword: credentials.password, // Use the login password as current
        NewPassword: password,
        ConfirmNewPassword: password
      });
      
      if (response.success) {
        logger.info('First time password reset successful', { username: credentials.username });
        
        // Check if user is still authenticated after password change
        if (authService.isAuthenticated()) {
          // User is still logged in, redirect to dashboard
          showSuccess('Contraseña establecida correctamente. Redirigiendo al dashboard...');
          
          // Get user roles and redirect accordingly
          const user = authService.getCurrentUser();
          const userRoles = user?.roles || [];
          
          const hasRole = (role: string) => userRoles.some((r: string | { name: string }) => 
            typeof r === 'string' ? r === role : r.name === role
          );
          
          const redirectPath = hasRole(UserRole.SUPER_ADMIN) ? '/admin/areas' : '/area';
          
          setTimeout(() => {
            navigate(redirectPath);
          }, 1500);
        } else {
          // User needs to login again
          showSuccess('Contraseña establecida correctamente. Por favor, inicia sesión con tu nueva contraseña.');
          setShowFirstTimeReset(false);
          setCredentials({ username: credentials.username, password: '' });
        }
      } else {
        throw new Error(response.errorMsg || 'Error al establecer la contraseña');
      }
    } catch (error) {
      logger.error('First time password reset failed', error);
      throw error;
    }
  };

  const handleBackToLogin = () => {
    setShowFirstTimeReset(false);
    setCredentials({ username: '', password: '' });
    setError('');
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-continental-bg">
      <div className="w-[500px] min-h-[520px] bg-continental-white rounded-lg shadow-lg flex flex-col items-center p-8">
        {/* Logo - 1/3 del contenedor */}
        <div className="h-1/3 flex items-center justify-center mb-6">
          <img
            src={Logo}
            alt="Continental Logo"
            className="max-h-full max-w-full object-contain"
          />
        </div>

        {/* Form Container */}
        {showFirstTimeReset ? (
          <FirstTimePasswordReset
            onBack={handleBackToLogin}
            onPasswordReset={handleFirstTimePasswordReset}
            userIdentifier={credentials.username}
            isEmployee={false}
          />
        ) : (
          <form onSubmit={handleSubmit} className="w-full flex-1 flex flex-col justify-between">
          <div className="space-y-6">
            {/* Email Input */}
            <div className="space-y-2">
              <Label htmlFor="email" className="text-sm font-medium text-continental-gray-1">
                Correo
              </Label>
              <Input
                id="username"
                placeholder="Ingresa tu correo"
                //TODO: Validar que sea un correo electronico
                type="text"
                value={credentials.username}
                onChange={(e) => setCredentials({ ...credentials, username: e.target.value })}
                className="w-full"
                required
              />
            </div>

            {/* Password Input */}
            <div className="space-y-2">
              <Label htmlFor="password" className="text-sm font-medium text-continental-gray-1">
                Contraseña
              </Label>
              <div className="relative">
                <Input
                  id="password"
                  placeholder="Ingresa tu contraseña"
                  type={isVisible ? "text" : "password"}
                  value={credentials.password}
                  onChange={(e) => setCredentials({ ...credentials, password: e.target.value })}
                  className="w-full pr-10"
                  required
                />
                <button
                  className="absolute inset-y-0 right-0 flex h-full w-10 items-center justify-center text-continental-gray-2 hover:text-continental-gray-1 transition-colors"
                  type="button"
                  onClick={toggleVisibility}
                  aria-label={isVisible ? "Ocultar contraseña" : "Mostrar contraseña"}
                >
                  {isVisible ? (
                    <EyeOff size={16} />
                  ) : (
                    <Eye size={16} />
                  )}
                </button>
              </div>
            </div>

            {/* Error Message */}
            {error && (
              <div className="text-continental-red text-sm text-center bg-red-50 p-3 rounded-lg border border-red-200">
                {error}
              </div>
            )}

            {/* Forgot Password Link */}
            <div className="text-right">
              <button
                type="button"
                className="text-sm text-continental-yellow hover:underline transition-all"
                onClick={async () => navigate('/restablecer-acceso')}
              >
                ¿Olvidaste tu contraseña?
              </button>
            </div>
          </div>

          {/* Submit Button */}
          <div className="text-right mt-6">
            <Button
              type="submit"
              variant="continental"
              className="w-[70px] h-[45px] font-medium rounded-lg"
              disabled={isLoading}
            >
              {isLoading ? 'Entrando...' : 'Entrar'}
            </Button>
          </div>
        </form>
        )}
      </div>
    </div>
  );
};
