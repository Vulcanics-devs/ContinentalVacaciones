/**
 * =============================================================================
 * SOLICITUD DETALLE
 * =============================================================================
 * 
 * @description
 * Componente para mostrar el detalle completo de una solicitud de vacaciones
 * o reprogramación. Incluye toda la información relevante y botones de acción
 * para aprobar, rechazar o volver a la lista de solicitudes.
 * 
 * @inputs (Props del componente)
 * - data: SolicitudDetalleData - Datos completos de la solicitud
 *   - id: string - ID único de la solicitud
 *   - nomina: string - Número de nómina del empleado
 *   - nombre: string - Nombre completo del empleado
 *   - grupo: string - Grupo de trabajo del empleado
 *   - diaAModificar: string - Día original a modificar con turno
 *   - nuevoDia: string - Nuevo día propuesto con turno
 *   - tipo: 'REPROGRAMACIÓN' | 'FESTIVO TRABAJADO' - Tipo de solicitud
 * - onBack?: () => void - Callback para volver a la vista anterior
 * - onAccept?: () => void - Callback para aceptar la solicitud
 * - onReject?: (motivo?: string) => void - Callback para rechazar con motivo
 * - children?: React.ReactNode - Contenido adicional opcional
 * 
 * @used_in (Componentes padre)
 * - src/components/Dashboard-Area/SolicitudDetallePage.tsx
 * - src/components/Dashboard-Area/SolicitudesComponent.tsx
 * 
 * @user_roles (Usuarios que acceden)
 * - Jefe de Área
 * 
 * @dependencies
 * - React: Framework base y hooks (useState)
 * - lucide-react: Iconos (ChevronLeft, XCircle, CheckCircle, ArrowUpDown)
 * - ./RejectModal: Modal de confirmación para rechazos
 * - ./ApproveModal: Modal de confirmación para aprobaciones
 * 
 * @author Vulcanics Dev Team
 * @created 2024
 * @last_modified 2025-08-20
 * =============================================================================
 */

import React, { useState } from 'react'
import {
    ChevronLeft,
    XCircle,
    CheckCircle,
    ArrowUpDown
} from 'lucide-react'
import RejectModal from './RejectModal'
import ApproveModal from './ApproveModal'

export interface SolicitudDetalleData {
    id: string
    nomina: string
    nombre: string
    grupo: string
    diaAModificar: string        // ej. "29 Octubre de 2025 (Turno 1)"
    nuevoDia: string             // ej. "01 Noviembre de 2025 (Turno 2)"
    tipo: 'REPROGRAMACIÓN' | 'FESTIVO TRABAJADO'
}

interface Props {
    data: SolicitudDetalleData
    onBack?: () => void
    onAccept?: () => void
    onReject?: (motivo?: string) => void
    children?: React.ReactNode
}

const SolicitudDetalle: React.FC<Props> = ({
    data,
    onBack,
    onAccept,
    onReject,
    children
}) => {
    const [showRejectModal, setShowRejectModal] = useState(false);
    const [showApproveModal, setShowApproveModal] = useState(false);

    const handleRejectClick = () => {
        setShowRejectModal(true);
    };

    const handleApproveClick = () => {
        setShowApproveModal(true);
    };

    const handleRejectConfirm = (motivo: string) => {
        if (onReject) {
            onReject(motivo);
        }
        setShowRejectModal(false);
    };

    const handleRejectCancel = () => {
        setShowRejectModal(false);
    };

    const handleApproveConfirm = () => {
        if (onAccept) {
            onAccept();
        }
        setShowApproveModal(false);
    };

    const handleApproveCancel = () => {
        setShowApproveModal(false);
    };

    return (
        <div className="space-y-6">
            <div className="flex items-start justify-between">
                <button
                    onClick={onBack}
                    className="flex items-center gap-1 text-sm text-continental-gray-2 hover:text-continental-black cursor-pointer"
                >
                    <ChevronLeft className="w-4 h-4" />
                    Regresar
                </button>

                <div className="flex items-center gap-3">
                    <button
                        onClick={handleRejectClick}
                        className="inline-flex h-8 items-center justify-center gap-1 rounded-md border border-rose-500
                       px-4 text-sm font-semibold text-rose-600 hover:bg-rose-50 focus:outline-none
                       focus:ring-2 focus:ring-rose-200 cursor-pointer"
                    >
                        <XCircle className="w-4 h-4" />
                        Rechazar
                    </button>

                    <button
                        onClick={handleApproveClick}
                        className="inline-flex h-8 items-center justify-center gap-1 rounded-md px-4 text-sm font-semibold
                       text-black hover:opacity-90 focus:outline-none focus:ring-2 focus:ring-yellow-200 cursor-pointer"
                        style={{ backgroundColor: 'var(--color-continental-yellow, #FDB41C)' }}
                    >
                        <CheckCircle className="w-4 h-4" />
                        Aceptar
                    </button>
                </div>
            </div>

            <div className="grid grid-cols-[160px_1fr] gap-y-2 text-sm">
                <span className="font-semibold text-continental-black">No. Nómina:</span>
                <span>{data.nomina}</span>

                <span className="font-semibold text-continental-black">Nombre:</span>
                <span className="uppercase">{data.nombre}</span>

                <span className="font-semibold text-continental-black">Grupo:</span>
                <span>{data.grupo}</span>

                <span className="font-semibold text-continental-black">Día a modificar:</span>
                <span className="font-medium text-red-600">{data.diaAModificar}</span>

                <span className="col-span-1 flex items-center pl-1 my-1 text-gray-700" aria-hidden="true">
                    <ArrowUpDown className="w-5 h-5" />
                </span>

                <span className="col-span-1" />
                <span className="font-semibold text-continental-black">Nuevo día:</span>
                <span className="font-medium text-blue-600">{data.nuevoDia}</span>
            </div>
            {children}

            <RejectModal
                show={showRejectModal}
                onClose={handleRejectCancel}
                onConfirm={handleRejectConfirm}
                solicitudId={data.id}
                nombreEmpleado={data.nombre}
            />

            <ApproveModal
                show={showApproveModal}
                onClose={handleApproveCancel}
                onConfirm={handleApproveConfirm}
                solicitudId={data.id}
                nombreEmpleado={data.nombre}
                tipoSolicitud={data.tipo}
                fechaActual={data.diaAModificar}
                fechaReprogramacion={data.nuevoDia}
            />
        </div>
    )
}

export default SolicitudDetalle
