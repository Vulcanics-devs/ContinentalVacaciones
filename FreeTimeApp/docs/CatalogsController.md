# CatalogsController

Este controlador expone endpoints para consultar catálogos del sistema, como tipos de actividad, turnos y tipos de incidencia. Todos los endpoints requieren autenticación.

---

## Endpoints

### GET /api/Catalogs/tipos-actividad

Retorna los tipos de actividad definidos en el enum `TipoActividadDelDiaEnum`.

#### Ejemplo de petición
```http
GET /api/Catalogs/tipos-actividad HTTP/1.1
Authorization: Bearer <token>
Content-Type: application/json
```

#### Ejemplo de respuesta exitosa
```json
{
  "success": true,
  "data": [
    { "Value": 0, "Name": "Incidencia O Permiso" },
    { "Value": 1, "Name": "Inhabil Por Ley" },
    { "Value": 2, "Name": "Inhabil Por Continental" },
    { "Value": 3, "Name": "Vacaciones Auto Asignadas Por App" },
    { "Value": 4, "Name": "Vacaciones Seleccionadas Por Emp" },
    { "Value": 5, "Name": "Descanso Semanal" },
    { "Value": 6, "Name": "Laboral" }
  ],
  "errorMsg": "Tipos de actividad del día obtenidos correctamente"
}
```

---

### GET /api/Catalogs/turnos

Retorna los turnos definidos en el enum `TurnosEnum`.

#### Ejemplo de petición
```http
GET /api/Catalogs/turnos HTTP/1.1
Authorization: Bearer <token>
Content-Type: application/json
```

#### Ejemplo de respuesta exitosa
```json
{
  "success": true,
  "data": [
    { "Value": 1, "Name": "Matutino" },
    { "Value": 2, "Name": "Vespertino" },
    { "Value": 3, "Name": "Nocturno" },
    { "Value": 4, "Name": "Descanso" }
  ],
  "errorMsg": "Turnos obtenidos correctamente"
}
```

---

### GET /api/Catalogs/tipos-incidencia

Retorna los tipos de incidencia definidos en el enum `TiposDeIncedenciaEnum`.

#### Ejemplo de petición
```http
GET /api/Catalogs/tipos-incidencia HTTP/1.1
Authorization: Bearer <token>
Content-Type: application/json
```

#### Ejemplo de respuesta exitosa
```json
{
  "success": true,
  "data": [
    { "Value": 0, "Name": "Permiso Con Goce" },
    { "Value": 1, "Name": "Permiso Defuncion" },
    { "Value": 2, "Name": "Permiso Sin Goce" },
    { "Value": 3, "Name": "Incapacidad Enfermedad General" },
    { "Value": 4, "Name": "Incapacidad Accidente Trabajo" },
    { "Value": 5, "Name": "Incapacidad Por Maternidad" },
    { "Value": 6, "Name": "Incapacidad Probable Riesgo Trabajo" },
    { "Value": 7, "Name": "Suspension" },
    { "Value": 8, "Name": "PCG Por Paternidad" }
  ],
  "errorMsg": "Tipos de incidencia obtenidos correctamente"
}
```

---

## Posibles respuestas de error

- **401 Unauthorized:**
  ```json
  {
    "success": false,
    "data": null,
    "errorMsg": "No autorizado"
  }
  ```

- **500 Internal Server Error:**
  ```json
  {
    "success": false,
    "data": null,
    "errorMsg": "Error inesperado al obtener catálogo"
  }
  ```

---
