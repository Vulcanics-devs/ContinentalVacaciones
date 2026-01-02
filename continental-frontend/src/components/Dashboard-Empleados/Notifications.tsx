import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { getStatusIcon, getStatusColor, formatDate } from "./MyRequests";
import { ReprogramacionService } from "@/services/reprogramacionService";
import { vacacionesService } from "@/services/vacacionesService";
import type { SolicitudReprogramacion } from "@/interfaces/Api.interface";
import type { UsuarioInfoDto } from "@/interfaces/Api.interface";

export const Notifications = ({selectedEmployee}: {selectedEmployee: UsuarioInfoDto}) => {
  const [solicitudes, setSolicitudes] = useState<SolicitudReprogramacion[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchSolicitudes = async () => {
      if (!selectedEmployee?.id) {
        setLoading(false);
        return;
      }

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

        // Obtener las últimas 3 solicitudes
        const ultimasSolicitudes = await ReprogramacionService.obtenerUltimasSolicitudes(
          selectedEmployee.id,
          anioVigente,
          3
        );
        console.log({ultimasSolicitudes});

        setSolicitudes(ultimasSolicitudes);
      } catch (error) {
        console.error('Error al cargar solicitudes:', error);
        setSolicitudes([]);
      } finally {
        setLoading(false);
      }
    };

    fetchSolicitudes();
  }, [selectedEmployee?.id]);

  // Mapear el estado de la solicitud al formato del componente existente
  const mapEstadoToStatus = (estado: string): 'approved' | 'rejected' | 'pending' => {
    switch (estado) {
      case 'Aprobada':
        return 'approved';
      case 'Rechazada':
        return 'rejected';
      default:
        return 'pending';
    }
  };

  if (loading) {
    return (
      <div className="flex flex-col gap-4 h-full w-full p-4 border border-continental-blue-dark rounded-lg bg-continental-gray-4/20">
        <h2 className="text-2xl font-bold text-continental-blue-dark">Notificaciones</h2>
        <div className="flex items-center justify-center h-32 text-gray-500">
          <p className="text-sm">Cargando solicitudes...</p>
        </div>
      </div>
    );
  }

  if (solicitudes.length === 0) {
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
        {solicitudes.map((solicitud) => (
          <div
            key={solicitud.id}
            className="relative bg-white border border-gray-200 rounded-lg p-4 shadow-sm hover:shadow-md transition-all duration-200 cursor-pointer flex-1 max-w-1/3"
          >
            {/* Header con tipo y status */}
            <div className="flex flex-col items-start justify-between mb-2">
              <div className="flex items-center gap-2">
                {getStatusIcon(mapEstadoToStatus(solicitud.estadoSolicitud))}
                <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${getStatusColor(mapEstadoToStatus(solicitud.estadoSolicitud))}`}>
                  {solicitud.estadoSolicitud}
                </span>
              </div>
              <div className="flex items-center gap-2">
                <span className="text-base font-medium text-gray-600">
                  Reprogramación
                </span>
              </div>
            </div>

            {/* Información de la solicitud */}
            <div className="mb-2">
              <p className="text-sm text-gray-600 font-medium mb-1">
                {formatDate(solicitud.fechaOriginal, false)} → {formatDate(solicitud.fechaNueva, false)}
              </p>
              <p className="text-xs text-gray-500 line-clamp-2">
                {solicitud.motivo}
              </p>
              {solicitud.motivoRechazo && (
                <p className="text-xs text-red-600 mt-1">
                  Motivo rechazo: {solicitud.motivoRechazo}
                </p>
              )}
              <p className="text-xs text-gray-400 absolute top-4 right-4">
                {formatDate(solicitud.fechaSolicitud)}
              </p>
            </div>

          </div>
        ))}
      </div>
            <Link className="text-blue-500 text-xs hover:text-blue-600 ml-auto" to={`/empleados/mis-solicitudes`}>
              Ver todas las solicitudes
            </Link>
    </div>
  );
}
