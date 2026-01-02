# Documentación del Sistema de Gestión de Vacaciones - Continental

## Índice
1. [Descripción General](#descripción-general)
2. [Arquitectura del Sistema](#arquitectura-del-sistema)
3. [Autenticación y Seguridad](#autenticación-y-seguridad)
4. [Endpoints de la API](#endpoints-de-la-api)
5. [Flujos de Trabajo Principales](#flujos-de-trabajo-principales)
6. [Configuración del Sistema](#configuración-del-sistema)

---

## Descripción General

El **Sistema de Gestión de Vacaciones** es una plataforma integral desarrollada para Continental que permite administrar y coordinar las vacaciones del personal de manera eficiente, asegurando la cobertura operativa continua mientras se respetan los derechos vacacionales de los empleados.

### Características Principales

- **Gestión de Bloques de Vacaciones**: Sistema rotativo que divide a los empleados en bloques para mantener la operación
- **Programación Anual**: Planificación anticipada de vacaciones para todo el año
- **Gestión por Antigüedad**: Asignación de días según la antigüedad del empleado
- **Control de Cobertura**: Validación automática de porcentajes mínimos de personal
- **Reprogramación Flexible**: Solicitudes de cambios y ajustes de fechas
- **Festivos Trabajados**: Gestión de días festivos trabajados y su compensación
- **Notificaciones Automáticas**: Sistema de alertas y recordatorios por email

### Roles del Sistema

- **SuperUsuario**: Acceso total al sistema
- **Administrador**: Gestión completa de su área
- **JefeArea**: Aprobación y gestión de su área
- **Ingeniero**: Gestión de múltiples áreas asignadas
- **LiderGrupo**: Gestión de su grupo específico
- **Empleado**: Consulta y solicitudes personales

---

## Arquitectura del Sistema

### Tecnologías Utilizadas
- **Backend**: .NET 9 con C#
- **Base de Datos**: SQL Server
- **Autenticación**: JWT Bearer Token
- **Logging**: Serilog
- **Email**: SMTP con configuración empresarial

### Estructura de Base URL
```
http://localhost:5050/api/
```

### Formato de Respuesta Estándar
```json
{
  "success": true|false,
  "data": { ... },
  "errorMsg": "mensaje de error si aplica"
}
```

---

## Autenticación y Seguridad

### Login
**POST** `/api/auth/login`

Autentica a un usuario y devuelve un token JWT.

**Request:**
```json
{
  "username": "nomina_empleado",
  "password": "contraseña"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "userId": 123,
    "username": "12345",
    "fullName": "Juan Pérez",
    "roles": ["Empleado", "LiderGrupo"],
    "expiresIn": 86400
  }
}
```

### Logout
**POST** `/api/auth/logout`

Invalida el token actual del usuario.

**Headers Requeridos:**
```
Authorization: Bearer {token}
```

### Recuperación de Contraseña

**POST** `/api/recuperacion-password/solicitar`

Envía un código de verificación al email del empleado.

**Request:**
```json
{
  "nomina": "12345"
}
```

**POST** `/api/recuperacion-password/validar-codigo`

Valida el código enviado por email.

**Request:**
```json
{
  "nomina": "12345",
  "codigo": "ABC123"
}
```

**POST** `/api/recuperacion-password/cambiar-password`

Establece la nueva contraseña.

**Request:**
```json
{
  "nomina": "12345",
  "codigo": "ABC123",
  "nuevaPassword": "NuevaContraseña123!"
}
```

---

## Endpoints de la API

### 1. Gestión de Usuarios

#### Obtener Usuario Actual
**GET** `/api/user/current`

Retorna la información del usuario autenticado.

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 123,
    "nomina": "12345",
    "fullName": "Juan Pérez",
    "username": "juan.perez",
    "areaId": 1,
    "grupoId": 5,
    "roles": ["Empleado"],
    "diasVacacionesAsignados": 12,
    "antiguedadAnios": 5
  }
}
```

#### Listar Usuarios
**GET** `/api/user?pageNumber=1&pageSize=20`

Lista usuarios con paginación y filtros opcionales.

**Query Parameters:**
- `pageNumber` (int): Número de página
- `pageSize` (int): Tamaño de página
- `searchTerm` (string): Búsqueda por nombre/nómina
- `areaId` (int): Filtrar por área
- `grupoId` (int): Filtrar por grupo
- `status` (string): active/inactive

#### Crear Usuario
**POST** `/api/user`

Crea un nuevo usuario en el sistema.

**Request:**
```json
{
  "nomina": "12345",
  "fullName": "Juan Pérez",
  "username": "juan.perez",
  "password": "Password123!",
  "areaId": 1,
  "grupoId": 5,
  "fechaIngreso": "2020-01-15",
  "posicion": "Operador",
  "centroCoste": "CC001"
}
```

#### Actualizar Usuario
**PUT** `/api/user/{id}`

Actualiza la información de un usuario existente.

#### Eliminar Usuario
**DELETE** `/api/user/{id}`

Elimina (borrado lógico) un usuario.

---

### 2. Bloques de Reservación

#### Generar Bloques
**POST** `/api/bloques-reservacion/generar`

Genera los bloques de vacaciones para el año especificado.

**Roles requeridos:** SuperUsuario, Administrador

**Request:**
```json
{
  "fechaInicioGeneracion": "2025-01-01T06:00:00",
  "anioObjetivo": 2025,
  "grupoIds": [1, 2, 3],  // Opcional, vacío = todos
  "soloSimulacion": false
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "anioObjetivo": 2025,
    "totalGruposProcesados": 3,
    "totalBloqueGenerados": 45,
    "totalEmpleadosAsignados": 150,
    "resumenPorGrupo": [...],
    "generacionExitosa": true
  }
}
```

#### Consultar Bloques por Fecha
**GET** `/api/bloques-reservacion/por-fecha`

Obtiene los bloques activos para una fecha específica.

**Query Parameters:**
- `fecha` (DateTime): Fecha a consultar **(requerido)**
- `areaId` (int): Filtrar por área
- `grupoId` (int): Filtrar por grupo
- `anioObjetivo` (int): Año objetivo

**Response:**
```json
{
  "success": true,
  "data": {
    "fechaConsulta": "2025-01-15T00:00:00",
    "bloquesPorGrupo": [
      {
        "grupoId": 5,
        "nombreGrupo": "Grupo A",
        "bloqueActual": {
          "id": 123,
          "numeroBloque": 1,
          "fechaHoraInicio": "2025-01-10T06:00:00",
          "fechaHoraFin": "2025-01-17T06:00:00",
          "empleadosAsignados": [
            {
              "empleadoId": 456,
              "nombreCompleto": "Juan Pérez",
              "nomina": "12345",
              "estado": "Asignado"
            }
          ]
        },
        "bloqueSiguiente": {...}
      }
    ]
  }
}
```

#### Aprobar Bloques
**POST** `/api/bloques-reservacion/aprobar`

Aprueba bloques generados para hacerlos oficiales.

**Roles requeridos:** SuperUsuario, Administrador

**Request:**
```json
{
  "anioObjetivo": 2025,
  "grupoIds": [1, 2, 3],
  "observaciones": "Aprobación de bloques Q1 2025"
}
```

#### Cambiar Empleado de Bloque
**POST** `/api/bloques-reservacion/cambiar-bloque`

Transfiere un empleado de un bloque a otro.

**Request:**
```json
{
  "empleadoId": 123,
  "bloqueOrigenId": 456,
  "bloqueDestinoId": 789,
  "motivo": "Solicitud médica",
  "observacionesAdicionales": "Requiere fechas específicas por cirugía"
}
```

#### Eliminar Bloques
**DELETE** `/api/bloques-reservacion/eliminar`

Elimina bloques de un año específico.

**Query Parameters:**
- `anioObjetivo` (int): Año de los bloques a eliminar
- `grupoIds` (array): IDs de grupos específicos

---

### 3. Vacaciones Programadas

#### Consultar Vacaciones
**GET** `/api/vacaciones`

Lista las vacaciones programadas con filtros.

**Query Parameters:**
- `empleadoId` (int): Filtrar por empleado
- `grupoId` (int): Filtrar por grupo
- `areaId` (int): Filtrar por área
- `fechaInicio` (date): Fecha inicial del rango
- `fechaFin` (date): Fecha final del rango
- `estado` (string): Programada/Tomada/Cancelada

#### Asignar Vacaciones
**POST** `/api/vacaciones/asignar`

Asigna días de vacaciones a un empleado.

**Request:**
```json
{
  "empleadoId": 123,
  "fechaInicio": "2025-03-15",
  "fechaFin": "2025-03-22",
  "diasSolicitados": 5,
  "observaciones": "Vacaciones de primavera"
}
```

#### Aprobar/Rechazar Vacaciones
**PUT** `/api/vacaciones/{id}/aprobar`

Aprueba o rechaza una solicitud de vacaciones.

**Request:**
```json
{
  "aprobado": true,
  "observaciones": "Aprobado según política"
}
```

#### Cancelar Vacaciones
**DELETE** `/api/vacaciones/{id}`

Cancela una programación de vacaciones.

---

### 4. Reprogramación de Vacaciones

#### Solicitar Reprogramación
**POST** `/api/reprogramacion/solicitar`

Crea una solicitud para cambiar fechas de vacaciones.

**Request:**
```json
{
  "vacacionOriginalId": 123,
  "fechaNuevaSolicitada": "2025-04-15",
  "observacionesEmpleado": "Cambio por evento familiar"
}
```

#### Listar Solicitudes Pendientes
**GET** `/api/reprogramacion/pendientes`

Obtiene las solicitudes de reprogramación pendientes de aprobación.

**Query Parameters:**
- `areaId` (int): Filtrar por área
- `grupoId` (int): Filtrar por grupo

#### Aprobar/Rechazar Reprogramación
**PUT** `/api/reprogramacion/{id}/responder`

Responde a una solicitud de reprogramación.

**Request:**
```json
{
  "aprobado": true,
  "observacionesJefe": "Aprobado, no afecta la operación"
}
```

---

### 5. Festivos Trabajados

#### Registrar Festivo Trabajado
**POST** `/api/festivo-trabajado/registrar`

Registra que un empleado trabajó en día festivo.

**Request:**
```json
{
  "empleadoId": 123,
  "fechaFestivo": "2025-01-01",
  "horasTrabajadas": 8,
  "motivo": "Cobertura de operación crítica"
}
```

#### Solicitar Intercambio de Festivo
**POST** `/api/festivo-trabajado/solicitar-intercambio`

Solicita cambiar un festivo trabajado por un día de vacaciones.

**Request:**
```json
{
  "festivoTrabajadoId": 456,
  "fechaNuevaSolicitada": "2025-02-15",
  "motivo": "Preferencia personal"
}
```

#### Consultar Festivos Trabajados
**GET** `/api/festivo-trabajado`

Lista los festivos trabajados con filtros.

**Query Parameters:**
- `empleadoId` (int): Filtrar por empleado
- `anio` (int): Filtrar por año
- `estado` (string): Pendiente/Compensado/Pagado

---

### 6. Ausencias y Cobertura

#### Calcular Ausencias por Fecha
**GET** `/api/ausencia/por-fecha`

Calcula las ausencias y cobertura para una fecha específica.

**Query Parameters:**
- `fecha` (date): Fecha a consultar
- `areaId` (int): Filtrar por área

**Response:**
```json
{
  "success": true,
  "data": {
    "fecha": "2025-03-15",
    "totalEmpleados": 50,
    "totalAusentes": 8,
    "porcentajeAusencia": 16.0,
    "porcentajeCobertura": 84.0,
    "detalleAusencias": [
      {
        "empleadoId": 123,
        "nombre": "Juan Pérez",
        "tipoAusencia": "Vacaciones",
        "grupo": "Grupo A"
      }
    ]
  }
}
```

#### Validar Porcentaje de Cobertura
**POST** `/api/validacion-porcentaje/validar`

Valida si una fecha cumple con el porcentaje mínimo de cobertura.

**Request:**
```json
{
  "grupoId": 5,
  "fecha": "2025-03-15",
  "empleadosAusentes": [123, 456, 789]
}
```

---

### 7. Programación Anual

#### Crear Programación Anual
**POST** `/api/programacion-anual`

Crea la estructura de programación para un año.

**Request:**
```json
{
  "anio": 2025,
  "descripcion": "Programación Anual 2025",
  "fechaInicio": "2025-01-01",
  "fechaFin": "2025-12-31"
}
```

#### Consultar Programación Activa
**GET** `/api/programacion-anual/activa`

Obtiene la programación anual actualmente en uso.

#### Activar Programación
**PUT** `/api/programacion-anual/{id}/activar`

Activa una programación anual específica.

---

### 8. Configuración del Sistema

#### Días Inhábiles

**GET** `/api/dias-inhabiles`

Lista los días inhábiles (festivos) del sistema.

**POST** `/api/dias-inhabiles`

Agrega un nuevo día inhábil.

**Request:**
```json
{
  "fecha": "2025-12-25",
  "descripcion": "Navidad",
  "aplicaATodos": true
}
```

#### Configuración de Vacaciones por Antigüedad

**GET** `/api/configuracion-vacaciones/antiguedad`

Obtiene la tabla de días por antigüedad.

**PUT** `/api/configuracion-vacaciones/antiguedad`

Actualiza los días asignados por antigüedad.

**Request:**
```json
{
  "configuraciones": [
    {
      "aniosAntiguedad": 1,
      "diasVacaciones": 6
    },
    {
      "aniosAntiguedad": 2,
      "diasVacaciones": 8
    }
  ]
}
```

#### Reglas de Turnos

**GET** `/api/configuracion-vacaciones/reglas-turnos`

Obtiene las reglas de turnos configuradas.

**Response:**
```json
{
  "success": true,
  "data": {
    "reglas": [
      {
        "codigo": "R0144",
        "descripcion": "Rotación 3 turnos",
        "patron": ["1", "1", "1", "1", "1", "D", "D", ...]
      }
    ]
  }
}
```

---

### 9. Notificaciones

#### Enviar Notificación Manual
**POST** `/api/notificaciones/enviar`

Envía una notificación manual a empleados específicos.

**Request:**
```json
{
  "destinatarios": [123, 456],
  "asunto": "Recordatorio de Vacaciones",
  "mensaje": "Por favor confirme sus fechas de vacaciones",
  "prioridad": "Alta"
}
```

#### Consultar Historial de Notificaciones
**GET** `/api/notificaciones/historial`

Obtiene el historial de notificaciones enviadas.

**Query Parameters:**
- `empleadoId` (int): Filtrar por empleado
- `fechaInicio` (date): Fecha inicial
- `fechaFin` (date): Fecha final
- `tipo` (string): Email/SMS/Sistema

---

### 10. Reportes y Estadísticas

#### Reporte de Empleados que No Respondieron
**GET** `/api/bloques-reservacion/empleados-no-respondio`

Lista empleados que no confirmaron su asignación de bloque.

**Query Parameters:**
- `anioObjetivo` (int): Año a consultar
- `grupoId` (int): Filtrar por grupo

#### Estadísticas de Vacaciones
**GET** `/api/vacaciones/estadisticas`

Obtiene estadísticas generales de vacaciones.

**Query Parameters:**
- `anio` (int): Año a consultar
- `areaId` (int): Filtrar por área

**Response:**
```json
{
  "success": true,
  "data": {
    "totalDiasAsignados": 1200,
    "totalDiasTomados": 800,
    "totalDiasPendientes": 400,
    "promediosDiasPorEmpleado": 12,
    "porcentajeUtilizacion": 66.7
  }
}
```

---

### 11. Catálogos

#### Áreas
**GET** `/api/area`

Lista todas las áreas de la empresa.

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "areaId": 1,
      "nombreGeneral": "Producción",
      "grupos": 5,
      "empleados": 150
    }
  ]
}
```

#### Grupos
**GET** `/api/grupo`

Lista todos los grupos de trabajo.

**Query Parameters:**
- `areaId` (int): Filtrar por área

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "grupoId": 5,
      "areaId": 1,
      "rol": "R0144_01",
      "personasPorTurno": 10,
      "duracionDeturno": 168,
      "liderId": 123
    }
  ]
}
```

#### Roles
**GET** `/api/rol`

Lista los roles disponibles en el sistema.

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "SuperUsuario",
      "description": "Acceso total al sistema"
    }
  ]
}
```

---

## Flujos de Trabajo Principales

### 1. Flujo de Generación de Vacaciones Anuales

1. **Crear Programación Anual** → Define el año y períodos
2. **Configurar Días Inhábiles** → Establece festivos oficiales
3. **Generar Bloques** → Crea la estructura de bloques por grupo
4. **Asignar Empleados** → Sistema asigna automáticamente por antigüedad
5. **Aprobar Bloques** → Administrador valida y aprueba
6. **Notificar Empleados** → Sistema envía notificaciones
7. **Confirmar Asistencia** → Empleados confirman o solicitan cambios

### 2. Flujo de Reprogramación

1. **Empleado solicita cambio** → Indica nueva fecha deseada
2. **Sistema valida cobertura** → Verifica porcentajes mínimos
3. **Jefe revisa solicitud** → Aprueba o rechaza con justificación
4. **Sistema actualiza** → Modifica fechas si fue aprobada
5. **Notifica resultado** → Informa al empleado la decisión

### 3. Flujo de Festivos Trabajados

1. **Registrar festivo trabajado** → Sistema o admin registra el día
2. **Empleado solicita compensación** → Elige día alternativo
3. **Validación de cobertura** → Verifica disponibilidad
4. **Aprobación** → Jefe aprueba el intercambio
5. **Generación de día libre** → Sistema crea la vacación compensatoria

---

## Configuración del Sistema

### Variables de Entorno

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=Vacaciones;..."
  },
  "SmtpSettings": {
    "Host": "smtp.empresa.com",
    "Port": 587,
    "EnableSsl": true,
    "FromEmail": "sistema@empresa.com"
  }
}
```

### Configuración de Logging

El sistema utiliza Serilog con los siguientes niveles:
- **Development**: Debug y superior
- **Production**: Information y superior

Los logs se almacenan en:
- Consola (todos los ambientes)
- Archivos en `/Logs/app-log-{fecha}.txt`

### Configuración de Seguridad

- **JWT Token**: Expiración en 24 horas
- **Política de Contraseñas**: Mínimo 8 caracteres, mayúsculas, números
- **CORS**: Configurado para origenes específicos del frontend

---

## Estados y Enumeraciones

### Estados de Vacaciones
- `Programada`: Vacación asignada pendiente
- `Tomada`: Vacación ya disfrutada
- `Cancelada`: Vacación cancelada
- `Reprogramada`: Cambió de fecha

### Estados de Bloques
- `Pendiente`: Generado pero no aprobado
- `Aprobado`: Listo para uso
- `Activo`: En ejecución actual
- `Completado`: Ya finalizado
- `Cancelado`: Anulado

### Estados de Asignación
- `Asignado`: Empleado asignado al bloque
- `Reservado`: Reserva pendiente de confirmación
- `Completado`: Ya tomó las vacaciones
- `Transferido`: Cambió a otro bloque
- `NoRespondio`: Sin confirmación del empleado

### Estados de Solicitudes
- `Pendiente`: Esperando aprobación
- `Aprobada`: Solicitud aceptada
- `Rechazada`: Solicitud denegada

---

## Notas Importantes

1. **Horarios de Bloques**: Los bloques respetan la hora especificada en `FechaInicioGeneracion`, no siempre inician a las 00:00

2. **Prioridad por Antigüedad**: El sistema siempre prioriza a empleados con mayor antigüedad para los primeros bloques del año

3. **Validación de Cobertura**: Todas las operaciones que afectan ausencias validan automáticamente los porcentajes mínimos de cobertura

4. **Borrado Lógico**: El sistema usa borrado lógico para mantener historial, los registros no se eliminan físicamente

5. **Zona Horaria**: Todas las fechas/horas se manejan en UTC internamente y se convierten según la zona del cliente

---

## Soporte y Contacto

Para soporte técnico o preguntas sobre el sistema:
- **Email Sistema**: mats.ti_sl_fa@conti.com.mx
- **Documentación Técnica**: [Repositorio interno]
- **Versión**: 1.0.0
- **Última Actualización**: Diciembre 2024