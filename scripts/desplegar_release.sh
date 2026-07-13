#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

ENV_FILE="${1:-deploy/.env.release}"
COMPOSE_FILE="deploy/docker-compose.release.yml"

fail() {
  printf '\nERROR: %s\n' "$1" >&2
  exit 1
}

for command in docker curl grep; do
  command -v "$command" >/dev/null || fail "Falta el comando requerido: $command"
done
docker compose version >/dev/null 2>&1 || fail "Docker Compose no esta disponible."

[[ -f "$ENV_FILE" ]] || fail "No existe $ENV_FILE. Copie deploy/.env.release.example y complete los valores."

# Revisar solamente asignaciones reales. Los comentarios de la plantilla pueden
# mencionar CAMBIAR_ como instruccion y no deben bloquear el despliegue.
if grep -Eq '^[[:space:]]*[A-Za-z_][A-Za-z0-9_]*=[[:space:]]*CAMBIAR_' "$ENV_FILE"; then
  fail "El archivo $ENV_FILE conserva valores CAMBIAR_."
fi

set -a
# shellcheck disable=SC1090
source "$ENV_FILE"
set +a

[[ ${#MSSQL_SA_PASSWORD} -ge 16 ]] || fail "MSSQL_SA_PASSWORD debe tener al menos 16 caracteres."
[[ ${#JWT_KEY} -ge 48 ]] || fail "JWT_KEY debe tener al menos 48 caracteres aleatorios."
[[ ${#ADMIN_PASSWORD} -ge 12 ]] || fail "ADMIN_PASSWORD debe tener al menos 12 caracteres."
[[ -n "${RELEASE_TAG:-}" ]] || fail "RELEASE_TAG es obligatorio."

printf '\n==> Validando configuracion Compose\n'
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" config --quiet

if docker inspect indotel-release-sqlserver >/dev/null 2>&1 && [[ "${SKIP_BACKUP:-0}" != "1" ]]; then
  printf '\n==> Respaldo preventivo\n'
  bash scripts/backup_sqlserver.sh "$ENV_FILE"
fi

printf '\n==> Construyendo imagenes de la version %s\n' "$RELEASE_TAG"
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" build --pull core gateway web

printf '\n==> Iniciando servicios\n'
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d --remove-orphans

PUBLIC_HEALTH="https://${INDOTEL_DOMAIN}:${HTTPS_PORT}/health"
PUBLIC_WEB="https://${INDOTEL_DOMAIN}:${HTTPS_PORT}/"

printf '\n==> Esperando disponibilidad\n'
for ((i=1; i<=120; i++)); do
  STATUS="$(curl -k -sS --max-time 5 -o /dev/null -w '%{http_code}' "$PUBLIC_HEALTH" 2>/dev/null || true)"
  if [[ "$STATUS" == "200" ]]; then
    break
  fi
  if [[ "$i" == "120" ]]; then
    docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" ps
    docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" logs --tail=100 core gateway web caddy
    fail "El despliegue no alcanzo HTTP 200 en $PUBLIC_HEALTH."
  fi
  sleep 2
done

mkdir -p artifacts
cat > "artifacts/release-${RELEASE_TAG}.txt" <<EOF
release_tag=$RELEASE_TAG
fecha_utc=$(date -u +%Y-%m-%dT%H:%M:%SZ)
web=$PUBLIC_WEB
gateway_health=$PUBLIC_HEALTH
commit=$(git rev-parse HEAD)
EOF

cat <<EOF

Despliegue completado.
Web: $PUBLIC_WEB
Gateway/health: $PUBLIC_HEALTH

Caja debe configurar ApiBaseUrl con:
https://${INDOTEL_DOMAIN}:${HTTPS_PORT}/

Para localhost, el certificado de Caddy es interno. El navegador o Windows deben
confiar en la CA de Caddy antes de una demostracion sin advertencias.
EOF
