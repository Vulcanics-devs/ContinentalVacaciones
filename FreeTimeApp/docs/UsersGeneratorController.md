# UsersGeneratorController

## Descripción
Este controlador permite a los SuperUsuarios generar usuarios en el sistema a partir de los empleados registrados. El acceso está restringido únicamente a usuarios con el rol `SuperUsuario`.

---

## Endpoints

### POST /api/UsersGenerator/generate-users-from-empleados

Genera usuarios a partir de los empleados existentes en la base de datos. Solo puede ser invocado por SuperUsuarios.

#### Seguridad
- Requiere autenticación JWT.
- Requiere el rol: `SuperUsuario`.

#### Payload de la petición
No requiere body. Solo se debe realizar la petición POST autenticada.

#### Ejemplo de petición
```http
POST /api/UsersGenerator/generate-users-from-empleados HTTP/1.1
Authorization: Bearer <token>
Content-Type: application/json
```

#### Ejemplo de respuesta exitosa
**Status Code: 200 OK**
```json
{
  "success": true,
  "data": {
    "created": 5
  },
  "errorMsg": "Usuarios generados correctamente: 5"
}
```

#### Ejemplo de respuesta si no hay empleados válidos
**Status Code: 200 OK**
```json
{
  "success": true,
  "data": {
    "created": 0
  },
  "errorMsg": "Usuarios generados correctamente: 0"
}
```

#### Ejemplo de respuesta si el usuario no tiene permisos
**Status Code: 403 Forbidden**
```json
{
  "success": false,
  "data": null,
  "errorMsg": "No autorizado"
}
```

#### Ejemplo de respuesta si ocurre un error interno
**Status Code: 500 Internal Server Error**
```json
{
  "success": false,
  "data": null,
  "errorMsg": "Error inesperado al generar usuarios"
}
```

---

## Notas
- El endpoint sigue la respuesta estandarizada definida en la documentación general de la API.
- El campo `created` indica el número de usuarios generados exitosamente.
- El mensaje puede variar según el resultado de la operación.
