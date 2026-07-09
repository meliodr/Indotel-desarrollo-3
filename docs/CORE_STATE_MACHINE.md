# Maquina de estados de reclamaciones - Core INDOTEL

Este documento define el flujo permitido para una reclamacion dentro del Core INDOTEL.

No es codigo todavia. Es una decision funcional y tecnica para que el Core no permita movimientos incorrectos.

## 1. Objetivo

La maquina de estados evita que una reclamacion se mueva de forma desordenada.

Ejemplo de problema que queremos evitar:

```text
RECIBIDA -> CERRADA
```

Ese salto no debe permitirse porque primero debe validarse, enviarse a prestadora, recibir respuesta, evaluarse y resolverse.

## 2. Principio general

```text
Una reclamacion solo puede pasar a un estado si el estado actual permite esa transicion.
```

El Core debe validar esto, no la Web ni la Caja.

Web y Caja pueden mostrar botones, pero el Core es quien decide si la accion es valida.

## 3. Estados oficiales

| Estado | Descripcion | Tipo |
|---|---|---|
| RECIBIDA | La reclamacion fue registrada, pero aun no ha sido revisada por INDOTEL. | Inicial |
| VALIDADA | INDOTEL reviso la reclamacion y confirma que cumple requisitos minimos. | Operativo |
| OBSERVADA | La reclamacion necesita correccion, informacion adicional o documentos. | Operativo |
| ENVIADA_A_PRESTADORA | La reclamacion fue enviada a la prestadora para respuesta. | Operativo |
| RESPONDIDA_POR_PRESTADORA | La prestadora registro una respuesta. | Operativo |
| EN_REVISION_INDOTEL | INDOTEL evalua la respuesta y el expediente. | Operativo |
| RESUELTA | INDOTEL emitio una decision o resultado del caso. | Final previo |
| CERRADA | El expediente fue cerrado formalmente. | Final |
| RECHAZADA | La reclamacion no procede o no cumple condiciones. | Final |
| ARCHIVADA | El expediente se archiva por decision administrativa o inactividad. | Final |
| VENCIDA | La prestadora no respondio dentro del plazo definido. | Especial/futuro |

## 4. Estados actuales del Core

El Core actual ya trabaja con varios estados:

```text
RECIBIDA
VALIDADA
ENVIADA_A_PRESTADORA
RESPONDIDA_POR_PRESTADORA
EN_REVISION
RESUELTA
CERRADA
RECHAZADA
```

## 5. Decision de normalizacion

Para evitar confusion, se adopta esta decision:

| Estado anterior | Estado normalizado | Decision |
|---|---|---|
| EN_REVISION | EN_REVISION_INDOTEL | Usar nombre mas claro en documentacion futura |
| EN_REVISION_INDOTEL | EN_REVISION_INDOTEL | Estado recomendado |

En codigo se puede mantener temporalmente `EN_REVISION` para no romper pruebas actuales, pero la siguiente mejora deberia aceptar o migrar a `EN_REVISION_INDOTEL`.

## 6. Flujo principal ideal

```text
RECIBIDA
   ↓
VALIDADA
   ↓
ENVIADA_A_PRESTADORA
   ↓
RESPONDIDA_POR_PRESTADORA
   ↓
EN_REVISION_INDOTEL
   ↓
RESUELTA
   ↓
CERRADA
```

## 7. Flujo con observacion

```text
RECIBIDA
   ↓
OBSERVADA
   ↓
RECIBIDA
   ↓
VALIDADA
```

Uso:

- Falta documento.
- Falta informacion.
- El ciudadano debe corregir datos.

## 8. Flujo con rechazo

```text
RECIBIDA -> RECHAZADA
VALIDADA -> RECHAZADA
OBSERVADA -> RECHAZADA
```

Uso:

- No corresponde al INDOTEL.
- Datos falsos o insuficientes.
- El caso no cumple condiciones minimas.

## 9. Flujo con archivo

```text
OBSERVADA -> ARCHIVADA
RESUELTA -> ARCHIVADA
CERRADA -> ARCHIVADA
```

Uso:

- El ciudadano no completo informacion.
- El expediente debe conservarse, pero no seguir activo.
- Decision administrativa.

## 10. Flujo con vencimiento futuro

```text
ENVIADA_A_PRESTADORA -> VENCIDA
VENCIDA -> EN_REVISION_INDOTEL
VENCIDA -> RESUELTA
```

Uso:

- La prestadora no respondio en el plazo definido.
- El Core o una tarea automatica marca el vencimiento.

Este flujo no es obligatorio para el MVP actual, pero se documenta para no olvidarlo.

## 11. Transiciones permitidas

| Estado actual | Puede pasar a | Quien puede hacerlo | Motivo |
|---|---|---|---|
| RECIBIDA | VALIDADA | Administrador, AnalistaDAU | Cumple requisitos |
| RECIBIDA | OBSERVADA | Administrador, AnalistaDAU | Falta informacion |
| RECIBIDA | RECHAZADA | Administrador, AnalistaDAU | No procede |
| OBSERVADA | RECIBIDA | Administrador, AnalistaDAU, Ciudadano propio via correccion | Se completo informacion |
| OBSERVADA | RECHAZADA | Administrador, AnalistaDAU | No se corrigio o no procede |
| OBSERVADA | ARCHIVADA | Administrador, AnalistaDAU, SupervisorDAU | Inactividad o cierre administrativo |
| VALIDADA | ENVIADA_A_PRESTADORA | Administrador, AnalistaDAU | Se envia a empresa |
| VALIDADA | RECHAZADA | Administrador, AnalistaDAU | Se detecta improcedencia |
| ENVIADA_A_PRESTADORA | RESPONDIDA_POR_PRESTADORA | RepresentantePrestadora, Administrador | Prestadora responde |
| ENVIADA_A_PRESTADORA | VENCIDA | Sistema, Administrador | Vencio plazo |
| RESPONDIDA_POR_PRESTADORA | EN_REVISION_INDOTEL | Administrador, AnalistaDAU | INDOTEL revisa respuesta |
| EN_REVISION_INDOTEL | RESUELTA | Administrador, AnalistaDAU, SupervisorDAU | Se emite decision |
| EN_REVISION_INDOTEL | ENVIADA_A_PRESTADORA | Administrador, AnalistaDAU | Se pide aclaracion a prestadora |
| EN_REVISION_INDOTEL | OBSERVADA | Administrador, AnalistaDAU | Se pide informacion al ciudadano |
| VENCIDA | EN_REVISION_INDOTEL | Administrador, AnalistaDAU, Sistema | Se revisa por vencimiento |
| VENCIDA | RESUELTA | Administrador, SupervisorDAU | Se resuelve por silencio o decision |
| RESUELTA | CERRADA | Administrador, SupervisorDAU | Cierre formal |
| RESUELTA | ARCHIVADA | Administrador, SupervisorDAU | Archivo posterior |
| CERRADA | ARCHIVADA | Administrador, SupervisorDAU | Archivo administrativo |

## 12. Transiciones prohibidas

Estas transiciones deben devolver error.

| Transicion | Motivo |
|---|---|
| RECIBIDA -> CERRADA | No se puede cerrar sin validar y resolver |
| RECIBIDA -> RESUELTA | No se puede resolver sin proceso |
| RECIBIDA -> RESPONDIDA_POR_PRESTADORA | No se ha enviado a prestadora |
| VALIDADA -> RESPONDIDA_POR_PRESTADORA | Falta enviar a prestadora |
| ENVIADA_A_PRESTADORA -> CERRADA | Falta respuesta/evaluacion/resolucion |
| RESPONDIDA_POR_PRESTADORA -> CERRADA | Falta revision INDOTEL y resolucion |
| CERRADA -> VALIDADA | Un caso cerrado no debe reabrirse en MVP |
| CERRADA -> ENVIADA_A_PRESTADORA | Un caso cerrado no debe operar |
| RECHAZADA -> VALIDADA | Reapertura no permitida en MVP |
| ARCHIVADA -> VALIDADA | Archivo no se reabre en MVP |

## 13. Estados finales

Estados considerados finales:

```text
CERRADA
RECHAZADA
ARCHIVADA
```

Regla:

```text
Un estado final no debe modificarse en el MVP.
```

Excepcion futura:

Un SupervisorDAU o Administrador podria reabrir un expediente por decision especial, pero eso debe documentarse e implementarse luego.

## 14. Acciones de negocio y estado resultante

| Accion | Ruta posible | Estado resultante |
|---|---|---|
| Validar reclamacion | PUT/PATCH /api/reclamaciones/{id}/estado | VALIDADA |
| Observar reclamacion | PUT/PATCH /api/reclamaciones/{id}/estado | OBSERVADA |
| Enviar a prestadora | POST /api/reclamaciones/{id}/enviar-prestadora | ENVIADA_A_PRESTADORA |
| Responder prestadora | POST /api/reclamaciones/{id}/respuesta-prestadora | RESPONDIDA_POR_PRESTADORA |
| Revisar en INDOTEL | PUT/PATCH /api/reclamaciones/{id}/estado | EN_REVISION_INDOTEL |
| Resolver | POST /api/reclamaciones/{id}/resolver | RESUELTA |
| Cerrar | POST /api/reclamaciones/{id}/cerrar | CERRADA |
| Rechazar | POST /api/reclamaciones/{id}/rechazar | RECHAZADA |
| Archivar | POST /api/reclamaciones/{id}/archivar | ARCHIVADA |

Para la siguiente etapa, los endpoints especificos pueden esperar. Primero se debe controlar bien `PUT/PATCH /estado`.

## 15. Respuesta de error recomendada

Si una transicion no esta permitida, el Core debe devolver:

```http
409 Conflict
```

Ejemplo JSON:

```json
{
  "mensaje": "No se permite cambiar la reclamacion de RECIBIDA a CERRADA.",
  "estadoActual": "RECIBIDA",
  "estadoSolicitado": "CERRADA",
  "codigo": "TRANSICION_ESTADO_INVALIDA"
}
```

## 16. Historial obligatorio

Cada cambio de estado debe registrar historial.

Datos minimos:

- ReclamacionId.
- EstadoAnterior.
- EstadoNuevo.
- Comentario.
- UsuarioId.
- FechaCambio.

## 17. Auditoria recomendada

Ademas del historial funcional, debe registrarse auditoria tecnica/institucional.

Datos recomendados:

- UsuarioId.
- IP.
- Entidad: Reclamacion.
- EntidadId.
- Accion: CambioEstado.
- Detalle.
- Fecha.

## 18. Reglas para documentos

No debe ser obligatorio subir documento para crear una reclamacion en el MVP, pero si el estado pasa a `VALIDADA`, el analista debe poder verificar si hay evidencia.

Futuro:

- Si el tipo de reclamacion requiere evidencia, no permitir VALIDADA sin documento.
- Si falta documento, usar OBSERVADA.

## 19. Reglas para prestadora

- La prestadora solo puede responder reclamaciones en estado `ENVIADA_A_PRESTADORA`.
- La prestadora no puede cambiar directamente a `RESUELTA`, `CERRADA` o `RECHAZADA`.
- La prestadora solo puede ver casos de su propia empresa.

## 20. Reglas para ciudadano

- El ciudadano puede crear reclamaciones.
- El ciudadano puede consultar sus propias reclamaciones.
- El ciudadano puede adjuntar documentos a sus reclamaciones.
- El ciudadano no puede cambiar estados internos.
- Si una reclamacion esta OBSERVADA, el ciudadano puede completar informacion o adjuntar documentos.

## 21. Reglas para Caja

Caja debe mostrar botones segun estado y rol.

Ejemplos:

| Estado | Botones sugeridos para AnalistaDAU |
|---|---|
| RECIBIDA | Validar, Observar, Rechazar |
| OBSERVADA | Validar si fue corregida, Rechazar, Archivar |
| VALIDADA | Enviar a prestadora, Rechazar |
| ENVIADA_A_PRESTADORA | Ver estado, Marcar vencida si aplica |
| RESPONDIDA_POR_PRESTADORA | Pasar a revision INDOTEL |
| EN_REVISION_INDOTEL | Resolver, Observar, Pedir aclaracion |
| RESUELTA | Cerrar |
| CERRADA | Solo lectura |
| RECHAZADA | Solo lectura |
| ARCHIVADA | Solo lectura |

## 22. Pruebas manuales en Swagger

### Pruebas permitidas

1. Crear reclamacion: debe quedar `RECIBIDA`.
2. Cambiar `RECIBIDA` a `VALIDADA`: debe dar 200.
3. Cambiar `VALIDADA` a `ENVIADA_A_PRESTADORA`: debe dar 200.
4. Registrar respuesta de prestadora: debe cambiar a `RESPONDIDA_POR_PRESTADORA`.
5. Cambiar `RESPONDIDA_POR_PRESTADORA` a `EN_REVISION_INDOTEL`: debe dar 200.
6. Cambiar `EN_REVISION_INDOTEL` a `RESUELTA`: debe dar 200.
7. Cambiar `RESUELTA` a `CERRADA`: debe dar 200.

### Pruebas rechazadas

1. Crear reclamacion y cambiar `RECIBIDA` a `CERRADA`: debe dar 409.
2. Cambiar `VALIDADA` a `CERRADA`: debe dar 409.
3. Cambiar `RESPONDIDA_POR_PRESTADORA` a `CERRADA`: debe dar 409.
4. Cambiar `CERRADA` a `VALIDADA`: debe dar 409.

## 23. Impacto en codigo futuro

Para implementar esta maquina de estados se recomienda crear:

```text
Services/ReclamacionEstadoService.cs
```

Responsabilidades:

- Validar si un estado existe.
- Validar si una transicion esta permitida.
- Devolver mensaje claro si no esta permitida.
- Centralizar reglas de estado.

Tambien se recomienda crear constantes:

```text
Constants/ReclamacionEstados.cs
```

Para evitar escribir strings repetidos en controladores.

## 24. Decision final

Para la siguiente etapa del Core:

1. Se mantiene `RECIBIDA` como estado inicial.
2. Se adopta el flujo principal documentado.
3. Se deben impedir saltos de estado incorrectos.
4. Se debe devolver `409 Conflict` cuando la transicion no sea permitida.
5. Todo cambio de estado debe crear historial.
6. Auditoria automatica queda como siguiente mejora despues de la maquina de estados.
