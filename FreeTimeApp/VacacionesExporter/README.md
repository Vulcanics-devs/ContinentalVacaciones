# Exportador de Vacaciones Programadas

Herramienta de línea de comandos para exportar las vacaciones programadas del sistema Continental a un archivo Excel (.xlsx).

## Características

- 📊 Exporta todas las vacaciones programadas de la base de datos
- 📅 Filtro opcional por año
- 👥 Incluye información completa del empleado (nómina, nombre, área, grupo)
- 📈 Genera resumen estadístico de la exportación
- 🎨 Formato Excel profesional con encabezados y filtros
- ❄️ Congela la primera fila para facilitar navegación

## Requisitos

- .NET 9.0 SDK
- Acceso a la base de datos SQL Server "Vacaciones"
- Las dependencias se instalan automáticamente al compilar

## Configuración

Edita el archivo `appsettings.json` para configurar la conexión a la base de datos:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Vacaciones;User Id=sa;Password=TuPassword;TrustServerCertificate=True"
  }
}
```

## Uso

### Exportar todas las vacaciones

```bash
cd FreeTimeApp/VacacionesExporter
dotnet run
```

Esto generará un archivo con el formato: `VacacionesProgramadas_Todas_YYYYMMDD_HHMMSS.xlsx`

### Exportar vacaciones de un año específico

```bash
dotnet run 2026
```

Esto generará un archivo con el formato: `VacacionesProgramadas_2026_YYYYMMDD_HHMMSS.xlsx`

### Ejemplos

```bash
# Exportar vacaciones del 2025
dotnet run 2025

# Exportar vacaciones del 2026
dotnet run 2026

# Exportar todas las vacaciones (sin filtro de año)
dotnet run
```

## Salida del Archivo Excel

El archivo Excel generado incluye las siguientes columnas:

| Columna | Descripción |
|---------|-------------|
| ID | ID único de la vacación |
| Nómina | Número de nómina del empleado |
| Nombre Completo | Nombre completo del empleado |
| Área | Área de trabajo del empleado |
| Grupo | Grupo de trabajo del empleado |
| Fecha Vacación | Fecha de la vacación programada |
| Tipo Vacación | Tipo: Anual, Reprogramacion, AsignadaAutomaticamente |
| Origen Asignación | Manual o Automatica |
| Estado Vacación | Activa, Intercambiada, Cancelada |
| Periodo Programación | ProgramacionAnual o Reprogramacion |
| Fecha Programación | Fecha en que se programó |
| Puede ser Intercambiada | Sí/No |
| Observaciones | Notas adicionales |
| Fecha Creación | Fecha de creación del registro |
| Creado Por | ID del usuario que creó |
| Última Actualización | Fecha de última modificación |
| Actualizado Por | ID del usuario que actualizó |

## Resumen Estadístico

Al finalizar la exportación, el programa muestra un resumen que incluye:

- Total de registros exportados
- Número de empleados únicos
- Desglose por tipo de vacación
- Desglose por origen de asignación
- Desglose por estado

Ejemplo de salida:

```
📋 RESUMEN DE EXPORTACIÓN
═══════════════════════════════════════
Total de registros:     2543
Empleados únicos:       847

Por Tipo de Vacación:
  • Anual                        : 1890
  • AsignadaAutomaticamente      :  653

Por Origen de Asignación:
  • Automatica                   :  653
  • Manual                       : 1890

Por Estado:
  • Activa                       : 2543

═══════════════════════════════════════
📁 Ruta completa: C:\...\VacacionesProgramadas_2026_20251013_155823.xlsx

✓ Exportación completada exitosamente!
```

## Solución de Problemas

### Error de conexión a base de datos

Si obtienes un error de conexión, verifica:
1. Que SQL Server esté ejecutándose
2. Que las credenciales en `appsettings.json` sean correctas
3. Que el servidor y nombre de base de datos sean correctos
4. Que `TrustServerCertificate=True` esté incluido en la cadena de conexión

### No se encuentra el archivo appsettings.json

Asegúrate de ejecutar el comando desde el directorio correcto:
```bash
cd FreeTimeApp/VacacionesExporter
dotnet run
```

### Error al generar Excel

Si el archivo Excel no se puede generar, verifica que:
1. Tengas permisos de escritura en el directorio actual
2. No haya otro proceso usando un archivo con el mismo nombre

## Compilar como ejecutable

Para crear un ejecutable standalone:

```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained

# Linux
dotnet publish -c Release -r linux-x64 --self-contained

# macOS
dotnet publish -c Release -r osx-x64 --self-contained
```

El ejecutable estará en: `bin/Release/net9.0/{runtime}/publish/`

## Dependencias

- **ClosedXML** (0.105.0): Generación de archivos Excel
- **Microsoft.EntityFrameworkCore.SqlServer** (9.0.9): Acceso a SQL Server
- **Microsoft.Extensions.Configuration** (9.0.9): Manejo de configuración
- **Microsoft.Extensions.Configuration.Json** (9.0.9): Soporte para JSON

## Autor

Sistema de Gestión de Vacaciones - Continental

## Licencia

Uso interno de Continental AG - Todos los derechos reservados
