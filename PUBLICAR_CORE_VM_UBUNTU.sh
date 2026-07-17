#!/usr/bin/env bash
set -Eeuo pipefail

CORE_PORT="${CORE_HOST_PORT:-5085}"
PROJECT_DIR="${1:-}"

log() { printf '\n==> %s\n' "$*"; }
fail() { printf '\nERROR: %s\n' "$*" >&2; exit 1; }

find_project() {
  local candidates=(
    "$HOME/Descargas/INDOTEL-VM-Automatizada/INDOTEL-Proyecto-Completo"
    "$HOME/Descargas/INDOTEL-Etapa-6-Final/INDOTEL-Proyecto-Completo"
    "$HOME/Descargas/INDOTEL_PROYECTO_COMPLETO_VM_WHATSAPP_FINAL/INDOTEL-Proyecto-Completo"
    "$HOME/Downloads/INDOTEL-VM-Automatizada/INDOTEL-Proyecto-Completo"
    "$HOME/Downloads/INDOTEL-Etapa-6-Final/INDOTEL-Proyecto-Completo"
  )

  local candidate
  for candidate in "${candidates[@]}"; do
    if [[ -f "$candidate/docker-compose.yml" && -f "$candidate/docker.env" ]]; then
      printf '%s\n' "$candidate"
      return 0
    fi
  done

  find "$HOME/Descargas" "$HOME/Downloads" -maxdepth 5 \
    -type f -name docker-compose.yml -print 2>/dev/null \
    | while IFS= read -r compose_file; do
        candidate="$(dirname "$compose_file")"
        if [[ -f "$candidate/docker.env" && -f "$candidate/INDOTEL.sln" ]]; then
          printf '%s\n' "$candidate"
          break
        fi
      done
}

command -v docker >/dev/null 2>&1 || fail "Docker no está instalado."
docker info >/dev/null 2>&1 || fail "Docker no está iniciado o el usuario no tiene permiso."
docker compose version >/dev/null 2>&1 || fail "Docker Compose no está disponible."
command -v curl >/dev/null 2>&1 || fail "curl no está instalado."

if [[ -z "$PROJECT_DIR" ]]; then
  PROJECT_DIR="$(find_project || true)"
fi
[[ -n "$PROJECT_DIR" ]] || fail "No se encontró la carpeta del proyecto. Pase la ruta como primer argumento."
[[ -f "$PROJECT_DIR/docker-compose.yml" ]] || fail "No existe docker-compose.yml en: $PROJECT_DIR"
[[ -f "$PROJECT_DIR/docker.env" ]] || fail "No existe docker.env en: $PROJECT_DIR"

cd "$PROJECT_DIR"
log "Proyecto detectado"
printf '%s\n' "$PWD"

IMAGE_TAG="$(awk -F= '$1=="LOCAL_IMAGE_TAG" {gsub(/\r/,"",$2); print $2; exit}' docker.env)"
IMAGE_TAG="${IMAGE_TAG:-etapa6}"
CORE_IMAGE="indotel/core:${IMAGE_TAG}"

docker image inspect "$CORE_IMAGE" >/dev/null 2>&1 \
  || fail "No existe la imagen local $CORE_IMAGE. No se intentará descargar ni recompilar."

docker inspect indotel-sqlserver-completo >/dev/null 2>&1 \
  || fail "No existe el contenedor indotel-sqlserver-completo. Inicie primero la Etapa 6."

SQL_RUNNING="$(docker inspect -f '{{.State.Running}}' indotel-sqlserver-completo 2>/dev/null || true)"
[[ "$SQL_RUNNING" == "true" ]] || fail "SQL Server no está ejecutándose."

cat > docker-compose.vm.override.yml <<YAML
services:
  core:
    ports:
      - "${CORE_PORT}:8080"
YAML

log "Publicando Core API en el puerto ${CORE_PORT}, sin descargar ni recompilar"
docker compose \
  --env-file docker.env \
  -f docker-compose.yml \
  -f docker-compose.vm.override.yml \
  up -d \
  --no-build \
  --pull never \
  --no-deps \
  --force-recreate \
  core

log "Esperando Core API"
ready=0
for attempt in $(seq 1 60); do
  if curl -fsS --max-time 4 "http://127.0.0.1:${CORE_PORT}/health/ready" >/dev/null 2>&1; then
    ready=1
    break
  fi
  printf 'Esperando Core... %s/60\r' "$attempt"
  sleep 2
done
printf '\n'

if [[ "$ready" -ne 1 ]]; then
  docker compose \
    --env-file docker.env \
    -f docker-compose.yml \
    -f docker-compose.vm.override.yml \
    ps || true
  printf '\nÚltimos logs del Core:\n' >&2
  docker logs --tail 220 indotel-core-completo 2>&1 || true
  fail "Core no respondió en http://127.0.0.1:${CORE_PORT}/health/ready"
fi

if command -v ufw >/dev/null 2>&1; then
  if sudo ufw status 2>/dev/null | head -n1 | grep -Eqi 'active|activo'; then
    log "Abriendo ${CORE_PORT}/tcp en UFW"
    sudo ufw allow "${CORE_PORT}/tcp" >/dev/null
  fi
fi

HOST_IP="$(ip -4 route get 1.1.1.1 2>/dev/null | awk '{for(i=1;i<=NF;i++) if($i=="src"){print $(i+1); exit}}')"
HOST_IP="${HOST_IP:-192.168.1.124}"

log "Verificación final"
docker compose \
  --env-file docker.env \
  -f docker-compose.yml \
  -f docker-compose.vm.override.yml \
  ps core

printf '\nCORE PUBLICADO CORRECTAMENTE\n'
printf 'Ubuntu: %s\n' "$HOST_IP"
printf 'Core:   http://%s:%s/health/ready\n' "$HOST_IP" "$CORE_PORT"
printf '\nEn Windows vuelva a ejecutar:\n'
printf '  CONFIGURAR_Y_VALIDAR_VM_WINDOWS.bat\n'
