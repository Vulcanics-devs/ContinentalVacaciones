import { useState, useCallback } from 'react'
import { solicitudesService } from '../services/solicitudesService'
import type {
    Solicitud,
    SolicitudesListResponse,
    SolicitudFilters,
    CreateSolicitudRequest,
    UpdateSolicitudRequest
} from '../interfaces/Solicitudes.interface'

interface UseSolicitudesReturn {
    // Estado de datos
    solicitudes: Solicitud[]
    totalSolicitudes: number
    pendientes: number
    aprobadas: number
    rechazadas: number

    // Estados de UI
    loading: boolean
    error: string | null

    // Métodos de datos
    fetchSolicitudes: (filters?: SolicitudFilters) => Promise<void>
    createSolicitud: (solicitudData: CreateSolicitudRequest) => Promise<Solicitud>
    updateSolicitud: (id: number, updateData: UpdateSolicitudRequest) => Promise<Solicitud>
    aprobarSolicitud: (id: number, comentarios?: string) => Promise<Solicitud>
    rechazarSolicitud: (id: number, comentarios?: string) => Promise<Solicitud>
    refetch: () => Promise<void>
}

export const useSolicitudes = (): UseSolicitudesReturn => {
    // Estado de datos
    const [solicitudes, setSolicitudes] = useState<Solicitud[]>([])
    const [totalSolicitudes, setTotalSolicitudes] = useState(0)
    const [pendientes, setPendientes] = useState(0)
    const [aprobadas, setAprobadas] = useState(0)
    const [rechazadas, setRechazadas] = useState(0)

    // Estados de UI
    const [loading, setLoading] = useState(false)
    const [error, setError] = useState<string | null>(null)

    // Filtros actuales
    const [currentFilters, setCurrentFilters] = useState<SolicitudFilters | undefined>()

    const fetchSolicitudes = useCallback(async (filters?: SolicitudFilters) => {
        setLoading(true)
        setError(null)

        try {
            console.log('useSolicitudes: Fetching solicitudes with filters:', filters)

            const response: SolicitudesListResponse = await solicitudesService.getSolicitudesList(filters)

            console.log('useSolicitudes: Received response:', response)

            setSolicitudes(response.solicitudes)
            setTotalSolicitudes(response.totalSolicitudes)
            setPendientes(response.pendientes)
            setAprobadas(response.aprobadas)
            setRechazadas(response.rechazadas)
            setCurrentFilters(filters)

        } catch (err) {
            console.error('useSolicitudes: Error fetching solicitudes:', err)
            setError(err instanceof Error ? err.message : 'Error desconocido al cargar solicitudes')
            setSolicitudes([])
            setTotalSolicitudes(0)
            setPendientes(0)
            setAprobadas(0)
            setRechazadas(0)
        } finally {
            setLoading(false)
        }
    }, [])

    const createSolicitud = useCallback(async (solicitudData: CreateSolicitudRequest): Promise<Solicitud> => {
        setLoading(true)
        setError(null)

        try {
            console.log('useSolicitudes: Creating solicitud:', solicitudData)

            const newSolicitud = await solicitudesService.createSolicitud(solicitudData)

            console.log('useSolicitudes: Created solicitud:', newSolicitud)

            // Refetch para actualizar la lista
            await fetchSolicitudes(currentFilters)

            return newSolicitud
        } catch (err) {
            console.error('useSolicitudes: Error creating solicitud:', err)
            const errorMessage = err instanceof Error ? err.message : 'Error desconocido al crear solicitud'
            setError(errorMessage)
            throw new Error(errorMessage)
        } finally {
            setLoading(false)
        }
    }, [fetchSolicitudes, currentFilters])

    const updateSolicitud = useCallback(async (id: number, updateData: UpdateSolicitudRequest): Promise<Solicitud> => {
        setLoading(true)
        setError(null)

        try {
            console.log('useSolicitudes: Updating solicitud:', { id, updateData })

            const updatedSolicitud = await solicitudesService.updateSolicitud(id, updateData)

            console.log('useSolicitudes: Updated solicitud:', updatedSolicitud)

            // Refrescar la lista con los filtros actuales para mantener consistencia
            await fetchSolicitudes(currentFilters)

            return updatedSolicitud
        } catch (err) {
            console.error('useSolicitudes: Error updating solicitud:', err)
            const errorMessage = err instanceof Error
                ? err.message
                : (err as any)?.errorMsg || 'Error desconocido al actualizar solicitud'
            setError(errorMessage)
            throw new Error(errorMessage)
        } finally {
            setLoading(false)
        }
    }, [fetchSolicitudes, currentFilters])

    const aprobarSolicitud = useCallback(async (id: number, comentarios?: string): Promise<Solicitud> => {
        return updateSolicitud(id, {
            status: 'Aprobada' as any, // Usar el enum correcto
            comentarios
        })
    }, [updateSolicitud])

    const rechazarSolicitud = useCallback(async (id: number, comentarios?: string): Promise<Solicitud> => {
        return updateSolicitud(id, {
            status: 'Rechazada' as any, // Usar el enum correcto
            comentarios
        })
    }, [updateSolicitud])

    const refetch = useCallback(async () => {
        await fetchSolicitudes(currentFilters)
    }, [fetchSolicitudes, currentFilters])

    // No cargar datos iniciales automáticamente
    // Los componentes que usen este hook deben llamar fetchSolicitudes() con los filtros apropiados

    return {
        // Estado de datos
        solicitudes,
        totalSolicitudes,
        pendientes,
        aprobadas,
        rechazadas,

        // Estados de UI
        loading,
        error,

        // Métodos de datos
        fetchSolicitudes,
        createSolicitud,
        updateSolicitud,
        aprobarSolicitud,
        rechazarSolicitud,
        refetch
    }
}

export default useSolicitudes
