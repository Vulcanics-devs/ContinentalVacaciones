# UserController

## PATCH /api/User/update-maquina/{id}
- Descripción: Modifica el campo `Maquina` de un usuario. Solo pueden modificar usuarios con los roles: Super_Usuario, Jefe_De_Area, Lider_De_Grupo, Ingeniero_Industrial. El usuario objetivo debe tener el rol Empleado_Sindicalizado.
- Headers: Authorization: Bearer {token}
- Body esperado:
  ```json
  {
    "Maquina": "PC-01"
  }
  ```
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": {
        "Id": 1,
        "FullName": "Nombre Completo",
        "Username": "usuario1",
        "Maquina": "PC-01",
        "UpdatedAt": "2025-08-27T12:30:00Z",
        "UpdatedBy": "admin"
      },
      "errorMsg": null
    }
    ```
  - 400 BadRequest
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Solo se puede asignar máquina a usuarios con rol Empleado_Sindicalizado"
    }
    ```
  - 403 Forbidden
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "No autorizado"
    }
    ```
  - 401 Unauthorized
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "No hay sesión iniciada"
    }
    ```
  - 404 NotFound
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Usuario no encontrado"
    }
    ```

## PATCH /api/User/update/{id}
- Descripción: Modifica los datos de un usuario. Solo el propio usuario o un SuperUsuario pueden modificar los datos. Valida área, grupo y roles.
- Headers: Authorization: Bearer {token}
- Body esperado:
  ```json
  {
    "Username": "usuario1",
    "FullName": "Nombre Completo",
    "AreaId": 1,
    "GrupoId": 2,
    "Roles": [1, 2]
  }
  ```
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": {
        "Id": 1,
        "FullName": "Nombre Completo",
        "Username": "usuario1",
        "AreaId": 1,
        "GrupoId": 2,
        "Status": "Activo",
        "CreatedAt": "2025-08-27T12:00:00Z",
        "CreatedBy": "admin",
        "UpdatedAt": "2025-08-27T12:30:00Z",
        "UpdatedBy": "admin"
      },
      "errorMsg": null
    }
    ```
  - 400 BadRequest
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Uno o más roles no son válidos"
    }
    ```
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Área no válida"
    }
    ```
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Grupo no válido"
    }
    ```
  - 403 Forbidden
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "No autorizado"
    }
    ```
  - 404 NotFound
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Usuario no encontrado"
    }
    ```

## POST /api/User/usuarios-por-rol
- Descripción: Retorna el listado de usuarios activos que tienen el rol especificado. El rol puede enviarse como entero (id) o como string (nombre), ambos deben corresponder a un valor válido del enum `RolEnum`.
- Headers: Authorization: Bearer {token}
- Body esperado:
  ```json
  {
    "RolInt": 1,
    "RolString": "Super_Usuario"
  }
  ```
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": [
        {
          "Id": 1,
          "FullName": "Nombre Completo",
          "Username": "usuario1",
          "UnidadOrganizativaSap": "SAP1",
          "Rol": "Super_Usuario"
        }
      ],
      "errorMsg": null
    }
    ```
  - 400 BadRequest
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Se debe proporcionar un rol válido (string o int)"
    }
    ```
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "El rol proporcionado no es válido"
    }
    ```

## PATCH /api/User/change-status/{id}
- Descripción: Modifica el estatus de un usuario. Solo SuperUsuario. El estatus se envía como entero según el enum UserStatus.
- Headers: Authorization: Bearer {token}
- Body esperado:
  ```json
  {
    "NewStatus": 1
  }
  ```
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": {
        "msg": "Estatus actualizado correctamente."
      },
      "errorMsg": null
    }
    ```
    ```json
    {
      "success": true,
      "data": {
        "msg": "El estatus ya es el mismo, no se realizaron cambios."
      },
      "errorMsg": null
    }
    ```
  - 400 BadRequest
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "El estatus proporcionado no es válido"
    }
    ```
  - 404 NotFound
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Usuario no encontrado"
    }
    ```

## GET /api/User/detail/{id}
- Descripción: Retorna los datos generales del usuario basado en el Id recibido.
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": {
        "Id": 1,
        "FullName": "Nombre Completo",
        "Username": "usuario1",
        "AreaId": 1,
        "GrupoId": 2,
        "Status": "Activo",
        "CreatedAt": "2025-08-27T12:00:00Z",
        "CreatedBy": "admin",
        "UpdatedAt": "2025-08-27T12:30:00Z",
        "UpdatedBy": "admin"
      },
      "errorMsg": null
    }
    ```
  - 404 NotFound
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Usuario no encontrado"
    }
    ```

## GET /api/User/usuarios-por-area/{areaId}
- Descripción: Lista los usuarios relacionados a una área específica.
- Headers: Authorization: Bearer {token}
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": [
        {
          "FullName": "Nombre Completo",
          "Username": "usuario1",
          "UnidadOrganizativaSap": "SAP1",
          "Rol": "RolGrupo"
        }
      ],
      "errorMsg": null
    }
    ```

## GET /api/User/usuarios-por-grupo/{grupoId}
- Descripción: Lista los usuarios relacionados a un grupo específico (excluye empleados sindicalizados).
- Headers: Authorization: Bearer {token}
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": [
        {
          "FullName": "Nombre Completo",
          "Username": "usuario1",
          "UnidadOrganizativaSap": "SAP1",
          "Rol": "RolGrupo"
        }
      ],
      "errorMsg": null
    }
    ```

## POST /api/User/empleados-sindicalizados
- Descripción: Lista los usuarios con rol Empleado_Sindicalizado de un área específica, opcionalmente filtrando por grupo.
- Headers: Authorization: Bearer {token}
- Body esperado:
  ```json
  {
    "AreaId": 1,
    "GrupoId": 2
  }
  ```
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": [
        {
          "FullName": "Nombre Completo",
          "Username": "usuario1",
          "UnidadOrganizativaSap": "SAP1",
          "Rol": "Empleado_Sindicalizado"
        }
      ],
      "errorMsg": null
    }
    ```
  - 400 BadRequest
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "ÁreaId es requerido y debe ser mayor a 0"
    }
    ```

## GET /api/User/profile
- Descripción: Retorna el perfil del usuario autenticado, incluyendo área, grupo, los campos adicionales `Maquina` y `FechaIngreso`, y la lista de roles con detalles (`rols`).
- Headers: Authorization: Bearer {token}
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": {
        "Id": 1,
        "FullName": "Nombre Completo",
        "Username": "usuario1",
        "Area": {
          "AreaId": 1,
          "UnidadOrganizativaSap": "SAP1",
          "NombreGeneral": "Area1"
        },
        "Grupo": {
          "GrupoId": 2,
          "Rol": "RolGrupo",
          "IdentificadorSAP": "SAP-G2"
        },
        "Status": "Activo",
        "CreatedAt": "2025-08-27T12:00:00Z",
        "CreatedBy": "admin",
        "UpdatedAt": "2025-08-27T12:30:00Z",
        "UpdatedBy": "admin",
        "Maquina": "PC-01",
        "FechaIngreso": "2025-08-27T08:00:00Z",
        "rols": [
          {
            "Name": "SuperUsuario",
            "Description": "Usuario con todos los permisos",
            "Abreviation": "SUP"
          },
          {
            "Name": "Empleado_Sindicalizado",
            "Description": "Empleado sindicalizado",
            "Abreviation": "EMP"
          }
        ]
      },
      "errorMsg": null
    }
    ```
  - 401 Unauthorized
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "No autorizado"
    }
    ```
  - 404 NotFound
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Usuario no encontrado"
    }
    ```
