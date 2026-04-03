import React, { useEffect, useState } from 'react';
import {
    PieChart, Pie, Cell, Tooltip, Legend, ResponsiveContainer,
    BarChart, Bar, XAxis, YAxis, CartesianGrid
} from 'recharts';
import { ausenciasService } from '@/services/ausenciasService';
import { httpClient } from '@/services/httpClient';
import type { AusenciasPorGrupo } from '@/interfaces/Ausencias.interface';

const MESES = ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'];

const COLORES_MOTIVOS: Record<string, string> = {
    Anual: '#22c55e',
    Reprogramacion: '#3b82f6',
    FestivoTrabajado: '#f59e0b',
    Permiso: '#a855f7',
    Incapacidad: '#ef4444',
    Vacacion: '#22c55e',
};

const DEFAULT_COLOR = '#6b7280';

type AnualRow = { mes: number; vacacion: number; reprogramacion: number; festivoTrabajado: number; permiso: number; incapacidad: number };
type SemanalRow = { semana: number; vacacion: number; reprogramacion: number; festivoTrabajado: number; permiso: number; incapacidad: number };
type MotivoRow = { motivo: string; total: number };

export const Dashboard: React.FC = () => {
    const [grupos, setGrupos] = useState<AusenciasPorGrupo[]>([]);
    const [anualData, setAnualData] = useState<AnualRow[]>([]);
    const [semanalData, setSemanalData] = useState<SemanalRow[]>([]);
    const [motivosData, setMotivosData] = useState<MotivoRow[]>([]);
    const [anioSel, setAnioSel] = useState(new Date().getFullYear());
    const [loading, setLoading] = useState(true);

    // Cargar gráfica de pastel (hoy)
    useEffect(() => {
        const fetchHoy = async () => {
            try {
                const today = new Date();
                const data = await ausenciasService.calcularAusenciasParaCalendario({ fechaInicio: today, view: 'daily' });
                const todayStr = today.toISOString().split('T')[0];
                setGrupos(data.find(d => d.fecha === todayStr)?.ausenciasPorGrupo ?? []);
            } catch (e) { console.error(e); }
        };
        fetchHoy();
    }, []);

    // Cargar datos históricos
    useEffect(() => {
        const fetchHistorico = async () => {
            setLoading(true);
            try {
                const [anual, semanal, motivos] = await Promise.all([
                    httpClient.get<any>(`/api/dashboard/ausencias-anuales?anio=${anioSel}`),
                    httpClient.get<any>(`/api/dashboard/ausencias-semanales`),
                    httpClient.get<any>(`/api/dashboard/ausencias-motivos`),
                ]);
                setAnualData(anual?.data ?? anual ?? []);
                setSemanalData(semanal?.data ?? semanal ?? []);
                setMotivosData(motivos?.data ?? motivos ?? []);
            } catch (e) { console.error(e); }
            finally { setLoading(false); }
        };
        fetchHistorico();
    }, [anioSel]);

    const totales = grupos.reduce(
        (acc, g) => ({ disponible: acc.disponible + g.personalDisponible, ausente: acc.ausente + g.personalNoDisponible }),
        { disponible: 0, ausente: 0 }
    );
    const pieData = [
        { name: 'Disponible', value: totales.disponible },
        { name: 'Ausente', value: totales.ausente },
    ];
    const PIE_COLORS = ['#22c55e', '#ef4444'];

    const anualForChart = anualData.map(r => ({
        name: MESES[r.mes - 1],
        Vacación: r.vacacion,
        Reprogramación: r.reprogramacion,
        'Festivo Trab.': r.festivoTrabajado,
        Permiso: r.permiso,
        Incapacidad: r.incapacidad,
    }));

    const semanalForChart = semanalData.map(r => ({
        name: `Sem ${r.semana}`,
        Vacación: r.vacacion,
        Reprogramación: r.reprogramacion,
        'Festivo Trab.': r.festivoTrabajado,
        Permiso: r.permiso,
        Incapacidad: r.incapacidad,
    }));

    const motivosPie = motivosData.map(m => ({ name: m.motivo, value: m.total }));

    const [grupoExpandido, setGrupoExpandido] = useState<number | null>(null);

    return (
        <div className="p-8 space-y-8">
            <div className="flex items-center justify-between">
                <h1 className="text-2xl font-bold text-gray-900">
                    Dashboard — {new Date().toLocaleDateString('es-ES', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })}
                </h1>
                <div className="flex items-center gap-2 text-sm">
                    <label className="text-gray-600 font-medium">Año:</label>
                    <select
                        value={anioSel}
                        onChange={e => setAnioSel(Number(e.target.value))}
                        className="border border-gray-300 rounded px-2 py-1 text-sm"
                    >
                        {[0, 1, 2].map(i => {
                            const y = new Date().getFullYear() - i;
                            return <option key={y} value={y}>{y}</option>;
                        })}
                    </select>
                </div>
            </div>

            {/* Fila 1 — Personal hoy + Motivos hoy */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="bg-white border border-gray-200 rounded-lg p-6">
                    <h2 className="text-base font-semibold text-gray-700 mb-4">Personal hoy (total)</h2>
                    <ResponsiveContainer width="100%" height={260}>
                        <PieChart>
                            <Pie data={pieData} dataKey="value" nameKey="name" cx="50%" cy="50%" outerRadius={90}
                                label={({ name, value }) => `${name}: ${value}`}>
                                {pieData.map((_, i) => <Cell key={i} fill={PIE_COLORS[i]} />)}
                            </Pie>
                            <Tooltip /><Legend />
                        </PieChart>
                    </ResponsiveContainer>
                </div>

                <div className="bg-white border border-gray-200 rounded-lg p-6">
                    <h2 className="text-base font-semibold text-gray-700 mb-4">Motivos de ausencia — hoy</h2>
                    {motivosPie.length === 0 ? (
                        <div className="flex items-center justify-center h-[260px] text-gray-400 text-sm">Sin ausencias hoy</div>
                    ) : (
                        <ResponsiveContainer width="100%" height={260}>
                            <PieChart>
                                <Pie data={motivosPie} dataKey="value" nameKey="name" cx="50%" cy="50%" outerRadius={90}
                                    label={({ name, value }) => `${name}: ${value}`}>
                                    {motivosPie.map((entry, i) => (
                                        <Cell key={i} fill={COLORES_MOTIVOS[entry.name] ?? DEFAULT_COLOR} />
                                    ))}
                                </Pie>
                                <Tooltip /><Legend />
                            </PieChart>
                        </ResponsiveContainer>
                    )}
                </div>
            </div>

            {/* Fila 2 — Anual */}
            <div className="bg-white border border-gray-200 rounded-lg p-6">
                <h2 className="text-base font-semibold text-gray-700 mb-4">Ausencias por mes — {anioSel}</h2>
                {loading ? (
                    <div className="flex items-center justify-center h-[280px] text-gray-400 text-sm">Cargando...</div>
                ) : (
                    <ResponsiveContainer width="100%" height={280}>
                        <BarChart data={anualForChart}>
                            <CartesianGrid strokeDasharray="3 3" />
                            <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                            <YAxis allowDecimals={false} tick={{ fontSize: 12 }} />
                            <Tooltip />
                            <Legend />
                            <Bar dataKey="Vacación" fill="#22c55e" />
                            <Bar dataKey="Reprogramación" fill="#3b82f6" />
                            <Bar dataKey="Festivo Trab." fill="#f59e0b" />
                            <Bar dataKey="Permiso" fill="#a855f7" />
                            <Bar dataKey="Incapacidad" fill="#ef4444" />
                        </BarChart>
                    </ResponsiveContainer>
                )}
            </div>

            {/* Fila 3 — Semanal + Tabla resumen */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="bg-white border border-gray-200 rounded-lg p-6">
                    <h2 className="text-base font-semibold text-gray-700 mb-4">
                        Ausencias por semana — {new Date().toLocaleDateString('es-ES', { month: 'long', year: 'numeric' })}
                    </h2>
                    {loading ? (
                        <div className="flex items-center justify-center h-[260px] text-gray-400 text-sm">Cargando...</div>
                    ) : (
                        <ResponsiveContainer width="100%" height={260}>
                            <BarChart data={semanalForChart}>
                                <CartesianGrid strokeDasharray="3 3" />
                                <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                                <YAxis allowDecimals={false} tick={{ fontSize: 12 }} />
                                <Tooltip />
                                <Legend />
                                <Bar dataKey="Vacación" fill="#22c55e" />
                                <Bar dataKey="Reprogramación" fill="#3b82f6" />
                                <Bar dataKey="Festivo Trab." fill="#f59e0b" />
                                <Bar dataKey="Permiso" fill="#a855f7" />
                                <Bar dataKey="Incapacidad" fill="#ef4444" />
                            </BarChart>
                        </ResponsiveContainer>
                    )}
                </div>

                <div className="bg-white border border-gray-200 rounded-lg p-6">
                    <h2 className="text-base font-semibold text-gray-700 mb-4">Resumen por grupo — hoy</h2>
                    <table className="w-full text-sm">
                        <thead>
                            <tr className="bg-gray-50 text-gray-600">
                                <th className="text-left px-3 py-2">Grupo</th>
                                <th className="text-center px-3 py-2">Total</th>
                                <th className="text-center px-3 py-2">Disponible</th>
                                <th className="text-center px-3 py-2">Ausente</th>
                                <th className="text-center px-3 py-2">%</th>
                            </tr>
                        </thead>
                        <tbody>
                            {grupos.map(g => (
                                <React.Fragment key={g.grupoId}>
                                    {/* Fila del grupo — clickeable */}
                                    <tr
                                        className={`border-t cursor-pointer hover:bg-gray-50 transition-colors ${g.excedeLimite ? 'bg-red-50' : ''
                                            } ${grupoExpandido === g.grupoId ? 'bg-blue-50' : ''}`}
                                        onClick={() => setGrupoExpandido(prev => prev === g.grupoId ? null : g.grupoId)}
                                    >
                                        <td className="px-3 py-2 font-medium flex items-center gap-2">
                                            <span className="text-gray-400 text-xs">
                                                {grupoExpandido === g.grupoId ? '▼' : '▶'}
                                            </span>
                                            {g.nombreGrupo}
                                        </td>
                                        <td className="px-3 py-2 text-center">{g.personalTotal}</td>
                                        <td className="px-3 py-2 text-center text-green-600">{g.personalDisponible}</td>
                                        <td className="px-3 py-2 text-center text-red-600">{g.personalNoDisponible}</td>
                                        <td className={`px-3 py-2 text-center font-semibold ${g.excedeLimite ? 'text-red-600' : 'text-gray-700'
                                            }`}>
                                            {g.porcentajeAusencia.toFixed(1)}%
                                            {g.excedeLimite && <span className="ml-1">⚠️</span>}
                                        </td>
                                    </tr>

                                    {/* Fila expandida — empleados ausentes */}
                                    {grupoExpandido === g.grupoId && (
                                        <tr>
                                            <td colSpan={5} className="px-4 py-3 bg-gray-50 border-b">
                                                {g.empleadosAusentes && g.empleadosAusentes.length > 0 ? (
                                                    <div className="space-y-1">
                                                        <p className="text-xs font-semibold text-gray-500 mb-2 uppercase tracking-wide">
                                                            Empleados ausentes
                                                        </p>
                                                        <table className="w-full text-xs">
                                                            <thead>
                                                                <tr className="text-gray-500">
                                                                    <th className="text-left py-1 pr-4">Nombre</th>
                                                                    <th className="text-left py-1 pr-4">Nómina</th>
                                                                    <th className="text-left py-1 pr-4">Motivo</th>
                                                                    <th className="text-left py-1">Tipo</th>
                                                                </tr>
                                                            </thead>
                                                            <tbody>
                                                                {g.empleadosAusentes.map(emp => (
                                                                    <tr key={emp.empleadoId} className="border-t border-gray-200">
                                                                        <td className="py-1 pr-4 font-medium text-gray-800">
                                                                            {emp.nombreCompleto}
                                                                        </td>
                                                                        <td className="py-1 pr-4 text-gray-600">
                                                                            {emp.nomina ?? '—'}
                                                                        </td>
                                                                        <td className="py-1 pr-4">
                                                                            <span className={`px-2 py-0.5 rounded text-xs font-medium ${emp.tipoAusencia === 'Vacacion'
                                                                                    ? 'bg-green-100 text-green-700'
                                                                                    : emp.tipoAusencia === 'Reprogramacion'
                                                                                        ? 'bg-blue-100 text-blue-700'
                                                                                        : emp.tipoAusencia === 'Incapacidad'
                                                                                            ? 'bg-red-100 text-red-700'
                                                                                            : emp.tipoAusencia === 'Permiso'
                                                                                                ? 'bg-purple-100 text-purple-700'
                                                                                                : emp.tipoAusencia === 'Festivo Trabajado'
                                                                                                    ? 'bg-yellow-100 text-yellow-700'
                                                                                                    : 'bg-gray-100 text-gray-700'
                                                                                }`}>
                                                                                {emp.tipoAusencia}
                                                                            </span>
                                                                        </td>
                                                                        <td className="py-1 text-gray-500">
                                                                            {emp.tipoVacacion ?? '—'}
                                                                        </td>
                                                                    </tr>
                                                                ))}
                                                            </tbody>
                                                        </table>
                                                    </div>
                                                ) : (
                                                    <p className="text-xs text-gray-400">No hay empleados ausentes en este grupo</p>
                                                )}

                                                {/* Empleados disponibles (cantidad) */}
                                                {g.empleadosDisponibles && g.empleadosDisponibles.length > 0 && (
                                                    <div className="mt-3 pt-3 border-t border-gray-200">
                                                        <p className="text-xs font-semibold text-gray-500 mb-1 uppercase tracking-wide">
                                                            Empleados disponibles ({g.personalDisponible})
                                                        </p>
                                                        <div className="flex flex-wrap gap-1">
                                                            {g.empleadosDisponibles.map(emp => (
                                                                <span key={emp.empleadoId}
                                                                    className="text-xs bg-green-50 text-green-700 border border-green-200 px-2 py-0.5 rounded">
                                                                    {emp.nombreCompleto}
                                                                </span>
                                                            ))}
                                                        </div>
                                                    </div>
                                                )}
                                            </td>
                                        </tr>
                                    )}
                                </React.Fragment>
                            ))}
                            {grupos.length === 0 && (
                                <tr>
                                    <td colSpan={5} className="px-3 py-6 text-center text-gray-400">Sin datos</td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    );
};