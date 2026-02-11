import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { getStatusIcon, getStatusColor, formatDate } from "./MyRequests";
import notificacionesService, { type Notificacion } from "@/services/notificacionesService";
import { vacacionesService } from "@/services/vacacionesService";
import type { UsuarioInfoDto } from "@/interfaces/Api.interface";

export const Notifications = ({ selectedEmployee }: { selectedEmployee: UsuarioInfoDto }) => {
    const [notificaciones, setNotificaciones] = useState<Notificacion[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchNotificaciones = async () => {
            try {
                setLoading(true);

                // Obtener año vigente de la configuración
                let anioVigente = new Date().getFullYear();
                try {
                    const config = await vacacionesService.getConfig();
                    anioVigente = config.anioVigente;
                } catch (error) {
                    console.log('Usando año por defecto:', anioVigente);
                }

                console.log('📅 Buscando notificaciones del año:', anioVigente);

                // Obtener notificaciones filtradas por tipo de reprogramación
                const response = await notificacionesService.obtenerNotificaciones({
                    tipoNotificacion: 4, // TipoNotificacion.SolicitudReprogramacion
                    pagina: 1,
                    tamañoPagina: 3,
                    ordenarPor: 'FechaAccion',
                    direccionOrden: 'DESC'
                });

                if (response.success && response.data) {
                    setNotificaciones(response.data.notificaciones);
                    console.log('✅ Notificaciones cargadas:', response.data.notificaciones.length);
                } else {
                    setNotificaciones([]);
                }
            } catch (error) {
                console.error('Error al cargar notificaciones:', error);
                setNotificaciones([]);
            } finally {
                setLoading(false);
            }
        };

        fetchNotificaciones();
    }, [selectedEmployee?.id]);

    // Mapear estatus de notificación a formato del componente
    const mapEstatusToStatus = (estatus: number): 'approved' | 'rejected' | 'pending' => {
        // Suponiendo que en el mensaje dice "Aprobada", "Rechazada", etc.
        // O basándonos en el tipo de notificación
        return 'pending'; // Por defecto
    };

    if (loading) {
        return (
            <div className="flex flex-col gap-4 h-full w-full p-4 border border-continental-blue-dark rounded-lg bg-continental-gray-4/20">
                <h2 className="text-2xl font-bold text-continental-blue-dark">Notificaciones</h2>
                <div className="flex items-center justify-center h-32 text-gray-500">
                    <p className="text-sm">Cargando notificaciones...</p>
                </div>
            </div>
        );
    }

    if (notificaciones.length === 0) {
        return (
            <div className="flex flex-col gap-4 h-full w-full p-4 border border-continental-blue-dark rounded-lg bg-continental-gray-4/20">
                <h2 className="text-2xl font-bold text-continental-blue-dark">Notificaciones</h2>
                <div className="flex items-center justify-center h-32 text-gray-500">
                    <p className="text-sm">No hay notificaciones recientes</p>
                </div>
            </div>
        );
    }

    return (
        <div className="flex flex-col gap-4 h-full w-full p-4 border border-continental-blue-dark rounded-lg bg-continental-gray-4/20">
            <h2 className="text-2xl font-bold text-continental-blue-dark">Notificaciones</h2>
            <div className="flex gap-3">
                {notificaciones.map((notificacion) => {
                    // Extraer estado del mensaje o título
                    const esAprobada = notificacion.mensaje.toLowerCase().includes('aprobad');
                    const esRechazada = notificacion.mensaje.toLowerCase().includes('rechazad');
                    const status: 'approved' | 'rejected' | 'pending' =
                        esAprobada ? 'approved' : esRechazada ? 'rejected' : 'pending';

                    return (
                        <div
                            key={notificacion.id}
                            className="relative bg-white border border-gray-200 rounded-lg p-4 shadow-sm hover:shadow-md transition-all duration-200 cursor-pointer flex-1 max-w-1/3"
                        >
                            {/* Header con tipo y status */}
                            <div className="flex flex-col items-start justify-between mb-2">
                                <div className="flex items-center gap-2">
                                    {getStatusIcon(status)}
                                    <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${getStatusColor(status)}`}>
                                        {esAprobada ? 'Aprobada' : esRechazada ? 'Rechazada' : 'Pendiente'}
                                    </span>
                                </div>
                                <div className="flex items-center gap-2">
                                    <span className="text-base font-medium text-gray-600">
                                        {notificacion.titulo}
                                    </span>
                                </div>
                            </div>

                            {/* Información de la notificación */}
                            <div className="mb-2">
                                <p className="text-sm text-gray-600 line-clamp-3">
                                    {notificacion.mensaje}
                                </p>
                                <p className="text-xs text-gray-400 absolute top-4 right-4">
                                    {new Date(notificacion.fechaAccion).toLocaleString('es-MX', {
                                        day: '2-digit',
                                        month: 'short',
                                        year: 'numeric',
                                        hour: '2-digit',
                                        minute: '2-digit'
                                    })}
                                </p>
                            </div>
                        </div>
                    );
                })}
            </div>
            <Link className="text-blue-500 text-xs hover:text-blue-600 ml-auto" to={`/empleados/mis-solicitudes`}>
                Ver todas las solicitudes
            </Link>
        </div>
    );
}