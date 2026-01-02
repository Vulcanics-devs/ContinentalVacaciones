import React from 'react';
import CalendarWidget from '../Dashboard-Area/CalendarWidget';

// Ejemplos de cómo usar CalendarWidget en diferentes vistas

// 1. Solo calendario sin tabs ni sidebar
export const SimpleCalendarView: React.FC = () => {
    return (
        <CalendarWidget 
            showTabs={false}
            defaultView="calendar"
            showHeader={false}
            showSidebar={false}
            className="bg-white rounded-lg shadow-md"
        />
    );
};

// 2. Solo tabla sin header
export const TableOnlyView: React.FC = () => {
    return (
        <CalendarWidget 
            showTabs={false}
            defaultView="table"
            showHeader={false}
            showSidebar={true}
            className="bg-gray-50"
        />
    );
};

// 3. Vista completa con todos los elementos (igual a la original)
export const FullCalendarView: React.FC = () => {
    return (
        <CalendarWidget 
            showTabs={true}
            defaultView="calendar"
            showHeader={true}
            showSidebar={true}
        />
    );
};

// 4. Vista minimalista solo con calendario y navegación
export const MinimalCalendarView: React.FC = () => {
    return (
        <div className="p-4">
            <h2 className="text-xl font-bold mb-4">Calendario de Manning</h2>
            <CalendarWidget 
                showTabs={false}
                defaultView="calendar"
                showHeader={true}
                showSidebar={false}
                className="border border-gray-200 rounded-lg"
            />
        </div>
    );
};

// 5. Dashboard con múltiples widgets
export const DashboardView: React.FC = () => {
    return (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 p-6">
            <div className="bg-white rounded-lg shadow-md p-4">
                <h3 className="text-lg font-semibold mb-4">Vista Rápida - Calendario</h3>
                <CalendarWidget 
                    showTabs={false}
                    defaultView="calendar"
                    showHeader={false}
                    showSidebar={false}
                />
            </div>
            
            <div className="bg-white rounded-lg shadow-md p-4">
                <h3 className="text-lg font-semibold mb-4">Vista Rápida - Tabla</h3>
                <CalendarWidget 
                    showTabs={false}
                    defaultView="table"
                    showHeader={false}
                    showSidebar={false}
                />
            </div>
        </div>
    );
};
