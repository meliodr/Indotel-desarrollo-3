# Checklist de mejoras del Core INDOTEL

Este checklist resume lo que ya tenemos y lo que falta para convertir el Core de un MVP funcional a un Core mas completo y profesional.

## Estado actual

```text
Core funcional para demostracion academica: 95%
Core listo para produccion real: 60%
```

## 1. Lo que ya esta completado

### Base tecnica

- [x] Proyecto ASP.NET Core Web API.
- [x] Swagger / OpenAPI configurado.
- [x] Endpoint de salud `GET /health`.
- [x] Entity Framework Core instalado.
- [x] SQL Server configurado.
- [x] SQL Server ejecutandose con Docker.
- [x] Base de datos `IndotelCoreDb` creada.
- [x] Migracion inicial creada.
- [x] `IndotelDbContext` creado.
- [x] `.gitignore` configurado para no subir secretos.
- [x] `docker-compose.yml` agregado.
- [x] `docker.env.example` agregado.

### Modelos y tablas

- [x] Rol.
- [x] Usuario.
- [x] Ciudadano.
- [x] Prestadora.
- [x] ServicioTelecom.
- [x] Reclamacion.
- [x] DocumentoReclamacion.
- [x] RespuestaPrestadora.
- [x] HistorialReclamacion.
- [x] Auditoria.

### Seguridad

- [x] Login con JWT.
- [x] PasswordHasher para guardar clave segura.
- [x] Usuario administrador inicial.
- [x] Swagger con boton `Authorize`.
- [x] Endpoint `POST /api/auth/login`.
- [x] Endpoint `GET /api/auth/me`.
- [x] Endpoints protegidos con `[Authorize]`.

### Datos iniciales

- [x] Roles semilla.
- [x] Servicios de telecomunicaciones semilla.
- [x] Prestadoras semilla.
- [x] Usuario `admin@indotel.test`.

### Catalogos

- [x] `GET /api/catalogos/roles`.
- [x] `GET /api/catalogos/servicios`.
- [x] `GET /api/catalogos/prestadoras`.

### Ciudadanos

- [x] `GET /api/ciudadanos`.
- [x] `GET /api/ciudadanos/{id}`.
- [x] `POST /api/ciudadanos`.
- [x] Validacion de cedula duplicada.

### Reclamaciones

- [x] `GET /api/reclamaciones`.
- [x] `GET /api/reclamaciones/{id}`.
- [x] `POST /api/reclamaciones`.
- [x] Numero de expediente automatico.
- [x] Estado inicial `RECIBIDA`.
- [x] `PUT /api/reclamaciones/{id}/estado`.
- [x] `POST /api/reclamaciones/{id}/respuesta-prestadora`.
- [x] `GET /api/reclamaciones/{id}/historial`.
- [x] `GET /api/reclamaciones/{id}/respuestas`.

### Reportes

- [x] `GET /api/reportes/resumen`.
- [x] `GET /api/reportes/reclamaciones-por-estado`.
- [x] `GET /api/reportes/reclamaciones-por-prestadora`.

### Documentacion

- [x] `CORE_MASTER_PLAN.md`.
- [x] `DATABASE_MODEL.md`.
- [x] `ENDPOINTS.md`.
- [x] `DEMO_FLOW.md`.
- [x] `CORE_ARCHITECTURE.md`.
- [x] `CORE_PRESENTATION_SCRIPT.md`.
- [x] `CORE_FINAL_STATUS.md`.

## 2. Mejoras prioritarias

Estas mejoras son las mas importantes si queremos que el Core se vea mas completo.

### 2.1 RBAC por roles

Prioridad: Muy alta.

- [ ] Crear politicas de autorizacion por rol.
- [ ] Restringir endpoints de usuarios a `Administrador`.
- [ ] Restringir cambio de estado a `Administrador` y `AnalistaDAU`.
- [ ] Restringir respuesta de prestadora a rol `Prestadora`.
- [ ] Restringir reportes a `Administrador`, `AnalistaDAU` y `Auditor`.
- [ ] Evitar que un ciudadano vea reclamaciones de otros ciudadanos.
- [ ] Evitar que una prestadora vea reclamaciones de otra prestadora.

### 2.2 CRUD de usuarios

Prioridad: Muy alta.

- [ ] Crear `UsuariosController`.
- [ ] Crear DTO para registrar usuario.
- [ ] Crear DTO para editar usuario.
- [ ] Crear endpoint `GET /api/usuarios`.
- [ ] Crear endpoint `GET /api/usuarios/{id}`.
- [ ] Crear endpoint `POST /api/usuarios`.
- [ ] Crear endpoint `PUT /api/usuarios/{id}`.
- [ ] Crear endpoint para desactivar usuario.
- [ ] Crear endpoint para cambiar password.
- [ ] Relacionar usuario con rol.
- [ ] Relacionar usuario tipo prestadora con una prestadora.
- [ ] Relacionar usuario tipo ciudadano con un ciudadano.

### 2.3 Maquina de estados estricta

Prioridad: Alta.

- [ ] Crear clase `ReclamacionEstadoService`.
- [ ] Definir transiciones permitidas.
- [ ] Evitar saltos incorrectos de estado.
- [ ] Flujo sugerido:
  - [ ] `RECIBIDA` -> `VALIDADA`.
  - [ ] `VALIDADA` -> `ENVIADA_A_PRESTADORA`.
  - [ ] `ENVIADA_A_PRESTADORA` -> `RESPONDIDA_POR_PRESTADORA`.
  - [ ] `RESPONDIDA_POR_PRESTADORA` -> `EN_REVISION`.
  - [ ] `EN_REVISION` -> `RESUELTA`.
  - [ ] `RESUELTA` -> `CERRADA`.
  - [ ] Cualquier estado valido -> `RECHAZADA` solo si aplica.
- [ ] Devolver error claro cuando el cambio de estado no sea permitido.

### 2.4 Validaciones fuertes

Prioridad: Alta.

- [ ] Agregar DataAnnotations a DTOs.
- [ ] Validar cedula requerida.
- [ ] Validar correo con formato correcto.
- [ ] Validar telefono requerido.
- [ ] Validar titulo de reclamacion requerido.
- [ ] Validar descripcion requerida.
- [ ] Validar longitud minima y maxima de campos.
- [ ] Mejorar respuestas `400 Bad Request`.

## 3. Mejoras de calidad institucional

### 3.1 Auditoria automatica

Prioridad: Muy alta para un sistema gubernamental.

- [ ] Crear `AuditoriaService`.
- [ ] Registrar creacion de ciudadano.
- [ ] Registrar creacion de reclamacion.
- [ ] Registrar cambio de estado.
- [ ] Registrar respuesta de prestadora.
- [ ] Registrar consultas importantes.
- [ ] Guardar usuario que hizo la accion.
- [ ] Guardar IP del usuario.
- [ ] Guardar fecha y hora.
- [ ] Guardar entidad afectada.
- [ ] Guardar ID del registro afectado.

### 3.2 Relaciones completas de Entity Framework

Prioridad: Alta.

- [ ] Agregar navegacion `Usuario -> Rol`.
- [ ] Agregar navegacion `Reclamacion -> Ciudadano`.
- [ ] Agregar navegacion `Reclamacion -> Prestadora`.
- [ ] Agregar navegacion `Reclamacion -> ServicioTelecom`.
- [ ] Agregar navegacion `Reclamacion -> HistorialReclamaciones`.
- [ ] Agregar navegacion `Reclamacion -> RespuestasPrestadora`.
- [ ] Configurar llaves foraneas en `OnModelCreating`.
- [ ] Crear nueva migracion.
- [ ] Aplicar `dotnet ef database update`.

### 3.3 DTOs de respuesta limpios

Prioridad: Media.

- [ ] Crear DTO de respuesta de ciudadano.
- [ ] Crear DTO de respuesta de reclamacion.
- [ ] Incluir nombre del ciudadano en respuesta de reclamacion.
- [ ] Incluir nombre de prestadora en respuesta de reclamacion.
- [ ] Incluir nombre de servicio en respuesta de reclamacion.
- [ ] Evitar devolver campos internos innecesarios.

## 4. Mejoras funcionales

### 4.1 Validacion simulada JCE

Prioridad: Media-alta.

- [ ] Crear endpoint `POST /api/integraciones/jce/validar-cedula`.
- [ ] Crear servicio `JceValidationService`.
- [ ] Simular respuesta con cedula, nombres, apellidos y fecha de nacimiento.
- [ ] Validar ciudadano antes de registrarlo.
- [ ] Documentar que es una simulacion academica.

### 4.2 Archivos y evidencias

Prioridad: Alta.

- [ ] Crear carpeta local para archivos.
- [ ] Crear endpoint `POST /api/reclamaciones/{id}/documentos`.
- [ ] Crear endpoint `GET /api/reclamaciones/{id}/documentos`.
- [ ] Crear endpoint `DELETE /api/documentos/{id}`.
- [ ] Permitir solo `.pdf`, `.jpg`, `.jpeg`, `.png`.
- [ ] Limitar tamano maximo a 5MB.
- [ ] Guardar ruta del archivo en `DocumentoReclamacion`.
- [ ] Vincular documento con reclamacion.

### 4.3 SLA y vencimientos

Prioridad: Media-alta.

- [ ] Agregar campo `FechaLimiteRespuesta` a reclamacion.
- [ ] Calcular fecha limite al enviar a prestadora.
- [ ] Crear estado `VENCIDA`.
- [ ] Crear endpoint para consultar casos vencidos.
- [ ] Crear servicio para marcar vencimientos.
- [ ] Documentar que el plazo es configurable.

### 4.4 Notificaciones

Prioridad: Media.

- [ ] Crear `NotificationService`.
- [ ] Crear `EmailService` simulado.
- [ ] Notificar creacion de reclamacion.
- [ ] Notificar cambio de estado.
- [ ] Notificar respuesta de prestadora.
- [ ] Guardar registro de notificaciones enviadas.

## 5. Mejoras para consultas y reportes

### 5.1 Filtros y paginacion

Prioridad: Media.

- [ ] Agregar paginacion a `GET /api/reclamaciones`.
- [ ] Filtrar reclamaciones por estado.
- [ ] Filtrar reclamaciones por prestadora.
- [ ] Filtrar reclamaciones por ciudadano.
- [ ] Filtrar reclamaciones por rango de fechas.
- [ ] Agregar paginacion a `GET /api/ciudadanos`.

### 5.2 Reportes avanzados

Prioridad: Media.

- [ ] Reporte de reclamaciones por mes.
- [ ] Reporte de reclamaciones vencidas.
- [ ] Reporte de tiempo promedio de respuesta.
- [ ] Reporte de prestadoras con mas reclamaciones.
- [ ] Reporte de servicios con mas reclamaciones.

## 6. Mejoras tecnicas avanzadas

### 6.1 Manejo global de errores

Prioridad: Media.

- [ ] Crear middleware global de errores.
- [ ] Estandarizar respuestas de error.
- [ ] Evitar mostrar detalles tecnicos al usuario final.
- [ ] Registrar errores en logs.

### 6.2 Versionado de API

Prioridad: Baja-media.

- [ ] Cambiar rutas a `/api/v1/...`.
- [ ] Documentar version actual.
- [ ] Preparar compatibilidad para futuras versiones.

### 6.3 Pruebas automaticas

Prioridad: Media.

- [ ] Crear proyecto de pruebas.
- [ ] Probar login correcto.
- [ ] Probar login incorrecto.
- [ ] Probar crear ciudadano.
- [ ] Probar cedula duplicada.
- [ ] Probar crear reclamacion.
- [ ] Probar cambio de estado permitido.
- [ ] Probar cambio de estado no permitido.

## 7. Prioridad recomendada de trabajo

Si hay poco tiempo, hacer en este orden:

1. [ ] RBAC por roles.
2. [ ] CRUD de usuarios.
3. [ ] Maquina de estados estricta.
4. [ ] Validaciones fuertes.
5. [ ] Auditoria automatica.
6. [ ] Archivos y evidencias.
7. [ ] Filtros y paginacion.
8. [ ] SLA.
9. [ ] Notificaciones.
10. [ ] Pruebas automaticas.

## 8. Definicion de terminado para la siguiente etapa

La siguiente etapa del Core se considera terminada cuando:

- [ ] Existan usuarios reales por rol.
- [ ] Los endpoints respeten permisos por rol.
- [ ] La reclamacion no pueda saltarse estados.
- [ ] Las acciones importantes queden auditadas.
- [ ] Los datos tengan validaciones claras.
- [ ] Se pueda explicar que el Core ya no solo funciona, sino que tambien controla seguridad, permisos y trazabilidad.
