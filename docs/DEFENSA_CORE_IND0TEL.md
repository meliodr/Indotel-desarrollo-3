# Guia de defensa del Core INDOTEL

Proyecto: Sistema Digital INDOTEL
Modulo: Core Backend
Rama: `core`

## 1. Frase principal de defensa

> Nuestro Core no es solo un CRUD. Es un motor regulatorio que valida estados, controla plazos SLA, protege documentos por dueno real, registra auditoria institucional y genera reportes para seguimiento operativo.

## 2. Como presentar el estado del proyecto

Usar esta formulacion:

```text
El Core esta validado al 100% dentro del alcance academico definido.
Es un prototipo institucional avanzado, funcional y probado end-to-end.
No se declara como produccion gubernamental real certificada.
```

Evitar decir sin contexto:

```text
Esta listo para produccion real.
Es 100% de todo.
No le falta nada.
```

## 3. Que problema resuelve

El Core centraliza el proceso de reclamaciones de usuarios contra prestadoras de servicios de telecomunicaciones.

Permite:

- Registrar reclamaciones.
- Validar el caso.
- Enviar el caso a la prestadora.
- Recibir respuesta de la prestadora.
- Revisar el expediente.
- Emitir resolucion.
- Cerrar el caso.
- Auditar cada accion sensible.
- Medir plazos de respuesta.
- Proteger evidencias/documentos.
- Generar reportes regulatorios.

## 4. Los tres motores que prueban que no es CRUD

### 4.1 Motor de estados

Un CRUD permitiria cambiar cualquier campo libremente.

El Core INDOTEL no permite eso. Valida las transiciones de estado.

Ejemplos:

```text
RECIBIDA -> VALIDADA                      permitido
VALIDADA -> ENVIADA_A_PRESTADORA          permitido
ENVIADA_A_PRESTADORA -> RESPONDIDA...     permitido
RECIBIDA -> CERRADA                       bloqueado
Cerrar sin resolver                       bloqueado
```

Cuando una transicion no tiene sentido, el Core responde con conflicto `409`.

### 4.2 Motor SLA

El Core no solo guarda fechas.

Calcula y administra plazos regulatorios:

- Fecha de envio a prestadora.
- Fecha limite de respuesta.
- Dias habiles de SLA.
- Fecha de respuesta.
- Reclamaciones vencidas.
- Marcado de vencidas.

Esto permite saber cuando una prestadora incumple el plazo esperado.

### 4.3 Motor de auditoria

Cada accion sensible deja rastro institucional.

La auditoria guarda:

- Usuario.
- Correo.
- Rol.
- Entidad afectada.
- Accion.
- Estado anterior.
- Estado nuevo.
- Metodo HTTP.
- Ruta.
- IP.
- User-Agent.
- CorrelationId.
- Fecha.

Esto permite responder preguntas como:

```text
Quien cambio esta reclamacion?
Cuando lo hizo?
Desde donde?
Que estado tenia antes?
Que estado quedo despues?
Que endpoint ejecuto?
```

## 5. Otros puntos fuertes

### Seguridad

- JWT Bearer Authentication.
- Roles.
- Proteccion de ciudadano por dueno real.
- Bloqueo de documentos a usuarios ajenos.
- Bloqueo de notificaciones a usuarios ajenos.

### Documentos seguros

- Subida de documentos.
- Listado por reclamacion.
- Descarga protegida.
- Validacion de ruta fisica segura.
- Auditoria de subida, descarga y eliminacion.

### Reportes

- Resumen general.
- Reclamaciones por estado.
- Reclamaciones por prestadora.
- Reclamaciones por servicio.
- Reclamaciones por provincia.
- Reclamaciones por tipo.
- SLA.
- Productividad.

### Notificaciones

- Creacion de notificaciones internas.
- Marcado como enviada.
- Marcado como leida.
- Consulta paginada.
- Proteccion por dueno real.

## 6. Evidencia de pruebas

El Core fue probado mediante scripts funcionales end-to-end.

Pruebas ejecutadas:

- Login admin.
- Registro de ciudadano.
- Creacion de reclamacion.
- Transiciones validas.
- Transiciones invalidas.
- Respuesta de prestadora.
- Resolucion.
- Cierre.
- SLA.
- Auditoria.
- Documentos seguros.
- Bloqueo 403 a ciudadano ajeno.
- Busqueda paginada.
- Reportes.
- Notificaciones.
- Health checks.

Resultado final:

```text
PRUEBA FINAL TERMINADA CORRECTAMENTE
CORE INDOTEL VALIDADO AL 100% DEL ALCANCE ACADEMICO DEFINIDO
```

## 7. Comparacion con un CRUD simple

| CRUD simple | Core INDOTEL |
|---|---|
| Guarda datos | Aplica reglas regulatorias |
| Actualiza estados libremente | Valida maquina de estados |
| No mide plazos | Calcula SLA y vencimientos |
| No deja trazabilidad fuerte | Registra auditoria institucional |
| No valida dueno real | Bloquea recursos ajenos |
| No protege documentos | Descarga segura con control de acceso |
| Reportes basicos o ausentes | Reportes regulatorios |

## 8. Preguntas dificiles y respuestas recomendadas

### Pregunta: ¿Esto esta listo para produccion real?

Respuesta:

> No lo estamos declarando como produccion gubernamental real. Lo declaramos como prototipo institucional avanzado, validado funcionalmente dentro del alcance academico. Para produccion real faltan hardening de seguridad, pruebas automatizadas formales, observabilidad y almacenamiento documental externo o cifrado.

### Pregunta: ¿Por que dicen 100%?

Respuesta:

> Porque paso el 100% de las pruebas funcionales definidas para el alcance academico del Core. No significa cobertura absoluta de produccion, sino validacion completa del flujo principal y los casos criticos contemplados.

### Pregunta: ¿Por que no usaron Clean Architecture pura?

Respuesta:

> Priorizamos entregar un Core funcional probado. El estilo actual es monolitico modular con separacion por controladores, DTOs, modelos, servicios, datos y middleware. Una fase futura separaria Domain, Application, Infrastructure y API.

### Pregunta: ¿Que le falta?

Respuesta:

> Refresh token, logout real, bloqueo por intentos fallidos, rate limiting, pruebas automatizadas con xUnit, repositorios/servicios mas estrictos, almacenamiento documental externo o cifrado, logs estructurados y observabilidad avanzada.

### Pregunta: ¿Por que no es solo CRUD?

Respuesta:

> Porque implementa motores de negocio: maquina de estados, SLA regulatorio, auditoria institucional, resolucion/cierre estructurado, documentos seguros, proteccion por dueno real y reportes regulatorios.

## 9. Frase de cierre para exposicion

> Elegimos profundidad sobre amplitud. En lugar de muchas pantallas superficiales, construimos un Core regulatorio real, probado de punta a punta, con reglas de negocio, trazabilidad, documentos seguros y evidencia funcional.
