import { useState } from 'react'
import { XCircle } from 'lucide-react'
import { Button } from '../ui/button'

interface ModalRechazoPermisoProps {
    show: boolean
    nombreEmpleado: string
    onClose: () => void
    onConfirm: (motivo: string) => void
}

export default function ModalRechazoPermiso({
    show,
    nombreEmpleado,
    onClose,
    onConfirm
}: ModalRechazoPermisoProps) {
    const [motivo, setMotivo] = useState('')

    if (!show) return null

    const handleConfirm = () => {
        if (!motivo.trim()) {
            alert('Debe ingresar un motivo de rechazo')
            return
        }
        onConfirm(motivo)
        setMotivo('')
    }

    const handleClose = () => {
        setMotivo('')
        onClose()
    }

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
            <div className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4">
                <div className="flex items-center justify-between p-6 border-b">
                    <h3 className="text-lg font-semibold text-gray-900">
                        Rechazar Solicitud de Permiso
                    </h3>
                    <button
                        onClick={handleClose}
                        className="text-gray-400 hover:text-gray-600"
                    >
                        <XCircle className="w-5 h-5" />
                    </button>
                </div>

                <div className="p-6">
                    <p className="text-sm text-gray-600 mb-4">
                        Empleado: <span className="font-semibold">{nombreEmpleado}</span>
                    </p>

                    <label className="block text-sm font-medium text-gray-700 mb-2">
                        Motivo del rechazo *
                    </label>
                    <textarea
                        value={motivo}
                        onChange={(e) => setMotivo(e.target.value)}
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-red-500"
                        rows={4}
                        placeholder="Ingrese el motivo del rechazo..."
                    />
                </div>

                <div className="flex gap-3 px-6 pb-6">
                    <Button
                        onClick={handleClose}
                        variant="outline"
                        className="flex-1"
                    >
                        Cancelar
                    </Button>
                    <Button
                        onClick={handleConfirm}
                        className="flex-1 bg-red-600 hover:bg-red-700 text-white"
                    >
                        Rechazar Solicitud
                    </Button>
                </div>
            </div>
        </div>
    )
}