import React from 'react';
import { Clock, Calendar as CalendarIcon } from 'lucide-react';
import { format, startOfMonth, endOfMonth, eachDayOfInterval, isSameMonth, startOfWeek, endOfWeek } from 'date-fns';
import { es } from 'date-fns/locale';
import type { ExcepcionPorcentaje } from '@/interfaces/Api.interface';

interface OvertimeCalendarProps {
    excepciones: ExcepcionPorcentaje[];
    currentMonth: Date;
    empleadoNombre: string;
}

export const OvertimeCalendar: React.FC<OvertimeCalendarProps> = ({
    excepciones,
    currentMonth,
    empleadoNombre
}) => {
    // Obtener todos los días del mes con padding para completar semanas
    const monthStart = startOfMonth(currentMonth);
    const monthEnd = endOfMonth(currentMonth);
    const startDate = startOfWeek(monthStart, { locale: es });
    const endDate = endOfWeek(monthEnd, { locale: es });
    const days = eachDayOfInterval({ start: startDate, end: endDate });

    const hasException = (date: Date) => {
        const dateStr = format(date, 'yyyy-MM-dd');
        return excepciones.find(exc => exc.fecha === dateStr);
    };

    const dayNames = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'];

    return (
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4">
            <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold flex items-center gap-2">
                    <CalendarIcon size={20} className="text-orange-500" />
                    Calendario de Tiempo Extra
                </h3>
                <span className="text-sm text-gray-500">
                    {format(currentMonth, 'MMMM yyyy', { locale: es })}
                </span>
            </div>

            {excepciones.length === 0 ? (
                <div className="text-center py-8 bg-gray-50 rounded-lg">
                    <Clock size={48} className="mx-auto text-gray-300 mb-2" />
                    <p className="text-gray-500 text-sm">
                        No hay tiempo extra programado para {empleadoNombre} en este mes
                    </p>
                </div>
            ) : (
                <>
                    {/* Grid del calendario */}
                    <div className="grid grid-cols-7 gap-1 mb-2">
                        {dayNames.map(day => (
                            <div key={day} className="text-center text-xs font-semibold text-gray-600 py-2">
                                {day}
                            </div>
                        ))}
                    </div>

                    <div className="grid grid-cols-7 gap-1">
                        {days.map((day, index) => {
                            const exception = hasException(day);
                            const isCurrentMonth = isSameMonth(day, currentMonth);
                            const isToday = format(day, 'yyyy-MM-dd') === format(new Date(), 'yyyy-MM-dd');

                            return (
                                <div
                                    key={index}
                                    className={`
                                        relative aspect-square border rounded-lg p-2 text-center
                                        ${!isCurrentMonth ? 'bg-gray-50 text-gray-300' : 'bg-white'}
                                        ${isToday ? 'ring-2 ring-blue-500' : ''}
                                        ${exception ? 'bg-orange-50 border-orange-300 hover:bg-orange-100' : 'border-gray-200'}
                                        transition-colors duration-150
                                    `}
                                    title={exception ?
                                        `Tiempo Extra: ${exception.porcentajeMaximoPermitido}%${exception.motivo ? ` - ${exception.motivo}` : ''}`
                                        : undefined
                                    }
                                >
                                    <div className="text-sm font-medium">
                                        {format(day, 'd')}
                                    </div>
                                    {exception && (
                                        <div className="absolute inset-0 flex items-center justify-center">
                                            <div className="bg-orange-500 text-white rounded-full p-1">
                                                <Clock size={16} />
                                            </div>
                                        </div>
                                    )}
                                    {exception && (
                                        <div className="absolute bottom-0 left-0 right-0 text-xs font-bold text-orange-600">
                                            {exception.porcentajeMaximoPermitido}%
                                        </div>
                                    )}
                                </div>
                            );
                        })}
                    </div>

                    {/* Leyenda */}
                    <div className="mt-4 pt-4 border-t border-gray-200">
                        <div className="flex items-center gap-2 text-sm">
                            <div className="flex items-center gap-1">
                                <div className="w-4 h-4 bg-orange-50 border-2 border-orange-300 rounded"></div>
                                <span className="text-gray-600">Día con tiempo extra</span>
                            </div>
                        </div>
                    </div>

                    {/* Lista de excepciones */}
                    <div className="mt-4 space-y-2 max-h-40 overflow-y-auto">
                        <div className="text-sm font-semibold text-gray-700 mb-2">
                            Detalles ({excepciones.length} días):
                        </div>
                        {excepciones.map(exc => (
                            <div key={exc.id} className="text-xs bg-orange-50 border border-orange-200 rounded p-2">
                                <div className="flex justify-between items-start">
                                    <span className="font-semibold">
                                        {format(new Date(exc.fecha + 'T00:00:00'), "d 'de' MMMM", { locale: es })}
                                    </span>
                                    <span className="font-bold text-orange-600">
                                        {exc.porcentajeMaximoPermitido}%
                                    </span>
                                </div>
                                {exc.motivo && (
                                    <p className="text-gray-600 mt-1">{exc.motivo}</p>
                                )}
                            </div>
                        ))}
                    </div>
                </>
            )}
        </div>
    );
};