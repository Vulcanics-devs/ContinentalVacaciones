-- Clear all schedule-related data to start fresh

-- Delete dependent data first (to avoid foreign key constraints)
IF OBJECT_ID('BloquesReservacion', 'U') IS NOT NULL DELETE FROM BloquesReservacion;
IF OBJECT_ID('EmpleadosXBloquesDeTurnos', 'U') IS NOT NULL DELETE FROM EmpleadosXBloquesDeTurnos;
IF OBJECT_ID('CalendarioEmpleados', 'U') IS NOT NULL DELETE FROM CalendarioEmpleados;
IF OBJECT_ID('CalendarioPorEmpleadoOriginal', 'U') IS NOT NULL DELETE FROM CalendarioPorEmpleadoOriginal;
IF OBJECT_ID('VacacionesAplicadasPorEmpleado', 'U') IS NOT NULL DELETE FROM VacacionesAplicadasPorEmpleado;
IF OBJECT_ID('DiasFestivosTrabajados', 'U') IS NOT NULL DELETE FROM DiasFestivosTrabajados;
IF OBJECT_ID('IntercambiosDiaFestivoPorDescanso', 'U') IS NOT NULL DELETE FROM IntercambiosDiaFestivoPorDescanso;
IF OBJECT_ID('SolicitudesIntercambiosOReprogramacion', 'U') IS NOT NULL DELETE FROM SolicitudesIntercambiosOReprogramacion;
IF OBJECT_ID('SolicitudesFestivosTrabajados', 'U') IS NOT NULL DELETE FROM SolicitudesFestivosTrabajados;
IF OBJECT_ID('RolInicialPorEmpleado', 'U') IS NOT NULL DELETE FROM RolInicialPorEmpleado;
IF OBJECT_ID('Ausencias', 'U') IS NOT NULL DELETE FROM Ausencias;
IF OBJECT_ID('Notificaciones', 'U') IS NOT NULL DELETE FROM Notificaciones;

-- Finally delete the ProgramacionAnual
DELETE FROM ProgramacionesAnuales;

-- Verify deletion
SELECT 'ProgramacionesAnuales' AS TableName, COUNT(*) AS Count FROM ProgramacionesAnuales
UNION ALL SELECT 'BloquesReservacion', COUNT(*) FROM BloquesReservacion
UNION ALL SELECT 'CalendarioEmpleados', COUNT(*) FROM CalendarioEmpleados
UNION ALL SELECT 'EmpleadosXBloquesDeTurnos', COUNT(*) FROM EmpleadosXBloquesDeTurnos;

PRINT 'Schedule data cleared successfully!';
