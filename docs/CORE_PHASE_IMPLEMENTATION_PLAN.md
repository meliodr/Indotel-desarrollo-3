# Plan detallado de implementacion por fases

Proyecto: Sistema Digital INDOTEL
Modulo: Core Backend
Rama: `core`

## 1. Regla principal del plan

El Core actual ya paso la prueba final. Por eso, antes de la defensa no se deben hacer cambios grandes que puedan romper lo que ya funciona.

La estrategia queda asi:

```text
Fase 1: Mantener y defender lo ya construido.
Fase 2: Agregar modulos regulatorios priorizados.
Fase 3: Endurecer seguridad, arquitectura y pruebas para produccion real.
```

## 2. Fase 1 - Core actual de reclamaciones

Estado: completado.
Porcentaje del alcance academico priorizado: 40%.
Objetivo: defender el Core como motor regulatorio de Atencion al Usuario y Reclamaciones.

### 2.1 Que incluye

```text
Autenticacion JWT.
Roles.
Ciudadanos.
Prestadoras.
Servicios telecom.
Reclamaciones.
Clasificacion.
SLA.
Respuesta de prestadora.
Resolucion.
Cierre.
Documentos seguros.
Auditoria institucional.
Busqueda paginada.
Reportes.
Notificaciones.
Health checks.
Manejo global de errores.
CI basico.
```

### 2.2 Que se hace ahora

```text
No agregar logica nueva critica.
Ejecutar prueba final antes de presentar.
Revisar documentos de defensa.
Preparar explicacion de por que no es CRUD.
Sincronizar GitHub y maquina local.
```

### 2.3 Checklist de cierre de Fase 1

- [x] API compila.
- [x] API levanta correctamente.
- [x] Base de datos conecta.
- [x] Login admin funciona.
- [x] Reclamacion se crea correctamente.
- [x] Documento se sube correctamente.
- [x] Documento se descarga por el dueno real.
- [x] Documento se bloquea a usuario ajeno.
- [x] Reportes responden 200.
- [x] Notificaciones funcionan.
- [x] Auditoria registra eventos criticos.
- [x] Prueba final end-to-end pasa completa.
- [x] Documentacion de defensa creada.
- [x] Roadmap futuro documentado.

### 2.4 Criterio de aceptacion

La fase esta lista cuando el script final termina con:

```text
PRUEBA FINAL TERMINADA CORRECTAMENTE
CORE INDOTEL VALIDADO AL 100% DEL ALCANCE ACADEMICO DEFINIDO
```

## 3. Fase 2A - Resoluciones institucionales

Estado: futuro corto.
Porcentaje estimado: 8%.
Objetivo: agregar resoluciones oficiales institucionales que puedan relacionarse con reclamaciones, prestadoras, autorizaciones o licencias.

### 3.1 Por que va primero

Es la extension mas pequena y mas facil de conectar con lo actual. Reutiliza documentos, auditoria y reportes.

### 3.2 Alcance funcional

```text
Crear resolucion institucional.
Consultar resoluciones.
Consultar resolucion por id.
Asociar resolucion a reclamacion o prestadora.
Adjuntar documento oficial.
Aprobar resolucion.
Publicar resolucion.
Archivar resolucion.
Auditar cada cambio.
```

### 3.3 Modelos sugeridos

```text
ResolucionInstitucional
TipoResolucion
DocumentoResolucion
```

Campos sugeridos para `ResolucionInstitucional`:

```text
Id
NumeroResolucion
Titulo
Resumen
TipoResolucionId
Estado
ReclamacionId nullable
PrestadoraId nullable
FechaAprobacion nullable
FechaPublicacion nullable
UsuarioCreacionId
UsuarioAprobacionId nullable
DocumentoId nullable
CreatedAt
UpdatedAt
```

### 3.4 Estados

```text
BORRADOR
APROBADA
PUBLICADA
ARCHIVADA
```

### 3.5 Endpoints sugeridos

```text
POST /api/resoluciones
GET /api/resoluciones
GET /api/resoluciones/{id}
PATCH /api/resoluciones/{id}/aprobar
PATCH /api/resoluciones/{id}/publicar
PATCH /api/resoluciones/{id}/archivar
POST /api/resoluciones/{id}/documento
GET /api/reportes/resoluciones
```

### 3.6 Reglas de negocio

```text
Una resolucion solo puede publicarse si esta aprobada.
Una resolucion publicada no debe modificarse libremente.
Una resolucion archivada no puede volver a borrador.
Toda aprobacion, publicacion y archivo genera auditoria.
```

### 3.7 Checklist de implementacion

- [ ] Crear modelo `ResolucionInstitucional`.
- [ ] Crear catalogo `TipoResolucion`.
- [ ] Crear DTOs.
- [ ] Agregar DbSet al DbContext.
- [ ] Crear migracion.
- [ ] Crear controlador.
- [ ] Crear endpoints CRUD basicos.
- [ ] Crear endpoints de aprobar/publicar/archivar.
- [ ] Reutilizar modulo documental.
- [ ] Agregar auditoria.
- [ ] Agregar reporte.
- [ ] Probar transiciones validas.
- [ ] Probar transiciones invalidas.

### 3.8 Criterio de aceptacion

```text
Una resolucion se crea, se aprueba, se publica, se audita y puede consultarse en reporte.
No se permite publicar una resolucion no aprobada.
```

## 4. Fase 2B - Autorizaciones y certificaciones

Estado: futuro medio.
Porcentaje estimado: 12%.
Objetivo: modelar solicitudes institucionales de autorizacion y certificacion para entidades reguladas.

### 4.1 Por que va despues de resoluciones

Autorizaciones y certificaciones requieren mas flujo documental y mas estados. Es mejor tener primero resoluciones oficiales porque pueden cerrar o respaldar estas solicitudes.

### 4.2 Alcance funcional

```text
Crear solicitud de autorizacion.
Crear solicitud de certificacion.
Cargar documentos requeridos.
Marcar documentacion incompleta.
Pasar solicitud a revision.
Aprobar solicitud.
Rechazar solicitud.
Emitir autorizacion/certificacion.
Controlar fecha de vencimiento.
Renovar autorizacion/certificacion.
Enviar notificaciones.
Generar reportes.
```

### 4.3 Modelos sugeridos

```text
SolicitudAutorizacion
SolicitudCertificacion
TipoAutorizacion
TipoCertificacion
DocumentoSolicitud
```

Campos sugeridos para `SolicitudAutorizacion`:

```text
Id
NumeroSolicitud
SolicitanteNombre
SolicitanteRnc
PrestadoraId nullable
TipoAutorizacionId
Estado
Descripcion
FechaSolicitud
FechaRevision nullable
FechaAprobacion nullable
FechaVencimiento nullable
UsuarioResponsableId nullable
ResolucionInstitucionalId nullable
CreatedAt
UpdatedAt
```

### 4.4 Estados

```text
RECIBIDA
EN_REVISION
DOCUMENTACION_INCOMPLETA
APROBADA
RECHAZADA
VENCIDA
RENOVADA
```

### 4.5 Endpoints sugeridos

```text
POST /api/autorizaciones
GET /api/autorizaciones
GET /api/autorizaciones/{id}
PATCH /api/autorizaciones/{id}/estado
POST /api/autorizaciones/{id}/documentos
POST /api/autorizaciones/{id}/renovar

POST /api/certificaciones
GET /api/certificaciones
GET /api/certificaciones/{id}
PATCH /api/certificaciones/{id}/estado
POST /api/certificaciones/{id}/documentos
POST /api/certificaciones/{id}/renovar

GET /api/reportes/autorizaciones
GET /api/reportes/certificaciones
```

### 4.6 Reglas de negocio

```text
No se puede aprobar una solicitud sin documentos requeridos.
Una solicitud rechazada no se puede aprobar sin reabrir o crear nueva solicitud.
Una solicitud aprobada puede generar fecha de vencimiento.
Una solicitud vencida puede renovarse.
Toda aprobacion o rechazo debe auditarse.
Toda aprobacion puede generar notificacion.
```

### 4.7 Checklist de implementacion

- [ ] Crear modelos.
- [ ] Crear catalogos.
- [ ] Crear DTOs.
- [ ] Agregar DbSets.
- [ ] Crear migracion.
- [ ] Crear controladores.
- [ ] Crear endpoints principales.
- [ ] Reutilizar documentos seguros.
- [ ] Reutilizar auditoria.
- [ ] Reutilizar notificaciones.
- [ ] Crear reportes.
- [ ] Probar flujo aprobado.
- [ ] Probar flujo rechazado.
- [ ] Probar vencimiento/renovacion.

### 4.8 Criterio de aceptacion

```text
Una autorizacion o certificacion puede recibirse, revisarse, aprobarse o rechazarse, generar auditoria, adjuntar documentos y emitir notificacion.
```

## 5. Fase 2C - Espectro radioelectrico y licencias tecnicas

Estado: futuro medio/largo.
Porcentaje estimado: 10%.
Objetivo: representar la administracion tecnica del espectro radioelectrico y licencias tecnicas.

### 5.1 Por que va despues

Es el modulo mas tecnico. Requiere cuidado para no inventar un sistema demasiado grande. Se implementa con alcance controlado.

### 5.2 Alcance funcional

```text
Registrar frecuencias.
Consultar frecuencias disponibles.
Asignar frecuencia a prestadora o entidad.
Crear licencia tecnica.
Controlar vigencia.
Alertar licencias por vencer.
Cancelar licencia.
Consultar uso por provincia o region.
Generar reportes tecnicos.
```

### 5.3 Modelos sugeridos

```text
FrecuenciaRadioelectrica
AsignacionFrecuencia
LicenciaTecnica
RegionCobertura
```

Campos sugeridos para `FrecuenciaRadioelectrica`:

```text
Id
RangoInicioMHz
RangoFinMHz
Banda
ServicioUso
Provincia nullable
Region
Estado
Observacion
CreatedAt
UpdatedAt
```

Campos sugeridos para `LicenciaTecnica`:

```text
Id
NumeroLicencia
PrestadoraId nullable
EntidadAsignada
FrecuenciaRadioelectricaId
Estado
FechaInicio
FechaVencimiento
FechaCancelacion nullable
MotivoCancelacion nullable
ResolucionInstitucionalId nullable
CreatedAt
UpdatedAt
```

### 5.4 Estados

Frecuencia:

```text
DISPONIBLE
ASIGNADA
RESERVADA
SUSPENDIDA
```

Licencia:

```text
SOLICITADA
EN_EVALUACION_TECNICA
APROBADA
ACTIVA
POR_VENCER
VENCIDA
CANCELADA
```

### 5.5 Endpoints sugeridos

```text
POST /api/espectro/frecuencias
GET /api/espectro/frecuencias
GET /api/espectro/frecuencias/{id}
PATCH /api/espectro/frecuencias/{id}/estado

POST /api/espectro/asignaciones
GET /api/espectro/asignaciones
GET /api/espectro/asignaciones/{id}

POST /api/licencias-tecnicas
GET /api/licencias-tecnicas
GET /api/licencias-tecnicas/{id}
PATCH /api/licencias-tecnicas/{id}/estado
POST /api/licencias-tecnicas/{id}/cancelar

GET /api/reportes/espectro
GET /api/reportes/licencias-tecnicas
```

### 5.6 Reglas de negocio

```text
No se puede asignar una frecuencia que ya esta asignada.
Una licencia activa debe tener fecha de vencimiento.
Una licencia vencida debe aparecer en reportes.
Una licencia por vencer debe generar alerta/notificacion.
Toda asignacion o cancelacion se audita.
```

### 5.7 Checklist de implementacion

- [ ] Crear modelos de frecuencia y licencia.
- [ ] Crear DTOs.
- [ ] Agregar DbSets.
- [ ] Crear migracion.
- [ ] Crear controlador de espectro.
- [ ] Crear controlador de licencias tecnicas.
- [ ] Validar no duplicar asignaciones.
- [ ] Crear alerta de vencimiento.
- [ ] Agregar auditoria.
- [ ] Agregar reportes.
- [ ] Probar asignacion valida.
- [ ] Probar asignacion duplicada bloqueada.
- [ ] Probar licencia vencida.

### 5.8 Criterio de aceptacion

```text
El sistema puede registrar frecuencias, asignarlas, crear licencias tecnicas, controlar vencimiento y reportar el estado del espectro.
```

## 6. Fase 2D - Estadisticas regulatorias ampliadas

Estado: futuro corto/medio.
Porcentaje estimado: 5%.
Objetivo: ampliar los reportes actuales para que parezcan indicadores regulatorios institucionales.

### 6.1 Por que puede hacerse despues o en paralelo

No requiere grandes modelos nuevos. Aprovecha datos de reclamaciones y, cuando existan, datos de autorizaciones, resoluciones y licencias.

### 6.2 Alcance funcional

```text
Ranking de prestadoras con mas reclamaciones.
Ranking de prestadoras con mas SLA vencidas.
Tiempo promedio de respuesta.
Reclamaciones por mes.
Reclamaciones por provincia.
Servicios mas reclamados.
Resoluciones por periodo.
Autorizaciones por estado.
Certificaciones por estado.
Licencias tecnicas por vencimiento.
```

### 6.3 Endpoints sugeridos

```text
GET /api/reportes/prestadoras-ranking
GET /api/reportes/sla-ranking
GET /api/reportes/reclamaciones-mensual
GET /api/reportes/tiempo-promedio-respuesta
GET /api/reportes/servicios-mas-reclamados
GET /api/reportes/resoluciones-periodo
GET /api/reportes/autorizaciones-estado
GET /api/reportes/licencias-vencimiento
```

### 6.4 Checklist de implementacion

- [ ] Crear consultas agregadas.
- [ ] Crear DTOs de reportes.
- [ ] Agregar endpoints.
- [ ] Validar permisos por rol.
- [ ] Agregar pruebas con datos semilla.
- [ ] Evaluar exportacion CSV.
- [ ] Evaluar exportacion Excel.

### 6.5 Criterio de aceptacion

```text
Los reportes deben mostrar indicadores utiles para tomar decisiones regulatorias, no solamente listas de datos.
```

## 7. Fase 3 - Hardening para produccion real

Estado: futuro tecnico.
Objetivo: mejorar seguridad, arquitectura, pruebas y observabilidad.

Esta fase no suma funcionalidades de INDOTEL, pero mejora calidad de ingenieria.

### 7.1 Seguridad

```text
Refresh token.
Logout real.
Revocacion de tokens.
Bloqueo por intentos fallidos.
Rate limiting.
Politicas de contrasena mas fuertes.
Reset password con token persistido y hasheado.
```

Checklist:

- [ ] Crear tabla `RefreshTokens`.
- [ ] Crear endpoint `POST /api/auth/refresh-token`.
- [ ] Crear endpoint `POST /api/auth/logout`.
- [ ] Agregar bloqueo por intentos fallidos.
- [ ] Agregar rate limiting.
- [ ] Mejorar reset password.

### 7.2 Arquitectura

```text
Separar Controllers -> Services -> Repositories.
Crear servicios de aplicacion.
Crear validadores.
Centralizar reglas de estado.
Centralizar reglas de acceso documental.
```

Checklist:

- [ ] Crear `ReclamacionService`.
- [ ] Crear `DocumentoService`.
- [ ] Crear `AuditoriaService`.
- [ ] Crear `SlaService`.
- [ ] Crear repositorios donde aporte valor.
- [ ] Mover reglas repetidas fuera de controladores.

### 7.3 Pruebas

```text
xUnit.
Pruebas unitarias.
Pruebas de integracion.
Pruebas de seguridad por dueno real.
Pruebas de transiciones invalidas.
```

Checklist:

- [ ] Crear proyecto `Indotel.Core.Tests`.
- [ ] Probar login.
- [ ] Probar RBAC.
- [ ] Probar estados.
- [ ] Probar SLA.
- [ ] Probar documentos.
- [ ] Probar auditoria.
- [ ] Integrar pruebas al CI.

### 7.4 Observabilidad

```text
Logs estructurados.
Serilog.
CorrelationId en logs.
Metricas basicas.
Dashboard de errores.
```

Checklist:

- [ ] Agregar Serilog.
- [ ] Agregar request logging.
- [ ] Guardar CorrelationId en todos los logs.
- [ ] Medir errores 4xx/5xx.
- [ ] Medir tiempo de respuesta.

## 8. Fases descartadas para este proyecto academico

No se implementan:

```text
Consulta IMEI / SSN.
Consulta de lineas prepago por cedula.
Firma digital completa.
Comercio electronico completo.
Sandbox regulatorio.
Conectividad e infraestructura.
Licitaciones.
Portal de transparencia.
Noticias.
Aplicaciones moviles.
Integraciones reales con entidades externas.
```

Motivo:

```text
Aumentan demasiado el alcance.
No fortalecen directamente el Core ya probado.
Requieren integraciones o datos externos.
Pueden distraer de la defensa.
```

## 9. Orden recomendado de trabajo

```text
1. Mantener Fase 1 cerrada.
2. Presentar y defender Core actual.
3. Si hay tiempo despues: Fase 2A Resoluciones.
4. Luego Fase 2B Autorizaciones/Certificaciones.
5. Luego Fase 2D Estadisticas ampliadas.
6. Luego Fase 2C Espectro/Licencias tecnicas.
7. En paralelo, iniciar Fase 3 con pruebas automatizadas y seguridad.
```

Aunque el espectro es muy importante, se recomienda implementarlo despues de resoluciones y autorizaciones porque es mas tecnico y puede crecer demasiado.

## 10. Frase final

> El plan por fases conserva estable lo ya probado y permite evolucionar el Core hacia un sistema regulatorio mas completo. No agregamos todo INDOTEL; priorizamos lo que aporta mas valor academico y tecnico sin romper el alcance.
