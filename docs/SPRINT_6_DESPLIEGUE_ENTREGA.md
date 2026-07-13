# Sprint 6 — Despliegue, documentación y entrega

## Estado

**Implementación preparada en la rama `integracion`.**

El cierre definitivo requiere ejecutar la validación con construcción de imágenes, smoke del release, backup/restauración y ejecución de Caja en Windows.

## Objetivo

Dejar el sistema reproducible en una computadora limpia, con HTTPS, configuración externa, persistencia, respaldos, rollback, manuales y guion de defensa.

## Entregables implementados

### Contenedores

- `deploy/Dockerfile.core`
- `deploy/Dockerfile.gateway`
- `deploy/Dockerfile.web`
- imágenes basadas en .NET 8;
- publicación Release multi-stage;
- ejecución con usuario no root `indotel`;
- health checks;
- diagnóstico .NET deshabilitado en runtime.

### Orquestación

`deploy/docker-compose.release.yml` contiene:

- SQL Server 2022;
- Core;
- API Gateway;
- Web;
- Caddy;
- redes separadas `frontend` y `backend`;
- backend interna;
- volúmenes persistentes;
- dependencias mediante health checks;
- ninguna publicación del puerto de SQL Server.

### HTTPS

`deploy/Caddyfile`:

- entrada única;
- HTTPS con CA interna para laboratorio;
- Web en rutas generales;
- Gateway en `/api/*` y `/health*`;
- compresión;
- encabezados de seguridad;
- logs JSON.

Para dominio público debe reemplazarse `tls internal` por la configuración de certificado institucional o TLS automático con DNS y puertos públicos adecuados.

### Configuración externa

Plantilla:

```text
deploy/.env.release.example
```

El archivo real:

```text
deploy/.env.release
```

está excluido de Git y debe tener permisos restrictivos.

Variables principales:

- versión del release;
- contraseña SQL;
- clave JWT;
- duración de tokens;
- administrador inicial;
- rate limit;
- dominio y puertos.

### Operación

- `scripts/desplegar_release.sh`
- `scripts/backup_sqlserver.sh`
- `scripts/restore_sqlserver.sh`
- `scripts/rollback_release.sh`
- `scripts/validar_sprint6_despliegue.sh`

### Documentación

- `docs/ARQUITECTURA_FINAL.md`
- `docs/MANUAL_TECNICO.md`
- `docs/MANUAL_USUARIO.md`
- `docs/GUIA_DEMO_DEFENSA.md`
- `docs/SPRINT_5_INTEGRACION_SEGURIDAD.md`
- `docs/SPRINT_6_DESPLIEGUE_ENTREGA.md`

## Preparación

```bash
cd /home/jarry/indotel-prueba-integracion
cp deploy/.env.release.example deploy/.env.release
chmod 600 deploy/.env.release
```

Reemplazar todos los valores `CAMBIAR_...`.

Generación recomendada:

```bash
python3 - <<'PY'
import secrets
print(secrets.token_urlsafe(64))
print(secrets.token_urlsafe(24) + 'Aa1!')
print(secrets.token_urlsafe(20) + 'Aa1!')
PY
```

No pegar las claves en Git, documentación, capturas o reportes.

## Validación estructural

```bash
bash scripts/validar_sprint6_despliegue.sh
```

Valida:

- entregables presentes;
- sintaxis Bash;
- usuario no root;
- Compose válido;
- plantilla utilizable;
- ausencia de secretos reales rastreados;
- manuales presentes.

## Validación completa

```bash
BUILD_IMAGES=1 RUN_RELEASE_SMOKE=1 \
bash scripts/validar_sprint6_despliegue.sh
```

Debe:

1. construir Core, Gateway y Web;
2. iniciar SQL Server;
3. aplicar migraciones;
4. iniciar la entrada HTTPS;
5. obtener HTTP 200 en `/health`;
6. detener el entorno de smoke sin borrar volúmenes.

## Despliegue

```bash
bash scripts/desplegar_release.sh deploy/.env.release
```

Resultado predeterminado:

```text
https://localhost:8443/          Portal Web
https://localhost:8443/health    Gateway
https://localhost:8443/api/...   API
```

Caja debe apuntar a:

```text
https://localhost:8443/
```

## Migraciones

El primer arranque aplica migraciones del Core. Antes de cada actualización:

1. ejecutar validaciones;
2. crear backup;
3. conservar imágenes anteriores;
4. desplegar;
5. probar health, login y reclamación;
6. desactivar seed después de crear el administrador inicial.

## Backup y restauración

Backup:

```bash
bash scripts/backup_sqlserver.sh deploy/.env.release
```

Restauración destructiva:

```bash
CONFIRM_RESTORE=YES \
bash scripts/restore_sqlserver.sh \
  backups/IndotelCoreDb-FECHA.bak \
  deploy/.env.release
```

La suma SHA-256 se verifica cuando está disponible.

## Rollback

```bash
CONFIRM_ROLLBACK=YES \
bash scripts/rollback_release.sh \
  TAG_ANTERIOR \
  backups/IndotelCoreDb-ANTES.bak \
  deploy/.env.release
```

Requisitos:

- imágenes anteriores todavía presentes;
- backup compatible;
- autorización explícita;
- validación posterior.

## Caja para Windows

El workflow de Caja debe producir:

```text
indotel-caja-win-x64
```

Procedimiento de entrega:

1. descargar artefacto;
2. descomprimir en Windows limpio;
3. configurar `ApiBaseUrl`;
4. instalar .NET Desktop Runtime 8 cuando aplique;
5. confiar en el certificado del Gateway;
6. ejecutar login por rol;
7. probar flujo interno;
8. conservar evidencia.

## Evidencia final requerida

- 20 pruebas Core.
- 14 pruebas Gateway.
- 12 pruebas Web.
- 15 pruebas Caja.
- Workflow Windows de Caja.
- Runtime integrado Sprint 5.
- Build de tres imágenes.
- Smoke HTTPS.
- Backup y checksum.
- Restauración probada.
- Rollback probado o ensayado en entorno separado.
- Captura de arquitectura.
- Flujo ciudadano.
- Flujo interno.
- Auditor solo lectura.
- Core apagado → 503.
- SQL apagado → readiness 503.

## Criterio de cierre

El Sprint 6 queda cerrado cuando:

- una copia limpia puede seguir el manual técnico;
- el Compose se despliega sin editar código;
- HTTPS funciona;
- los secretos no están en Git;
- Caja se ejecuta en Windows;
- existe backup restaurable;
- rollback está documentado y probado;
- los manuales coinciden con el sistema;
- la demostración completa puede ejecutarse sin improvisación.

## Limitación transparente

Este despliegue está diseñado para laboratorio y defensa académica. Antes de una producción institucional real se requieren certificados públicos o institucionales, ambiente ASP.NET `Production` con soporte formal de proxy inverso, monitoreo central, gestión empresarial de secretos, pruebas de penetración, alta disponibilidad, políticas de respaldo institucionales y aprobación de gobierno de datos.
