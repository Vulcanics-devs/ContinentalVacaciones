/**
 * =============================================================================
 * CALENDAR VIEW
 * =============================================================================
 * 
 * @description
 * Componente que maneja la visualización del calendario con diferentes vistas
 * (mensual, semanal, diaria). Gestiona la navegación entre fechas y el cambio
 * de vistas según la interacción del usuario.
 * 
 * @inputs (Props del componente)
 * - calendarData: CalendarData | null - Datos del calendario con días y estadísticas
 * - currentDate: Date - Fecha actualmente seleccionada/mostrada
 * - filters: CalendarFilters - Filtros activos aplicados al calendario
 * - monthNames: string[] - Nombres de los meses para localización
 * - dayNames: string[] - Nombres de los días para localización
 * - monthlyStats: any - Estadísticas mensuales del calendario
 * - onNavigateMonth: (direction: 'prev' | 'next') => void - Navegación entre meses
 * - onChangeView: (view: 'monthly' | 'weekly' | 'daily') => void - Cambio de vista
 * 
 * @used_in (Componentes padre)
 * - src/components/Dashboard-Area/CalendarWidget.tsx
 * 
 * @user_roles (Usuarios que acceden)
 * - Jefe de Área
 * 
 * @dependencies
 * - React: Framework base y hooks (useState)
 * - lucide-react: Iconos de navegación (ChevronLeft, ChevronRight)
 * - ../../interfaces/Calendar.interface: Tipos de datos del calendario
 * - ./CalendarViews: Componentes de vistas específicas (MonthlyView, WeeklyView, DailyView)
 * 
 * @author Vulcanics Dev Team
 * @created 2024
 * @last_modified 2025-08-20
 * =============================================================================
 */

import React, { useState } from 'react';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import type { CalendarData, CalendarFilters } from '../../interfaces/Calendar.interface';
import type { AusenciasPorFecha } from '../../interfaces/Ausencias.interface';
import type { Grupo } from '@/interfaces/Areas.interface';
import { MonthlyView, WeeklyView, DailyView } from './CalendarViews';

interface CalendarViewProps {
    calendarData: CalendarData | null;
    currentDate: Date;
    filters: CalendarFilters;
    monthNames: string[];
    dayNames: string[];
    monthlyStats: any;
    onNavigateMonth: (direction: 'prev' | 'next') => void;
    onNavigateWeek?: (direction: 'prev' | 'next') => void;  // ← AGREGAR
    onNavigateDay?: (direction: 'prev' | 'next') => void;
    onChangeView: (view: 'monthly' | 'weekly' | 'daily') => void;
    onNavigateToDate?: (date: Date) => void;
    currentAreaGroups?: Grupo[];
    ausenciasData: AusenciasPorFecha[];
}

const CalendarView: React.FC<CalendarViewProps> = ({
    calendarData,
    currentDate,
    filters,
    monthNames,
    dayNames,
    monthlyStats,
    onNavigateMonth,
    onNavigateWeek,    // ← AGREGAR
    onNavigateDay,
    onChangeView,
    onNavigateToDate,
    currentAreaGroups,
    ausenciasData
}) => {
    const [selectedDate, setSelectedDate] = useState(currentDate);
    React.useEffect(() => {
        setSelectedDate(currentDate);
    }, [currentDate]);
    const handleSelectedDate = (date: Date) => {
        onChangeView('daily');
        setSelectedDate(date);
        // También necesitamos actualizar la fecha en el componente padre para que cargue los datos correctos
        onNavigateToDate?.(date);
    };
    return (
        <>
            <div className="px-8 py-4">
                <div className="calendar-nav-row">
                    <div className="month-nav">
                        {filters.view === 'monthly' && (
                            <>
                                <button onClick={() => onNavigateMonth('prev')} className="nav-arrow-button cursor-pointer">
                                    <ChevronLeft className="w-5 h-5" />
                                </button>
                                <span className="calendar-month-title">
                                    {monthNames[currentDate.getMonth()]} {currentDate.getFullYear()}
                                </span>
                                <button onClick={() => onNavigateMonth('next')} className="nav-arrow-button cursor-pointer">
                                    <ChevronRight className="w-5 h-5" />
                                </button>
                            </>
                        )}

                        {filters.view === 'weekly' && (
                            <>
                                <button onClick={() => onNavigateWeek?.('prev')} className="nav-arrow-button cursor-pointer">
                                    <ChevronLeft className="w-5 h-5" />
                                </button>
                                <span className="calendar-month-title">
                                    Semana del {currentDate.getDate()} {monthNames[currentDate.getMonth()]} {currentDate.getFullYear()}
                                </span>
                                <button onClick={() => onNavigateWeek?.('next')} className="nav-arrow-button cursor-pointer">
                                    <ChevronRight className="w-5 h-5" />
                                </button>
                            </>
                        )}

                        {filters.view === 'daily' && (
                            <>
                                <button onClick={() => onNavigateDay?.('prev')} className="nav-arrow-button cursor-pointer">
                                    <ChevronLeft className="w-5 h-5" />
                                </button>
                                <span className="calendar-month-title">
                                    {currentDate.getDate()} de {monthNames[currentDate.getMonth()]} {currentDate.getFullYear()}
                                </span>
                                <button onClick={() => onNavigateDay?.('next')} className="nav-arrow-button cursor-pointer">
                                    <ChevronRight className="w-5 h-5" />
                                </button>
                            </>
                        )}
                    </div>

                    <div className="monthly-stats monthly-stats-center">
                        {monthlyStats && (
                            <>
                                <span className="stats-label">% Promedio Mensual:</span>
                                <span
                                    className={`stats-value ${monthlyStats.averagePercentage < 70
                                        ? 'text-red-600'
                                        : monthlyStats.averagePercentage < 90
                                            ? 'text-yellow-600'
                                            : 'text-green-600'
                                        }`}
                                >
                                    {monthlyStats.averagePercentage}%
                                </span>
                            </>
                        )}
                    </div>

                    <div className="flex items-center gap-3">
                        <span className="text-sm font-medium text-gray-700">Vista</span>
                        <div className="view-controls">
                            <button onClick={() => onChangeView('weekly')} className={`view-button ${filters.view === 'weekly' ? 'view-button-active' : ''}`}>Semanal</button>
                            <button onClick={() => onChangeView('monthly')} className={`view-button ${filters.view === 'monthly' ? 'view-button-active' : ''}`}>Mensual</button>
                            <button onClick={() => onChangeView('daily')} className={`view-button ${filters.view === 'daily' ? 'view-button-active' : ''}`}>Día</button>
                        </div>
                    </div>
                </div>
            </div>
            {filters.view === 'monthly' && (
                <MonthlyView
                    calendarData={calendarData}
                    currentDate={currentDate}
                    selectedGroups={filters.selectedGroups}
                    dayNames={dayNames}
                    onSelectDate={handleSelectedDate}
                    currentAreaGroups={currentAreaGroups}
                    ausenciasData={ausenciasData}
                />
            )}
            {filters.view === 'weekly' && (
                <WeeklyView
                    calendarData={calendarData}
                    currentDate={currentDate}
                    selectedGroups={filters.selectedGroups}
                    dayNames={dayNames}
                    onSelectDate={handleSelectedDate}
                    currentAreaGroups={currentAreaGroups}
                    ausenciasData={ausenciasData}
                />
            )}
            {filters.view === 'daily' && (
                <DailyView
                    calendarData={calendarData}
                    currentDate={currentDate}
                    selectedGroups={filters.selectedGroups}
                    onSelectDate={handleSelectedDate}
                    currentAreaGroups={currentAreaGroups}
                    ausenciasData={ausenciasData}
                />
            )}
        </>
    );
};

export default CalendarView;
