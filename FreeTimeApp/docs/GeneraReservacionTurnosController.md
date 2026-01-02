# Documentación del Endpoint Ejecutar en GeneraReservacionTurnosController

## Endpoint
`POST /api/genera-reservacion-turnos/ejecutar`

## Descripción
Este endpoint inicia el proceso de generación de calendarios y asignación de vacaciones para empleados sindicalizados, según los parámetros recibidos. Solo puede ser ejecutado por usuarios con el rol `SuperUsuario`.

## Parámetros de entrada
El cuerpo de la solicitud debe ser un objeto JSON con la siguiente estructura (`AsignacionDeVacacionesRequest`):

```
{
  "fechaInicio": "2025-09-01T00:00:00Z",
  "fechaFinal": "2025-12-31T00:00:00Z",
  "fechaInicioReservaciones": "2025-08-15T00:00:00Z"
}
```

- `fechaInicio` (DateTime): Fecha de inicio del periodo de la programacion anual.
- `fechaFinal` (DateTime): Fecha de fin del periodo de la programación anual.
- `fechaInicioReservaciones` (DateTime): Fecha en la que inicia la reservación de turnos.

## Validaciones
- `fechaInicio` debe ser menor que `fechaFinal`.
- `fechaInicioReservaciones` debe ser menor que `fechaInicio`.

Si alguna validación falla, se retorna un error de validación.

## Respuestas y códigos de estado
- **200 OK**: Proceso iniciado correctamente o error interno (si no es de validación). Siempre retorna un objeto `APIResponse`.
- **400 Bad Request**: Error de validación en los parámetros. Retorna un objeto `APIResponse` con el mensaje de error.

### Estructura de la respuesta (`APIResponse`)
```
{
  "success": true | false,
  "data": "Mensaje de éxito" | null,
  "errorMsg": "Mensaje de error" | null
}
```

## Ejemplos

### Solicitud válida
```
POST /api/genera-reservacion-turnos/ejecutar
Content-Type: application/json

{
  "fechaInicio": "2025-09-01T00:00:00Z",
  "fechaFinal": "2025-12-31T00:00:00Z",
  "fechaInicioReservaciones": "2025-08-15T00:00:00Z"
}
```

**Respuesta:**
```
200 OK
{
  "success": true,
  "data": "La generación de calendarios y reservaciones se ha iniciado correctamente.",
  "errorMsg": null
}
```

### Solicitud inválida (error de validación)
```
POST /api/genera-reservacion-turnos/ejecutar
Content-Type: application/json

{
  "fechaInicio": "2025-09-01T00:00:00Z",
  "fechaFinal": "2025-08-01T00:00:00Z",
  "fechaInicioReservaciones": "2025-08-15T00:00:00Z"
}
```

**Respuesta:**
```
400 Bad Request
{
  "success": false,
  "data": null,
  "errorMsg": "La FechaInicio debe ser menor a la FechaFinal."
}
```

### Error interno (no de validación)
```
200 OK
{
  "success": false,
  "data": null,
  "errorMsg": "Ocurrió un error al ejecutar la operación."
}
```

## Seguridad
- Requiere autenticación y el rol `SuperUsuario`.

---

[Volver a la documentación general de la API](APIRestDocumentation.md)
