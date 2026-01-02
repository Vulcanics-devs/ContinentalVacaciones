import { type JSX } from 'react'
import { Routes, Route } from 'react-router-dom'

import EmployeeHome from './EmployeeHome'
import RequestVacations from './RequestVacations'
import MyVacations from './MyVacations'
import MyRequests from './MyRequests'
import { PeriodOptions } from '@/interfaces/Calendar.interface'
import { Plantilla } from './Plantilla'
import { useVacationConfig } from '@/hooks/useVacationConfig'
import WeeklyRoles from './WeeklyRoles'
import MisPermutas from './MisPermutas';


const EmployeeDashboard = (): JSX.Element => {
    const { currentPeriod, loading, error } = useVacationConfig();

    // Mostrar loading mientras se obtiene la configuración
    if (loading) {
        return (
            <div className="flex flex-col min-h-screen w-full">
                <div className="flex-1 bg-gray-100 min-h-screen h-full flex items-center justify-center">
                    <div className="text-center">
                        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
                        <p className="text-gray-600">Cargando configuración...</p>
                    </div>
                </div>
            </div>
        );
    }

    // Mostrar error si hay problemas al cargar la configuración
    if (error) {
        return (
            <div className="flex flex-col min-h-screen w-full">
                <div className="flex-1 bg-gray-100 min-h-screen h-full flex items-center justify-center">
                    <div className="text-center">
                        <div className="text-red-500 mb-4">
                            <svg className="w-16 h-16 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                            </svg>
                        </div>
                        <h3 className="text-lg font-semibold text-gray-900 mb-2">Error al cargar configuración</h3>
                        <p className="text-gray-600 mb-4">{error}</p>
                        <button
                            onClick={() => window.location.reload()}
                            className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
                        >
                            Reintentar
                        </button>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="flex flex-col min-h-screen w-full">

            {/* Main Content */}
            <div className="flex-1 bg-gray-100 min-h-screen h-full">
                <Routes>
                    <Route index element={<EmployeeHome currentPeriod={currentPeriod} />} />

                    {/* Rutas condicionales basadas en el período actual */}
                    {currentPeriod === PeriodOptions.annual && (
                        <Route path="solicitar-vacaciones" element={<RequestVacations />} />
                    )}

                    {currentPeriod === PeriodOptions.reprogramming && (
                        <Route path="mis-solicitudes" element={<MyRequests />} />
                    )}

                    {/* Rutas disponibles en todos los períodos */}
                    <Route path="plantilla" element={<Plantilla />} />
                    <Route path="mis-vacaciones" element={<MyVacations currentPeriod={currentPeriod} />} />
                    <Route path="roles-semanales" element={<WeeklyRoles />} />
                    <Route path="mis-permutas" element={<MisPermutas />} />
                </Routes>
            </div>
        </div>
    )
}

export default EmployeeDashboard
