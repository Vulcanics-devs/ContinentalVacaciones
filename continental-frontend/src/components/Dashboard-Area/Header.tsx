/**
 * =============================================================================
 * HEADER
 * =============================================================================
 * 
 * @description
 * Header del calendario que contiene los controles de filtrado por grupos,
 * selección de área y botón de exportación. Proporciona la interfaz principal
 * para controlar qué información se muestra en el calendario.
 * 
 * @inputs (Props del componente)
 * - selectedGroups: string[] - IDs de los grupos actualmente seleccionados
 * - onToggleGroup: (groupId: string) => void - Callback para activar/desactivar grupo
 * - onExport: () => void - Callback para exportar datos del calendario
 * - areas?: Area[] - Lista de áreas disponibles para selección
 *   - id: string - Identificador único del área
 *   - name: string - Nombre descriptivo del área
 * - selectedArea?: string - Área actualmente seleccionada
 * - onAreaChange?: (areaId: string) => void - Callback cambio de área
 * 
 * @used_in (Componentes padre)
 * - src/components/Dashboard-Area/CalendarWidget.tsx
 * 
 * @user_roles (Usuarios que acceden)
 * - Jefe de Área
 * 
 * @dependencies
 * - React: Framework base
 * - ../../services/calendarService: Servicio para obtener grupos disponibles
 * 
 * @author Vulcanics Dev Team
 * @created 2024
 * @last_modified 2025-08-20
 * =============================================================================
 */

import React, { useMemo } from 'react';
import type { Grupo } from '@/interfaces/Areas.interface';
import Popover from '@/components/ui/popover';

interface Area {
    id: string;
    name: string;
}

interface HeaderProps {
    selectedGroups: string[];
    onToggleGroup: (groupId: string) => void;
    onExport: () => void;
    areas?: Area[]; // Lista de áreas disponibles
    selectedArea?: string; // Área seleccionada actual
    onAreaChange?: (areaId: string) => void; // Callback cuando cambia el área
    currentAreaGroups?: Grupo[]; // Grupos de la área actual
    bossName?: string; // Nombre del Jefe de Área (desde API)
    leadersByGroup?: Record<number, string>;
}

const Header: React.FC<HeaderProps> = ({
    currentAreaGroups,
    selectedGroups,
    onToggleGroup,
    onExport,
    areas,
    selectedArea,
    onAreaChange,
    bossName,
    leadersByGroup,
}) => {
    const currentArea = useMemo(() => {
        return areas?.find((a) => a.id === selectedArea);
    }, [areas, selectedArea]);

    return (
        <div className="calendar-header">
            <div className="flex justify-between items-start">
                <div className="flex flex-col gap-4">
                    <div>
                        <span className="text-sm text-gray-500 block">Área</span>
                        {areas && areas.length && onAreaChange ? (
                            <select
                                value={selectedArea}
                                onChange={(e) => onAreaChange(e.target.value)}
                                className="font-semibold text-gray-900 bg-transparent border-none outline-none cursor-pointer hover:text-blue-600 transition-colors"
                            >
                                {areas.map((area) => (
                                    <option key={area.id} value={area.id}>
                                        {area.name}
                                    </option>
                                ))}
                            </select>
                        ) : (
                            <div className="font-semibold text-gray-900">{currentArea?.name ?? '—'}</div>
                        )}
                    </div>

                    <div>
                        <span className="text-sm text-gray-500 block">Grupo</span>
                        <div className="flex gap-2 mt-1">
                            {currentAreaGroups?.map((group) => {
                                const isSelected = selectedGroups.includes(group.grupoId.toString());
                                const leaderName = leadersByGroup?.[group.grupoId];
                                //const label = `${group.rol.split('_')[0]} ${index + 1}`;
                                const label = `${group.rol}`;

                                const firstName = leaderName?.split(' ')[0];
                                return (
                                    <Popover
                                        key={group.grupoId}
                                        placement="top"
                                        offset={8}
                                        triggerMode="hover"
                                        content={
                                            <div className="space-y-1">
                                                <div className="font-medium text-gray-900">{label}</div>
                                                {leaderName && (
                                                    <div className="text-xs text-gray-600">
                                                        <span className="font-semibold">Líder:</span> {leaderName}
                                                    </div>
                                                )}
                                            </div>
                                        }
                                        trigger={
                                            <button
                                                onClick={() => onToggleGroup(group.grupoId.toString())}
                                                className={`px-3 py-1 rounded text-sm font-medium transition-all cursor-pointer ${
                                                    isSelected ? 'group-tab-selected' : 'group-tab'
                                                }`}
                                            >
                                                <span className="inline-flex items-center gap-2 truncate">
                                                    <span className="truncate">{label}</span>
                                                    {firstName && (
                                                        <span className="text-xs text-gray-800/80 truncate">— {firstName}</span>
                                                    )}
                                                </span>
                                            </button>
                                        }
                                    />
                                );
                            })}
                        </div>
                    </div>

                    <div>
                        <span className="text-sm text-gray-500 block">Jefe de Área</span>
                        <div className="font-semibold text-gray-900">{bossName || '—'}</div>
                    </div>
                </div>

                <button
                    onClick={onExport}
                    className="px-4 py-2 bg-gray-100 border-2 border-solid rounded-md text-sm font-medium transition-colors cursor-pointer"
                    style={{ color: '#9F6934', borderColor: '#9F6934' }}
                >
                    Descargar reporte
                </button>
            </div>
        </div>
    );
};

export default Header;
