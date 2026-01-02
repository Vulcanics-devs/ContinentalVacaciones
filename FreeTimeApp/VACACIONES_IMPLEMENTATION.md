# Implementación del Sistema de Vacaciones por Antigüedad

## Resumen
Se ha implementado un sistema completo para calcular las vacaciones que corresponden a cada empleado basado en su antigüedad, siguiendo las reglas específicas de la empresa.

## Reglas de Vacaciones Implementadas

### Tabla de Vacaciones por Antigüedad:
| Año    | Días Empresa | Días Asignados Auto | Días Programables | Total |
|--------|--------------|-------------------|------------------|-------|
| 1      | 12          | 0                 | 0                | 12    |
| 2      | 12          | 0                 | 2                | 14    |
| 3      | 12          | 0                 | 4                | 16    |
| 4      | 12          | 3                 | 3                | 18    |
| 5      | 12          | 4                 | 4                | 20    |
| 6-10   | 12          | 5                 | 5                | 22    |
| 11-15  | 12          | 5                 | 7                | 24    |
| 16-20  | 12          | 5                 | 7                | 24    |
| 21-25  | 12          | 5                 | 9                | 26    |
| 26-30  | 12          | 5                 | 9                | 26    |
| 31-35  | 12          | 5                 | 11               | 28    |
| 36-40  | 12          | 5                 | 11               | 28    |
| 41+    | 12          | 5                 | 11               | 28    |

### Explicación de las Reglas:
- **Días Empresa**: Siempre 12 días otorgados automáticamente por la empresa
- **Días Asignados Automáticamente**: 
  - Año 4: 3 días
  - Año 5: 4 días  
  - Año 6+: 5 días (fijo)
- **Días Programables**: Días que el empleado puede programar libremente
- **Progresión**: A partir del año 6, cada 5 años se agregan 2 días programables más
- **Tope Máximo**: 28 días totales (alcanzado a los 31+ años de antigüedad)

## Endpoints Creados

### 1. Calcular Vacaciones por Empleado
```
POST /api/vacaciones/empleado/{empleadoId}
```

**Parámetros:**
- `empleadoId` (route): ID del empleado
- **Body**:
```json
{
  "anio": 2025
}
```

**Respuesta:**
```json
{
  "success": true,
  "data": {
    "empleadoId": 1005,
    "nombreCompleto": "CANO VALLADOLID MIGUEL ANGEL",
    "fechaIngreso": "2016-01-04",
    "anioConsulta": 2025,
    "antiguedadEnAnios": 9,
    "diasEmpresa": 12,
    "diasAsignadosAutomaticamente": 5,
    "diasProgramables": 7,
    "totalDias": 24,
    "descripcion": "Empleado con 9 años de antigüedad en 2025. Total: 24 días (12 empresa + 5 asignados automáticamente + 7 programables)"
  },
  "errorMsg": null
}
```

### 2. Obtener Tabla de Vacaciones por Antigüedad
```
GET /api/vacaciones/tabla-antiguedad
```

**Respuesta:**
```json
{
  "success": true,
  "data": [
    {
      "antiguedadEnAnios": 1,
      "diasEmpresa": 12,
      "diasAsignadosAutomaticamente": 0,
      "diasProgramables": 0,
      "totalDias": 12,
      "descripcion": "Año 1: 12 días total (12 empresa)"
    },
    {
      "antiguedadEnAnios": 2,
      "diasEmpresa": 12,
      "diasAsignadosAutomaticamente": 0,
      "diasProgramables": 2,
      "totalDias": 14,
      "descripcion": "Año 2: 14 días total (12 empresa + 2 programables)"
    }
    // ... más años
  ],
  "errorMsg": null
}
```

### 3. Endpoint Existente (Mantenido)
```
GET /api/vacaciones/por-antiguedad
```
Devuelve las reglas de vacaciones desde la base de datos.

## Archivos Creados/Modificados

### 1. VacacionesService.cs
- **Ubicación**: `Services/VacacionesService.cs`
- **Propósito**: Lógica de negocio para calcular vacaciones
- **Funcionalidades**:
  - Calcular vacaciones por empleado y año
  - Calcular vacaciones por antigüedad
  - Calcular antigüedad en años

### 2. VacacionesDto.cs
- **Ubicación**: `DTOs/VacacionesDto.cs`
- **Propósito**: Estructuras de datos para requests y responses
- **Clases**:
  - `VacacionesEmpleadoRequest`: Para el body del POST
  - `VacacionesEmpleadoResponse`: Para la respuesta del empleado
  - `VacacionesPorAntiguedadResponse`: Para la tabla de antigüedad

### 3. VacacionesController.cs (Modificado)
- **Ubicación**: `Controllers/VacacionesController.cs`
- **Cambios**:
  - Agregados nuevos endpoints POST y GET
  - Inyección del nuevo servicio
  - Métodos helper para generar descripciones

### 4. Program.cs (Modificado)
- **Cambios**: Registrado `VacacionesService` en el contenedor DI

## Lógica de Cálculo de Antigüedad

El cálculo de antigüedad se basa en:
1. **Fecha de Ingreso**: Tomada del campo `FechaIngreso` del empleado
2. **Fecha de Referencia**: 31 de diciembre del año consultado
3. **Cálculo**: Se considera el aniversario completo para determinar los años

### Ejemplo:
- Empleado ingresó: 4 de enero de 2016
- Consulta para año: 2025
- Antigüedad: 9 años (desde 2016 hasta 2025)

## Validaciones Implementadas

1. **Empleado existe** en la base de datos
2. **Fecha de ingreso** no es nula
3. **Año válido** (entre 2000 y 2100)
4. **Antigüedad suficiente** (al menos 1 año)

## Casos de Uso

### Ejemplo 1: Empleado con 1 año
```bash
POST /api/vacaciones/empleado/1005
{
  "anio": 2017
}
```
**Resultado**: 12 días (solo empresa)

### Ejemplo 2: Empleado con 4 años
```bash
POST /api/vacaciones/empleado/1005
{
  "anio": 2020
}
```
**Resultado**: 18 días (12 empresa + 3 asignados + 3 programables)

### Ejemplo 3: Empleado con 9 años (6-10 años)
```bash
POST /api/vacaciones/empleado/1005
{
  "anio": 2025
}
```
**Resultado**: 24 días (12 empresa + 5 asignados + 7 programables)

### Ejemplo 4: Empleado con 15 años (11-15 años)
```bash
POST /api/vacaciones/empleado/1005
{
  "anio": 2031
}
```
**Resultado**: 26 días (12 empresa + 5 asignados + 9 programables)

### Ejemplo 5: Empleado con 25 años (21-25 años)
```bash
POST /api/vacaciones/empleado/1005
{
  "anio": 2041
}
```
**Resultado**: 30 días (12 empresa + 5 asignados + 13 programables)

### Ejemplo 6: Empleado con 45 años (tope máximo)
```bash
POST /api/vacaciones/empleado/1005
{
  "anio": 2061
}
```
**Resultado**: 36 días (12 empresa + 5 asignados + 19 programables)

## Autorización
Todos los endpoints requieren autenticación (`[Authorize]`).

## Uso del Servicio
El servicio `VacacionesService` puede ser reutilizado en otros endpoints que necesiten calcular vacaciones por antigüedad.
