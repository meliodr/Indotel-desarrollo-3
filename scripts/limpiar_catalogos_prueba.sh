#!/usr/bin/env bash
set -euo pipefail

CONTAINER_NAME="${CONTAINER_NAME:-indotel-sqlserver}"
DATABASE_NAME="${DATABASE_NAME:-IndotelCoreDb}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SQL_FILE="$SCRIPT_DIR/limpiar_catalogos_prueba.sql"

if ! docker ps --format '{{.Names}}' | grep -qx "$CONTAINER_NAME"; then
  echo "El contenedor $CONTAINER_NAME no está ejecutándose." >&2
  exit 1
fi

SQL_PASSWORD="$({
  docker inspect "$CONTAINER_NAME" \
    --format '{{range .Config.Env}}{{println .}}{{end}}' |
    sed -n 's/^MSSQL_SA_PASSWORD=//p'
} | head -n 1)"

if [[ -z "$SQL_PASSWORD" ]]; then
  echo "No se pudo obtener MSSQL_SA_PASSWORD del contenedor." >&2
  exit 1
fi

if docker exec "$CONTAINER_NAME" test -x /opt/mssql-tools18/bin/sqlcmd; then
  SQLCMD_PATH="/opt/mssql-tools18/bin/sqlcmd"
elif docker exec "$CONTAINER_NAME" test -x /opt/mssql-tools/bin/sqlcmd; then
  SQLCMD_PATH="/opt/mssql-tools/bin/sqlcmd"
else
  echo "No se encontró sqlcmd dentro del contenedor." >&2
  exit 1
fi

echo "Desactivando registros de prueba en $DATABASE_NAME..."
docker exec -i "$CONTAINER_NAME" "$SQLCMD_PATH" \
  -S localhost \
  -U sa \
  -P "$SQL_PASSWORD" \
  -C \
  -d "$DATABASE_NAME" < "$SQL_FILE"

echo "Limpieza completada. Los registros fueron desactivados, no eliminados."
