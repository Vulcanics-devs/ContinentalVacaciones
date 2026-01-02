# Sistema de Cálculo de Ausencias y Porcentajes

## Resumen
Se ha implementado un sistema completo para calcular porcentajes de ausencia por día, considerando el manning requerido de cada área y el personal disponible vs. no disponible.

## Lógica de Cálculo

### Fórmula del Porcentaje de Ausencia:
```
Personal Disponible = Personal Total - Personal No Disponible
Porcentaje Disponible = (Personal Disponible / Manning Requerido) × 100
Porcentaje de Ausencia = 100% - Porcentaje Disponible
```

### Ejemplo:
- **Manning Requerido**: 43 personas
- **Personal Total**: 46 personas  
- **Personal No Disponible**: 5 personas (vacaciones + incapacidades)
- **Personal Disponible**: 46 - 5 = 41 personas
- **Porcentaje Disponible**: (41/43) × 100 = 95.3%
- **Porcentaje de Ausencia**: 100% - 95.3% = **4.7%**

## Endpoints Implementados

### 1. Calcular Ausencias por Fechas
```
POST /api/ausencias/calcular
```

**Request Body:**
```json
{
  "fechas": ["2025-01-15", "2025-01-16"],
  "grupoId": 6,     // Opcional: filtrar por grupo
  "areaId": 53      // Opcional: filtrar por área
}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "fecha": "2025-01-15",
      "ausenciasPorGrupo": [
        {
          "grupoId": 6,
          "nombreGrupo": "R0144",
          "areaId": 53,
          "nombreArea": "Producción A",
          "manningRequerido": 43,
          "personalTotal": 46,
          "personalNoDisponible": 5,
          "personalDisponible": 41,
          "porcentajeDisponible": 95.35,
          "porcentajeAusencia": 4.65,
          "porcentajeMaximoPermitido": 4.5,
          "excedeLimite": true,
          "empleadosAusentes": [
            {
              "empleadoId": 1005,
              "nombreCompleto": "CANO VALLADOLID MIGUEL ANGEL",
              "tipoAusencia": "Vacacion",
              "tipoVacacion": "Anual"
            }
          ]
        }
      ]
    }
  ]
}
```

### 2. Validar Disponibilidad de Día
```
POST /api/ausencias/validar-disponibilidad
```

**Request Body:**
```json
{
  "empleadoId": 1005,
  "fecha": "2025-01-15",
  "tipoVacacion": "Anual"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "diaDisponible": false,
    "porcentajeAusenciaActual": 4.65,
    "porcentajeAusenciaConEmpleado": 6.98,
    "porcentajeMaximoPermitido": 4.5,
    "motivo": "Excede el límite permitido (4.5%)",
    "detalleGrupo": {
      "grupoId": 6,
      "nombreGrupo": "R0144",
      "manningRequerido": 43,
      "personalTotal": 46,
      "personalNoDisponible": 6,
      "personalDisponible": 40,
      "porcentajeAusencia": 6.98,
      "excedeLimite": true
    }
  }
}
```

### 3. Obtener Ausencias por Grupo y Fecha
```
GET /api/ausencias/grupo/{grupoId}/fecha/{fecha}
```

**Ejemplo:**
```
GET /api/ausencias/grupo/6/fecha/2025-01-15
```

### 4. Obtener Ausencias por Fecha
```
GET /api/ausencias/fecha/{fecha}?areaId={areaId}
```

**Ejemplo:**
```
GET /api/ausencias/fecha/2025-01-15?areaId=53
```

## Endpoints de Configuración del Sistema

### 5. Obtener Configuración Actual
```
GET /api/configuracion-vacaciones
```

**Roles permitidos**: Super Usuario, Jefe De Area, Ingeniero Industrial

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "porcentajeAusenciaMaximo": 4.5,
    "periodoActual": "Cerrado",
    "anioVigente": 2025,
    "createdAt": "2025-01-01T00:00:00",
    "updatedAt": "2025-01-15T10:30:00",
    "updatedByUser": "Juan Pérez"
  }
}
```

### 6. Actualizar Configuración
```
PUT /api/configuracion-vacaciones
```

**Roles permitidos**: Super Usuario, Ingeniero Industrial

**Request Body:**
```json
{
  "porcentajeAusenciaMaximo": 5.0,
  "periodoActual": "ProgramacionAnual",
  "anioVigente": 2025
}
```

### 7. Cambiar Período del Sistema
```
POST /api/configuracion-vacaciones/cambiar-periodo
```

**Roles permitidos**: Super Usuario, Ingeniero Industrial

**Request Body:**
```json
{
  "nuevoPeriodo": "ProgramacionAnual"
}
```

### 8. Obtener Excepciones de Porcentaje
```
GET /api/configuracion-vacaciones/excepciones?grupoId={id}&fechaInicio={fecha}&fechaFin={fecha}
```

**Roles permitidos**: Super Usuario, Jefe De Area, Ingeniero Industrial

### 9. Crear Excepción de Porcentaje
```
POST /api/configuracion-vacaciones/excepciones
```

**Roles permitidos**: Super Usuario, Jefe De Area, Ingeniero Industrial

**Request Body:**
```json
{
  "grupoId": 6,
  "fecha": "2025-12-25",
  "porcentajeMaximoPermitido": 10.0,
  "motivo": "Día festivo - mayor flexibilidad"
}
```

### 10. Actualizar Excepción de Porcentaje
```
PUT /api/configuracion-vacaciones/excepciones/{excepcionId}
```

### 11. Eliminar Excepción de Porcentaje
```
DELETE /api/configuracion-vacaciones/excepciones/{excepcionId}
```

**Roles permitidos**: Super Usuario, Ingeniero Industrial

## Configuración del Sistema

### Porcentaje Máximo Permitido:
- **Por defecto**: 4.5%
- **Configurable**: A través de tabla `ConfiguracionVacaciones`
- **Excepciones**: Por día/grupo específico en tabla `ExcepcionesPorcentaje`

## Estructura de Datos Propuesta

### Tabla: ConfiguracionVacaciones
```sql
CREATE TABLE ConfiguracionVacaciones (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PorcentajeAusenciaMaximo DECIMAL(5,2) DEFAULT 4.5,
    PeriodoActual NVARCHAR(20) NOT NULL, -- 'ProgramacionAnual', 'Reprogramacion', 'Cerrado'
    AnioVigente INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);
```

### Tabla: ExcepcionesPorcentaje
```sql
CREATE TABLE ExcepcionesPorcentaje (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    GrupoId INT NOT NULL,
    Fecha DATE NOT NULL,
    PorcentajeMaximoPermitido DECIMAL(5,2) NOT NULL,
    Motivo NVARCHAR(200),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (GrupoId) REFERENCES Grupos(GrupoId)
);
```

### Tabla: VacacionesProgramadas (Unificada)
```sql
CREATE TABLE VacacionesProgramadas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmpleadoId INT NOT NULL,
    FechaVacacion DATE NOT NULL,
    TipoVacacion NVARCHAR(50) NOT NULL, -- 'Anual', 'Reprogramacion', 'AsignadaAutomaticamente'
    OrigenAsignacion NVARCHAR(30) NOT NULL, -- 'Manual', 'Automatica'
    EstadoVacacion NVARCHAR(20) DEFAULT 'Activa', -- 'Activa', 'Intercambiada', 'Cancelada'
    PeriodoProgramacion NVARCHAR(20) NOT NULL, -- 'ProgramacionAnual', 'Reprogramacion'
    FechaProgramacion DATETIME DEFAULT GETDATE(),
    PuedeSerIntercambiada BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (EmpleadoId) REFERENCES Users(Id)
);
```

### Tabla: SolicitudesReprogramacion
```sql
CREATE TABLE SolicitudesReprogramacion (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmpleadoId INT NOT NULL,
    VacacionOriginalId INT NOT NULL,
    FechaNuevaSolicitada DATE NOT NULL,
    EstadoSolicitud NVARCHAR(20) DEFAULT 'Pendiente',
    MotivoRechazo NVARCHAR(500) NULL,
    JefeAreaId INT NULL,
    FechaSolicitud DATETIME DEFAULT GETDATE(),
    FechaRespuesta DATETIME NULL,
    PorcentajeCalculado DECIMAL(5,2) NULL,
    
    FOREIGN KEY (EmpleadoId) REFERENCES Users(Id),
    FOREIGN KEY (VacacionOriginalId) REFERENCES VacacionesProgramadas(Id),
    FOREIGN KEY (JefeAreaId) REFERENCES Users(Id)
);
```

## Casos de Uso

### 1. Consultar Ausencias de Múltiples Días
Útil para planificación semanal o mensual.

### 2. Validar Disponibilidad Antes de Reservar
Antes de que un empleado reserve un día, validar si está disponible.

### 3. Monitoreo en Tiempo Real
Dashboard para jefes de área para ver porcentajes actuales.

### 4. Reportes de Ausencias
Análisis histórico de patrones de ausencias.

## Estado Actual

✅ **Implementado:**
- Servicio de cálculo de ausencias
- Endpoints REST completos
- DTOs y validaciones
- Lógica de porcentajes
- Simulación de empleados adicionales

⏳ **Pendiente (requiere tablas de BD):**
- Persistencia de vacaciones programadas
- Configuración dinámica de porcentajes
- Excepciones por día/grupo
- Integración con incapacidades

## Próximos Pasos

1. Crear las tablas en la base de datos
2. Implementar servicios de programación anual
3. Implementar servicios de reprogramación
4. Crear algoritmo de asignación automática
5. Desarrollar interfaz de usuario

## Autorización
Todos los endpoints requieren autenticación (`[Authorize]`).
