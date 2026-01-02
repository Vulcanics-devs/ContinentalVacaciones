# IncidenciaOPermisosController

## Endpoint: Crear Incidencias

### POST /api/IncidenciaOPermisos

Crea una o varias incidencias para un empleado en el rango de fechas especificado.

#### Payload de entrada

```json
{
  "fechaInicial": "2025-09-01",
  "fechaFinal": "2025-09-03",
  "detalles": "Motivo de la incidencia",
  "tiposDeIncedencia": 1,
  "idUsuarioEmpleado": 123,
  "idUsuarioSindicato": 456, // Opcional
  "nominaEmpleado": 789      // Opcional
}
```

- `fechaInicial` (DateOnly, requerido): Fecha de inicio de la incidencia.
- `fechaFinal` (DateOnly, requerido): Fecha de fin de la incidencia.
- `detalles` (string, opcional): Motivo o detalles de la incidencia (máx. 250 caracteres).
- `tiposDeIncedencia` (int, requerido): Tipo de incidencia (ver catálogo de tipos).
- `idUsuarioEmpleado` (int, requerido): ID del usuario empleado.
- `idUsuarioSindicato` (int, opcional): ID del usuario sindicato.
- `nominaEmpleado` (int, opcional): Nómina del empleado.

#### Ejemplo de respuesta exitosa (201 Created)

```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "fecha": "2025-09-01",
      "fechaInicial": "2025-09-01",
      "fechaFinal": "2025-09-03",
      "diaDeLaSemana": 1,
      "fechaRegistro": "2025-09-04T10:00:00",
      "nominaEmpleado": 789,
      "idGrupo": 10,
      "idRegla": 20,
      "detalles": "Motivo de la incidencia",
      "tiposDeIncedencia": 1,
      "usuarioAutoriza": { "id": 2, "fullName": "Autoriza" },
      "usuarioSindicato": { "id": 456, "fullName": "Sindicato" },
      "usuarioEmpleado": { "id": 123, "fullName": "Empleado" },
      "grupo": { "grupoId": 10, "rol": "TestRol" },
      "regla": { "id": 20, "nombre": "TestRegla" }
    }
    // ... más incidencias si el rango de fechas es mayor
  ],
  "errorMsg": null
}
```

#### Ejemplo de error de validación (400 BadRequest)

```json
{
  "success": false,
  "data": null,
  "errorMsg": "La fecha inicial no puede ser mayor a la fecha final"
}
```

#### Ejemplo de error interno (500 Internal Server Error)

```json
{
  "success": false,
  "data": null,
  "errorMsg": "Error interno al guardar las incidencias"
}
```

#### Notas
- El endpoint valida los campos requeridos y la coherencia de las fechas.
- Si el rango de fechas abarca varios días, se crearán varias incidencias (una por día).
- El usuario que autoriza se obtiene del token JWT.

---

## Endpoint: Eliminar Incidencia o Permiso

### DELETE /api/IncidenciaOPermisos/{id}

Elimina una incidencia o grupo de incidencias/permiso según los criterios de agrupación (mismo Detalles, TiposDeIncedencia, IdUsuarioEmpleado, FechaInicial y FechaFinal).

#### Parámetro de ruta

- `id` (int, requerido): Identificador de la incidencia o permiso a eliminar. Debe ser un entero positivo.

#### Ejemplo de petición

```
DELETE /api/IncidenciaOPermisos/15
```

#### Ejemplo de respuesta exitosa (200 OK)

```json
{
  "success": true,
  "data": { "eliminados": 3 },
  "errorMsg": "Se eliminaron 3 incidencia(s) o permiso(s)"
}
```

#### Ejemplo de error por id inválido (400 BadRequest)

```json
{
  "success": false,
  "data": null,
  "errorMsg": "El id debe ser un entero positivo"
}
```

#### Ejemplo de error por no encontrado (404 Not Found)

```json
{
  "success": false,
  "data": null,
  "errorMsg": "No se encontró la incidencia o permiso con id 15"
}
```

#### Ejemplo de error interno (500 Internal Server Error)

```json
{
  "success": false,
  "data": null,
  "errorMsg": "Error interno al eliminar la incidencia o permiso"
}
```

#### Notas
- Si la incidencia o permiso pertenece a un grupo, se eliminan todas las incidencias o permisos del grupo.
- Si no pertenece a un grupo, solo se elimina la incidencia o permiso especificado.
- La respuesta siempre sigue el formato estandarizado de la API.

---

## Endpoint: Consultar Incidencias

### POST /api/IncidenciaOPermisos/retrieve

Consulta incidencias filtrando por usuario, fechas, sindicato, grupo o regla.

#### Payload de entrada

```json
{
  "idUsuarioEmpleado": 123,           // Opcional
  "fechaInicial": "2025-09-01T00:00:00", // Opcional
  "fechaFinal": "2025-09-03T00:00:00",   // Opcional
  "idUsuarioSindicato": 456,          // Opcional
  "grupoId": 10,                      // Opcional
  "reglaId": 20                       // Opcional
}
```

- Todos los campos son opcionales, pero se recomienda enviar al menos uno para filtrar resultados.
- Las fechas deben estar en formato ISO 8601 (DateTime).

#### Ejemplo de respuesta exitosa (200 OK)

```json
{
  "success": true,
  "data": {
    "count": 2,
    "items": [
      {
        "id": 1,
        "fecha": "2025-09-01",
        "fechaInicial": "2025-09-01",
        "fechaFinal": "2025-09-03",
        "diaDeLaSemana": 1,
        "fechaRegistro": "2025-09-04T10:00:00",
        "nominaEmpleado": 789,
        "idGrupo": 10,
        "idRegla": 20,
        "detalles": "Motivo de la incidencia",
        "tiposDeIncedencia": 1,
        "usuarioAutoriza": { "id": 2, "fullName": "Autoriza" },
        "usuarioSindicato": { "id": 456, "fullName": "Sindicato" },
        "usuarioEmpleado": { "id": 123, "fullName": "Empleado" },
        "grupo": { "grupoId": 10, "rol": "TestRol" },
        "regla": { "id": 20, "nombre": "TestRegla" }
      }
      // ... más incidencias si existen
    ]
  },
  "errorMsg": null
}
```

#### Ejemplo de error de validación (400 BadRequest)

```json
{
  "success": false,
  "data": null,
  "errorMsg": "La fecha inicial no puede ser mayor que la fecha final"
}
```

#### Ejemplo de error interno (500 Internal Server Error)

```json
{
  "success": false,
  "data": null,
  "errorMsg": "Error interno al consultar las incidencias"
}
```

#### Notas
- El endpoint permite filtrar por cualquier combinación de los campos disponibles.
- Si no se envía ningún filtro, se retornan todas las incidencias.
- La respuesta siempre sigue el formato estandarizado de la API.
