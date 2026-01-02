# Documentación del Endpoint ObtenerCalendarioPorGrupo en CalendarioPorGrupoController

## Endpoint
`GET /api/calendario/por-grupo/{grupoId}?fechaInicio=yyyy-MM-dd&fechaFinal=yyyy-MM-dd`

## Descripción
Obtiene el calendario de todos los empleados sindicalizados de un grupo en un rango de fechas. Devuelve una lista con la información de cada empleado y sus días de calendario, filtrando por grupo y fechas.

## Parámetros
- `grupoId` (int, requerido, en ruta): ID del grupo de empleados sindicalizados.
- `fechaInicio` (DateTime, requerido, en query): Fecha de inicio del rango (formato ISO 8601).
- `fechaFinal` (DateTime, requerido, en query): Fecha final del rango (formato ISO 8601).

## Validaciones
- `grupoId` debe ser un entero positivo.
- `fechaInicio` debe ser menor a `fechaFinal`.
- El rango entre `fechaInicio` y `fechaFinal` no debe ser mayor a 31 días.
- El `grupoId` debe corresponder a un grupo existente en la base de datos.

## Respuestas y códigos de estado
- **200 OK**: Solicitud válida. Devuelve una lista de empleados sindicalizados y sus días de calendario.
- **400 Bad Request**: Error de validación en los parámetros (grupoId inválido, fechas incorrectas, rango mayor a 31 días).
- **404 Not Found**: El grupo especificado no existe.
- **500 Internal Server Error**: Error inesperado en el servidor.

### Estructura de la respuesta (`ApiResponse`)
```json
{
  "success": true | false,
  "data": [
    {
      "idUsuarioEmpleadoSindicalizado": 1,
      "idGrupo": 1,
      "nominaEmpleado": "123",
      "nombreCompletoEmpleado": "Empleado Uno",
      "dias": [
        {
          "idDiaCalendarioEmpleado": 1,
          "fecha": "2025-09-01T00:00:00Z",
          "tipoActividadDelDia": "1",
          "detalles": "Laboral"
        }
      ]
    }
  ],
  "errorMsg": null
}
```

## Ejemplos

### Solicitud válida
```
GET /api/calendario/por-grupo/1?fechaInicio=2025-09-01&fechaFinal=2025-09-10
```
**Respuesta:**
```json
200 OK
{
  "success": true,
  "data": [
    {
      "idUsuarioEmpleadoSindicalizado": 1,
      "idGrupo": 1,
      "nominaEmpleado": "123",
      "nombreCompletoEmpleado": "Empleado Uno",
      "dias": [
        {
          "idDiaCalendarioEmpleado": 1,
          "fecha": "2025-09-01T00:00:00Z",
          "tipoActividadDelDia": "1",
          "detalles": "Laboral"
        }
      ]
    }
  ],
  "errorMsg": null
}
```

### Solicitud con grupoId inválido
```
GET /api/calendario/por-grupo/-1?fechaInicio=2025-09-01&fechaFinal=2025-09-10
```
**Respuesta:**
```json
400 Bad Request
{
  "success": false,
  "data": null,
  "errorMsg": "El grupoId debe ser un entero positivo."
}
```

### Solicitud con fechaInicio mayor a fechaFinal
```
GET /api/calendario/por-grupo/1?fechaInicio=2025-09-10&fechaFinal=2025-09-01
```
**Respuesta:**
```json
400 Bad Request
{
  "success": false,
  "data": null,
  "errorMsg": "La fechaInicio debe ser menor a la fechaFinal."
}
```

### Solicitud con rango de fechas mayor a 31 días
```
GET /api/calendario/por-grupo/1?fechaInicio=2025-09-01&fechaFinal=2025-10-10
```
**Respuesta:**
```json
400 Bad Request
{
  "success": false,
  "data": null,
  "errorMsg": "El rango de fechas no puede ser mayor a 31 días."
}
```

### Grupo no existente
```
GET /api/calendario/por-grupo/999?fechaInicio=2025-09-01&fechaFinal=2025-09-10
```
**Respuesta:**
```json
404 Not Found
{
  "success": false,
  "data": null,
  "errorMsg": "El grupo especificado no existe."
}
```

### Error inesperado
**Respuesta:**
```json
500 Internal Server Error
{
  "success": false,
  "data": null,
  "errorMsg": "Error inesperado: <mensaje>"
}
```

---

[Volver a la documentación general de la API](APIRestDocumentation.md)
