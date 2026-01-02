# AreaController

## POST /api/Area
- Descripción: Crea un área (solo SuperUsuario).
- Body esperado:
  ```json
  {
    "AreaId": 1,
    "UnidadOrganizativaSap": "SAP-01",
    "NombreGeneral": "Producción"
  }
  ```
- Ejemplo de respuesta:
  - 201 Created
    ```json
    {
      "success": true,
      "data": {
        "AreaId": 1,
        "UnidadOrganizativaSap": "SAP-01",
        "NombreGeneral": "Producción"
      },
      "errorMsg": null
    }
    ```
  - 400 BadRequest
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "El área es requerida"
    }
    ```

## GET /api/Area/{id}
- Descripción: Detalle de un área.
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": {
        "AreaId": 1,
        "UnidadOrganizativaSap": "SAP-01",
        "NombreGeneral": "Producción"
      },
      "errorMsg": null
    }
    ```
  - 404 NotFound
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Área no encontrada"
    }
    ```

## GET /api/Area
- Descripción: Lista todas las áreas.
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": [
        {
          "AreaId": 1,
          "UnidadOrganizativaSap": "SAP-01",
          "NombreGeneral": "Producción"
        }
      ],
      "errorMsg": null
    }
    ```

## DELETE /api/Area/{areaId}/Grupo/{grupoId}
- Descripción: Elimina un grupo de un área (solo SuperUsuario).
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": {
        "AreaId": 1,
        "Grupos": []
      },
      "errorMsg": null
    }
    ```
  - 404 NotFound
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Grupo no encontrado"
    }
    ```

## POST /api/Area/{areaId}/Grupo
- Descripción: Agrega un grupo a un área (solo SuperUsuario).
- Body esperado:
  ```json
  {
    "rol": "Lider"
  }
  ```
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": {
        "AreaId": 1,
        "Grupos": [
          {
            "GrupoId": 1,
            "Rol": "Lider"
          }
        ]
      },
      "errorMsg": null
    }
    ```
  - 404 NotFound
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Área no encontrada"
    }
    ```

## PUT /api/Area/{id}
- Descripción: Actualiza datos de un área existente. Solo SuperUsuario. Valida duplicados.
- Headers: Authorization: Bearer {token}
- Body esperado:
  ```json
  {
    "UnidadOrganizativaSap": "SAP-01",
    "NombreGeneral": "Producción"
  }
  ```
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": {
        "AreaId": 1,
        "UnidadOrganizativaSap": "SAP-01",
        "NombreGeneral": "Producción"
      },
      "errorMsg": null
    }
    ```
  - 400 BadRequest
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "UnidadOrganizativaSap y NombreGeneral son requeridos"
    }
    ```
  - 404 NotFound
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Área no encontrada"
    }
    ```

## PATCH /api/Area/{id}/asignar-jefes
- Descripción: Asigna/desasigna jefe y/o jefe suplente. Solo SuperUsuario. Valida roles.
- Headers: Authorization: Bearer {token}
- Body esperado:
  ```json
  {
    "JefeId": 5,
    "JefeSuplenteId": 6
  }
  ```
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": {
        "AreaId": 1,
        "UnidadOrganizativaSap": "SAP-01",
        "NombreGeneral": "Producción",
        "JefeId": 5,
        "JefeSuplenteId": 6
      },
      "errorMsg": null
    }
    ```
  - 400 BadRequest
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "El jefe especificado no existe"
    }
    ```
  - 404 NotFound
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Área no encontrada"
    }
    ```
