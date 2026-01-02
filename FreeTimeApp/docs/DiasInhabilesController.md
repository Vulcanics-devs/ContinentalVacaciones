# Documentación de DiasInhabilesController


## Enum: TipoActividadDelDiaEnum
Valores posibles:
- 0: IncidenciaOPermiso
- 1: InhabilPorLey
- 2: InhabilPorContinental
- 3: VacacionesAutoAsignadasPorApp
- 4: VacacionesSeleccionadasPorEmp
- 5: DescansoSemanal
- 6: Laboral

---

## Requiere autenticación
Todos los endpoints requieren un usuario autenticado (Bearer Token).

---


## Endpoint: Crear días inhábiles

**POST** `/api/DiasInhabiles`

Crea uno o varios registros de días inhábiles según el rango de fechas y el tipo de actividad especificado. Requiere rol `SuperUsuario`.

### Body de la petición
```json
{
  "fechaInicial": "2025-09-03",
  "fechaFinal": "2025-09-05",
  "detalles": "Días festivos por ley",
  "tipoActividadDelDia": 1
}
```
- `fechaInicial` (DateOnly): Fecha de inicio del rango.
- `fechaFinal` (DateOnly): Fecha de fin del rango.
- `detalles` (string, máx 250): Descripción del motivo. **Requerido**.
- `tipoActividadDelDia` (int): Valor entero válido en el enum `TipoActividadDelDiaEnum`.

### Ejemplo de petición para un solo día
```json
{
  "fechaInicial": "2025-09-03",
  "fechaFinal": "2025-09-03",
  "detalles": "Día festivo nacional",
  "tipoActividadDelDia": 1
}
```

### Ejemplo de petición para varios días
```json
{
  "fechaInicial": "2025-09-03",
  "fechaFinal": "2025-09-05",
  "detalles": "Días festivos por ley",
  "tipoActividadDelDia": 1
}
```

### Respuesta exitosa
**HTTP 200 OK**
```json
{
  "success": true,
  "data": [12, 13, 14],
  "errorMsg": "Días inhábiles creados correctamente"
}
```
- `data`: Lista de IDs de los registros creados.

### Ejemplo de errores
- Tipo de actividad inválido:
**HTTP 400 Bad Request**
```json
{
  "success": false,
  "data": null,
  "errorMsg": "TipoActividadDelDia inválido"
}
```
- Detalles es requerido:
**HTTP 400 Bad Request**
```json
{
  "success": false,
  "data": null,
  "errorMsg": "El campo Detalles es requerido"
}
```
- Detalles excede el máximo:
**HTTP 400 Bad Request**
```json
{
  "success": false,
  "data": null,
  "errorMsg": "Detalles excede el máximo de 250 caracteres"
}
```
- Fecha inicial mayor que fecha final:
**HTTP 400 Bad Request**
```json
{
  "success": false,
  "data": null,
  "errorMsg": "La fecha inicial no puede ser mayor que la fecha final"
}
```
- Detalles duplicado:
**HTTP 409 Conflict**
```json
{
  "success": false,
  "data": null,
  "errorMsg": "Ya existe un día inhábil con ese valor en la columna Detalles"
}
```

---

## Endpoint: Consultar días inhábiles (filtrado)

**GET** `/api/DiasInhabiles`

Consulta los días inhábiles registrados, permitiendo filtrar por rango de fechas y tipo de día inhábil. Todos los parámetros son opcionales y se envían en el body como JSON.

### Body de la petición (opcional)
```json
{
  "fechaInicio": "2025-09-01",
  "fechaFin": "2025-09-30",
  "tipoDiaInhabil": 1
}
```
- `fechaInicio` (DateOnly, opcional): Fecha de inicio del rango.
- `fechaFin` (DateOnly, opcional): Fecha de fin del rango.
- `tipoDiaInhabil` (int, opcional): Valor entero válido en el enum `TipoActividadDelDiaEnum`.

#### Ejemplo: Filtrar por rango de fechas y tipo
```json
{
  "fechaInicio": "2025-09-01",
  "fechaFin": "2025-09-30",
  "tipoDiaInhabil": 1
}
```

#### Ejemplo: Solo rango de fechas
```json
{
  "fechaInicio": "2025-09-01",
  "fechaFin": "2025-09-30"
}
```

#### Ejemplo: Solo tipo de día inhábil
```json
{
  "tipoDiaInhabil": 1
}
```

#### Ejemplo: Sin parámetros (retorna todos los días inhábiles del año actual)
```json
{}
```

### Respuesta exitosa
**HTTP 200 OK**
```json
{
  "success": true,
  "data": [
    {
      "id": 12,
      "fecha": "2025-09-03",
      "fechaInicial": "2025-09-01",
      "fechaFinal": "2025-09-30",
      "detalles": "Día festivo nacional",
      "tipoActividadDelDia": 1
    },
    {
      "id": 13,
      "fecha": "2025-09-05",
      "fechaInicial": "2025-09-01",
      "fechaFinal": "2025-09-30",
      "detalles": "Día festivo nacional",
      "tipoActividadDelDia": 1
    }
  ],
  "errorMsg": null
}
```
- `data`: Lista de objetos con los días inhábiles encontrados.

### Ejemplo de errores
- Rango de fechas mayor a un año:
**HTTP 400 Bad Request**
```json
{
  "success": false,
  "data": null,
  "errorMsg": "El rango de fechas no puede ser mayor a un año"
}
```
- Fecha de inicio mayor que fecha de fin:
**HTTP 400 Bad Request**
```json
{
  "success": false,
  "data": null,
  "errorMsg": "La fecha de inicio no puede ser mayor que la fecha de fin"
}
```
- Tipo de día inhábil inválido:
**HTTP 400 Bad Request**
```json
{
  "success": false,
  "data": null,
  "errorMsg": "Tipo de día inhábil inválido"
}
```
- Sin resultados:
**HTTP 200 OK**
```json
{
  "success": true,
  "data": [],
  "errorMsg": "No hay días inhábiles para los parámetros proporcionados"
}
```

---

## Endpoint: Eliminar día(s) inhábil(es)

**DELETE** `/api/DiasInhabiles/{id}`

Elimina un día inhábil específico por su id. Si el día pertenece a un grupo (mismo Detalles, TipoActividadDelDia, FechaInicial y FechaFinal), elimina todo el grupo. Requiere rol `SuperUsuario`.

### Parámetro de ruta
- `id` (int): Identificador del día inhábil a eliminar. Debe ser un entero positivo.

### Ejemplo de petición
```
DELETE /api/DiasInhabiles/12
```

### Respuesta exitosa
**HTTP 200 OK**
```json
{
  "success": true,
  "data": 3,
  "errorMsg": "Se eliminaron 3 día(s) inhábil(es) correctamente"
}
```
- `data`: Número de días inhábiles eliminados.

### Ejemplo de errores
- Id inválido:
**HTTP 400 Bad Request**
```json
{
  "success": false,
  "data": null,
  "errorMsg": "El id debe ser un entero positivo"
}
```
- Día inhábil no encontrado:
**HTTP 404 Not Found**
```json
{
  "success": false,
  "data": null,
  "errorMsg": "No existe un día inhábil con el id especificado"
}
```
- Error interno:
**HTTP 500 Internal Server Error**
```json
{
  "success": false,
  "data": null,
  "errorMsg": "Error interno: <mensaje>"
}
```


