/**
 * =============================================================================
 * FESTIVOS TRABAJADOS - VALIDATION RULES
 * =============================================================================
 *
 * Reglas de negocio para solicitudes de festivos trabajados:
 *
 * 1. El empleado tiene 1 AÑO desde la fecha del festivo trabajado para SOLICITAR
 *    la compensación.
 * 2. Una vez solicitado, tiene 1 MES desde la fecha de solicitud para USAR el día,
 *    pero NO puede exceder el aniversario de 1 año + 1 mes desde la fecha trabajada.
 * 3. Si ya expiró, mostrar alerta específica.
 * 4. Si está cerca del vencimiento, mostrar advertencia con días restantes.
 *
 * @author Vulcanics Dev Team
 * =============================================================================
 */

export interface FestivoValidationResult {
    isValid: boolean
    isExpired: boolean
    isCloseToDeadline: boolean
    daysRemaining: number
    requestDeadline: Date
    useDeadline: Date
    alertMessage?: string
    warningMessage?: string
}

export interface FechaUsoValidationResult {
    isValid: boolean
    message?: string
    maxDate: string
}

/**
 * Parsea una fecha en formato "YYYY-MM-DD" de forma segura (sin problemas de zona horaria).
 */
function parseDateSafe(dateStr: string): Date {
    const partes = dateStr.split('-')
    return new Date(
        parseInt(partes[0]),
        parseInt(partes[1]) - 1,
        parseInt(partes[2])
    )
}

/**
 * Formatea una fecha para mostrar al usuario en español de México.
 */
function formatDateES(date: Date): string {
    return date.toLocaleDateString('es-MX', {
        day: 'numeric',
        month: 'long',
        year: 'numeric',
    })
}

/**
 * Valida si un festivo trabajado aún puede ser solicitado para compensación.
 *
 * @param fechaTrabajada - Fecha del festivo trabajado (formato "YYYY-MM-DD")
 * @param referenceDate  - Fecha de referencia (por defecto: hoy)
 * @returns Resultado de la validación con mensajes para el usuario
 */
export function validarFestivoParaSolicitud(
    fechaTrabajada: string,
    referenceDate?: Date
): FestivoValidationResult {
    const today = referenceDate ?? new Date()
    const fechaTrabajadaDate = parseDateSafe(fechaTrabajada)

    // Límite para SOLICITAR: 1 año desde la fecha trabajada
    const requestDeadline = new Date(fechaTrabajadaDate)
    requestDeadline.setFullYear(requestDeadline.getFullYear() + 1)

    // Límite absoluto para USAR: 1 año + 1 mes desde la fecha trabajada
    const useDeadline = new Date(fechaTrabajadaDate)
    useDeadline.setFullYear(useDeadline.getFullYear() + 1)
    useDeadline.setMonth(useDeadline.getMonth() + 1)

    // Calcular días restantes para solicitar
    const diffMs = requestDeadline.getTime() - today.getTime()
    const daysRemaining = Math.ceil(diffMs / (1000 * 60 * 60 * 24))

    const isExpired = daysRemaining < 0
    const isCloseToDeadline = !isExpired && daysRemaining <= 30

    const result: FestivoValidationResult = {
        isValid: !isExpired,
        isExpired,
        isCloseToDeadline,
        daysRemaining: Math.max(daysRemaining, 0),
        requestDeadline,
        useDeadline,
    }

    if (isExpired) {
        result.alertMessage =
            `El festivo trabajado del ${formatDateES(fechaTrabajadaDate)} ya expiró. ` +
            `Tenías hasta el ${formatDateES(requestDeadline)} para solicitarlo.`
    }

    if (isCloseToDeadline) {
        result.warningMessage =
            `Quedan ${daysRemaining} día${daysRemaining !== 1 ? 's' : ''} para solicitar este festivo trabajado. ` +
            `Fecha límite: ${formatDateES(requestDeadline)}.`
    }

    return result
}

/**
 * Valida que la fecha de USO seleccionada esté dentro de la ventana permitida.
 *
 * La fecha de uso debe cumplir AMBAS condiciones:
 * - Ser antes de 1 mes desde la fecha de solicitud (hoy)
 * - No exceder 1 año + 1 mes desde la fecha del festivo trabajado
 *
 * Se toma el mínimo de ambos límites.
 *
 * @param fechaTrabajada - Fecha del festivo trabajado (formato "YYYY-MM-DD")
 * @param fechaUso       - Fecha de uso seleccionada (formato "YYYY-MM-DD")
 * @param fechaSolicitud - Fecha de solicitud / referencia (por defecto: hoy)
 * @returns Resultado de la validación con fecha máxima permitida
 */
export function validarFechaDeUso(
    fechaTrabajada: string,
    fechaUso: string,
    fechaSolicitud?: Date
): FechaUsoValidationResult {
    const today = fechaSolicitud ?? new Date()
    const fechaTrabajadaDate = parseDateSafe(fechaTrabajada)
    const fechaUsoDate = parseDateSafe(fechaUso)

    // Límite 1: 1 mes desde la fecha de solicitud
    const limiteDesdeRequest = new Date(today)
    limiteDesdeRequest.setMonth(limiteDesdeRequest.getMonth() + 1)

    // Límite 2: 1 año + 1 mes desde la fecha trabajada (absoluto)
    const limiteAbsoluto = new Date(fechaTrabajadaDate)
    limiteAbsoluto.setFullYear(limiteAbsoluto.getFullYear() + 1)
    limiteAbsoluto.setMonth(limiteAbsoluto.getMonth() + 1)

    // El límite efectivo es el MENOR de los dos
    const maxDate = limiteAbsoluto
    const maxDateStr = maxDate.toISOString().split('T')[0]

    if (fechaUsoDate > maxDate) {
        return {
            isValid: false,
            message:
                `La fecha seleccionada excede el límite permitido. ` +
                `La fecha máxima para usar este festivo es el ${formatDateES(maxDate)}.`,
            maxDate: maxDateStr,
        }
    }

    return { isValid: true, maxDate: maxDateStr }
}

/**
 * Calcula la fecha máxima permitida para usar un festivo (útil para limitar el date picker).
 *
 * @param fechaTrabajada - Fecha del festivo trabajado (formato "YYYY-MM-DD")
 * @param fechaSolicitud - Fecha de solicitud (por defecto: hoy)
 * @returns Fecha máxima en formato "YYYY-MM-DD"
 */
export function calcularFechaMaximaUso(
    fechaTrabajada: string
): string {
    const fechaTrabajadaDate = parseDateSafe(fechaTrabajada)

    const limiteAbsoluto = new Date(fechaTrabajadaDate)
    limiteAbsoluto.setFullYear(limiteAbsoluto.getFullYear() + 1)
    limiteAbsoluto.setMonth(limiteAbsoluto.getMonth() + 1)

    return limiteAbsoluto.toISOString().split('T')[0]
}
