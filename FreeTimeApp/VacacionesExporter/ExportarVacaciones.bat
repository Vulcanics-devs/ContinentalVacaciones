@echo off
REM ========================================================
REM Exportador de Vacaciones Programadas - Continental
REM ========================================================

echo.
echo ╔════════════════════════════════════════════════════╗
echo ║  Exportador de Vacaciones Programadas             ║
echo ║  Continental Vacation Management System           ║
echo ╚════════════════════════════════════════════════════╝
echo.

REM Verificar que estamos en el directorio correcto
if not exist "VacacionesExporter.csproj" (
    echo ERROR: Este script debe ejecutarse desde el directorio VacacionesExporter
    echo Por favor, navega a: FreeTimeApp\VacacionesExporter\
    pause
    exit /b 1
)

REM Verificar si se pasó un año como argumento
if "%1"=="" (
    echo Ejecutando exportación de TODAS las vacaciones...
    echo.
    dotnet run
) else (
    echo Ejecutando exportación de vacaciones del año %1...
    echo.
    dotnet run %1
)

echo.
if errorlevel 1 (
    echo ════════════════════════════════════════════════════
    echo ERROR: La exportación falló. Revise los mensajes arriba.
    echo ════════════════════════════════════════════════════
) else (
    echo ════════════════════════════════════════════════════
    echo Exportación completada con éxito!
    echo ════════════════════════════════════════════════════
)

echo.
pause
