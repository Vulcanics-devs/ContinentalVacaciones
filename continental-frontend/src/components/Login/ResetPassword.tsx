import { useState, useEffect } from "react";
import { useNavigate, useLocation, Link } from "react-router-dom";
import { Input } from "../ui/input";
import { Label } from "../ui/label";
import { Button } from "../ui/button";
import { Eye, EyeOff, ArrowLeft, Lock, Mail, Shield } from "lucide-react";
import { passwordRecoveryService } from "@/services/passwordRecoveryService";
import { logger } from "@/utils/logger";
import { toast } from "sonner";
import Logo from "@/assets/Logo.webp";

interface ResetPasswordForm {
  email: string;
  codigoVerificacion: string;
  password: string;
  confirmPassword: string;
}

export const ResetPassword = () => {
  const [formData, setFormData] = useState<ResetPasswordForm>({ 
    email: '',
    codigoVerificacion: '',
    password: '', 
    confirmPassword: '' 
  });
  const [isVisible, setIsVisible] = useState(false);
  const [isConfirmVisible, setIsConfirmVisible] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string>('');
  const [success, setSuccess] = useState<string>('');
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    // Obtener el email del estado de navegación si viene de ForgotPassword
    const emailFromState = location.state?.email;
    if (emailFromState) {
      setFormData(prev => ({ ...prev, email: emailFromState }));
    }
  }, [location.state]);

  const validatePassword = (password: string): string | null => {
    if (password.length < 8) {
      return 'La contraseña debe tener al menos 8 caracteres';
    }

    return null;
  };

  const validateEmail = (email: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };


  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    // Basic validation
    if (!formData.email || !formData.codigoVerificacion || !formData.password || !formData.confirmPassword) {
      setError('Por favor, completa todos los campos');
      return;
    }

    // Email validation
    if (!validateEmail(formData.email)) {
      setError('Por favor, ingresa un correo electrónico válido');
      return;
    }

    // Code validation
    if (formData.codigoVerificacion.length !== 5) {
      setError('El código de verificación debe tener 5 dígitos');
      return;
    }

    // Password validation
    const passwordError = validatePassword(formData.password);
    if (passwordError) {
      setError(passwordError);
      return;
    }

    // Confirm password validation
    if (formData.password !== formData.confirmPassword) {
      setError('Las contraseñas no coinciden');
      return;
    }

    setIsLoading(true);

    try {
      const response = await passwordRecoveryService.cambiarPassword({
        email: formData.email,
        codigoVerificacion: formData.codigoVerificacion,
        nuevaPassword: formData.password,
        confirmarPassword: formData.confirmPassword
      });

      if (response.success) {
        logger.info('Password reset successful');
        toast.success(response.message);
        setSuccess('Tu contraseña ha sido restablecida exitosamente. Serás redirigido al login.');
        
        // Limpiar el formulario
        setFormData({ 
          email: '',
          codigoVerificacion: '',
          password: '', 
          confirmPassword: '' 
        });
        
        // Redirigir después de 3 segundos
        setTimeout(() => {
          navigate('/login');
        }, 3000);
      } else {
        setError(response.message);
      }

    } catch (err: unknown) {
      logger.error('Password reset failed', err);
      const errorMessage = err instanceof Error ? err.message : 'Error al restablecer la contraseña. Intenta nuevamente.';
      setError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  const togglePasswordVisibility = () => setIsVisible((prev) => !prev);
  const toggleConfirmVisibility = () => setIsConfirmVisible((prev) => !prev);

  return (
    <div className="min-h-screen flex items-center justify-center bg-continental-bg">
      <div className="w-[500px] min-h-[600px] bg-continental-white rounded-lg shadow-lg flex flex-col items-center p-8">
        {/* Back Button */}
        <div className="w-full flex justify-start mb-4">
          <Link 
            to="/login"
            className="flex items-center text-continental-gray-2 hover:text-continental-gray-1 transition-colors"
          >
            <ArrowLeft size={20} className="mr-2" />
            <span className="text-sm">Volver al login</span>
          </Link>
        </div>

        {/* Logo */}
        <div className="h-1/4 flex items-center justify-center mb-6">
          <img
            src={Logo}
            alt="Continental Logo"
            className="max-h-full max-w-full object-contain"
          />
        </div>

        {/* Form Container */}
        <form onSubmit={handleSubmit} className="w-full flex-1 flex flex-col justify-between">
          <div className="space-y-6">
            {/* Title */}
            <div className="text-center space-y-2">
              <h2 className="text-xl font-semibold text-continental-gray-1">
                Restablecer Contraseña
              </h2>
              <p className="text-sm text-continental-gray-2">
                Ingresa el código que recibiste por correo y tu nueva contraseña
              </p>
            </div>

            {/* Email Input */}
            <div className="space-y-2">
              <Label htmlFor="email" className="text-sm font-medium text-continental-gray-1">
                Correo Electrónico
              </Label>
              <div className="relative">
                <Input
                  id="email"
                  placeholder="ejemplo@continental.com"
                  type="email"
                  value={formData.email}
                  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                  className="w-full pl-10"
                  required
                />
                <Mail size={16} className="absolute left-3 top-1/2 transform -translate-y-1/2 text-continental-gray-2" />
              </div>
            </div>

            {/* Verification Code Input */}
            <div className="space-y-2">
              <Label htmlFor="codigoVerificacion" className="text-sm font-medium text-continental-gray-1">
                Código de Verificación
              </Label>
              <div className="relative">
                <Input
                  id="codigoVerificacion"
                  placeholder="12345"
                  type="text"
                  maxLength={5}
                  value={formData.codigoVerificacion}
                  onChange={(e) => {
                    const value = e.target.value.replace(/\D/g, ''); // Solo números
                    setFormData({ ...formData, codigoVerificacion: value });
                  }}
                  className="w-full pl-10 text-center text-lg tracking-widest"
                  required
                />
                <Shield size={16} className="absolute left-3 top-1/2 transform -translate-y-1/2 text-continental-gray-2" />
              </div>
              <div className="flex justify-between items-center">
                <p className="text-xs text-continental-gray-2">
                  Ingresa el código de 5 dígitos que recibiste por correo
                </p>
                <button
                  type="button"
                  onClick={async () => {
                    if (formData.email) {
                      const response = await passwordRecoveryService.solicitarCodigo({ email: formData.email });
                      if (response.success) {
                        toast.success('Código reenviado exitosamente');
                      }
                    } else {
                      toast.error('Ingresa tu email primero');
                    }
                  }}
                  className="text-xs text-blue-600 hover:text-blue-800 underline"
                >
                  Reenviar código
                </button>
              </div>
            </div>

            {/* Password Input */}
            <div className="space-y-2">
              <Label htmlFor="password" className="text-sm font-medium text-continental-gray-1">
                Nueva Contraseña
              </Label>
              <div className="relative">
                <Input
                  id="password"
                  placeholder="Ingresa tu nueva contraseña"
                  type={isVisible ? "text" : "password"}
                  value={formData.password}
                  onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                  className="w-full pl-10 pr-10"
                  required
                />
                <Lock size={16} className="absolute left-3 top-1/2 transform -translate-y-1/2 text-continental-gray-2" />
                <button
                  className="absolute inset-y-0 right-0 flex h-full w-10 items-center justify-center text-continental-gray-2 hover:text-continental-gray-1 transition-colors"
                  type="button"
                  onClick={togglePasswordVisibility}
                  aria-label={isVisible ? "Ocultar contraseña" : "Mostrar contraseña"}
                >
                  {isVisible ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </div>

            {/* Confirm Password Input */}
            <div className="space-y-2">
              <Label htmlFor="confirmPassword" className="text-sm font-medium text-continental-gray-1">
                Confirmar Contraseña
              </Label>
              <div className="relative">
                <Input
                  id="confirmPassword"
                  placeholder="Confirma tu nueva contraseña"
                  type={isConfirmVisible ? "text" : "password"}
                  value={formData.confirmPassword}
                  onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
                  className="w-full pl-10 pr-10"
                  required
                />
                <Lock size={16} className="absolute left-3 top-1/2 transform -translate-y-1/2 text-continental-gray-2" />
                <button
                  className="absolute inset-y-0 right-0 flex h-full w-10 items-center justify-center text-continental-gray-2 hover:text-continental-gray-1 transition-colors"
                  type="button"
                  onClick={toggleConfirmVisibility}
                  aria-label={isConfirmVisible ? "Ocultar contraseña" : "Mostrar contraseña"}
                >
                  {isConfirmVisible ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </div>

            {/* Password Requirements */}
            <div className="text-xs text-continental-gray-2 bg-gray-50 p-3 rounded-lg">
              <p className="font-medium mb-1">La contraseña debe contener:</p>
              <ul className="list-disc list-inside space-y-1">
                <li>Al menos 8 caracteres</li>
              </ul>
            </div>

            {/* Error Message */}
            {error && (
              <div className="text-continental-red text-sm text-center bg-red-50 p-3 rounded-lg border border-red-200">
                {error}
              </div>
            )}

            {/* Success Message */}
            {success && (
              <div className="text-green-700 text-sm text-center bg-green-50 p-3 rounded-lg border border-green-200">
                {success}
              </div>
            )}
          </div>

          {/* Submit Button */}
          <div className="text-center mt-6">
            <Button
              type="submit"
              variant="continental"
              className="w-full h-[45px] font-medium rounded-lg"
              disabled={isLoading}
            >
              {isLoading ? 'Restableciendo...' : 'Restablecer Contraseña'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
