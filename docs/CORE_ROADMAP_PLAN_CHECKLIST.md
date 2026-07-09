# Roadmap, plan y checklist priorizado del Core INDOTEL

Proyecto: Sistema Digital INDOTEL
Modulo actual: Core Backend
Rama: `core`

## 1. Objetivo del documento

Este documento define el alcance realista del proyecto, dividido en tres bloques:

```text
1. Lo que tenemos ahora.
2. Lo que haremos despues como Fase 2 priorizada.
3. Lo que dejamos fuera para no romper el alcance academico.
```

Los porcentajes no representan el INDOTEL real completo, porque una institucion real tiene muchas areas, procesos legales, administrativos, tecnicos y publicos. Estos porcentajes representan el alcance academico realista inspirado en INDOTEL.

## 2. Los tres porcentajes del alcance

| Bloque | Porcentaje | Significado |
|---|---:|---|
| Lo que tenemos ahora | 40% | Core de Atencion al Usuario y Reclamaciones, ya implementado y probado. |
| Lo que haremos en Fase 2 | 35% | Modulos regulatorios mas importantes que complementan el Core sin hacerlo gigante. |
| Lo que dejamos fuera | 25% | Funciones reales o institucionales que no conviene implementar por tiempo, complejidad o bajo valor para la defensa. |

Resumen:

```text
Tenemos ahora: 40%
Haremos despues: 35%
Dejamos fuera: 25%
Total: 100% del alcance academico priorizado
```

## 3. Lo que tenemos ahora - 40%

El Core actual ya cubre el eje principal de Atencion al Usuario y Reclamaciones.

### 3.1 Modulos implementados

```text
Autenticacion JWT.
Roles.
Ciudadanos.
Prestadoras.
Servicios telecom.
Reclamaciones.
Clasificacion de reclamaciones.
SLA regulatorio.
Respuesta de prestadora.
Resolucion.
Cierre.
Documentos seguros.
Auditoria institucional.
Busqueda paginada.
Reportes regulatorios.
Notificaciones internas.
Health checks.
Manejo global de errores.
CI basico.
```

### 3.2 Checklist de lo que ya esta completo

- [x] Login con JWT.
- [x] Registro ciudadano.
- [x] Roles principales.
- [x] CRUD de ciudadanos.
- [x] CRUD de prestadoras.
- [x] CRUD de servicios.
- [x] Crear reclamacion.
- [x] Validar reclamacion.
- [x] Enviar reclamacion a prestadora.
- [x] Respuesta de prestadora.
- [x] Revision interna.
- [x] Resolucion estructurada.
- [x] Cierre estructurado.
- [x] Bloqueo de transiciones invalidas.
- [x] Calculo de SLA.
- [x] Marcado de vencidas.
- [x] Subida de documentos.
- [x] Descarga segura de documentos.
- [x] Bloqueo 403 a ciudadano ajeno.
- [x] Auditoria de acciones sensibles.
- [x] Busqueda paginada.
- [x] Reportes regulatorios.
- [x] Notificaciones.
- [x] Health de API.
- [x] Health de base de datos.
- [x] Prueba final end-to-end.

## 4. Lo que haremos despues - 35%

Esta es la Fase 2 realista. No se implementa antes de la defensa si puede poner en riesgo lo que ya funciona.

### 4.1 Fase 2A - Resoluciones institucionales

Este modulo permite registrar resoluciones oficiales relacionadas con reclamaciones, prestadoras, autorizaciones o licencias.

#### Plan funcional

```text
Crear resolucion institucional.
Asociar resolucion a una reclamacion, prestadora, autorizacion o licencia.
Adjuntar documento oficial.
Publicar resolucion.
Archivar resolucion.
Auditar cambios.
```

#### Entidades sugeridas

```text
ResolucionInstitucional
DocumentoResolucion
TipoResolucion
```

#### Estados sugeridos

```text
BORRADOR
APROBADA
PUBLICADA
ARCHIVADA
```

#### Checklist

- [ ] Crear modelo `ResolucionInstitucional`.
- [ ] Crear DTOs de creacion y publicacion.
- [ ] Crear migracion.
- [ ] Crear endpoint `POST /api/resoluciones`.
- [ ] Crear endpoint `GET /api/resoluciones`.
- [ ] Crear endpoint `GET /api/resoluciones/{id}`.
- [ ] Crear endpoint `PATCH /api/resoluciones/{id}/publicar`.
- [ ] Permitir adjuntar documento oficial.
- [ ] Auditar creacion, aprobacion y publicacion.
- [ ] Agregar reporte de resoluciones por tipo y fecha.

### 4.2 Fase 2B - Autorizaciones y certificaciones

Este modulo acerca el sistema a funciones reales de INDOTEL relacionadas con permisos, autorizaciones y certificaciones.

#### Plan funcional

```text
Crear solicitud de autorizacion.
Crear solicitud de certificacion.
Validar documentos requeridos.
Marcar documentacion incompleta.
Aprobar o rechazar solicitud.
Emitir certificacion o autorizacion.
Controlar vencimiento y renovacion.
Notificar al solicitante.
```

#### Entidades sugeridas

```text
SolicitudAutorizacion
SolicitudCertificacion
TipoAutorizacion
TipoCertificacion
DocumentoSolicitud
```

#### Estados sugeridos

```text
RECIBIDA
EN_REVISION
DOCUMENTACION_INCOMPLETA
APROBADA
RECHAZADA
VENCIDA
RENOVADA
```

#### Checklist

- [ ] Crear modelo `SolicitudAutorizacion`.
- [ ] Crear modelo `SolicitudCertificacion`.
- [ ] Crear catalogo `TipoAutorizacion`.
- [ ] Crear catalogo `TipoCertificacion`.
- [ ] Crear migracion.
- [ ] Crear endpoint `POST /api/autorizaciones`.
- [ ] Crear endpoint `GET /api/autorizaciones`.
- [ ] Crear endpoint `PATCH /api/autorizaciones/{id}/estado`.
- [ ] Crear endpoint `POST /api/certificaciones`.
- [ ] Crear endpoint `GET /api/certificaciones`.
- [ ] Reutilizar documentos seguros.
- [ ] Reutilizar auditoria.
- [ ] Reutilizar notificaciones.
- [ ] Crear reporte de solicitudes por estado.
- [ ] Crear alerta de vencimientos.

### 4.3 Fase 2C - Espectro radioelectrico y licencias tecnicas

Este es el modulo mas tecnico y regulatorio. Se implementaria despues de autorizaciones porque requiere mas cuidado.

#### Plan funcional

```text
Registrar rangos de frecuencia.
Registrar asignaciones.
Vincular frecuencia a prestadora o entidad autorizada.
Crear licencia tecnica.
Controlar fecha de inicio y vencimiento.
Alertar licencias por vencer.
Consultar frecuencias disponibles/asignadas.
Reportar uso por provincia o region.
```

#### Entidades sugeridas

```text
FrecuenciaRadioelectrica
AsignacionFrecuencia
LicenciaTecnica
RegionCobertura
```

#### Estados sugeridos para frecuencia

```text
DISPONIBLE
ASIGNADA
RESERVADA
SUSPENDIDA
```

#### Estados sugeridos para licencia

```text
SOLICITADA
EN_EVALUACION_TECNICA
APROBADA
ACTIVA
POR_VENCER
VENCIDA
CANCELADA
```

#### Checklist

- [ ] Crear modelo `FrecuenciaRadioelectrica`.
- [ ] Crear modelo `AsignacionFrecuencia`.
- [ ] Crear modelo `LicenciaTecnica`.
- [ ] Crear migracion.
- [ ] Crear endpoint `POST /api/espectro/frecuencias`.
- [ ] Crear endpoint `GET /api/espectro/frecuencias`.
- [ ] Crear endpoint `POST /api/espectro/asignaciones`.
- [ ] Crear endpoint `GET /api/espectro/asignaciones`.
- [ ] Crear endpoint `POST /api/licencias-tecnicas`.
- [ ] Crear endpoint `GET /api/licencias-tecnicas`.
- [ ] Crear alerta de licencias por vencer.
- [ ] Auditar asignaciones y cancelaciones.
- [ ] Crear reporte de frecuencias por estado.
- [ ] Crear reporte de licencias por vencimiento.

### 4.4 Fase 2D - Estadisticas regulatorias ampliadas

Este modulo no requiere tantos modelos nuevos. Aprovecha datos existentes y los nuevos de la Fase 2.

#### Plan funcional

```text
Indicadores por prestadora.
Indicadores por servicio.
Indicadores por provincia.
Indicadores por tipo de reclamacion.
Tiempos promedio de respuesta.
Prestadoras con mas vencimientos SLA.
Solicitudes de autorizacion por estado.
Licencias tecnicas por vencimiento.
Resoluciones por periodo.
```

#### Checklist

- [ ] Agregar reporte de prestadoras con mas reclamaciones.
- [ ] Agregar reporte de prestadoras con mas SLA vencidas.
- [ ] Agregar reporte mensual de reclamaciones.
- [ ] Agregar reporte de tiempo promedio de respuesta.
- [ ] Agregar reporte de autorizaciones por estado.
- [ ] Agregar reporte de certificaciones por estado.
- [ ] Agregar reporte de licencias tecnicas por vencimiento.
- [ ] Agregar reporte de resoluciones por periodo.
- [ ] Evaluar exportacion CSV/Excel.

## 5. Lo que dejamos fuera - 25%

Estas funciones pueden existir alrededor de INDOTEL real, pero no entran en el alcance academico priorizado.

### 5.1 Lista fuera de alcance

```text
Consulta IMEI / SSN.
Consulta de lineas prepago por cedula.
Firma digital completa.
Comercio electronico completo.
Sandbox regulatorio.
Proyectos de conectividad e infraestructura.
Licitaciones.
Portal de transparencia.
Noticias y sala de prensa.
Buzon institucional interno de sugerencias.
Integracion real con entidades externas.
Aplicaciones moviles.
Portal ciudadano completo.
Portal prestadora completo.
```

### 5.2 Por que se deja fuera

```text
No fortalece directamente el Core ya probado.
Requiere integraciones externas o mucha data.
Puede convertir el proyecto en algo demasiado grande.
No es necesario para demostrar dominio de arquitectura backend.
No debe arriesgar la estabilidad antes de la defensa.
```

### 5.3 Checklist de decision

- [x] Dejar IMEI fuera.
- [x] Dejar lineas prepago fuera.
- [x] Dejar firma digital completa fuera.
- [x] Dejar comercio electronico fuera.
- [x] Dejar sandbox regulatorio fuera.
- [x] Dejar conectividad e infraestructura fuera.
- [x] Dejar licitaciones fuera.
- [x] Dejar transparencia/noticias fuera.
- [x] Dejar integraciones reales fuera.

## 6. Roadmap resumido

```text
Fase 1 - Core actual
Estado: completado
Porcentaje: 40%
Resultado: reclamaciones, SLA, documentos, auditoria, reportes, notificaciones.

Fase 2A - Resoluciones institucionales
Estado: futuro corto
Porcentaje estimado dentro de Fase 2: 8%

Fase 2B - Autorizaciones y certificaciones
Estado: futuro medio
Porcentaje estimado dentro de Fase 2: 12%

Fase 2C - Espectro radioelectrico y licencias tecnicas
Estado: futuro medio/largo
Porcentaje estimado dentro de Fase 2: 10%

Fase 2D - Estadisticas regulatorias ampliadas
Estado: futuro corto/medio
Porcentaje estimado dentro de Fase 2: 5%

Fuera de alcance
Estado: descartado para este proyecto academico
Porcentaje: 25%
```

## 7. Frase para defensa

> El proyecto no intenta copiar todo INDOTEL. Tomamos el eje mas defendible para Desarrollo 3: el Core de reclamaciones, SLA, documentos, auditoria y reportes. Luego priorizamos una Fase 2 con autorizaciones, certificaciones, espectro, licencias tecnicas, resoluciones y estadisticas. Dejamos fuera funciones que requieren integraciones externas o que no aportan directamente al Core academico.

## 8. Decision final

La decision final es:

```text
No tocar el Core funcional antes de la defensa.
Documentar la Fase 2 como evolucion realista.
Mantener fuera los modulos que aumentan riesgo y complejidad.
```
