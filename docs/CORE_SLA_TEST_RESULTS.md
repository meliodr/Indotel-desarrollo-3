# Resultados de prueba SLA regulatorio - Core INDOTEL

Fecha de prueba: 09/07/2026
Rama probada: `core`
Entorno: Desarrollo local
API: `http://localhost:5085`

## Objetivo

Validar la Fase 6 del Core INDOTEL: control de SLA regulatorio para reclamaciones enviadas a prestadoras.

## Datos creados durante la prueba

```text
CIUDADANO_ID=6
RECLAMACION_ID=11
EXPEDIENTE=IND-20260709074234456-450
```

## Resultado de cálculo SLA

Al cambiar la reclamación de `VALIDADA` a `ENVIADA_A_PRESTADORA`, el Core calculó correctamente los campos SLA:

```text
fechaEnvioPrestadora=2026-07-09T07:42:34.5979537Z
fechaLimiteRespuesta=2026-07-23T23:59:59Z
diasHabilesSla=10
estaVencida=False
```

## Resultado de respuesta de prestadora

Al registrar respuesta de prestadora, el Core actualizó correctamente:

```text
estado=RESPONDIDA_POR_PRESTADORA
fechaRespuestaPrestadora=2026-07-09T07:42:34.6910741
estaVencida=False
```

## Resultados

| Prueba | Resultado esperado | Resultado real |
|---|---:|---:|
| Login admin | Token válido | OK |
| Registrar ciudadano | 200 | OK |
| Crear reclamación para SLA | 201 | OK |
| Cambiar `RECIBIDA -> VALIDADA` | 200 | OK |
| Cambiar `VALIDADA -> ENVIADA_A_PRESTADORA` | 200 | OK |
| Calcular fecha de envío a prestadora | No vacío | OK |
| Calcular fecha límite de respuesta | No vacío | OK |
| Asignar días hábiles SLA | 10 | OK |
| Marcar caso como no vencido al enviarse | false | OK |
| Consultar reclamación y verificar SLA persistido | 200 | OK |
| Registrar respuesta de prestadora | 200 | OK |
| Cambiar estado a `RESPONDIDA_POR_PRESTADORA` | Estado correcto | OK |
| Registrar fecha de respuesta de prestadora | No vacío | OK |
| Mantener `EstaVencida=false` al responder | false | OK |
| Consultar reclamaciones vencidas | 200 | OK |
| Marcar vencidas por SLA | 200 | OK |
| Cantidad marcada como vencida en esta prueba | 0 | OK |

## Endpoints validados

```text
POST /api/reclamaciones
PATCH /api/reclamaciones/{id}/estado
POST /api/reclamaciones/{id}/respuesta-prestadora
GET /api/reclamaciones/{id}
GET /api/reclamaciones/sla/vencidas
POST /api/reclamaciones/sla/marcar-vencidas
```

## Resultado general

```text
FASE 6 - SLA REGULATORIO FUNCIONANDO
```

## Alcance implementado

El Core ya permite:

```text
Registrar fecha de envío a prestadora.
Calcular fecha límite de respuesta con días hábiles.
Guardar cantidad de días hábiles de SLA.
Detectar reclamaciones vencidas.
Marcar reclamaciones vencidas por SLA.
Registrar fecha de respuesta de prestadora.
Evitar que un caso respondido quede marcado como vencido.
Consultar reclamaciones vencidas.
```

## Nota técnica

Esta fase requiere migración porque agrega campos SLA a `Reclamacion`:

```text
FechaEnvioPrestadora
FechaLimiteRespuesta
FechaRespuestaPrestadora
DiasHabilesSla
EstaVencida
FechaMarcadaVencida
```

La migración debe subirse al repositorio junto al snapshot actualizado.
