#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

CORE_URL="${CORE_URL:-http://127.0.0.1:5085}"
GATEWAY_URL="${GATEWAY_URL:-http://127.0.0.1:5185}"
WEB_URL="${WEB_URL:-http://127.0.0.1:5234}"
SQL_PASSWORD="${INDOTEL_SQL_PASSWORD:-}"
ADMIN_EMAIL="${INDOTEL_ADMIN_EMAIL:-admin@indotel.test}"
ADMIN_PASSWORD="${INDOTEL_ADMIN_PASSWORD:-Admin123*}"
JWT_KEY="${INDOTEL_JWT_KEY:-$(python3 - <<'PY'
import secrets
print(secrets.token_urlsafe(48))
PY
)}"
LOG_DIR="${TMPDIR:-/tmp}/indotel-sprint5-runtime"
mkdir -p "$LOG_DIR"

CORE_PID=""
GATEWAY_PID=""
WEB_PID=""
STARTED_SQL=0
CREATED_DOCKER_ENV=0

cleanup() {
  set +e
  [[ -n "$WEB_PID" ]] && kill "$WEB_PID" 2>/dev/null
  [[ -n "$GATEWAY_PID" ]] && kill "$GATEWAY_PID" 2>/dev/null
  [[ -n "$CORE_PID" ]] && kill "$CORE_PID" 2>/dev/null
  wait "$WEB_PID" "$GATEWAY_PID" "$CORE_PID" 2>/dev/null
  if [[ "$STARTED_SQL" == "1" ]]; then
    docker compose stop indotel-sqlserver >/dev/null 2>&1 || true
  fi
  if [[ "$CREATED_DOCKER_ENV" == "1" ]]; then
    rm -f docker.env
  fi
}
trap cleanup EXIT

fail() {
  printf '\nERROR: %s\n' "$1" >&2
  printf '\n--- Core log ---\n' >&2
  tail -n 80 "$LOG_DIR/core.log" 2>/dev/null >&2 || true
  printf '\n--- Gateway log ---\n' >&2
  tail -n 80 "$LOG_DIR/gateway.log" 2>/dev/null >&2 || true
  printf '\n--- Web log ---\n' >&2
  tail -n 80 "$LOG_DIR/web.log" 2>/dev/null >&2 || true
  exit 1
}

for command in dotnet curl python3 docker; do
  command -v "$command" >/dev/null || fail "Falta el comando requerido: $command"
done

docker compose version >/dev/null 2>&1 || fail "Docker Compose no esta disponible."

wait_http() {
  local url="$1"
  local expected="${2:-200}"
  local attempts="${3:-60}"
  local status
  for ((i=1; i<=attempts; i++)); do
    status="$(curl -sS --max-time 3 -o /dev/null -w '%{http_code}' "$url" 2>/dev/null || true)"
    if [[ "$status" == "$expected" ]]; then
      return 0
    fi
    sleep 1
  done
  return 1
}

json_value() {
  local file="$1"
  local path="$2"
  python3 - "$file" "$path" <<'PY'
import json, sys
with open(sys.argv[1], encoding='utf-8') as fh:
    value = json.load(fh)
for part in sys.argv[2].split('.'):
    if isinstance(value, list):
        value = value[int(part)]
    else:
        value = value[part]
print(value)
PY
}

request() {
  local method="$1"
  local url="$2"
  local output="$3"
  local token="${4:-}"
  local data="${5:-}"
  local args=(-sS --max-time 20 -o "$output" -w '%{http_code}' -X "$method")
  [[ -n "$token" ]] && args+=(-H "Authorization: Bearer $token")
  if [[ -n "$data" ]]; then
    args+=(-H 'Content-Type: application/json' --data "$data")
  fi
  curl "${args[@]}" "$url"
}

printf '\n==> Preparando SQL Server\n'
if docker inspect -f '{{.State.Running}}' indotel-sqlserver 2>/dev/null | grep -q true; then
  printf 'SQL Server ya esta ejecutandose.\n'
else
  [[ -n "$SQL_PASSWORD" ]] || fail "INDOTEL_SQL_PASSWORD es obligatorio cuando el contenedor SQL no esta iniciado."
  if [[ ! -f docker.env ]]; then
    cat > docker.env <<EOF
ACCEPT_EULA=Y
MSSQL_PID=Developer
MSSQL_SA_PASSWORD=$SQL_PASSWORD
EOF
    CREATED_DOCKER_ENV=1
  fi
  docker compose up -d indotel-sqlserver
  STARTED_SQL=1
fi

if [[ -z "$SQL_PASSWORD" && -f docker.env ]]; then
  SQL_PASSWORD="$(sed -n 's/^MSSQL_SA_PASSWORD=//p' docker.env | head -n 1)"
fi
[[ -n "$SQL_PASSWORD" ]] || fail "No fue posible determinar la clave de SQL Server. Defina INDOTEL_SQL_PASSWORD."

for ((i=1; i<=90; i++)); do
  if (echo > /dev/tcp/127.0.0.1/1433) >/dev/null 2>&1; then
    break
  fi
  [[ "$i" == "90" ]] && fail "SQL Server no abrio el puerto 1433."
  sleep 1
done

CONNECTION_STRING="Server=127.0.0.1,1433;Database=IndotelSprint5Db;User Id=sa;Password=$SQL_PASSWORD;TrustServerCertificate=True;Encrypt=False"

printf '\n==> Iniciando Core\n'
ASPNETCORE_ENVIRONMENT=Development \
ASPNETCORE_URLS="$CORE_URL" \
ConnectionStrings__DefaultConnection="$CONNECTION_STRING" \
Jwt__Issuer="Indotel.Core" \
Jwt__Audience="Indotel.Clients" \
Jwt__Key="$JWT_KEY" \
Jwt__AccessTokenMinutes=30 \
Jwt__RefreshTokenDays=7 \
SeedData__Enabled=true \
SeedData__AdminEmail="$ADMIN_EMAIL" \
SeedData__AdminPassword="$ADMIN_PASSWORD" \
Database__ApplyMigrationsOnStartup=true \
Cors__AllowedOrigins__0="$WEB_URL" \
dotnet run --project core-indotel/Indotel.Core/Indotel.Core.csproj --configuration Release --no-build \
  >"$LOG_DIR/core.log" 2>&1 &
CORE_PID=$!

wait_http "$CORE_URL/health" 200 90 || fail "Core no inicio correctamente."
wait_http "$CORE_URL/api/health/db" 200 90 || fail "Readiness de SQL no alcanzo HTTP 200."

printf '\n==> Iniciando Gateway\n'
ASPNETCORE_ENVIRONMENT=Development \
ASPNETCORE_URLS="$GATEWAY_URL" \
Gateway__CoreBaseUrl="$CORE_URL" \
Cors__AllowedOrigins__0="$WEB_URL" \
dotnet run --project api-gateway/Indotel.ApiGateway/Indotel.ApiGateway.csproj --configuration Release --no-build \
  >"$LOG_DIR/gateway.log" 2>&1 &
GATEWAY_PID=$!

wait_http "$GATEWAY_URL/health" 200 60 || fail "Gateway no inicio correctamente."
wait_http "$GATEWAY_URL/health/ready" 200 60 || fail "Gateway no detecto el Core disponible."
wait_http "$GATEWAY_URL/api/health" 200 60 || fail "El proxy Gateway -> Core no respondio HTTP 200."

printf '\n==> Iniciando Web\n'
ASPNETCORE_ENVIRONMENT=Development \
ASPNETCORE_URLS="$WEB_URL" \
ApiSettings__GatewayBaseUrl="$GATEWAY_URL" \
Security__DataProtectionKeysPath="$LOG_DIR/data-protection-keys" \
dotnet run --project INDOTEL.Web/INDOTEL.WEB.csproj --configuration Release --no-build \
  >"$LOG_DIR/web.log" 2>&1 &
WEB_PID=$!

wait_http "$WEB_URL/" 200 60 || fail "Web no inicio correctamente."
wait_http "$WEB_URL/ruta-inexistente-sprint5" 404 20 || fail "La Web no devolvio la pagina 404 esperada."

printf '\n==> Registro y autenticacion de ciudadanos por Gateway\n'
SUFFIX="$(date +%s)"
CEDULA1="$(printf '8%010d' "$((SUFFIX % 10000000000))")"
CEDULA2="$(printf '9%010d' "$(((SUFFIX + 1) % 10000000000))")"
EMAIL1="sprint5-ciudadano1-$SUFFIX@test.local"
EMAIL2="sprint5-ciudadano2-$SUFFIX@test.local"
PASSWORD='Ciudadano123*'

REGISTER1=$(printf '{"cedula":"%s","nombres":"Prueba","apellidos":"Uno","telefono":"8090000001","correo":"%s","direccion":"Baní","password":"%s"}' "$CEDULA1" "$EMAIL1" "$PASSWORD")
REGISTER2=$(printf '{"cedula":"%s","nombres":"Prueba","apellidos":"Dos","telefono":"8090000002","correo":"%s","direccion":"Baní","password":"%s"}' "$CEDULA2" "$EMAIL2" "$PASSWORD")

STATUS="$(request POST "$GATEWAY_URL/api/auth/register-ciudadano" "$LOG_DIR/register1.json" '' "$REGISTER1")"
[[ "$STATUS" == "200" || "$STATUS" == "201" ]] || fail "Registro del ciudadano 1 devolvio HTTP $STATUS."
STATUS="$(request POST "$GATEWAY_URL/api/auth/register-ciudadano" "$LOG_DIR/register2.json" '' "$REGISTER2")"
[[ "$STATUS" == "200" || "$STATUS" == "201" ]] || fail "Registro del ciudadano 2 devolvio HTTP $STATUS."

TOKEN1="$(json_value "$LOG_DIR/register1.json" token)"
REFRESH1="$(json_value "$LOG_DIR/register1.json" refreshToken)"
TOKEN2="$(json_value "$LOG_DIR/register2.json" token)"

STATUS="$(request GET "$GATEWAY_URL/api/ciudadanos/me" "$LOG_DIR/me1.json" "$TOKEN1")"
[[ "$STATUS" == "200" ]] || fail "Perfil del ciudadano 1 devolvio HTTP $STATUS."
STATUS="$(request GET "$GATEWAY_URL/api/ciudadanos/me" "$LOG_DIR/me2.json" "$TOKEN2")"
[[ "$STATUS" == "200" ]] || fail "Perfil del ciudadano 2 devolvio HTTP $STATUS."
CID1="$(json_value "$LOG_DIR/me1.json" id)"

printf '\n==> Prueba de propiedad entre ciudadanos\n'
STATUS="$(request GET "$GATEWAY_URL/api/ciudadanos/$CID1" "$LOG_DIR/idor-ciudadano.json" "$TOKEN2")"
[[ "$STATUS" == "403" || "$STATUS" == "404" ]] || fail "Acceso indebido entre ciudadanos devolvio HTTP $STATUS; se esperaba 403 o 404."

printf '\n==> Creacion y propiedad de reclamacion\n'
STATUS="$(request GET "$GATEWAY_URL/api/catalogos/prestadoras" "$LOG_DIR/prestadoras.json" "$TOKEN1")"
[[ "$STATUS" == "200" ]] || fail "Catalogo de prestadoras devolvio HTTP $STATUS."
STATUS="$(request GET "$GATEWAY_URL/api/catalogos/servicios" "$LOG_DIR/servicios.json" "$TOKEN1")"
[[ "$STATUS" == "200" ]] || fail "Catalogo de servicios devolvio HTTP $STATUS."
PRESTADORA_ID="$(json_value "$LOG_DIR/prestadoras.json" 0.id)"
SERVICIO_ID="$(json_value "$LOG_DIR/servicios.json" 0.id)"

CLAIM=$(printf '{"ciudadanoId":%s,"prestadoraId":%s,"servicioTelecomId":%s,"canalRecepcion":"WEB","prioridad":"MEDIA","provincia":"Peravia","municipio":"Baní","titulo":"Prueba Sprint 5","descripcion":"Reclamación creada por la prueba integrada del Sprint 5."}' "$CID1" "$PRESTADORA_ID" "$SERVICIO_ID")
STATUS="$(request POST "$GATEWAY_URL/api/reclamaciones" "$LOG_DIR/reclamacion.json" "$TOKEN1" "$CLAIM")"
[[ "$STATUS" == "201" ]] || fail "Creacion de reclamacion devolvio HTTP $STATUS."
RID="$(json_value "$LOG_DIR/reclamacion.json" id)"

STATUS="$(request GET "$GATEWAY_URL/api/reclamaciones/$RID" "$LOG_DIR/reclamacion-propia.json" "$TOKEN1")"
[[ "$STATUS" == "200" ]] || fail "El ciudadano propietario no pudo consultar su reclamacion. HTTP $STATUS."
STATUS="$(request GET "$GATEWAY_URL/api/reclamaciones/$RID" "$LOG_DIR/idor-reclamacion.json" "$TOKEN2")"
[[ "$STATUS" == "403" || "$STATUS" == "404" ]] || fail "Otro ciudadano pudo consultar la reclamacion. HTTP $STATUS."

printf '\n==> Rotacion y reutilizacion de refresh token\n'
REFRESH_BODY=$(printf '{"refreshToken":"%s"}' "$REFRESH1")
STATUS="$(request POST "$GATEWAY_URL/api/auth/refresh-token" "$LOG_DIR/refresh-ok.json" '' "$REFRESH_BODY")"
[[ "$STATUS" == "200" ]] || fail "La rotacion de refresh token devolvio HTTP $STATUS."
STATUS="$(request POST "$GATEWAY_URL/api/auth/refresh-token" "$LOG_DIR/refresh-reuse.json" '' "$REFRESH_BODY")"
[[ "$STATUS" == "401" ]] || fail "La reutilizacion de refresh token devolvio HTTP $STATUS; se esperaba 401."
REUSE_CODE="$(python3 - "$LOG_DIR/refresh-reuse.json" <<'PY'
import json, sys
with open(sys.argv[1], encoding='utf-8') as fh:
    obj=json.load(fh)
print(obj.get('codigo',''))
PY
)"
[[ "$REUSE_CODE" == "REFRESH_TOKEN_REUTILIZADO" ]] || fail "No se recibio REFRESH_TOKEN_REUTILIZADO. Codigo: $REUSE_CODE"

printf '\n==> Comprobacion de Core apagado y Gateway controlado\n'
kill "$CORE_PID"
wait "$CORE_PID" 2>/dev/null || true
CORE_PID=""
sleep 2
STATUS="$(request GET "$GATEWAY_URL/api/health" "$LOG_DIR/core-down.json")"
[[ "$STATUS" == "503" ]] || fail "Con Core apagado el Gateway devolvio HTTP $STATUS; se esperaba 503."
CORRELATION="$(python3 - "$LOG_DIR/core-down.json" <<'PY'
import json, sys
with open(sys.argv[1], encoding='utf-8') as fh:
    obj=json.load(fh)
print(obj.get('correlationId',''))
PY
)"
[[ -n "$CORRELATION" ]] || fail "La respuesta 503 del Gateway no contiene correlationId."

cat <<EOF

Pruebas integradas aprobadas:
- Core -> SQL Server;
- Gateway -> Core;
- Web -> Gateway;
- registro y autenticacion de dos ciudadanos;
- proteccion de propiedad de ciudadano y reclamacion;
- creacion de reclamacion;
- rotacion y deteccion de reutilizacion de refresh token;
- Core apagado -> Gateway 503 con correlationId.

Logs: $LOG_DIR
EOF
