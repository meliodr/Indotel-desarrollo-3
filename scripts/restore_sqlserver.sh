#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

BACKUP_FILE="${1:-}"
ENV_FILE="${2:-deploy/.env.release}"
COMPOSE_FILE="deploy/docker-compose.release.yml"
CONTAINER="indotel-release-sqlserver"
DATABASE="IndotelCoreDb"

fail() {
  printf '\nERROR: %s\n' "$1" >&2
  exit 1
}

[[ -n "$BACKUP_FILE" ]] || fail "Uso: CONFIRM_RESTORE=YES bash scripts/restore_sqlserver.sh backups/archivo.bak [env_file]"
[[ -f "$BACKUP_FILE" ]] || fail "No existe el respaldo $BACKUP_FILE."
[[ "${CONFIRM_RESTORE:-}" == "YES" ]] || fail "Defina CONFIRM_RESTORE=YES para autorizar la restauracion destructiva."
[[ -f "$ENV_FILE" ]] || fail "No existe $ENV_FILE."

docker inspect -f '{{.State.Running}}' "$CONTAINER" 2>/dev/null | grep -q true \
  || fail "El contenedor $CONTAINER no esta ejecutandose."

if [[ -f "$BACKUP_FILE.sha256" ]]; then
  (cd "$(dirname "$BACKUP_FILE")" && sha256sum -c "$(basename "$BACKUP_FILE").sha256") \
    || fail "La suma SHA-256 del respaldo no coincide."
fi

FILE_NAME="$(basename "$BACKUP_FILE")"
CONTAINER_PATH="/var/opt/mssql/backup/$FILE_NAME"

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" stop web gateway core >/dev/null 2>&1 || true

docker cp "$BACKUP_FILE" "$CONTAINER:$CONTAINER_PATH"

printf '\n==> Restaurando %s desde %s\n' "$DATABASE" "$BACKUP_FILE"
docker exec -e BACKUP_PATH="$CONTAINER_PATH" "$CONTAINER" sh -lc '
  SQLCMD=/opt/mssql-tools18/bin/sqlcmd
  [ -x "$SQLCMD" ] || SQLCMD=/opt/mssql-tools/bin/sqlcmd
  "$SQLCMD" -C -b -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "
    IF DB_ID(N'"'"'IndotelCoreDb'"'"') IS NOT NULL
      ALTER DATABASE [IndotelCoreDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    RESTORE DATABASE [IndotelCoreDb]
      FROM DISK = N'"'"'$BACKUP_PATH'"'"'
      WITH REPLACE, CHECKSUM, RECOVERY, STATS = 10;
    ALTER DATABASE [IndotelCoreDb] SET MULTI_USER;"
'

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d core gateway web caddy

printf '\nRestauracion completada. Revise /health antes de habilitar usuarios.\n'
