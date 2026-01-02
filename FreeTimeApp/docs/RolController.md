# RolController

## GET /api/Rol
- Descripción: Lista los roles registrados. Solo usuarios autenticados.
- Headers: Authorization: Bearer {token}
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": [
        {
          "Id": 1,
          "Name": "SuperUsuario",
          "Description": "Usuario con todos los permisos",
          "Abreviation": "SUP"
        },
        {
          "Id": 2,
          "Name": "Empleado_Sindicalizado",
          "Description": "Empleado sindicalizado",
          "Abreviation": "EMP"
        }
      ],
      "errorMsg": null
    }
    ```

## PATCH /api/Rol/{id}
- Descripción: Edita nombre, descripción y abreviación de un rol. Solo SuperUsuario.
- Headers: Authorization: Bearer {token}
- Body esperado:
  ```json
  {
    "Name": "NuevoNombre",
    "Description": "Nueva descripción",
    "Abreviation": "NVO"
  }
  ```
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": {
        "Id": 1,
        "Name": "NuevoNombre",
        "Description": "Nueva descripción",
        "Abreviation": "NVO"
      },
      "errorMsg": null
    }
    ```
  - 400 BadRequest
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Todos los campos son requeridos"
    }
    ```
  - 404 NotFound
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Rol no encontrado"
    }
    ```
  - 409 Conflict
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Ya existe un rol con ese nombre"
    }
    ```
