# Checklist de producción real - Core INDOTEL

Este checklist convierte el Core académico en una base de producción real.

Leyenda:

```text
[ ] Pendiente
[x] Completado
[~] Parcial
```

## Estado base actual

- [x] API ASP.NET Core creada.
- [x] SQL Server configurado con Docker.
- [x] EF Core configurado.
- [x] Migración inicial aplicada.
- [x] Swagger funcionando.
- [x] Login JWT funcionando.
- [x] Usuario administrador inicial.
- [x] Roles base.
- [x] Ciudadanos base.
- [x] Reclamaciones base.
- [x] Máquina de estados estricta.
- [x] Historial de reclamaciones.
- [x] Respuesta prestadora.
- [x] Documentos/evidencias base.
- [x] Reportes básicos.
- [x] Consulta por expediente.
- [x] Script de pruebas.
- [x] Evidencia de pruebas.
- [x] Registro público ciudadano básico probado.
- [x] Cambio de contraseña autenticado probado.
- [x] Recuperación/restablecimiento de contraseña básico probado.

## Fase 1 - Autenticación pública y seguridad de usuario

- [x] Crear DTO `RegisterCiudadanoDto`.
- [x] Crear DTO `ChangePasswordDto`.
- [x] Crear DTO `ForgotPasswordDto`.
- [x] Crear DTO `ResetPasswordDto`.
- [x] Crear endpoint `POST /api/auth/register-ciudadano`.
- [x] Crear ciudadano y usuario en una sola operación.
- [x] Validar correo duplicado.
- [x] Validar cédula duplicada.
- [x] Hashear contraseña.
- [x] Asignar rol `Ciudadano` automáticamente.
- [x] Devolver JWT después del registro ciudadano.
- [x] Crear endpoint `POST /api/auth/change-password`.
- [x] Validar contraseña actual antes de cambiarla.
- [x] Hashear nueva contraseña.
- [x] Crear endpoint `POST /api/auth/forgot-password`.
- [x] Crear endpoint `POST /api/auth/reset-password`.
- [~] Token de recuperación temporal implementado con JWT.
- [x] Expirar token de recuperación.
- [ ] Guardar token de recuperación hasheado en base de datos.
- [ ] Invalidar token de recuperación después de usarlo.
- [ ] Crear refresh token.
- [ ] Crear endpoint `POST /api/auth/refresh-token`.
- [ ] Crear endpoint `POST /api/auth/logout`.
- [ ] Registrar último acceso.
- [ ] Contar intentos fallidos.
- [ ] Bloquear usuario por intentos fallidos.
- [x] Probar registro ciudadano.
- [x] Probar `/api/auth/me` con token ciudadano.
- [x] Probar cambio de contraseña propio.
- [x] Probar rechazo de contraseña anterior.
- [x] Probar login con contraseña nueva.
- [x] Probar flujo completo de recuperación y restablecimiento.

Estado de Fase 1:

```text
Fase 1 básica: completada y probada.
Fase 1 producción estricta: parcial.
```

## Fase 2 - RBAC fase 2 y dueño real de datos

- [ ] Agregar `CiudadanoId` nullable a `Usuario`.
- [ ] Agregar `PrestadoraId` nullable a `Usuario`.
- [ ] Crear migración.
- [ ] Actualizar seed/admin sin romper datos.
- [ ] Incluir `CiudadanoId` y `PrestadoraId` en JWT cuando existan.
- [ ] Filtrar reclamaciones por ciudadano autenticado.
- [ ] Filtrar reclamaciones por prestadora autenticada.
- [ ] Impedir que ciudadano vea reclamaciones ajenas.
- [ ] Impedir que prestadora vea casos de otra prestadora.
- [ ] Impedir que prestadora responda caso ajeno.
- [ ] Mantener auditor con solo lectura.
- [ ] Probar permisos por rol.
- [ ] Probar permisos por dueño real.

## Fase 3 - Gestión completa de prestadoras

- [ ] Crear DTO `PrestadoraCreateDto`.
- [ ] Crear DTO `PrestadoraUpdateDto`.
- [ ] Crear DTO `PrestadoraEstadoDto`.
- [ ] Crear `PrestadorasController` completo.
- [ ] `GET /api/prestadoras/{id}`.
- [ ] `POST /api/prestadoras`.
- [ ] `PUT /api/prestadoras/{id}`.
- [ ] `PATCH /api/prestadoras/{id}/estado`.
- [ ] `GET /api/prestadoras/{id}/reclamaciones`.
- [ ] `GET /api/prestadoras/{id}/usuarios`.
- [ ] `POST /api/prestadoras/{id}/usuarios`.
- [ ] Validar RNC duplicado.
- [ ] No eliminar prestadora con reclamaciones.
- [ ] Auditar cambios.
- [ ] Probar CRUD completo.

## Fase 4 - Gestión completa de servicios telecom

- [ ] Crear DTO `ServicioCreateDto`.
- [ ] Crear DTO `ServicioUpdateDto`.
- [ ] Crear DTO `ServicioEstadoDto`.
- [ ] Crear `ServiciosController` completo o ampliar catálogo actual.
- [ ] `GET /api/servicios/{id}`.
- [ ] `POST /api/servicios`.
- [ ] `PUT /api/servicios/{id}`.
- [ ] `PATCH /api/servicios/{id}/estado`.
- [ ] `GET /api/servicios/{id}/reclamaciones`.
- [ ] Validar nombre duplicado.
- [ ] No eliminar servicio con reclamaciones.
- [ ] Auditar cambios.
- [ ] Probar CRUD completo.

## Fase 5 - Tipos, motivos y clasificación de reclamaciones

- [ ] Crear modelo `TipoReclamacion`.
- [ ] Crear modelo `MotivoReclamacion`.
- [ ] Crear modelo `CanalRecepcion`.
- [ ] Agregar `TipoReclamacionId` a `Reclamacion`.
- [ ] Agregar `MotivoReclamacionId` a `Reclamacion`.
- [ ] Agregar `CanalRecepcion` a `Reclamacion`.
- [ ] Agregar `Provincia` y `Municipio` si aplica.
- [ ] Agregar `Prioridad`.
- [ ] Crear migración.
- [ ] Crear endpoints de catálogos para tipos y motivos.
- [ ] Actualizar creación de reclamación.
- [ ] Actualizar reportes.
- [ ] Probar clasificación.

## Fase 6 - SLA regulatorio

- [ ] Agregar `FechaEnvioPrestadora`.
- [ ] Agregar `FechaLimiteRespuesta`.
- [ ] Agregar `FechaRespuestaPrestadora`.
- [ ] Agregar `DiasHabilesSla`.
- [ ] Agregar `EstaVencida`.
- [ ] Agregar `FechaMarcadaVencida`.
- [ ] Crear migración.
- [ ] Calcular SLA al enviar a prestadora.
- [ ] Registrar respuesta dentro/fuera de plazo.
- [ ] Crear `GET /api/reclamaciones/vencidas`.
- [ ] Crear `GET /api/reclamaciones/proximas-vencer`.
- [ ] Crear `POST /api/reclamaciones/{id}/marcar-vencida`.
- [ ] Crear reporte SLA.
- [ ] Probar caso vencido.
- [ ] Probar caso respondido a tiempo.

## Fase 7 - Resolución, cierre y motivos estructurados

- [ ] Crear modelo `MotivoCierre`.
- [ ] Crear modelo `MotivoRechazo`.
- [ ] Agregar `ResolucionFinal` a `Reclamacion`.
- [ ] Agregar `MotivoCierreId`.
- [ ] Agregar `MotivoRechazoId`.
- [ ] Agregar `ResultadoCiudadano`.
- [ ] Agregar `MontoCompensado`.
- [ ] Agregar `RequiereSeguimiento`.
- [ ] Agregar `FechaResolucion`.
- [ ] Crear endpoint `POST /api/reclamaciones/{id}/resolver`.
- [ ] Crear endpoint `POST /api/reclamaciones/{id}/cerrar`.
- [ ] Crear endpoint `POST /api/reclamaciones/{id}/rechazar`.
- [ ] Crear endpoint `POST /api/reclamaciones/{id}/archivar`.
- [ ] Bloquear cierre sin resolución.
- [ ] Bloquear rechazo sin motivo.
- [ ] Auditar decisión final.
- [ ] Probar flujo completo.

## Fase 8 - Auditoría institucional completa

- [ ] Revisar modelo `Auditoria` actual.
- [ ] Ajustar modelo si faltan campos.
- [ ] Crear servicio `AuditoriaService`.
- [ ] Registrar login exitoso.
- [ ] Registrar login fallido.
- [ ] Registrar logout.
- [ ] Registrar crear usuario.
- [ ] Registrar desactivar usuario.
- [ ] Registrar cambiar clave.
- [ ] Registrar crear ciudadano.
- [ ] Registrar editar ciudadano.
- [ ] Registrar crear reclamación.
- [ ] Registrar cambio de estado.
- [ ] Registrar subir documento.
- [ ] Registrar eliminar documento.
- [ ] Registrar descargar documento.
- [ ] Registrar respuesta prestadora.
- [ ] Registrar cierre de caso.
- [ ] Crear `GET /api/auditoria`.
- [ ] Crear `GET /api/auditoria/reclamacion/{id}`.
- [ ] Crear `GET /api/auditoria/usuario/{id}`.
- [ ] Crear `GET /api/auditoria/entidad/{entidad}/{id}`.
- [ ] Probar trazabilidad.

## Fase 9 - Documentos seguros

- [x] Subir documento.
- [x] Listar documentos.
- [x] Eliminar documento.
- [x] Bloquear subida a caso cerrado.
- [ ] Crear `GET /api/documentos/{id}/metadata`.
- [ ] Crear `GET /api/documentos/{id}/descargar`.
- [ ] Validar MIME real.
- [ ] Guardar hash del archivo.
- [ ] Auditar descarga.
- [ ] Restringir descarga por dueño real.
- [ ] Configurar tamaño máximo por appsettings.
- [ ] Preparar almacenamiento externo.
- [ ] Probar descarga segura.

## Fase 10 - Filtros, búsqueda y paginación

- [ ] Crear DTO `ReclamacionFiltroDto`.
- [ ] Crear respuesta paginada estándar.
- [ ] Filtrar por estado.
- [ ] Filtrar por prestadora.
- [ ] Filtrar por servicio.
- [ ] Filtrar por ciudadano.
- [ ] Filtrar por rango de fechas.
- [ ] Buscar por expediente.
- [ ] Buscar por texto.
- [ ] Aplicar RBAC fase 2 a filtros.
- [ ] Probar con varios registros.

## Fase 11 - Reportes regulatorios avanzados

- [x] Reporte resumen básico.
- [x] Reporte por estado básico.
- [x] Reporte por prestadora básico.
- [ ] Reporte por servicio.
- [ ] Reporte por mes.
- [ ] Reporte SLA vencidas.
- [ ] Reporte tiempo promedio de respuesta.
- [ ] Reporte prestadoras con más reclamaciones.
- [ ] Reporte por motivo.
- [ ] Exportar Excel.
- [ ] Exportar PDF.
- [ ] Probar reportes con datos reales.

## Fase 12 - Notificaciones

- [ ] Crear modelo `Notificacion`.
- [ ] Crear migración.
- [ ] Notificar caso creado.
- [ ] Notificar envío a prestadora.
- [ ] Notificar respuesta de prestadora.
- [ ] Notificar caso vencido.
- [ ] Notificar caso resuelto.
- [ ] Notificar caso cerrado.
- [ ] Notificar documento recibido.
- [ ] Crear `GET /api/notificaciones`.
- [ ] Crear `PATCH /api/notificaciones/{id}/leida`.
- [ ] Crear `PATCH /api/notificaciones/marcar-todas-leidas`.
- [ ] Probar notificaciones.

## Fase 13 - Manejo global de errores y logs

- [ ] Crear `ExceptionHandlingMiddleware`.
- [ ] Crear `CorrelationIdMiddleware`.
- [ ] Crear respuesta estándar de error.
- [ ] Agregar logs estructurados.
- [ ] Registrar errores no controlados.
- [ ] Registrar traceId en cada respuesta de error.
- [ ] Separar mensajes internos de mensajes públicos.
- [ ] Probar errores 400, 401, 403, 404, 409 y 500.

## Fase 14 - Health checks, configuración y despliegue

- [x] Health básico existente.
- [ ] Crear `GET /health/live`.
- [ ] Crear `GET /health/ready`.
- [ ] Crear `GET /health/db`.
- [ ] Crear `GET /health/storage`.
- [ ] Restringir CORS por ambiente.
- [ ] Forzar HTTPS en producción.
- [ ] Mover secretos a variables de entorno.
- [ ] Crear appsettings de producción seguro.
- [ ] Documentar despliegue.
- [ ] Documentar backup y restore.

## Fase 15 - CI/CD y pruebas automáticas

- [x] Script manual automático con curl.
- [ ] Crear proyecto de pruebas xUnit.
- [ ] Test login correcto.
- [ ] Test login incorrecto.
- [x] Test manual de registro ciudadano.
- [ ] Test automático de registro ciudadano.
- [ ] Test crear reclamación.
- [ ] Test transición válida.
- [ ] Test transición inválida.
- [ ] Test respuesta prestadora.
- [ ] Test subir documento.
- [ ] Test bloquear documento en caso cerrado.
- [ ] Test RBAC ciudadano.
- [ ] Test RBAC prestadora.
- [ ] Test reportes básicos.
- [ ] Crear GitHub Action `dotnet build`.
- [ ] Crear GitHub Action `dotnet test`.
- [ ] Bloquear merge si fallan pruebas.

## Checklist de cierre para producción

El Core puede considerarse listo para producción cuando:

- [x] Tiene registro público de ciudadano básico.
- [x] Tiene recuperación de contraseña básica para demo.
- [ ] Tiene recuperación de contraseña estricta para producción.
- [ ] Tiene refresh token y logout.
- [ ] Tiene RBAC por rol y dueño real.
- [ ] Ciudadano ve solo sus datos.
- [ ] Prestadora ve solo sus casos.
- [ ] Tiene SLA regulatorio.
- [ ] Tiene auditoría completa.
- [ ] Tiene documentos seguros con descarga auditada.
- [ ] Tiene filtros y paginación.
- [ ] Tiene reportes regulatorios avanzados.
- [ ] Tiene notificaciones internas.
- [ ] Tiene manejo global de errores.
- [ ] Tiene logs estructurados.
- [ ] Tiene health checks reales.
- [ ] Tiene pruebas automáticas.
- [ ] Tiene CI/CD.
- [ ] Tiene documentación de despliegue.
- [ ] Tiene estrategia de backup y restore.

## Porcentaje objetivo

```text
Actual producción real: 73%
Meta mínima: 90%
Meta fuerte: 95%
```

Avance esperado:

```text
Fase 1 básica: 73%
Fase 1 estricta + Fase 2: 80%
Fases 3-4: 84%
Fases 5-7: 88%
Fases 8-11: 92%
Fases 12-15: 95%
```
