#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

ENV_FILE="${1:-deploy/.env.release}"
BACKUP_DIR="${BACKUP_DIR:-$ROOT_DIR/backups}"
CONTAINER="indotel-release-sqlserver"
DATABASE="IndotelCoreDb"

fail() {
  printf '\nERROR: %s\n' "$1" >&2
  exit 1
}

[[ -f "$ENV_FILE" ]] || fail "No existe $ENV_FILE."
docker inspect -f '{{.State.Running}}' "$CONTAINER" 2>/dev/null | grep -q true \
  || fail "El contenedor $CONTAINER no esta ejecutandose."

mkdir -p "$BACKUP_DIR"
TIMESTAMP="$(date -u +%Y%m%dT%H%M%SZ)"
FILE_NAME="${DATABASE}-${TIMESTAMP}.bak"
CONTAINER_PATH="/var/opt/mssql/backup/$FILE_NAME"
HOST_PATH="$BACKUP_DIR/$FILE_NAME"

printf '\n==> Creando respaldo de %s\n' "$DATABASE"
docker exec "$CONTAINER" sh -lc '
  SQLCMD=/opt/mssql-tools18/bin/sqlcmd
  [ -x "$SQLCMD" ] || SQLCMD=/opt/mssql-tools/bin/sqlcmd
  "$SQLCMD" -C -b -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
    -Q "BACKUP DATABASE [IndotelCoreDb] TO DISK = N'"'"'/var/opt/mssql/backup/'"$FILE_NAME"'"'"' WITH INIT, CHECKSUM, COMPRESSION, STATS = 10"
' FILE_NAME="$FILE_NAME"

docker cp "$CONTAINER:$CONTAINER_PATH" "$HOST_PATH"
sha256sum "$HOST_PATH" > "$HOST_PATH.sha256"

printf '\nRespaldo creado:\n%s\n%s\n' "$HOST_PATH" "$HOST_PATH.sha256"
