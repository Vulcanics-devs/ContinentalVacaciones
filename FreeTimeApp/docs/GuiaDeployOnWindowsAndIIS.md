# Guía de Deploy: ASP.NET Core 9.0 en Windows Server usando IIS

Esta guía explica cómo desplegar la aplicación ASP.NET Core (tiempo-libre) en un servidor Windows sin acceso a internet, usando IIS. Incluye pasos para el build, configuración, variables de entorno y puesta en marcha.

---

## 1. Requisitos previos

- **Windows Server 2016 o superior**
- **IIS instalado** (con el módulo ASP.NET Core)
- **.NET 9.0 Runtime & Hosting Bundle** (descargado previamente desde otro equipo con internet)
- **SQL Server** (local o accesible en red)
- **Archivos de la aplicación** (build local, transferidos por USB, red, etc.)

---

## 2. Preparar el servidor

### 2.1 Instalar IIS y características necesarias

1. Abre "Administrador del servidor" > "Agregar roles y características".
2. Instala:
   - Servidor Web (IIS)
   - Características: "Gestión de IIS", "CGI", "Extensiones de aplicaciones", "ASP.NET Core Module"

### 2.2 Instalar .NET 9.0 Hosting Bundle

1. Descarga el Hosting Bundle desde https://dotnet.microsoft.com/en-us/download/dotnet/9.0 en otro equipo.
2. Transfiere el instalador al servidor (USB, red).
3. Ejecuta el instalador como administrador.
4. Reinicia IIS: `iisreset`

---

## 3. Build y publicación de la aplicación

### 3.1 ¿Dónde y cómo hacer el build de la app?

#### ¿Dónde hacer el build?
- El build debe hacerse en tu equipo de desarrollo, donde tienes instalado .NET SDK y acceso al código fuente.
- No se hace el build directamente en el servidor de producción (especialmente si no tiene acceso a internet).

#### ¿Cómo hacer el build?
1. Abre una terminal (puedes usar PowerShell, CMD o Terminal de Visual Studio Code) en la carpeta raíz del proyecto, normalmente donde está el archivo `tiempo-libre.sln`.
2. Ejecuta el siguiente comando:
   ```sh
   dotnet publish tiempo-libre.app/tiempo-libre.csproj -c Release -r win-x64 --self-contained false -o ./publish
   ```
   - `dotnet publish`: compila y prepara la app para despliegue.
   - `-c Release`: usa la configuración de Release (optimizada para producción).
   - `-r win-x64`: prepara la app para Windows 64 bits.
   - `--self-contained false`: requiere que el servidor tenga instalado el .NET Runtime.
   - `-o ./publish`: los archivos listos para producción se guardan en la carpeta `publish`.

#### ¿Qué archivos se generan?
- En la carpeta `publish` estarán todos los archivos necesarios para ejecutar la app en el servidor (DLLs, ejecutable, archivos estáticos, configuración, etc.).

#### ¿Cómo transferir los archivos al servidor?
- Copia la carpeta `publish` al servidor usando USB, red local, o cualquier método disponible.
- Ubica los archivos en una ruta como `C:\inetpub\tiempo-libre`.

#### Recomendaciones para programadores
- Si tienes dudas sobre la ruta del proyecto, busca el archivo `.csproj` dentro de la carpeta `tiempo-libre.app`.
- Si usas Visual Studio, también puedes hacer clic derecho en el proyecto y seleccionar "Publicar", pero el comando anterior es el estándar multiplataforma.

### 3.2 Transferir archivos al servidor

1. Copia la carpeta `publish` al servidor (USB, red, etc.).
2. Ubica los archivos en una ruta como `C:\inetpub\tiempo-libre`.

---

## 4. Configuración de la aplicación

### 4.1 Configurar `appsettings.json` y variables de entorno


- **¿Dónde debe ir el archivo `appsettings.json`?**
   - El archivo `appsettings.json` debe estar en la carpeta raíz de la publicación, es decir, dentro de la carpeta `publish` que copiaste al servidor.
   - Ruta típica en el servidor: `C:\inetpub\tiempo-libre\publish\appsettings.json`
   - Si usas ambientes como `Production`, puedes agregar un archivo adicional llamado `appsettings.Production.json` en la misma carpeta. Este se usará automáticamente si la variable de entorno `ASPNETCORE_ENVIRONMENT` está configurada como `Production`.

- **¿Qué hacer si necesitas cambiar la configuración?**
   - Edita el archivo `appsettings.json` directamente en la carpeta de publicación en el servidor.
   - Si actualizas la configuración, reinicia IIS para que los cambios se apliquen.

- **Recomendación:**
   - Mantén una copia de respaldo del archivo original antes de modificarlo.
   - No guardes contraseñas o datos sensibles en archivos de configuración si el servidor es accesible por otros usuarios.

---

#### Ejemplo de `appsettings.json` para SQL Server local


#### Ejemplos de cadenas de conexión en `appsettings.json`

##### 1. SQL Server local (autenticación SQL)
```json
{
   "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;Database=TiempoLibreDB;User Id=sa;Password=TuPasswordSegura;TrustServerCertificate=True;"
   }
}
```

##### 2. SQL Server local (autenticación Windows)
```json
{
   "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;Database=TiempoLibreDB;Trusted_Connection=True;TrustServerCertificate=True;"
   }
}
```

##### 3. SQL Server en red (otro servidor Windows)
```json
{
   "ConnectionStrings": {
      "DefaultConnection": "Server=192.168.1.100;Database=TiempoLibreDB;User Id=appuser;Password=PasswordSegura;TrustServerCertificate=True;"
   }
}
```

##### 4. SQL Server con instancia nombrada
```json
{
   "ConnectionStrings": {
      "DefaultConnection": "Server=SERVIDOR\INSTANCIA;Database=TiempoLibreDB;User Id=appuser;Password=PasswordSegura;TrustServerCertificate=True;"
   }
}
```

##### 5. Azure SQL Database
```json
{
   "ConnectionStrings": {
      "DefaultConnection": "Server=tuservidor.database.windows.net;Database=TiempoLibreDB;User Id=usuario@tuservidor;Password=PasswordSegura;Encrypt=True;TrustServerCertificate=False;"
   }
}
```

##### 6. SQL Server autenticación integrada (usuario de dominio)
```json
{
   "ConnectionStrings": {
      "DefaultConnection": "Server=servidor;Database=TiempoLibreDB;Integrated Security=SSPI;"
   }
}
```

##### 7. SQL Server con puerto personalizado
```json
{
   "ConnectionStrings": {
      "DefaultConnection": "Server=servidor,1444;Database=TiempoLibreDB;User Id=appuser;Password=PasswordSegura;TrustServerCertificate=True;"
   }
}
```

##### Ejemplo completo de `appsettings.json`
```json
{
   "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;Database=TiempoLibreDB;User Id=sa;Password=TuPasswordSegura;TrustServerCertificate=True;"
   },
   "Serilog": {
      "MinimumLevel": {
         "Default": "Information"
      },
      "WriteTo": [
         {
            "Name": "File",
            "Args": {
               "path": "Logs/app-log-.txt",
               "rollingInterval": "Day"
            }
         }
      ]
   },
   "Jwt": {
      "Key": "TuClaveJWTSecreta",
      "Issuer": "TiempoLibreIssuer",
      "Audience": "TiempoLibreAudience"
   },
   "Swagger": {
      "Enabled": true
   },
   "AllowedHosts": "*"
}
```

#### Ejemplo de variables de entorno en Windows

- `ConnectionStrings__DefaultConnection=Server=localhost;Database=TiempoLibreDB;User Id=sa;Password=TuPasswordSegura;TrustServerCertificate=True;`
- `ASPNETCORE_ENVIRONMENT=Production`
- `Serilog__MinimumLevel__Default=Information`

### 4.2 Permisos de carpeta

- Da permisos de escritura a la carpeta de logs para el usuario de IIS (`IIS_IUSRS`).
- Ejemplo: C:\inetpub\tiempo-libre\Logs

#### Detalles sobre el usuario IIS_IUSRS

- **¿Qué es IIS_IUSRS?**
   - Es un grupo de seguridad local creado por IIS en Windows. Los procesos de IIS y los pools de aplicaciones se ejecutan bajo cuentas que son miembros de este grupo.

- **¿Cómo identificar el usuario/grupo que usa IIS?**
   - Por defecto, los pools de aplicaciones usan la cuenta integrada `ApplicationPoolIdentity`, que es miembro de `IIS_IUSRS`.
   - Puedes verificar el usuario configurado en el pool de aplicaciones desde el "Administrador de IIS":
      1. Ve a "Pools de aplicaciones".
      2. Selecciona el pool de tu sitio.
      3. Clic derecho > "Configuración avanzada".
      4. Revisa el campo "Identidad" (por defecto: `ApplicationPoolIdentity`).

- **¿Dónde ver los miembros de IIS_IUSRS?**
   - Abre una terminal (cmd) y ejecuta:
      ```
      net localgroup IIS_IUSRS
      ```
   - Esto mostrará los usuarios/cuentas que son miembros del grupo.

- **¿Cómo saber qué permisos tiene IIS_IUSRS?**
   - En el Explorador de archivos, clic derecho sobre la carpeta (ej. `Logs`), "Propiedades" > "Seguridad".
   - Busca el grupo `IIS_IUSRS` y revisa los permisos asignados (lectura, escritura, etc.).
   - Si no aparece, puedes agregarlo manualmente.

- **¿Cómo agregar permisos para IIS_IUSRS?**
   1. Clic derecho en la carpeta > "Propiedades" > "Seguridad" > "Editar" > "Agregar".
   2. Escribe `IIS_IUSRS` y confirma.
   3. Asigna los permisos necesarios (lectura, escritura).

- **¿Cómo crear un usuario personalizado para el pool de aplicaciones?**
   1. Crea un usuario local en Windows (ejemplo: `iis_app_user`).
       - En "Usuarios y grupos locales" > "Usuarios" > "Nuevo usuario".
   2. Asigna permisos a la carpeta de la app/logs para ese usuario.
   3. En IIS, en el pool de aplicaciones, "Configuración avanzada" > "Identidad" > "Personalizado" > ingresa el usuario y contraseña.
   4. Asegúrate que el usuario tenga permisos de lectura/escritura donde sea necesario.

- **¿Cómo verificar qué usuario está usando el pool de aplicaciones?**
   - En "Administrador de IIS" > "Pools de aplicaciones" > selecciona el pool > "Configuración avanzada" > "Identidad".
   - Si es `ApplicationPoolIdentity`, el proceso usará una cuenta virtual que es miembro de `IIS_IUSRS`.

- **Notas de seguridad:**
   - No des permisos de escritura a carpetas sensibles o a todo el disco.
   - Limita los permisos solo a las carpetas necesarias (ejemplo: `Logs`, archivos temporales).
   - Si usas un usuario personalizado, asegúrate de que solo tenga los permisos mínimos requeridos.

---

## 5. Configuración de SQL Server local

1. Instala SQL Server Express o Standard en el servidor.
2. Crea la base de datos `TiempoLibreDB`.
3. Crea el usuario y asigna permisos de lectura/escritura.
4. Si usas autenticación de Windows, ajusta la cadena de conexión:
    - `Server=localhost;Database=TiempoLibreDB;Trusted_Connection=True;`
5. Si usas autenticación SQL, usa el usuario y contraseña configurados.
6. Asegúrate de que el puerto 1433 esté habilitado si la app y SQL Server están en máquinas distintas.
---

### 4.2 Permisos de carpeta

- Da permisos de escritura a la carpeta de logs para el usuario de IIS (`IIS_IUSRS`).
- Ejemplo: C:\inetpub\tiempo-libre\Logs

---

## 5. Configuración de IIS

### 5.1 Crear el sitio en IIS

1. Abre "Administrador de IIS".
2. Clic derecho en "Sitios" > "Agregar sitio web".
3. Configura:
   - Nombre: `tiempo-libre`
   - Ruta física: `C:\inetpub\tiempo-libre\publish`
   - Puerto: 80 (o el que desees)
   - Hostname: (opcional)
4. En "Pool de aplicaciones":
   - Usa .NET CLR "No administrado" (ASP.NET Core usa Kestrel internamente)
   - Configura el pool para usar el usuario adecuado si necesitas acceso a recursos de red

### 5.2 Configurar permisos

- Verifica que el usuario del pool de aplicaciones tenga acceso de lectura/escritura a la carpeta de la app y logs.

### 5.3 Configurar bindings HTTPS (opcional)

- Si usas HTTPS, instala el certificado y configura el binding en IIS.

---

## 6. Verificación y puesta en marcha

1. Reinicia IIS: `iisreset`
2. Accede a la URL configurada (ej: http://localhost/ o http://<tu-servidor>/)
3. Verifica que la app levante y que los logs se generen correctamente.
4. Revisa el visor de eventos de Windows y los logs de Serilog ante cualquier error.

---

## 7. Solución de problemas comunes

- **Error 500:** Revisa los logs de la app y el visor de eventos.
- **No se encuentra .NET Core:** Verifica que el Hosting Bundle esté instalado.
- **Problemas de conexión a SQL Server:** Revisa la cadena de conexión y los permisos de red/firewall.
- **Permisos de escritura:** Asegúrate que la carpeta de logs y la app tengan permisos para el usuario de IIS.

---

## 8. Actualizaciones y mantenimiento

- Para actualizar la app, repite el proceso de build y reemplaza los archivos en el servidor.
- Mantén respaldos de `appsettings.json` y los logs.

---

**Notas:**
- No se requiere acceso a internet en el servidor para ejecutar la app, pero sí para descargar el Hosting Bundle y transferir archivos.
- No se usa Docker en este escenario.

---


---

## Preguntas frecuentes (FAQ) y enlaces útiles

### 1. ¿Dónde va el archivo `appsettings.json`?
- Debe estar en la carpeta raíz de la publicación (`publish`) en el servidor. Ejemplo: `C:\inetpub\tiempo-libre\publish\appsettings.json`
- Más detalles: [Configuración de la aplicación](#41-configurar-appsettingsjson-y-variables-de-entorno)

### 2. ¿Cómo cambio la cadena de conexión a la base de datos?
- Edita el archivo `appsettings.json` en el servidor y reinicia IIS.
- Ejemplos de cadenas de conexión: [Ejemplos de cadenas de conexión en appsettings.json](#ejemplos-de-cadenas-de-conexion-en-appsettingsjson)

### 3. ¿Qué hago si la app muestra error 500?
- Revisa los logs en la carpeta `Logs` y el visor de eventos de Windows.
- Más información: [Solución de problemas comunes](#7-solucion-de-problemas-comunes)

### 4. ¿Cómo sé qué usuario usa el pool de aplicaciones de IIS?
- Revisa la "Identidad" en la configuración avanzada del pool de aplicaciones en IIS.
- Más detalles: [Detalles sobre el usuario IIS_IUSRS](#detalles-sobre-el-usuario-iis_iusrs)

### 5. ¿Cómo agrego permisos de escritura para la app?
- Ve a "Propiedades" > "Seguridad" en la carpeta y agrega el grupo `IIS_IUSRS` o el usuario personalizado.
- Más detalles: [Permisos de carpeta](#42-permisos-de-carpeta)

### 6. ¿Dónde encuentro documentación oficial de .NET y IIS?
- [.NET Docs - Deploy to IIS](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-9.0)
- [Microsoft Docs - IIS](https://learn.microsoft.com/en-us/iis/)
- [Guía oficial de configuración de appsettings.json](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-9.0)

### 7. ¿Cómo publico desde Visual Studio?
- Puedes hacer clic derecho en el proyecto y seleccionar "Publicar". Más detalles en la documentación oficial: [Publicar en IIS desde Visual Studio](https://learn.microsoft.com/es-mx/aspnet/core/tutorials/publish-to-iis/?view=aspnetcore-9.0)

### 8. ¿Cómo configuro HTTPS en IIS?
- Instala el certificado en el servidor y configura el binding HTTPS en el sitio de IIS. Más información: [Configurar HTTPS en IIS](https://learn.microsoft.com/en-us/iis/manage/configuring-security/how-to-set-up-ssl-on-iis)

### 9. ¿Cómo cambio el ambiente de la app (Development/Production)?
- Configura la variable de entorno `ASPNETCORE_ENVIRONMENT` en el servidor. Más detalles: [Variables de entorno](#ejemplo-de-variables-de-entorno-en-windows)

### 10. ¿Dónde encuentro ayuda sobre errores específicos?
- Busca el mensaje de error en los logs y consulta la documentación oficial o foros como Stack Overflow.

---
