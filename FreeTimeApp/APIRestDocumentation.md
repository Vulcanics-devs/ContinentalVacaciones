# Documentación de la API REST

---

## Guía de despliegue usando IIS en Windows Server

Para instrucciones detalladas sobre cómo desplegar la aplicación en un servidor Windows sin acceso a internet usando IIS, consulta:

- [Guía de Deploy en Windows e IIS](docs/GuiaDeployOnWindowsAndIIS.md)

---


## Respuesta estandarizada

Todos los endpoints retornan una respuesta estandarizada, independientemente del resultado (éxito o error):

```json
{
  "success": true | false,
  "data": { ... }, // Puede ser un objeto, lista o null
  "errorMsg": "Mensaje de error opcional"
}
```

El tipo de `data` depende del endpoint y puede ser una entidad, un DTO o una lista.

**Nota:** El status code HTTP (por ejemplo, 200, 201, 400, 401, 404) variará según el resultado de la petición, pero la estructura de la respuesta siempre será la misma.

---


## Ejemplos de errores específicos

- **Validación de campos requeridos:**

  ```json
  {
    "success": false,
    "data": null,
    "errorMsg": "Rol e Identificador SAP son requeridos"
  }
  ```

- **Entidad no encontrada:**

  ```json
  {
    "success": false,
    "data": null,
    "errorMsg": "Área no encontrada"
  }
  ```

- **Usuario/Líder no existe:**

  ```json
  {
    "success": false,
    "data": null,
    "errorMsg": "El líder especificado no existe"
  }
  ```

- **Credenciales inválidas (Auth):**

  ```json
  {
    "success": false,
    "data": null,
    "errorMsg": "Credenciales inválidas"
  }
  ```

- **Acceso no autorizado:**
  ```json
  {
    "success": false,
    "data": null,
    "errorMsg": "No autorizado"
  }
  ```

---


## Explicación de status code

- **200 OK:** Petición exitosa, datos retornados correctamente.
- **201 Created:** Recurso creado exitosamente.
- **400 BadRequest:** Error de validación o datos faltantes/incorrectos.
- **401 Unauthorized:** No autenticado o token inválido.
- **403 Forbidden:** No autorizado para la acción.
- **404 NotFound:** Recurso no encontrado.
- **409 Conflict:** Conflicto de datos (por ejemplo, duplicados).

---



## Documentación de endpoints por controller

- [AuthController](docs/AuthController.md)
- [AreaController](docs/AreaController.md)
- [RolController](docs/RolController.md)
- [GrupoController](docs/GrupoController.md)
- [UserController](docs/UserController.md)
- [UsersGeneratorController](docs/UsersGeneratorController.md)
- [CatalogsController](docs/CatalogsController.md)
- [DiasInhabilesController](docs/DiasInhabilesController.md)
- [IncidenciaOPermisosController](docs/IncidenciaOPermisosController.md)
- [GeneraReservacionTurnosController](docs/GeneraReservacionTurnosController.md)
- [CalendarioPorGrupoController](docs/CalendarioPorGrupoController.md)

---
