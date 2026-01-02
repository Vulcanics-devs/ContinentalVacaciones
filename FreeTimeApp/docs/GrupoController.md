# GrupoController

## POST /api/Grupo
- Descripción: Crea un grupo (solo SuperUsuario). Valida campos requeridos y líderes.
- Headers: Authorization: Bearer {token}
- Body esperado:
  ```json
  {
    "Rol": "Lider",
    "AreaId": 2,
    "IdentificadorSAP": "G-01",
    "PersonasPorTurno": 10,
    "DuracionDeturno": 8,
    "LiderId": 5,
    "LiderSuplenteId": 6
  }
  ```
- Ejemplo de respuesta:
  - 201 Created
    ```json
    {
      "success": true,
      "data": {
        "GrupoId": 1,
        "Rol": "Lider",
        "AreaId": 2,
        "UnidadOrganizativaSap": "SAP-01",
        "NombreGeneral": "Area Producción",
        "IdentificadorSAP": "G-01",
        "PersonasPorTurno": 10,
        "DuracionDeturno": 8,
        "LiderId": 5,
        "LiderSuplenteId": 6
      },
      "errorMsg": null
    }
    ```
  - 400 BadRequest
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "Datos requeridos faltantes o inválidos"
    }
    ```
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "El líder especificado no existe"
    }
    ```
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "El líder suplente especificado no existe"
    }
    ```

## PUT /api/Grupo/{id}/Lider
- Descripción: Asigna el líder principal a un grupo. Solo SuperUsuario.
- Headers: Authorization: Bearer {token}
- Body esperado:
  ```json
  5
  ```
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": {
        "GrupoId": 1,
        "Rol": "Lider",
        "AreaId": 2,
        "UnidadOrganizativaSap": "SAP-01",
        "NombreGeneral": "Area Producción",
        "IdentificadorSAP": "G-01",
        "PersonasPorTurno": 10,
        "DuracionDeturno": 8,
        "LiderId": 5,
        "LiderSuplenteId": 6
      },
      "errorMsg": null
    }
    ```
  - 400 BadRequest
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "El líder especificado no existe"
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

## PUT /api/Grupo/{id}/LiderSuplente
- Descripción: Asigna el líder suplente a un grupo. Solo SuperUsuario.
- Headers: Authorization: Bearer {token}
- Body esperado:
  ```json
  6
  ```
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": {
        "GrupoId": 1,
        "Rol": "Lider",
        "AreaId": 2,
        "UnidadOrganizativaSap": "SAP-01",
        "NombreGeneral": "Area Producción",
        "IdentificadorSAP": "G-01",
        "PersonasPorTurno": 10,
        "DuracionDeturno": 8,
        "LiderId": 5,
        "LiderSuplenteId": 6
      },
      "errorMsg": null
    }
    ```
  - 400 BadRequest
    ```json
    {
      "success": false,
      "data": null,
      "errorMsg": "El líder suplente especificado no existe"
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

## GET /api/Grupo/{id}
- Descripción: Detalle de un grupo, incluye datos y referencias de área y líderes.
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": {
        "GrupoId": 1,
        "Rol": "Lider",
        "AreaId": 2,
        "UnidadOrganizativaSap": "SAP-01",
        "NombreGeneral": "Area Producción",
        "IdentificadorSAP": "G-01",
        "PersonasPorTurno": 10,
        "DuracionDeturno": 8,
        "LiderId": 5,
        "LiderSuplenteId": 6
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

## GET /api/Grupo
- Descripción: Lista todos los grupos con detalles completos de área y líderes.
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": [
        {
          "GrupoId": 1,
          "Rol": "Lider",
          "AreaId": 2,
          "UnidadOrganizativaSap": "SAP-01",
          "NombreGeneral": "Area Producción",
          "IdentificadorSAP": "G-01",
          "PersonasPorTurno": 10,
          "DuracionDeturno": 8,
          "LiderId": 5,
          "LiderSuplenteId": 6
        }
      ],
      "errorMsg": null
    }
    ```

## GET /api/Grupo/Area/{areaId}
- Descripción: Lista los grupos de un área específica.
- Ejemplo de respuesta:
  - 200 OK
    ```json
    {
      "success": true,
      "data": [
        {
          "GrupoId": 1,
          "Rol": "Lider",
          "AreaId": 2,
          "UnidadOrganizativaSap": "SAP-01",
          "NombreGeneral": "Area Producción",
          "IdentificadorSAP": "G-01",
          "PersonasPorTurno": 10,
          "DuracionDeturno": 8,
          "LiderId": 5,
          "LiderSuplenteId": 6
        }
      ],
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
