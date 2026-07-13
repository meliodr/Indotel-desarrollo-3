# Sprint 2 — API y Gateway

## Estado

**Implementación realizada en la rama `api-gateway`.**

La validación definitiva requiere ejecutar restore, build, pruebas automáticas y una prueba integrada contra el Core.

## Objetivo

Crear una entrada única, segura y resiliente para Web y Caja, evitando que los clientes consuman directamente los puertos internos del Core.

## Dirección local

- Gateway HTTP: `http://localhost:5185`
- Gateway HTTPS: `https://localhost:7185`
- Core esperado por defecto: `http://localhost:5085`

La dirección del Core se configura mediante:

```text
Gateway__CoreBaseUrl
```

## Correcciones implementadas

### Entrada única

- Todas las rutas `/api/**` se reenvían al Core.
- Se propagan `Authorization`, encabezados de contenido y parámetros de consulta.
- Se eliminan encabezados hop-by-hop.
- Se agregan `X-Forwarded-For`, `X-Forwarded-Proto` y `X-Forwarded-Host`.
- Se propaga o genera `X-Correlation-ID`.

### Disponibilidad

- `/health` y `/health/live` comprueban que el Gateway está vivo.
- `/health/ready` consulta la readiness del Core.
- Core apagado, conexión rechazada o timeout producen HTTP 503 estructurado.
- El Gateway no simula una respuesta correcta cuando el Core está caído.

### Resiliencia

- Timeout de conexión y timeout total configurables.
- Circuit breaker por fallos consecutivos.
- Respuesta `Retry-After` cuando el circuito está abierto.
- Solo GET, HEAD y OPTIONS sin cuerpo pueden reintentarse.
- POST, PUT, PATCH, DELETE y cargas multipart no se reintentan automáticamente.
- Un 500 funcional del Core se devuelve al cliente y no abre el circuito.

### Seguridad

- CORS explícito.
- Límite de tamaño de solicitud configurable.
- Rate limiting por IP y grupo de ruta.
- Encabezados de seguridad.
- HTTPS y HSTS fuera de Development.
- No se registran tokens ni cuerpos de documentos.
- El Core mantiene la autorización definitiva y las reglas de negocio.

### Errores

Los errores propios del Gateway usan `application/problem+json` y contienen:

- `mensaje`
- `codigo`
- `correlationId`
- `status`
- `fecha`

Códigos principales:

- `CORE_NO_DISPONIBLE`
- `CORE_TIMEOUT`
- `CIRCUITO_CORE_ABIERTO`
- `LIMITE_SOLICITUDES_EXCEDIDO`
- `RUTA_GATEWAY_NO_ENCONTRADA`

### Observabilidad

`GET /health/status` expone:

- estado del circuito;
- fallos consecutivos;
- solicitudes totales;
- solicitudes correctas y fallidas;
- reintentos;
- rechazos por circuito abierto.

No expone secretos, JWT ni cadenas de conexión.

## Pruebas automáticas

El proyecto de pruebas cubre:

- métodos que pueden reintentarse;
- prohibición de reintentos para escrituras;
- prohibición de reintentos cuando una consulta tiene cuerpo;
- apertura del circuito;
- reinicio de fallos después de una respuesta correcta;
- estructura de ProblemDetails;
- conservación y regeneración del correlationId.

## Validación local

```bash
cd /home/jarry/indotel-prueba-api-gateway
git fetch origin
git reset --hard origin/api-gateway
bash scripts/validar_sprint2_gateway.sh
```

## Prueba integrada mínima

1. Iniciar SQL Server.
2. Iniciar Core en `http://localhost:5085`.
3. Iniciar Gateway en `http://localhost:5185`.
4. Comprobar:

```bash
curl -i http://localhost:5185/health
curl -i http://localhost:5185/health/ready
curl -i http://localhost:5185/api/health
```

5. Apagar Core.
6. Comprobar que:

```bash
curl -i http://localhost:5185/health/ready
curl -i http://localhost:5185/api/catalogos/prestadoras
```

responden `503` con `codigo` y `correlationId`.

## Criterio de cierre

- Compilación Release aprobada.
- Pruebas automáticas aprobadas.
- Publicación aprobada.
- Core apagado produce 503 controlado.
- GET puede reintentarse de forma limitada.
- Operaciones de escritura nunca se reintentan automáticamente.
- Web y Caja apuntan al Gateway en los sprints correspondientes.
