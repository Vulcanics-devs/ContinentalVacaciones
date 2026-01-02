# Tiempo Libre API

Una API REST desarrollada con ASP.NET Core para gestión de usuarios y roles.

## Stack Tecnológico

### Backend
- **Framework**: ASP.NET Core Web API
- **Versión de .NET**: .NET 9.0
- **Lenguaje**: C#

### Base de Datos
- **Motor de Base de Datos**: Microsoft SQL Server 2022
- **ORM**: Entity Framework Core 9.0.7

### Containerización
- **Orquestación**: Docker Compose
- **Base de Datos en Contenedor**: SQL Server 2022 (mcr.microsoft.com/mssql/server:2022-latest)

### Documentación de API
- **Framework de Documentación**: Swagger/OpenAPI
- **Implementación**: Swashbuckle.AspNetCore

## Frameworks, Librerías y Dependencias

### Dependencias Principales

| Paquete | Versión | Descripción | Documentación |
|---------|---------|-------------|---------------|
| **Microsoft.AspNetCore.OpenApi** | 9.0.x | Soporte para OpenAPI en ASP.NET Core | [Documentación](https://learn.microsoft.com/en-us/aspnet/core/web-api/openapi) |
| **Microsoft.EntityFrameworkCore** | 9.0.x | ORM para .NET | [Documentación](https://learn.microsoft.com/en-us/ef/core/) |
| **Microsoft.EntityFrameworkCore.SqlServer** | 9.0.x | Proveedor de SQL Server para EF Core | [Documentación](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/) |
| **Microsoft.EntityFrameworkCore.Tools** | 9.0.x | Herramientas de línea de comandos para EF Core | [Documentación](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) |
| **Swashbuckle.AspNetCore** | 9.0.x | Generador de documentación Swagger para ASP.NET Core | [Documentación](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) |

### Tecnologías de Desarrollo

| Tecnología | Descripción | Enlace |
|------------|-------------|--------|
| **ASP.NET Core** | Framework web de Microsoft para aplicaciones modernas | [Sitio Oficial](https://learn.microsoft.com/en-us/aspnet/core/) |
| **Entity Framework Core** | ORM moderno para .NET | [Sitio Oficial](https://learn.microsoft.com/en-us/ef/core/) |
| **SQL Server** | Sistema de gestión de base de datos relacional | [Sitio Oficial](https://www.microsoft.com/en-us/sql-server) |
| **Docker** | Plataforma de containerización | [Sitio Oficial](https://www.docker.com/) |
| **Docker Compose** | Herramienta para definir aplicaciones multi-contenedor | [Documentación](https://docs.docker.com/compose/) |

## Prerequisitos

Antes de ejecutar la aplicación, asegúrate de tener instalado:

- [Docker](https://www.docker.com/get-started) (versión 20.0 o superior)
- [Docker Compose](https://docs.docker.com/compose/install/) (versión 2.0 o superior)

## Construcción y Ejecución del Contenedor

### 1. Configuración de Variables de Entorno

El proyecto incluye un archivo `.env_dev` con las configuraciones necesarias. Para que Docker Compose las cargue automáticamente, copia el archivo a `.env`:

```bash
cp .env_dev .env
```

Las variables incluyen:
```env
DB_NAME=FreeTime
DB_PASSWORD=YourStrong@Passw0rd
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5050
```

### 2. Nota Importante sobre Docker

⚠️ **Actualmente el proyecto no incluye un `Dockerfile`**, aunque el `docker-compose.yml` hace referencia a uno. Para ejecutar la aplicación, tienes las siguientes opciones:

#### Opción A: Desarrollo Local (Recomendado)

```bash
# 1. Ejecutar solo la base de datos con Docker
docker compose up db -d

# 2. Ejecutar la aplicación localmente
dotnet run
```

#### Opción B: Crear un Dockerfile

Si deseas usar Docker Compose completamente, necesitarás crear un `Dockerfile` en la raíz del proyecto:

```dockerfile
# Ejemplo de Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5050

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["tiempo-libre.csproj", "."]
RUN dotnet restore "tiempo-libre.csproj"
COPY . .
RUN dotnet build "tiempo-libre.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "tiempo-libre.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "tiempo-libre.dll"]
```

### 3. Ejecutar la Aplicación (Opción A - Recomendada)

```bash
# 1. Cargar variables de entorno
cp .env_dev .env

# 2. Ejecutar solo la base de datos
docker compose up db -d

# 3. Verificar que la base de datos esté lista (esperar que aparezca "Recovery is complete")
docker compose logs db -f

# 4. En otra terminal, ejecutar la aplicación .NET localmente
dotnet run
```

### 4. Verificar que la Aplicación Funciona

Una vez que veas el mensaje "Now listening on: http://localhost:5050", puedes verificar:

```bash
# Probar el endpoint de prueba
curl http://localhost:5050/WeatherForecast/test

# Resultado esperado: {"mensaje":"Este es un endpoint de prueba"}

# Acceder a Swagger UI desde el navegador
open http://localhost:5050/swagger
```

### 3. Servicios Disponibles

El archivo `docker-compose.yml` define dos servicios:

- **webapi**: La aplicación ASP.NET Core
  - Puerto: `5050` (HTTP)
  - Puerto adicional: `5055`
  
- **db**: Base de datos SQL Server 2022
  - Puerto: `1433`
  - Credenciales definidas en `.env_dev`

### 4. Verificar el Estado

Para verificar que los contenedores están ejecutándose correctamente:

```bash
# Ver el estado de los contenedores
docker-compose ps

# Ver los logs de la aplicación
docker-compose logs webapi

# Ver los logs de la base de datos
docker-compose logs db
```


## Restaurar la base de datos en el contenedor SQL Server

Para restaurar una base de datos desde un archivo `.bak` en el contenedor de SQL Server, sigue estos pasos:

### 1. Instalar herramientas necesarias en el contenedor

Accede al contenedor de la base de datos:

```bash
docker compose exec db bash
```

Dentro del contenedor, instala las herramientas necesarias (por ejemplo, `curl`, `unzip`, etc. si necesitas descargar o manipular archivos):

```bash
apt-get update && apt-get install -y curl unzip
```

### 2. Copiar el archivo .bak al contenedor

Desde tu máquina local, copia el archivo `.bak` al contenedor usando `docker cp`:

```bash
docker cp /ruta/local/tu_archivo.bak <nombre_contenedor_db>:/var/opt/mssql/backup/tu_archivo.bak
```

Puedes obtener el nombre del contenedor con:

```bash
docker ps
```

### 3. Restaurar la base de datos desde el archivo .bak

Accede al contenedor y usa `sqlcmd` para restaurar la base de datos:

```bash
docker compose exec db bash
```

Ejecuta el siguiente comando dentro del contenedor (ajusta los nombres según tu caso):

```bash
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "YourStrong@Passw0rd" \
  -Q "RESTORE DATABASE [FreeTime] FROM DISK = '/var/opt/mssql/backup/tu_archivo.bak' WITH REPLACE"
```

### Notas
- El directorio `/var/opt/mssql/backup/` es el recomendado para archivos de respaldo en contenedores SQL Server.
- Asegúrate de que el usuario y contraseña coincidan con los definidos en tu `.env`.
- Puedes instalar otras herramientas según lo que necesites manipular dentro del contenedor.

### URL de Swagger UI

Una vez que la aplicación esté ejecutándose, puedes acceder a la documentación interactiva de la API en:

```
http://localhost:5050/swagger
```

### Características de Swagger

- **Documentación Interactiva**: Explora todos los endpoints disponibles
- **Pruebas en Vivo**: Ejecuta peticiones directamente desde la interfaz
- **Esquemas de Datos**: Visualiza los modelos de datos y sus propiedades
- **Especificación OpenAPI**: Descarga la especificación en formato JSON

### Endpoints Disponibles

Actualmente la API incluye:

| Endpoint | Método | Descripción |
|----------|--------|-------------|
| `/WeatherForecast/test` | GET | Endpoint de prueba que retorna un mensaje |

### Swagger JSON

La especificación OpenAPI en formato JSON está disponible en:

```
http://localhost:5050/swagger/v1/swagger.json
```

## Estructura del Proyecto

```
tiempo-libre/
├── Controllers/           # Controladores de la API
├── Models/               # Modelos de datos y entidades
├── Repositories/         # Patrones de repositorio
├── Properties/           # Configuraciones del proyecto
├── appsettings.json     # Configuración de la aplicación
├── docker-compose.yml   # Configuración de Docker Compose
├── .env_dev            # Variables de entorno
├── Program.cs          # Punto de entrada de la aplicación
└── tiempo-libre.csproj # Archivo del proyecto .NET
```

## Modelos de Datos

### User (Usuario)
- Propiedades: Id, FullName, Username, PasswordHash, Roles, Status
- Validaciones personalizadas para Username
- Estados: Activo, Desactivado, Suspendido

### Rol (Rol)
- Propiedades: Id, Name, Description, Abreviation
- Validaciones de longitud y formato

## Comandos Útiles

### Para Desarrollo Local + Base de Datos en Docker

```bash
# Cargar variables de entorno
cp .env_dev .env

# Ejecutar solo la base de datos
docker compose up db -d

# Ver logs de la base de datos
docker compose logs db -f

# Detener la base de datos
docker compose down

# Ejecutar la aplicación .NET
dotnet run

# Ejecutar migraciones manualmente
dotnet ef database update
```

### Si tienes un Dockerfile completo

```bash
# Ejecutar toda la aplicación con Docker Compose
docker compose up --build

# Ejecutar en segundo plano
docker compose up -d --build

# Detener los contenedores
docker compose down

# Reconstruir solo la aplicación
docker compose build webapi

# Ver logs en tiempo real
docker compose logs -f

# Ejecutar comandos dentro del contenedor de la aplicación
docker compose exec webapi bash

# Limpiar volúmenes (CUIDADO: esto eliminará los datos de la BD)
docker compose down -v
```


## Ejecución local con perfil específico (HTTP/HTTPS)

Puedes ejecutar la aplicación localmente usando diferentes perfiles de entorno y protocolos:

### HTTP (por defecto)

```bash
dotnet run --launch-profile "http"
```

Esto usará la configuración definida en `Properties/launchSettings.json` bajo el perfil `http`.

### HTTPS

```bash
dotnet run --launch-profile "https"
```

Esto usará el perfil `https` y expondrá el endpoint seguro. Asegúrate de tener los certificados configurados en tu entorno local.

Puedes personalizar los perfiles en `Properties/launchSettings.json` para definir puertos, variables de entorno y certificados.

#### Ejemplo de configuración en `launchSettings.json`:

```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "applicationUrl": "http://localhost:5050"
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "applicationUrl": "https://localhost:5051"
    }
  }
}
```

### Pasos generales para desarrollo local

1. Asegúrate de tener SQL Server ejecutándose localmente
2. Actualiza la cadena de conexión en `appsettings.Development.json`
3. Ejecuta las migraciones: `dotnet ef database update`
4. Inicia la aplicación con el perfil deseado:
   - HTTP: `dotnet run --launch-profile "http"`
   - HTTPS: `dotnet run --launch-profile "https"`

## Notas Importantes

- La aplicación está configurada para ejecutarse solo en HTTP en el contenedor
- Las migraciones de Entity Framework se ejecutan automáticamente al iniciar
- Los datos de la base de datos se persisten en un volumen Docker llamado `sql-server-data`
- El proyecto usa .NET 9.0 y Entity Framework Core 9.x con las características más recientes habilitadas