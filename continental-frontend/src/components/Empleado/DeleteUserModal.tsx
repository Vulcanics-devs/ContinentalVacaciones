import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { userService } from "@/services/userService";
import { toast } from "sonner";

interface DeleteUserModalProps {
    show: boolean;
    onClose: () => void;
    userId: number;
    userName: string;
    onDeleted: () => void;
}

export const DeleteUserModal = ({ show, onClose, userId, userName, onDeleted }: DeleteUserModalProps) => {
    const [password, setPassword] = useState("");
    const [loading, setLoading] = useState(false);

    if (!show) return null;

    const handleDelete = async () => {
        if (!password.trim()) {
            toast.error("Ingresa tu contraseña para confirmar");
            return;
        }
        setLoading(true);
        try {
            await userService.deleteUser(userId, password);
            toast.success("Usuario eliminado correctamente");
            onDeleted();
        } catch (error) {
            toast.error(error instanceof Error ? error.message : "Contraseña incorrecta o error al eliminar");
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
            <div className="bg-white rounded-lg p-6 w-full max-w-md space-y-4 shadow-xl">
                <h2 className="text-lg font-bold text-red-600">Eliminar usuario</h2>
                <p className="text-sm text-gray-600">
                    Estas a punto de eliminar a <span className="font-semibold">{userName}</span>. Esta accion no se puede deshacer.
                </p>
                <p className="text-sm text-gray-600">Ingresa tu contraseña de administrador para confirmar:</p>
                <Input
                    type="password"
                    placeholder="Tu contraseña"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                />
                <div className="flex justify-end gap-2 pt-2">
                    <Button variant="outline" onClick={() => { setPassword(""); onClose(); }} disabled={loading}>
                        Cancelar
                    </Button>
                    <Button variant="destructive" onClick={handleDelete} disabled={loading}>
                        {loading ? "Eliminando..." : "Confirmar eliminacion"}
                    </Button>
                </div>
            </div>
        </div>
    );
};