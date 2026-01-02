import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { Input } from "../ui/input";
import { Label } from "../ui/label";
import { Button } from "../ui/button";
import { ArrowLeft, Mail } from "lucide-react";
import { passwordRecoveryService } from "@/services/passwordRecoveryService";
import { logger } from "@/utils/logger";
import { toast } from "sonner";
import Logo from "@/assets/Logo.webp";

interface ForgotPasswordForm {
  email: string;
}

export const ForgotPassword = () => {
  const [formData, setFormData] = useState<ForgotPasswordForm>({ email: '' });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string>('');
  const [success, setSuccess] = useState<string>('');
  const navigate = useNavigate();

  const validateEmail = (email: string): boolean => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  };

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    // Basic validation
    if (!formData.email) {
      setError('Por favor, ingresa tu correo electrónico');
      return;
    }

    if (!validateEmail(formData.email)) {
      setError('Por favor, ingresa un correo electrónico válido');
      return;
    }

    setIsLoading(true);

    try {
      const response = await passwordRecoveryService.solicitarCodigo({ 
        email: formData.email
      });

      logger.info('Forgot password request sent', { email: formData.email });
      
      if (response.success) {
        toast.success(response.message);
        setSuccess(`${response.message} El código expira en ${response.minutosExpiracion} minutos.`);
        
        // Redirigir a la pantalla de reset con el email
        navigate('/reset-password', { 
          state: { email: formData.email } 
        });
      } else {
        setError(response.message);
      }

    } catch (err: unknown) {
      logger.error('Forgot password failed', err);
      const errorMessage = err instanceof Error ? err.message : 'Error al enviar el código de verificación. Intenta nuevamente.';
      setError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-continental-bg">
      <div className="w-[500px] min-h-[520px] bg-continental-white rounded-lg shadow-lg flex flex-col items-center p-8">
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

        {/* Logo - 1/3 del contenedor */}
        <div className="h-1/3 flex items-center justify-center mb-6">
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
                Ingresa tu correo electrónico y te enviaremos un código de verificación
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
              {isLoading ? 'Enviando...' : 'Enviar Enlace'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
