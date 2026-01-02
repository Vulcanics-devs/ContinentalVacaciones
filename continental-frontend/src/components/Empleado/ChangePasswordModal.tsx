import { useState } from "react";
import { Button } from "../ui/button";
import { Input } from "../ui/input";
import { Label } from "../ui/label";
import { X, Eye, EyeOff } from "lucide-react";
import { toast } from "sonner";
import { authService } from "@/services/authService";

interface ChangePasswordModalProps {
  show: boolean;
  onClose: () => void;
  userId: number;
  userName: string;
  onPasswordChanged: () => void;
}

interface ChangePasswordRequest {
  UserId: number;
  NewPassword: string;
  ConfirmNewPassword: string;
}


export const ChangePasswordModal = ({
  show,
  onClose,
  userId,
  userName,
  onPasswordChanged
}: ChangePasswordModalProps) => {
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [showNewPassword, setShowNewPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Validaciones
    if (!newPassword.trim()) {
      toast.error("La nueva contraseña es requerida");
      return;
    }
    
    if (newPassword.length < 6) {
      toast.error("La contraseña debe tener al menos 6 caracteres");
      return;
    }
    
    if (newPassword !== confirmPassword) {
      toast.error("Las contraseñas no coinciden");
      return;
    }

    setIsLoading(true);

    try {
      const requestBody: ChangePasswordRequest = {
        UserId: userId,
        NewPassword: newPassword,
        ConfirmNewPassword: confirmPassword
      };

      const response = await authService.changeUserPassword(requestBody);

      if (response.success) {
        toast.success(response.data || "Contraseña actualizada correctamente");
        onPasswordChanged();
        handleClose();
      } else {
        toast.error(response.errorMsg || "Error al cambiar la contraseña");
      }
    } catch (error) {
      console.error('Error changing password:', error);
      toast.error("Error al cambiar la contraseña");
    } finally {
      setIsLoading(false);
    }
  };

  const handleClose = () => {
    setNewPassword("");
    setConfirmPassword("");
    setShowNewPassword(false);
    setShowConfirmPassword(false);
    onClose();
  };

  if (!show) return null;

  return (
    <div className="fixed inset-0 bg-opacity-50 flex items-center justify-center z-50">
      <div onClick={handleClose} className="absolute inset-0 bg-black/50 backdrop-blur-sm z-40 flex items-center justify-center">
      </div>
      <div className="bg-white rounded-lg p-6 w-full max-w-md mx-4 z-50">
        {/* Header */}
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-xl font-semibold text-continental-black">
            Cambiar Contraseña
          </h2>
          <button
            onClick={handleClose}
            className="text-continental-gray-1 hover:text-continental-black transition-colors"
          >
            <X size={24} />
          </button>
        </div>

        {/* Usuario info */}
        <div className="mb-6 p-3 bg-continental-gray-4 rounded-lg">
          <p className="text-sm text-continental-gray-1">Usuario:</p>
          <p className="font-medium text-continental-black">{userName}</p>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Nueva contraseña */}
          <div className="space-y-2">
            <Label htmlFor="newPassword" className="text-sm font-medium text-continental-gray-1">
              Nueva contraseña
            </Label>
            <div className="relative">
              <Input
                id="newPassword"
                type={showNewPassword ? "text" : "password"}
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                placeholder="Ingresa la nueva contraseña"
                className="pr-10"
                required
              />
              <button
                type="button"
                onClick={() => setShowNewPassword(!showNewPassword)}
                className="absolute right-3 top-1/2 transform -translate-y-1/2 text-continental-gray-1 hover:text-continental-black"
              >
                {showNewPassword ? <EyeOff size={16} /> : <Eye size={16} />}
              </button>
            </div>
          </div>

          {/* Confirmar contraseña */}
          <div className="space-y-2">
            <Label htmlFor="confirmPassword" className="text-sm font-medium text-continental-gray-1">
              Confirmar contraseña
            </Label>
            <div className="relative">
              <Input
                id="confirmPassword"
                type={showConfirmPassword ? "text" : "password"}
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                placeholder="Confirma la nueva contraseña"
                className="pr-10"
                required
              />
              <button
                type="button"
                onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                className="absolute right-3 top-1/2 transform -translate-y-1/2 text-continental-gray-1 hover:text-continental-black"
              >
                {showConfirmPassword ? <EyeOff size={16} /> : <Eye size={16} />}
              </button>
            </div>
          </div>

          {/* Buttons */}
          <div className="flex gap-3 pt-4">
            <Button
              type="button"
              variant="outline"
              onClick={handleClose}
              className="flex-1"
              disabled={isLoading}
            >
              Cancelar
            </Button>
            <Button
              type="submit"
              variant="continental"
              className="flex-1"
              disabled={isLoading}
            >
              {isLoading ? "Cambiando..." : "Cambiar Contraseña"}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
