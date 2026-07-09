# Matriz de roles y permisos del Core INDOTEL

Este documento define los roles funcionales que debe manejar el Core del Sistema Digital INDOTEL y que permisos debe tener cada uno.

No es codigo todavia. Es una decision de arquitectura para evitar programar permisos sin criterio.

## 1. Base investigada sobre INDOTEL

Segun el portal oficial de INDOTEL, el sistema institucional no solo trata reclamaciones. Tambien aparecen areas y servicios como:

- Departamento de Asistencia al Usuario DAU.
- Formulario de Reclamacion Electronico.
- Preguntas frecuentes.
- Derechos y deberes.
- Consulta de IMEI y estado de equipo movil.
- Estadisticas.
- Resoluciones.
- Comercio electronico.
- Firma digital.
- Asignaciones de radiofrecuencias.
- Espectro radioelectrico.
- Television digital.
- Telecable.
- Telefonicas.
- Radioaficionados.
- Radio.
- Internet.
- Revendedoras autorizadas.

En la pagina de todos los servicios tambien aparecen servicios como:

- Gestion de Autorizaciones.
- Gestion de Certificaciones.
- Atencion a Reclamaciones de usuarios ante prestadoras.
- Inscripcion en Registro Especial para Radioaficionado.
- Gestion de Autorizaciones para Firma Digital.
- Solucion de controversias entre prestadoras.

Conclusion: el Core debe ser transversal. No debe limitarse a login y reclamaciones; debe preparar una base comun para usuarios, roles, permisos, documentos, auditoria, workflow, catalogos y reportes.

## 2. Principio de diseno

El Core no debe tener pantallas. El Core debe controlar:

- Quien entra.
- Que rol tiene.
- Que puede ver.
- Que puede crear.
- Que puede modificar.
- Que acciones quedan auditadas.
- Que datos se comparten con Web, Caja y Gateway.

Web, Caja, Portal de Operadores y otros modulos deben consumir el Core por API.

## 3. Roles propuestos

| Rol | Descripcion | Tipo | Estado |
|---|---|---|---|
| Administrador | Usuario tecnico/funcional con control total del sistema. | Interno | Necesario ahora |
| AnalistaDAU | Personal del Departamento de Asistencia al Usuario que valida y da seguimiento a reclamaciones. | Interno | Necesario ahora |
| SupervisorDAU | Responsable de revisar casos, reasignar, cerrar o corregir decisiones. | Interno | Fase siguiente |
| RepresentantePrestadora | Usuario de una prestadora que responde reclamaciones asignadas a su empresa. | Externo institucional | Necesario ahora o siguiente |
| Ciudadano | Usuario que crea y consulta sus propias reclamaciones. | Externo ciudadano | Necesario para Web |
| Auditor | Usuario que consulta auditorias, historial y reportes sin alterar datos. | Interno/control | Necesario siguiente |
| Inspector | Personal relacionado con inspecciones, fiscalizacion o verificaciones tecnicas. | Interno | Futuro modulo |
| OperadorRegulatorio | Funcionario que gestiona autorizaciones, certificaciones, radioaficionados, espectro u otros tramites. | Interno | Futuro modulo |
| Sistema | Actor automatico para tareas programadas: vencimientos, notificaciones, limpieza o reportes. | Automatico | Futuro tecnico |

## 4. Roles que se implementan primero

Para no complicar el MVP, primero deben implementarse estos:

1. Administrador.
2. AnalistaDAU.
3. RepresentantePrestadora.
4. Ciudadano.
5. Auditor.

Los demas se documentan para no olvidarlos, pero no bloquean la siguiente etapa.

## 5. Leyenda de permisos

| Simbolo | Significado |
|---|---|
| SI | Puede ejecutar la accion. |
| NO | No puede ejecutar la accion. |
| PROPIO | Solo sobre sus propios datos. |
| EMPRESA | Solo sobre los datos de su prestadora/empresa. |
| LECTURA | Puede consultar, pero no modificar. |
| FUTURO | No se implementa ahora, pero se reserva para una fase posterior. |

## 6. Matriz general de permisos

| Funcion / Modulo | Administrador | AnalistaDAU | SupervisorDAU | RepresentantePrestadora | Ciudadano | Auditor | Inspector | OperadorRegulatorio | Sistema |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Iniciar sesion | SI | SI | SI | SI | SI | SI | SI | SI | NO |
| Ver su perfil | SI | SI | SI | SI | SI | SI | SI | SI | NO |
| Cambiar su password | SI | SI | SI | SI | SI | SI | SI | SI | NO |
| Gestionar usuarios | SI | NO | NO | NO | NO | NO | NO | NO | NO |
| Activar/desactivar usuarios | SI | NO | NO | NO | NO | NO | NO | NO | NO |
| Asignar roles | SI | NO | NO | NO | NO | NO | NO | NO | NO |
| Ver catalogos | SI | SI | SI | SI | SI | SI | SI | SI | SI |
| Gestionar catalogos | SI | NO | FUTURO | NO | NO | NO | NO | FUTURO | NO |
| Ver ciudadanos | SI | SI | SI | NO | PROPIO | LECTURA | FUTURO | FUTURO | NO |
| Crear ciudadano | SI | SI | SI | NO | PROPIO | NO | NO | NO | NO |
| Editar ciudadano | SI | SI | SI | NO | PROPIO | NO | NO | NO | NO |
| Buscar ciudadano por cedula | SI | SI | SI | NO | NO | LECTURA | FUTURO | FUTURO | NO |
| Ver prestadoras | SI | SI | SI | EMPRESA | LECTURA | LECTURA | SI | SI | NO |
| Gestionar prestadoras | SI | NO | FUTURO | NO | NO | NO | NO | FUTURO | NO |
| Crear reclamacion | SI | SI | SI | NO | PROPIO | NO | NO | NO | NO |
| Ver todas las reclamaciones | SI | SI | SI | NO | NO | LECTURA | FUTURO | FUTURO | NO |
| Ver reclamaciones propias | SI | SI | SI | EMPRESA | PROPIO | LECTURA | FUTURO | FUTURO | NO |
| Cambiar estado de reclamacion | SI | SI | SI | NO | NO | NO | NO | NO | FUTURO |
| Enviar a prestadora | SI | SI | SI | NO | NO | NO | NO | NO | FUTURO |
| Responder reclamacion | SI | NO | SI | EMPRESA | NO | NO | NO | NO | NO |
| Resolver reclamacion | SI | SI | SI | NO | NO | NO | NO | NO | NO |
| Cerrar reclamacion | SI | NO | SI | NO | NO | NO | NO | NO | FUTURO |
| Rechazar reclamacion | SI | SI | SI | NO | NO | NO | NO | NO | NO |
| Adjuntar documentos | SI | SI | SI | EMPRESA | PROPIO | NO | FUTURO | FUTURO | NO |
| Ver documentos | SI | SI | SI | EMPRESA | PROPIO | LECTURA | FUTURO | FUTURO | NO |
| Eliminar documentos | SI | SI | SI | NO | PROPIO_LIMITADO | NO | NO | NO | FUTURO |
| Ver historial | SI | SI | SI | EMPRESA | PROPIO | LECTURA | FUTURO | FUTURO | NO |
| Ver auditoria | SI | NO | SI | NO | NO | LECTURA | NO | NO | NO |
| Ver reportes basicos | SI | SI | SI | EMPRESA | NO | LECTURA | FUTURO | FUTURO | FUTURO |
| Exportar reportes | SI | SI | SI | EMPRESA | NO | LECTURA | FUTURO | FUTURO | FUTURO |
| Gestionar autorizaciones | SI | NO | FUTURO | EMPRESA | NO | LECTURA | NO | SI | NO |
| Gestionar certificaciones | SI | NO | FUTURO | EMPRESA | NO | LECTURA | NO | SI | NO |
| Gestionar radioaficionados | SI | NO | FUTURO | NO | PROPIO | LECTURA | NO | SI | NO |
| Gestionar espectro | SI | NO | FUTURO | NO | NO | LECTURA | SI | SI | NO |
| Gestionar inspecciones | SI | NO | FUTURO | NO | NO | LECTURA | SI | FUTURO | NO |
| Gestionar notificaciones | SI | NO | FUTURO | NO | NO | NO | NO | NO | SI |
| Ejecutar vencimientos/SLA | NO | NO | NO | NO | NO | NO | NO | NO | SI |

## 7. Permisos especificos para endpoints actuales

| Endpoint actual | Administrador | AnalistaDAU | RepresentantePrestadora | Ciudadano | Auditor | Nota |
|---|---:|---:|---:|---:|---:|---|
| POST /api/auth/login | SI | SI | SI | SI | SI | Publico controlado |
| GET /api/auth/me | SI | SI | SI | SI | SI | Usuario autenticado |
| GET /api/catalogos/roles | SI | SI | SI | SI | SI | Solo lectura |
| GET /api/catalogos/servicios | SI | SI | SI | SI | SI | Solo lectura |
| GET /api/catalogos/prestadoras | SI | SI | SI | SI | SI | Solo lectura |
| GET /api/ciudadanos | SI | SI | NO | NO | LECTURA | Evitar exposicion al ciudadano |
| GET /api/ciudadanos/{id} | SI | SI | NO | PROPIO | LECTURA | Ciudadano solo el suyo |
| POST /api/ciudadanos | SI | SI | NO | PROPIO | NO | Puede usarse para registro ciudadano |
| GET /api/reclamaciones | SI | SI | EMPRESA | PROPIO | LECTURA | Debe filtrarse segun rol |
| GET /api/reclamaciones/{id} | SI | SI | EMPRESA | PROPIO | LECTURA | Debe validar pertenencia |
| POST /api/reclamaciones | SI | SI | NO | PROPIO | NO | Caja o ciudadano |
| PUT /api/reclamaciones/{id}/estado | SI | SI | NO | NO | NO | Luego aplicar maquina de estados |
| POST /api/reclamaciones/{id}/respuesta-prestadora | SI | NO | EMPRESA | NO | NO | Prestadora solo su caso |
| GET /api/reclamaciones/{id}/historial | SI | SI | EMPRESA | PROPIO | LECTURA | Historial visible por pertenencia |
| GET /api/reclamaciones/{id}/respuestas | SI | SI | EMPRESA | PROPIO | LECTURA | Respuesta visible por pertenencia |
| GET /api/reportes/resumen | SI | SI | EMPRESA | NO | LECTURA | Prestadora solo datos de su empresa |
| GET /api/reportes/reclamaciones-por-estado | SI | SI | EMPRESA | NO | LECTURA | Filtrar por rol |
| GET /api/reportes/reclamaciones-por-prestadora | SI | SI | NO | NO | LECTURA | Prestadora no debe ver comparativo completo si no aplica |

## 8. Reglas de seguridad por rol

### Administrador

Puede administrar todo. Debe usarse con cuidado. No debe ser el usuario normal de trabajo diario.

### AnalistaDAU

Puede operar reclamaciones, revisar ciudadanos, validar casos y cambiar estados. No debe administrar usuarios ni cambiar roles.

### SupervisorDAU

Puede revisar casos complejos, cerrar casos, corregir errores y ver auditoria operativa. Se implementa despues.

### RepresentantePrestadora

Solo puede ver y responder reclamaciones asignadas a su prestadora. No puede ver datos de otras prestadoras ni cambiar estados internos del INDOTEL.

### Ciudadano

Solo puede crear reclamaciones y consultar sus propios expedientes. No puede ver listados generales ni datos de otros ciudadanos.

### Auditor

Solo lectura. Puede revisar auditoria, historial y reportes. No debe crear, editar, responder ni cerrar casos.

### Inspector

Reservado para modulo de inspecciones y fiscalizacion.

### OperadorRegulatorio

Reservado para autorizaciones, certificaciones, radioaficionados, espectro y otros tramites regulatorios.

### Sistema

Actor automatico. Puede ejecutar tareas programadas como vencimientos, notificaciones y limpieza, pero no inicia sesion como usuario humano.

## 9. Reglas que el codigo debe cumplir

- Ningun endpoint protegido debe depender solo de que exista un token.
- El token debe incluir ID, correo, nombre y rol.
- Las consultas deben filtrar datos segun el rol.
- Un ciudadano no puede consultar otro ciudadano.
- Una prestadora no puede consultar reclamaciones de otra prestadora.
- Un auditor no puede modificar registros.
- Todo cambio importante debe registrar historial y auditoria.
- Los endpoints de escritura deben validar datos antes de guardar.
- Los permisos deben estar documentados antes de implementarse.

## 10. Orden de implementacion recomendado

1. Crear/normalizar roles semilla:
   - Administrador.
   - AnalistaDAU.
   - RepresentantePrestadora.
   - Ciudadano.
   - Auditor.
2. Crear CRUD de usuarios.
3. Agregar propiedades para vincular Usuario con Ciudadano o Prestadora.
4. Aplicar `[Authorize(Roles = ...)]` a endpoints principales.
5. Filtrar datos segun rol en Ciudadanos, Reclamaciones y Reportes.
6. Crear pruebas manuales en Swagger por cada rol.
7. Documentar usuarios semilla de prueba por rol.

## 11. Usuarios semilla sugeridos para pruebas

| Rol | Correo sugerido | Uso |
|---|---|---|
| Administrador | admin@indotel.test | Control total |
| AnalistaDAU | analista@indotel.test | Gestionar reclamaciones |
| RepresentantePrestadora | claro@prestadora.test | Responder casos de Claro |
| Ciudadano | ciudadano@indotel.test | Crear y consultar sus casos |
| Auditor | auditor@indotel.test | Revisar historial y reportes |

Las claves deben definirse en ambiente de desarrollo y nunca usarse en produccion.

## 12. Decision para el proyecto

Para la siguiente etapa del Core, se adopta esta matriz como contrato inicial de permisos. Si un endpoint no aparece aqui, no debe programarse sin decidir primero que roles pueden usarlo.
