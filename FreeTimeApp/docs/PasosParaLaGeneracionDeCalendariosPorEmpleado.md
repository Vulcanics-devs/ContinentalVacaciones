# Pasos para la Generación de Calendarios por Empleado

Este documento explica detalladamente el proceso de generación de calendarios para empleados sindicalizados en el sistema, paso a paso y con subíndices para los procesos clave.

---

## ¿De dónde se extrae la información?

- **Empleados, Áreas, Grupos, Roles:** De las tablas correspondientes en la base de datos (`Users`, `Areas`, `Grupos`, `Roles`).
- **Días inhábiles, permisos, incapacidades:** De las tablas `DiasInhabiles` e `IncidenciasOPermisos`.
- **Reglas y programación anual:** De las tablas `Reglas`, `ProgramacionesAnuales`, `RolesInicialesPorEmpleado`, `TurnosXRolSemanalXRegla`.
- **Vacaciones por antigüedad:** De la tabla `VacacionesPorAntiguedad`.

---

## 1. Inicio del Proceso
- El método principal es `GenerateEmployeesCalendarsAsync(fechaInicio, fechaFinal)`.
- Se registra en el log que inicia la generación de calendarios para empleados sindicalizados.

## 2. Cálculo de Antigüedad
- Se calcula la antigüedad de todos los empleados sindicalizados (`CalculaAntiguedadParaTodosEmpleados`).
- Se actualiza la información de cada empleado en la base de datos, incluyendo los días de vacaciones que le corresponden según su antigüedad.

## 3. Obtención de Áreas
- Se obtienen todas las áreas registradas en la base de datos (`Areas`).
- Se registra en el log cuántas áreas se encontraron.

## 4. Procesamiento por Área
- Para cada área:
  - Se obtienen los usuarios (empleados) que pertenecen a esa área.
  - Se genera el calendario para cada usuario en ese rango de fechas (`GeneraCalendarioPara`).
  - Se asignan vacaciones a los empleados de esa área (`AsignaVacacionesAEmpleadosPorArea`).

## 5. Generación de Calendario por Usuario

### 5.1. Verificación de Rol
- Se verifica que el usuario tenga el rol "Empleado Sindicalizado".
- Si no lo tiene, se omite la generación de calendario para ese usuario y se registra en el log.

### 5.2. Obtención de Datos Iniciales
- Se obtienen los datos iniciales necesarios para el calendario:
  - Grupo al que pertenece el usuario.
  - Rol inicial del usuario.
  - Regla semanal y turno inicial.
  - Programación anual activa.
  - Vacaciones por antigüedad.
- Si falta algún dato, se omite la generación y se registra en el log.

### 5.3. Obtención de Incidencias y Días Inhábiles
- Se obtienen los permisos e incapacidades del usuario en el rango de fechas.
- Se obtienen los días inhábiles por ley y por Continental en el rango de fechas.

### 5.4. Recorrido Día a Día
- Se recorre cada día entre la fecha de inicio y la fecha final.
- Para cada día:
  - Se determina el tipo de actividad del día, siguiendo esta prioridad:
    1. **Inhabil por Ley**
    2. **Inhabil por Continental**
    3. **Permiso o Incapacidad**
    4. **Vacaciones**
    5. **Turno semanal según regla**
  - Se crea un registro de calendario para ese día (`DiasCalendarioEmpleado`), con toda la información relevante (actividad, turno, incidencias, vacaciones, etc.).
  - Se guarda el registro en la base de datos.
  - Se registra en el log la información del día generado.

### 5.5. Actualización de Usuario
- Al finalizar el recorrido de días:
  - Se actualiza el usuario en la base de datos con los días de vacaciones asignados.
  - Se registra en el log si el usuario tiene vacaciones por antigüedad asignadas y cuántos días le corresponden.

---

## 6. Asignación de Vacaciones Automáticas

### 6.1. Procesamiento por Grupo
- Para cada grupo en el área:
  - Se obtienen los empleados sindicalizados con vacaciones por asignar, ordenados por antigüedad y nómina.

### 6.2. Asignación Iterativa de Vacaciones
- Se llama al método `AsignaVacacionesAEmpleadosSiCorrespondeIterandoSobreLosDiasDeLaProgramacionAnualActual` con la lista de empleados.

#### 6.2.1. Validaciones Iniciales
- Se verifica que haya empleados sindicalizados en la lista.
- Se verifica que exista una programación anual activa.

#### 6.2.2. Recorrido de Fechas de Asignación
- Se define el rango de fechas para asignar vacaciones (normalmente excluyendo los primeros y últimos 21 días de la programación anual).
- Se recorre el rango de fechas y la lista de empleados, asignando vacaciones de la siguiente manera:

##### 6.2.2.1. Para cada empleado:
- Se recorre cada día del rango de fechas.
- Para cada día:
  - Se verifica si el día es laboral y si el porcentaje de ausencia del grupo lo permite (usando la lógica de manning).
  - Si es posible, se cambia el día de laboral a vacaciones (`CambiaDiaCalendarioEmpleadoDeLaboralAVacaciones`).
  - Se actualiza el contador de días asignados y el usuario.
  - Se registra en el log cada asignación o motivo por el que no se pudo asignar.

##### 6.2.2.2. Control de Iteraciones
- Si quedan empleados sin vacaciones asignadas después de recorrer todos los días, se reinicia el ciclo de días.
- Si se realizan demasiadas iteraciones sin completar la asignación, se detiene el proceso para evitar ciclos infinitos y se registra en el log.

---

## 7. Finalización
- Se guardan todos los cambios en la base de datos.
- Se registra en el log que se ha finalizado la generación de calendarios.

---

## Resumen Visual

1. **Obtiene empleados y áreas**
2. **Calcula antigüedad y días de vacaciones**
3. **Recorre cada área y usuario**
4. **Genera calendario día a día**
5. **Asigna vacaciones automáticamente**
6. **Guarda cambios y registra logs**

---

Este documento sirve como guía para entender el flujo y los puntos clave del proceso de generación de calendarios para empleados sindicalizados en el sistema.
