/**
 * =============================================================================
 * TABLE VIEW GROUP
 * =============================================================================
 * 
 * @description
 * Vista de tabla agrupada del calendario que organiza los empleados por grupos
 * y muestra sus datos semanales. Incluye navegación por semanas y meses
 * con información estructurada por grupos de trabajo.
 * 
 * @inputs (Props del componente)
 * - calendarData: CalendarData | null - Datos del calendario organizados por grupo
 * - currentDate: Date - Fecha actual seleccionada
 * - selectedGroups: string[] - Grupos seleccionados para mostrar
 * - weekOffset: number - Desplazamiento de semanas para navegación
 * - monthNames: string[] - Nombres de meses para localización
 * - onNavigateMonth: (direction: 'prev' | 'next') => void - Navegación entre meses
 * - onNavigateWeek: (direction: 'prev' | 'next') => void - Navegación entre semanas
 * 
 * @used_in (Componentes padre)
 * - src/components/Dashboard-Area/CalendarWidget.tsx
 * 
 * @user_roles (Usuarios que acceden)
 * - Jefe de Área
 * 
 * @dependencies
 * - React: Framework base y hooks (useState)
 * - lucide-react: Iconos de navegación (ChevronLeft, ChevronRight, ArrowLeft)
 * - ../../interfaces/Calendar.interface: Tipos de datos del calendario
 * 
 * @author Vulcanics Dev Team
 * @created 2024
 * @last_modified 2025-08-20
 * =============================================================================
 */

import React, { useState } from 'react';
import { ChevronLeft, ChevronRight, ArrowLeft } from 'lucide-react';
import type { CalendarData } from '../../interfaces/Calendar.interface';
import type { Grupo } from '@/interfaces/Areas.interface';

interface Employee {
    id: string;
    name: string;
    weekData: string[]; // Valores como "3D2", "1A1", etc.
}

interface GroupData {
    id: string;
    name: string;
    employees: Employee[];
}

interface TableViewGroupProps {
    calendarData: CalendarData | null;
    currentDate: Date;
    selectedGroups: string[];
    weekOffset: number;
    monthNames: string[];
    onNavigateMonth: (direction: 'prev' | 'next') => void;
    onNavigateWeek: (direction: 'prev' | 'next') => void;
    currentAreaGroups?: Grupo[];
}

const TableViewGroup: React.FC<TableViewGroupProps> = ({
    calendarData,
    currentDate,
    selectedGroups,
    weekOffset,
    monthNames,
    onNavigateMonth,
    onNavigateWeek,
    currentAreaGroups
}) => {
    const [selectedGroup, setSelectedGroup] = useState<string | null>(null);

    // Datos simulados de grupos y empleados
    const groupsData: GroupData[] = [
        {
            id: 'grupo1',
            name: 'Grupo 1',
            employees: [
                {
                    id: '32804568',
                    name: 'HECTOR LAINEZ',
                    weekData: ['3D2', '2A1', '4B3', '1C2', '5D1', '2A3', '3B2']
                },
                {
                    id: '32803598',
                    name: 'MIGUEL RUIZ',
                    weekData: ['2B1', '3C2', '1D3', '4A1', '2B2', '3C1', '1D2']
                },
                {
                    id: '32145364',
                    name: 'LUIS GOMEZ',
                    weekData: ['1A2', '4D1', '2B3', '3C2', '1A1', '4D2', '2B1']
                },
                {
                    id: '23567677',
                    name: 'JORGE LARA',
                    weekData: ['4C1', '1B2', '3A3', '2D1', '4C2', '1B1', '3A2']
                }
            ]
        },
        {
            id: 'grupo2',
            name: 'Grupo 2',
            employees: [
                {
                    id: '45678901',
                    name: 'MARIA GONZALEZ',
                    weekData: ['2D3', '3A2', '1B1', '4C3', '2D1', '3A1', '1B2']
                },
                {
                    id: '56789012',
                    name: 'CARLOS MARTINEZ',
                    weekData: ['3B2', '1C3', '4A1', '2D2', '3B1', '1C2', '4A3']
                }
            ]
        },
        {
            id: 'grupo3',
            name: 'Grupo 3',
            employees: [
                {
                    id: '67890123',
                    name: 'ANA RODRIGUEZ',
                    weekData: ['1C1', '4B2', '2A3', '3D1', '1C2', '4B1', '2A2']
                },
                {
                    id: '78901234',
                    name: 'PEDRO SANCHEZ',
                    weekData: ['4A2', '2C1', '3B3', '1D2', '4A1', '2C3', '3B1']
                }
            ]
        },
        {
            id: 'grupo4',
            name: 'Grupo 4',
            employees: [
                {
                    id: '89012345',
                    name: 'SOFIA LOPEZ',
                    weekData: ['2B3', '3D2', '1A1', '4C2', '2B1', '3D3', '1A2']
                }
            ]
        }
    ];

    // Obtener los días de la semana actual
    const getWeekDays = () => {
        const startOfMonth = new Date(currentDate.getFullYear(), currentDate.getMonth(), 1);
        const dates = [];
        for (let i = 0; i < 7; i++) {
            const date = new Date(startOfMonth);
            date.setDate(startOfMonth.getDate() + weekOffset + i);
            dates.push(date);
        }
        return dates;
    };

    const getDayNames = () => {
        const days = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'];
        return getWeekDays().map(date => days[date.getDay()]);
    };

    const handleGroupClick = (groupId: string) => {
        setSelectedGroup(groupId);
    };

    const handleBackClick = () => {
        setSelectedGroup(null);
    };

    const selectedGroupData = groupsData.find(group => group.id === selectedGroup);

    return (
        <div className="px-8 py-4">
            {/* Navegación de mes para la tabla */}
            <div className="flex justify-center items-center mb-4">
                <button onClick={() => onNavigateMonth('prev')} className="p-2 hover:bg-gray-100 rounded-md transition-colors cursor-pointer">
                    <ChevronLeft className="w-5 h-5 text-gray-600" />
                </button>
                <span className="text-lg font-semibold text-gray-900 mx-6">
                    {monthNames[currentDate.getMonth()]} {currentDate.getFullYear()}
                </span>
                <button onClick={() => onNavigateMonth('next')} className="p-2 hover:bg-gray-100 rounded-md transition-colors cursor-pointer">
                    <ChevronRight className="w-5 h-5 text-gray-600" />
                </button>
            </div>

            {/* Botón de regreso cuando se está viendo un grupo específico */}
            {selectedGroup && (
                <div className="mb-4">
                    <button
                        onClick={handleBackClick}
                        className="flex items-center px-4 py-2 bg-blue-100 hover:bg-blue-200 rounded-md text-sm font-medium text-blue-700 transition-colors cursor-pointer"
                    >
                        <ArrowLeft className="w-4 h-4 mr-2" />
                        Volver a grupos
                    </button>
                </div>
            )}

            <div className="bg-white rounded-lg overflow-hidden shadow-sm border border-gray-200">
                <table className="w-full">
                    <thead className="bg-gray-100">
                        <tr>
                            <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700 border-r border-gray-300">
                                {selectedGroup ? 'Empleado' : 'Grupo'}
                            </th>
                            {selectedGroup ? (
                                // Mostrar días de la semana cuando se selecciona un grupo
                                getDayNames().map((dayName, index) => (
                                    <th key={index} className="px-4 py-3 text-center text-sm font-semibold text-gray-700 border-r border-gray-300 last:border-r-0">
                                        {dayName}
                                    </th>
                                ))
                            ) : (
                                // Mostrar fechas cuando se ven todos los grupos
                                getWeekDays().map((date, index) => (
                                    <th key={index} className="px-4 py-3 text-center text-sm font-semibold text-gray-700 border-r border-gray-300 last:border-r-0">
                                        {date.getDate()}-{String(date.getMonth() + 1).padStart(2, '0')}-{date.getFullYear()}
                                    </th>
                                ))
                            )}
                        </tr>
                    </thead>
                    <tbody className="bg-white">
                        {selectedGroup ? (
                            // Vista detallada del grupo seleccionado
                            selectedGroupData?.employees.map((employee, employeeIndex) => (
                                <tr key={employee.id} className={employeeIndex % 2 === 0 ? 'bg-white' : 'bg-gray-50'}>
                                    <td className="px-4 py-3 text-left text-sm border-r border-gray-300 bg-gray-50">
                                        <div>
                                            <div className="font-medium text-gray-900">{employee.id}</div>
                                            <div className="text-xs text-gray-600">{employee.name}</div>
                                        </div>
                                    </td>
                                    {employee.weekData.map((value, dayIndex) => (
                                        <td key={dayIndex} className="px-4 py-3 text-center text-sm border-r border-gray-300 last:border-r-0 text-gray-900 font-medium">
                                            {value}
                                        </td>
                                    ))}
                                </tr>
                            ))
                        ) : (
                            // Vista de grupos con empleados
                            selectedGroups.length > 0 ? (
                                selectedGroups.map((groupId) => {
                                    // Etiqueta real del grupo
                                    const g = currentAreaGroups?.find(ag => ag.grupoId.toString() === groupId);
                                    //const label = g ? `${g.rol.split('_')[0]} ${(currentAreaGroups || []).findIndex(ag => ag.grupoId.toString() === groupId) + 1}` : `Grupo ${groupId}`;
                                    const label = g ? `${g.rol} ${(currentAreaGroups || []).findIndex(ag => ag.grupoId.toString() === groupId) + 1}` : `Grupo ${groupId}`;

                                    return (
                                        <React.Fragment key={groupId}>
                                            {/* Fila del grupo (clickeable) */}
                                            <tr
                                                className="bg-blue-50 hover:bg-blue-100 cursor-pointer transition-colors"
                                                onClick={() => handleGroupClick(groupId)}
                                            >
                                                <td className="px-4 py-3 text-left text-sm font-bold text-blue-800 border-r border-gray-300 bg-blue-100">
                                                    {label}
                                                </td>
                                                {getWeekDays().map((date, dayIndex) => {
                                                    const dayData = calendarData?.days.find(d => d.day === date.getDate());
                                                    const percentage = (() => {
                                                        const m = dayData?.manning.find(m => m.group === groupId);
                                                        return m?.percentage ?? dayData?.averagePercentage ?? 0;
                                                    })();

                                                    const isAlert = percentage > 4.5;

                                                    return (
                                                        <td key={dayIndex} className={`px-4 py-3 text-center text-sm border-r border-gray-300 last:border-r-0 ${isAlert
                                                            ? 'bg-red-500 text-white font-semibold'
                                                            : 'text-gray-900'
                                                            }`}>
                                                            {percentage.toFixed(1)}%
                                                        </td>
                                                    );
                                                })}
                                            </tr>
                                            {/* Filas de empleados del grupo */}
                                            {/** TODO: Integrar empleados reales por grupo */}
                                            {(groupsData.find(gd => gd.id === 'grupo1')?.employees || []).map((employee, employeeIndex) => (
                                                <tr key={employee.id} className={employeeIndex % 2 === 0 ? 'bg-white' : 'bg-gray-50'}>
                                                    <td className="px-4 py-3 text-left text-sm border-r border-gray-300 pl-8">
                                                        <div>
                                                            <div className="font-medium text-gray-900">{employee.id}</div>
                                                            <div className="text-xs text-gray-600">{employee.name}</div>
                                                        </div>
                                                    </td>
                                                    {employee.weekData.map((value, dayIndex) => (
                                                        <td key={dayIndex} className="px-4 py-3 text-center text-sm border-r border-gray-300 last:border-r-0 text-gray-700">
                                                            {value}
                                                        </td>
                                                    ))}
                                                </tr>
                                            ))}
                                        </React.Fragment>
                                    );
                                })
                            ) : (
                                <tr>
                                    <td colSpan={8} className="px-4 py-8 text-center text-sm text-gray-500">
                                        Selecciona uno o más grupos para ver los datos en la tabla
                                    </td>
                                </tr>
                            )
                        )}
                    </tbody>
                </table>
            </div>

            {/* Navegación por semanas */}
            <div className="flex justify-between items-center mt-4">
                <button
                    onClick={() => onNavigateWeek('prev')}
                    className="px-4 py-2 bg-gray-100 hover:bg-gray-200 rounded-md text-sm font-medium text-gray-700 transition-colors cursor-pointer"
                >
                    ← Semana anterior
                </button>
                <span className="text-sm font-medium text-gray-700">
                    {(() => {
                        const startDate = new Date(currentDate.getFullYear(), currentDate.getMonth(), 1 + weekOffset);
                        const endDate = new Date(currentDate.getFullYear(), currentDate.getMonth(), 7 + weekOffset);
                        return `${startDate.getDate()}-${String(startDate.getMonth() + 1).padStart(2, '0')} al ${endDate.getDate()}-${String(endDate.getMonth() + 1).padStart(2, '0')} ${endDate.getFullYear()}`;
                    })()}
                </span>
                <button
                    onClick={() => onNavigateWeek('next')}
                    className="px-4 py-2 bg-gray-100 hover:bg-gray-200 rounded-md text-sm font-medium text-gray-700 transition-colors cursor-pointer"
                >
                    Semana siguiente →
                </button>
            </div>
        </div>
    );
};

export default TableViewGroup;