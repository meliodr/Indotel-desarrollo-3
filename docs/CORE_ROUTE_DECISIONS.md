# Decisiones de rutas del Core INDOTEL

Este documento define las rutas oficiales del Core y las rutas alias que se aceptaran para facilitar la integracion con Web, Caja y Gateway.

El objetivo es evitar contradicciones entre documentos, Swagger y codigo.

## 1. Problema detectado

Durante la revision del Core y los documentos de Web/Caja se detectaron diferencias de nombres:

- El Core actual usa algunas rutas como `/api/catalogos/prestadoras`.
- Los documentos de Web y Caja mencionan rutas como `/api/prestadoras`.
- El Core actual usa `PUT` para cambio de estado.
- El documento de Caja menciona `PATCH` para cambio de estado.
- Web pide `POST /api/auth/register-ciudadano`, pero el Core actual tiene `POST /api/ciudadanos`.
- Caja menciona endpoints especificos como `/resolver`, `/cerrar` y `/enviar-prestadora`, mientras el Core actual usa cambio de estado generico.

La decision no debe ser borrar lo que ya existe. La decision correcta es mantener compatibilidad y documentar rutas oficiales.

## 2. Principio general

```text
Una ruta oficial representa el contrato principal del Core.
Una ruta alias existe para compatibilidad con Web, Caja o documentos anteriores.
Un alias no debe duplicar logica; debe llamar al mismo servicio interno.
```

## 3. Convencion de rutas

Para esta etapa se usara:

```text
/api/{modulo}
```

Ejemplos:

```text
/api/auth/login
/api/ciudadanos
/api/reclamaciones
/api/reportes/resumen
```

No se usara todavia `/api/v1` para evitar romper el avance actual.

La ruta `/api/v1` queda como mejora futura.

## 4. Rutas oficiales actuales

Estas rutas ya existen y se mantienen como oficiales para el MVP actual:

| Modulo | Metodo | Ruta oficial | Estado |
|---|---|---|---|
| Health | GET | /health | Actual |
| Auth | POST | /api/auth/login | Actual |
| Auth | GET | /api/auth/me | Actual |
| Catalogos | GET | /api/catalogos/roles | Actual |
| Catalogos | GET | /api/catalogos/servicios | Actual |
| Catalogos | GET | /api/catalogos/prestadoras | Actual |
| Ciudadanos | GET | /api/ciudadanos | Actual |
| Ciudadanos | GET | /api/ciudadanos/{id} | Actual |
| Ciudadanos | POST | /api/ciudadanos | Actual |
| Reclamaciones | GET | /api/reclamaciones | Actual |
| Reclamaciones | GET | /api/reclamaciones/{id} | Actual |
| Reclamaciones | POST | /api/reclamaciones | Actual |
| Reclamaciones | PUT | /api/reclamaciones/{id}/estado | Actual |
| Reclamaciones | POST | /api/reclamaciones/{id}/respuesta-prestadora | Actual |
| Reclamaciones | GET | /api/reclamaciones/{id}/historial | Actual |
| Reclamaciones | GET | /api/reclamaciones/{id}/respuestas | Actual |
| Reportes | GET | /api/reportes/resumen | Actual |
| Reportes | GET | /api/reportes/reclamaciones-por-estado | Actual |
| Reportes | GET | /api/reportes/reclamaciones-por-prestadora | Actual |

## 5. Alias que se deben agregar

Estos alias se agregaran para que Web y Caja puedan consumir rutas mas simples sin romper las rutas actuales.

| Modulo | Metodo | Alias nuevo | Apunta a | Motivo |
|---|---|---|---|---|
| Prestadoras | GET | /api/prestadoras | /api/catalogos/prestadoras | Web/Caja lo esperan |
| Servicios | GET | /api/servicios | /api/catalogos/servicios | Web/Caja lo esperan |
| Reclamaciones | PATCH | /api/reclamaciones/{id}/estado | PUT /api/reclamaciones/{id}/estado | Caja menciona PATCH |

Decision: estos alias se implementaran primero porque no requieren cambios fuertes de base de datos.

## 6. Rutas que se agregaran proximamente

Estas rutas no existen todavia, pero Web y Caja las necesitan.

| Modulo | Metodo | Ruta | Prioridad | Motivo |
|---|---|---|---|---|
| Usuarios | GET | /api/usuarios | Alta | Caja necesita gestionar usuarios internos |
| Usuarios | GET | /api/usuarios/{id} | Alta | Ver detalle de usuario |
| Usuarios | POST | /api/usuarios | Alta | Crear usuarios por rol |
| Usuarios | PUT | /api/usuarios/{id} | Alta | Editar usuario |
| Usuarios | PATCH | /api/usuarios/{id}/estado | Alta | Activar/desactivar usuario |
| Usuarios | PUT | /api/usuarios/{id}/password | Media | Cambio administrativo de password |
| Ciudadanos | GET | /api/ciudadanos/cedula/{cedula} | Alta | Caja necesita buscar por cedula |
| Ciudadanos | PUT | /api/ciudadanos/{id} | Alta | Web/Caja necesitan editar perfil |
| Ciudadanos | GET | /api/ciudadanos/{id}/reclamaciones | Alta | Web necesita Mis reclamaciones |
| Reclamaciones | GET | /api/reclamaciones/expediente/{numero} | Alta | Web necesita consultar expediente |
| Documentos | POST | /api/reclamaciones/{id}/documentos | Alta | Web/Caja necesitan evidencias |
| Documentos | GET | /api/reclamaciones/{id}/documentos | Alta | Ver documentos del caso |
| Documentos | DELETE | /api/documentos/{id} | Media | Eliminar evidencia si aplica |
| Auditoria | GET | /api/auditoria/reclamacion/{id} | Media | Caja y auditor necesitan trazabilidad |

## 7. Registro ciudadano

### Situacion actual

Existe:

```text
POST /api/ciudadanos
```

Este endpoint registra datos del ciudadano, pero no necesariamente crea una cuenta de usuario con login.

### Ruta que Web espera

```text
POST /api/auth/register-ciudadano
```

### Decision

Se mantendra:

```text
POST /api/ciudadanos
```

Uso:

```text
Registrar datos ciudadanos desde Caja o Core.
```

Se agregara despues:

```text
POST /api/auth/register-ciudadano
```

Uso:

```text
Crear Ciudadano + Usuario + Rol Ciudadano + PasswordHash.
```

Esta ruta debe esperar hasta que se implemente CRUD de usuarios y vinculacion Usuario-Ciudadano.

## 8. Cambio de estado

### Situacion actual

Existe:

```text
PUT /api/reclamaciones/{id}/estado
```

### Ruta que Caja menciona

```text
PATCH /api/reclamaciones/{id}/estado
```

### Decision

Se aceptaran ambos:

```text
PUT /api/reclamaciones/{id}/estado
PATCH /api/reclamaciones/{id}/estado
```

Ambos deben usar la misma logica interna.

La decision funcional es:

- `PUT` se mantiene por compatibilidad con lo ya probado.
- `PATCH` se agrega porque semanticamente cambiar solo el estado es una modificacion parcial.

## 9. Endpoints especificos de reclamacion

Caja menciona:

```text
POST /api/reclamaciones/{id}/enviar-prestadora
POST /api/reclamaciones/{id}/resolver
POST /api/reclamaciones/{id}/cerrar
```

Actualmente el Core usa:

```text
PUT /api/reclamaciones/{id}/estado
```

### Decision

Primero se implementara una maquina de estados estricta.

Despues se pueden crear endpoints especificos como alias funcionales:

| Ruta futura | Estado que aplicaria | Motivo |
|---|---|---|
| POST /api/reclamaciones/{id}/enviar-prestadora | ENVIADA_A_PRESTADORA | Mas facil para Caja |
| POST /api/reclamaciones/{id}/resolver | RESUELTA | Accion clara de negocio |
| POST /api/reclamaciones/{id}/cerrar | CERRADA | Cierre formal del expediente |
| POST /api/reclamaciones/{id}/rechazar | RECHAZADA | Rechazo formal |

Estas rutas no deben duplicar reglas. Deben llamar al mismo servicio de estados.

## 10. Catalogos vs modulos administrativos

### Catalogos de solo lectura

Se mantienen en:

```text
/api/catalogos/roles
/api/catalogos/servicios
/api/catalogos/prestadoras
```

Uso:

```text
Llenar selectores y listas simples.
```

### Modulos administrativos futuros

Se agregaran si hace falta:

```text
/api/prestadoras
/api/servicios
```

Pero para esta etapa:

```text
GET /api/prestadoras
GET /api/servicios
```

seran alias de lectura.

Mas adelante, si se crean CRUD completos, esas rutas podran tener:

```text
POST /api/prestadoras
PUT /api/prestadoras/{id}
POST /api/servicios
PUT /api/servicios/{id}
```

## 11. Reglas para no duplicar logica

- Los controladores pueden tener varias rutas.
- La logica no debe copiarse y pegarse entre rutas.
- Si dos rutas hacen lo mismo, deben llamar al mismo metodo privado o servicio.
- Si una ruta es alias, debe indicarse en la documentacion.
- Swagger debe mostrar claramente las rutas disponibles.

## 12. Impacto para Web

Web podra usar:

```text
POST /api/auth/login
GET /api/prestadoras
GET /api/servicios
POST /api/reclamaciones
GET /api/reclamaciones/{id}
GET /api/reclamaciones/{id}/historial
```

Cuando se implementen mejoras, Web tambien podra usar:

```text
POST /api/auth/register-ciudadano
GET /api/ciudadanos/{id}/reclamaciones
GET /api/reclamaciones/expediente/{numero}
POST /api/reclamaciones/{id}/documentos
```

## 13. Impacto para Caja

Caja podra usar:

```text
POST /api/auth/login
GET /api/reportes/resumen
GET /api/reclamaciones
GET /api/reclamaciones/{id}
POST /api/reclamaciones
PUT /api/reclamaciones/{id}/estado
PATCH /api/reclamaciones/{id}/estado
GET /api/prestadoras
GET /api/servicios
```

Cuando se implementen mejoras, Caja tambien podra usar:

```text
GET /api/usuarios
GET /api/ciudadanos/cedula/{cedula}
PUT /api/ciudadanos/{id}
POST /api/reclamaciones/{id}/documentos
GET /api/auditoria/reclamacion/{id}
```

## 14. Impacto para Gateway

El Gateway debe poder enrutar rutas del Core sin cambiar la logica del Core.

Ejemplo futuro:

```text
/gateway/auth/login -> /api/auth/login
/gateway/reclamaciones -> /api/reclamaciones
/gateway/catalogos/prestadoras -> /api/catalogos/prestadoras
```

El Gateway no debe cambiar reglas de negocio. Solo debe enrutar, proteger, limitar o transformar si se define asi.

## 15. Orden de implementacion de rutas

Primero:

- [ ] GET /api/prestadoras.
- [ ] GET /api/servicios.
- [ ] PATCH /api/reclamaciones/{id}/estado.

Segundo:

- [ ] GET /api/ciudadanos/cedula/{cedula}.
- [ ] PUT /api/ciudadanos/{id}.
- [ ] GET /api/ciudadanos/{id}/reclamaciones.
- [ ] GET /api/reclamaciones/expediente/{numero}.

Tercero:

- [ ] CRUD de usuarios.
- [ ] POST /api/auth/register-ciudadano.

Cuarto:

- [ ] Documentos/evidencias.
- [ ] Auditoria.
- [ ] Endpoints especificos de estados.

## 16. Decision final

Para la siguiente etapa se aprueban estas reglas:

1. No se eliminan rutas actuales.
2. Se agregan alias para compatibilidad con Web y Caja.
3. PUT de estado se mantiene.
4. PATCH de estado se agrega.
5. Registro ciudadano con login espera a Usuarios.
6. Endpoints especificos de reclamacion esperan a la maquina de estados.
7. Toda nueva ruta debe documentarse antes o al mismo tiempo que se programa.
