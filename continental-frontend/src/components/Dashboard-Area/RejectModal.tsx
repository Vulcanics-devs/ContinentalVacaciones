/**
 * =============================================================================
 * REJECT MODAL
 * =============================================================================
 * 
 * @description
 * Modal de confirmación para rechazar solicitudes de vacaciones o reprogramaciones.
 * Requiere que el jefe de área proporcione un motivo obligatorio para el rechazo
 * antes de confirmar la acción.
 * 
 * @inputs (Props del componente)
 * - show: boolean - Controla la visibilidad del modal
 * - onClose: () => void - Callback para cerrar el modal
 * - onConfirm: (motivo: string) => void - Callback cuando se confirma el rechazo con motivo
 * - solicitudId: string - ID único de la solicitud a rechazar
 * - nombreEmpleado: string - Nombre del empleado solicitante
 * 
 * @used_in (Componentes padre)
 * - src/components/Dashboard-Area/SolicitudDetalle.tsx
 * - src/components/Dashboard-Area/SolicitudesComponent.tsx
 * 
 * @user_roles (Usuarios que acceden)
 * - Jefe de Área
 * 
 * @dependencies
 * - React: Framework base y hooks (useState)
 * - @/components/ui/button: Componente de botón reutilizable
 * - lucide-react: Iconos (XCircle)
 * 
 * @author Vulcanics Dev Team
 * @created 2024
 * @last_modified 2025-08-20
 * =============================================================================
 */

import React, { useState } from 'react';
import { Button } from '@/components/ui/button';
import { XCircle } from 'lucide-react';

interface RejectModalProps {
    show: boolean;
    onClose: () => void;
    onConfirm: (motivo: string) => void;
    solicitudId: string;
    nombreEmpleado: string;
}

const RejectModal: React.FC<RejectModalProps> = ({
    show,
    onClose,
    onConfirm,
    solicitudId,
    nombreEmpleado
}) => {
    const [motivo, setMotivo] = useState('');

    const onSubmit = () => {
        if (!motivo.trim()) {
            alert("Debes proporcionar un motivo para rechazar la solicitud");
            return;
        }

        onConfirm(motivo.trim());
        console.log("Solicitud rechazada exitosamente");
        setMotivo(''); // Limpiar el formulario
        onClose();
    };

    const onCancel = () => {
        setMotivo(''); // Limpiar el formulario al cancelar
        onClose();
    };

    return (
        <div
            className={`fixed inset-0 z-50 flex items-center justify-center bg-black/50 ${show ? "block" : "hidden"
                }`}
        >
            <div className="fixed inset-0 -z-10" onClick={onClose} />
            <div className="relative z-50 w-full max-w-lg p-4">
                <div className="bg-white rounded-lg shadow-lg">
                    <div className="p-4">
                        <div className="flex items-center gap-2 mb-3">
                            <XCircle className="w-5 h-5 text-rose-500" />
                            <h2 className="text-lg font-semibold text-gray-900">¿Rechazar solicitud de reprogramación?</h2>
                        </div>

                        <p className="text-sm text-gray-600 mb-4">
                            Estás a punto de rechazar la solicitud de reprogramación.
                        </p>

                        <div className="mb-4 p-3 bg-gray-50 rounded-lg">
                            <div className="text-sm text-gray-600">
                                <span className="font-medium">Solicitud:</span> #{solicitudId}
                            </div>
                            <div className="text-sm text-gray-600">
                                <span className="font-medium">Empleado:</span> {nombreEmpleado}
                            </div>
                        </div>

                        <div className="mb-4 flex flex-col gap-2">
                            <label className="block text-sm font-medium text-gray-700" htmlFor="motivo">
                                Por favor, indica el motivo de tu decisión para que el solicitante pueda recibir retroalimentación
                            </label>
                            <textarea
                                id="motivo"
                                value={motivo}
                                onChange={(e) => setMotivo(e.target.value)}
                                placeholder="Describe el motivo por el cual se rechaza esta solicitud..."
                                className="w-full px-3 py-2 border border-gray-300 rounded-md resize-none focus:outline-none focus:ring-2 focus:ring-rose-500 focus:border-transparent"
                                rows={4}
                                maxLength={500}
                            />
                            <div className="text-xs text-gray-500 text-right">
                                {motivo.length}/500 caracteres
                            </div>
                        </div>
                    </div>

                    <div className="p-4 flex justify-end gap-2 border-t border-gray-200">
                        <Button
                            className="cursor-pointer"
                            variant="outline"
                            size="lg"
                            onClick={onCancel}
                        >
                            Cancelar
                        </Button>
                        <Button
                            className="cursor-pointer text-black"
                            style={{ backgroundColor: 'var(--color-continental-yellow)' }}
                            size="lg"
                            onClick={onSubmit}
                        >
                            Rechazar
                        </Button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default RejectModal;
