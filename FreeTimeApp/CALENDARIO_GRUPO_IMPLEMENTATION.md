# Implementación del Endpoint de Calendario de Grupo

## Resumen
Se ha implementado un nuevo endpoint para obtener el calendario de un grupo específico basado en las reglas de turnos definidas.

## Endpoint Creado
```
POST /api/calendario/por-grupo/{id_Grupo}
```

### Parámetros
- **grupoId** (route): ID del grupo
- **Body**: 
  ```json
  {
    "inicio": "2025-01-01T00:00:00",
    "fin": "2025-01-31T23:59:59"
  }
  ```

### Respuesta
```json
{
  "success": true,
  "data": {
    "grupoId": 1,
    "nombreGrupo": "R0144_02",
    "regla": "R0144",
    "fechaInicio": "2025-01-01T00:00:00",
    "fechaFin": "2025-01-31T23:59:59",
    "calendario": [
      {
        "fecha": "2025-01-01T00:00:00",
        "turno": "1",
        "tipo": "Turno de trabajo"
      },
      {
        "fecha": "2025-01-02T00:00:00",
        "turno": "D",
        "tipo": "Descanso"
      }
    ]
  },
  "errorMsg": null
}
```

## Archivos Creados/Modificados

### 1. TipoCalendarioEnum.cs
- **Ubicación**: `Models/Enums/TipoCalendarioEnum.cs`
- **Propósito**: Define los tipos de calendario y sus conversiones
- **Tipos soportados**:
  - 1, 2, 3: Turnos de trabajo
  - D: Descanso
  - P: Permiso con Goce
  - PD: Permiso Defunción
  - V: Vacación
  - VA: Vacación asignada
  - G: Permiso sin Goce
  - E: Inc. Enfermedad General
  - A: Inc. Accidente de Trabajo
  - M: Inc. por Maternidad
  - R: Inc. Pble. Riesgo Trabajo
  - S: Suspensión
  - PP: PCG por Paternidad

### 2. CalendarioGrupoDto.cs
- **Ubicación**: `DTOs/CalendarioGrupoDto.cs`
- **Propósito**: Define las estructuras de datos para request y response
- **Clases**:
  - `CalendarioGrupoRequest`: Para el body del POST
  - `CalendarioGrupoDiaDto`: Para cada día del calendario
  - `CalendarioGrupoResponse`: Para la respuesta completa

### 3. CalendarioGrupoService.cs
- **Ubicación**: `Services/CalendarioGrupoService.cs`
- **Propósito**: Lógica de negocio para generar calendarios
- **Funcionalidades**:
  - Parseo del IdentificadorSAP del grupo
  - Generación de calendarios basados en reglas
  - Aplicación de patrones de turnos por grupo
  - Fecha de referencia: 15 de septiembre de 2025

### 4. CalendarioPorGrupoController.cs (Modificado)
- **Ubicación**: `Controllers/CalendarioPorGrupoController.cs`
- **Cambios**:
  - Agregado nuevo endpoint POST
  - Inyección del nuevo servicio
  - Autorización para roles específicos

### 5. Program.cs (Modificado)
- **Cambios**: Registrado `CalendarioGrupoService` en el contenedor DI

## Reglas Implementadas
Las siguientes reglas están implementadas con sus patrones de turnos:

- **R0144**: 28 días (4 semanas)
- **N0439**: 7 días (1 semana)
- **R0135**: 14 días (2 semanas)
- **R0229**: 28 días (4 semanas)
- **R0154**: 14 días (2 semanas)
- **R0267**: 21 días (3 semanas)
- **R0130**: 28 días (4 semanas)
- **N0440**: 7 días (1 semana)
- **N0A01**: 7 días (1 semana)
- **R0133**: 14 días (2 semanas)

## Lógica de Generación de Calendario

1. **Parseo del Grupo**: Se extrae la regla y número de grupo del `IdentificadorSAP`
2. **Creación del Rol**: Se genera el patrón específico para el número de grupo
3. **Fecha de Referencia**: Se usa el 15 de septiembre de 2025 como punto de inicio
4. **Generación**: Se itera desde la fecha de referencia aplicando el patrón cíclico

## Autorización
El endpoint requiere uno de los siguientes roles:
- Super Usuario
- Administrador
- Jefe De Area
- Jefe De Area Suplente
- Lider De Grupo

## Validaciones
- grupoId debe ser positivo
- fechaInicio debe ser menor que fechaFin
- Rango máximo de 365 días
- El grupo debe existir en la base de datos
- El IdentificadorSAP debe tener formato válido (ej: "R0144_02")

## Pruebas
Se incluye un archivo de pruebas `TestCalendarioGrupo.cs` para validar:
- Conversiones de enum
- Generación de roles
- Creación de calendarios

## Uso del Servicio
El servicio `CalendarioGrupoService` puede ser reutilizado en otros endpoints que necesiten generar calendarios basados en reglas de grupo.
