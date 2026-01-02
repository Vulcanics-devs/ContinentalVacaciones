# AuthController

## POST /Auth/login
- Descripción: Inicia sesión y retorna un JWT.
- Body esperado:
  ```json
  {
    "Username": "usuario1",
    "Password": "miPassword"
  }
  ```
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": {
        "Token": "jwt_token_string",
        "Expiration": "2025-08-27T12:34:56Z"
      },
      "errorMsg": null
    }
    ```
  - 401 Unauthorized
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Usuario o contraseña incorrectos"
    }
    ```

## POST /Auth/refresh-token
- Descripción: Renueva el JWT para el usuario autenticado.
- Headers: Authorization: Bearer {token}
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": {
        "Token": "nuevo_jwt_token_string",
        "Expiration": "2025-08-27T13:00:00Z"
      },
      "errorMsg": null
    }
    ```
  - 401 Unauthorized
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "No hay sesión iniciada."
    }
    ```

## POST /Auth/logout
- Descripción: Cierra la sesión y retorna un token especial de sesión cerrada.
- Headers: Authorization: Bearer {token}
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": {
        "Token": "jwt_logout_token_string",
        "Expiration": "2035-08-27T12:34:56Z"
      },
      "errorMsg": null
    }
    ```
  - 401 Unauthorized
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Sesión cerrada. El token corresponde a una sesión cerrada."
    }
    ```

## POST /Auth/change-password
- Descripción: Cambia la contraseña del usuario autenticado.
- Headers: Authorization: Bearer {token}
- Body esperado:
  ```json
  {
    "CurrentPassword": "miPasswordActual",
    "NewPassword": "miNuevoPassword",
    "ConfirmNewPassword": "miNuevoPassword"
  }
  ```
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": "Contraseña actualizada correctamente.",
      "errorMsg": null
    }
    ```
  - 400 BadRequest
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "La contraseña actual es incorrecta."
    }
    ```

## POST /Auth/change-user-password
- Descripción: SuperUsuario cambia la contraseña de otro usuario.
- Headers: Authorization: Bearer {token}
- Body esperado:
  ```json
  {
    "UserId": 1,
    "NewPassword": "nuevoPassword",
    "ConfirmNewPassword": "nuevoPassword"
  }
  ```
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": "Contraseña actualizada correctamente.",
      "errorMsg": null
    }
    ```
  - 400 BadRequest / 404 NotFound
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Usuario no encontrado."
    }
    ```

## POST /Auth/register
- Descripción: Registra un nuevo usuario (solo SuperUsuario).
- Body esperado:
  ```json
  {
    "Username": "usuario2",
    "Password": "password123",
    "FullName": "Juan Pérez",
    "AreaId": 1,
    "GrupoId": 2,
    "Roles": ["SuperUsuario", "Empleado_Sindicalizado"]
  }
  ```
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": "Usuario creado exitosamente.",
      "errorMsg": null
    }
    ```
  - 400 BadRequest
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Mensaje de error"
    }
    ```
