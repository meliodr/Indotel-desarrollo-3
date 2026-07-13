#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

fail() {
  printf '\nERROR: %s\n' "$1" >&2
  exit 1
}

REQUIRED_FILES=(
  deploy/Dockerfile.core
  deploy/Dockerfile.gateway
  deploy/Dockerfile.web
  deploy/docker-compose.release.yml
  deploy/Caddyfile
  deploy/.env.release.example
  scripts/desplegar_release.sh
  scripts/backup_sqlserver.sh
  scripts/restore_sqlserver.sh
  scripts/rollback_release.sh
  docs/SPRINT_6_DESPLIEGUE_ENTREGA.md
  docs/MANUAL_TECNICO.md
  docs/MANUAL_USUARIO.md
  docs/ARQUITECTURA_FINAL.md
  docs/GUIA_DEMO_DEFENSA.md
)

printf '\n==> Verificando entregables\n'
for file in "${REQUIRED_FILES[@]}"; do
  [[ -f "$file" ]] || fail "Falta $file"
  printf 'OK  %s\n' "$file"
done

printf '\n==> Validando sintaxis Bash\n'
for script in scripts/desplegar_release.sh \
              scripts/backup_sqlserver.sh \
              scripts/restore_sqlserver.sh \
              scripts/rollback_release.sh \
              scripts/validar_sprint5_integracion.sh \
              scripts/probar_sprint5_runtime.sh; do
  bash -n "$script" || fail "Sintaxis invalida en $script"
done

printf '\n==> Validando usuarios no root en imagenes\n'
for dockerfile in deploy/Dockerfile.core deploy/Dockerfile.gateway deploy/Dockerfile.web; do
  grep -q '^USER indotel$' "$dockerfile" || fail "$dockerfile no declara USER indotel."
done

printf '\n==> Validando plantilla de variables\n'
TMP_ENV="$(mktemp)"
RELEASE_ENV=""

cleanup() {
  if [[ -n "$RELEASE_ENV" && -f "$RELEASE_ENV" ]]; then
    docker compose --env-file "$RELEASE_ENV" -f deploy/docker-compose.release.yml down --remove-orphans >/dev/null 2>&1 || true
    rm -f "$RELEASE_ENV"
  fi
  rm -f "$TMP_ENV"
}
trap cleanup EXIT

cp deploy/.env.release.example "$TMP_ENV"
sed -i \
  -e 's/CAMBIAR_CLAVE_SQL_DE_16_O_MAS/LocalSql-Validacion-2026!/' \
  -e 's/CAMBIAR_CLAVE_JWT_ALEATORIA_DE_64_O_MAS/ValidacionJwtLocal_0123456789_ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz/' \
  -e 's/CAMBIAR_CLAVE_ADMIN_SEGURA/Admin-Validacion-2026!/' \
  "$TMP_ENV"

# Los puertos del smoke pueden sobrescribirse para no interrumpir otros servicios
# locales. Ejemplo: SMOKE_HTTP_PORT=18080 SMOKE_HTTPS_PORT=18443.
if [[ -n "${SMOKE_HTTP_PORT:-}" ]]; then
  sed -i "s/^HTTP_PORT=.*/HTTP_PORT=${SMOKE_HTTP_PORT}/" "$TMP_ENV"
fi
if [[ -n "${SMOKE_HTTPS_PORT:-}" ]]; then
  sed -i "s/^HTTPS_PORT=.*/HTTPS_PORT=${SMOKE_HTTPS_PORT}/" "$TMP_ENV"
fi

command -v docker >/dev/null || fail "Docker es obligatorio para validar Compose."
docker compose version >/dev/null 2>&1 || fail "Docker Compose no esta disponible."
docker compose --env-file "$TMP_ENV" -f deploy/docker-compose.release.yml config --quiet

printf '\n==> Revisando secretos reales rastreados\n'
TRACKED_LOCAL="$(git ls-files | grep -E '(^|/)(\.env\.release|docker\.env|appsettings\.Development\.json)$|\.(pfx|p12|key)$' || true)"
[[ -z "$TRACKED_LOCAL" ]] || {
  printf '%s\n' "$TRACKED_LOCAL" >&2
  fail "Hay secretos o configuracion local rastreados."
}

if [[ "${BUILD_IMAGES:-0}" == "1" ]]; then
  printf '\n==> Construyendo imagenes de comprobacion\n'
  docker compose --env-file "$TMP_ENV" -f deploy/docker-compose.release.yml build core gateway web
else
  printf '\nConstruccion de imagenes omitida. Use BUILD_IMAGES=1 para incluirla.\n'
fi

if [[ "${RUN_RELEASE_SMOKE:-0}" == "1" ]]; then
  printf '\n==> Despliegue smoke de comprobacion\n'
  RELEASE_ENV="$(mktemp)"
  cp "$TMP_ENV" "$RELEASE_ENV"

  # Limpiar una ejecución anterior incompleta del mismo proyecto Compose.
  docker compose --env-file "$RELEASE_ENV" -f deploy/docker-compose.release.yml down --remove-orphans >/dev/null 2>&1 || true

  SKIP_BACKUP=1 bash scripts/desplegar_release.sh "$RELEASE_ENV"
fi

cat <<'EOF'

Sprint 6 validado a nivel de estructura:
- archivos de despliegue presentes;
- scripts con sintaxis correcta;
- imagenes configuradas con usuario no root;
- Compose valido;
- plantilla sin secretos reales;
- manuales y guion de defensa presentes.

Para cierre completo ejecute:
  BUILD_IMAGES=1 RUN_RELEASE_SMOKE=1 bash scripts/validar_sprint6_despliegue.sh

Si los puertos 8080 o 8443 estan ocupados, use por ejemplo:
  SMOKE_HTTP_PORT=18080 SMOKE_HTTPS_PORT=18443 BUILD_IMAGES=1 RUN_RELEASE_SMOKE=1 bash scripts/validar_sprint6_despliegue.sh
EOF
