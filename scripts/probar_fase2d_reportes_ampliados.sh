#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:5085}"
ADMIN_EMAIL="${ADMIN_EMAIL:-admin@indotel.test}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:?Debe definir ADMIN_PASSWORD}"

TMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TMP_DIR"' EXIT

ok() { echo "OK: $1"; }
fail() { echo "ERROR: $1" >&2; exit 1; }

request_get() {
  local url="$1"
  local out_file="$2"
  local status
  status=$(curl -sS -o "$out_file" -w "%{http_code}" -X GET "$url" \
    -H "Authorization: Bearer $TOKEN")
  [[ "$status" == "200" ]] || { cat "$out_file" >&2 || true; fail "GET $url devolvio $status"; }
}

echo "===================================="
echo "PRUEBA FASE 2D - REPORTES REGULATORIOS AMPLIADOS"
echo "===================================="

echo "1) Health check..."
curl -sS "$BASE_URL/api/health" | jq . >/dev/null
ok "GET /api/health"

echo "2) Login admin..."
LOGIN_BODY="$TMP_DIR/login.json"
cat > "$LOGIN_BODY" <<JSON
{
  "correo": "$ADMIN_EMAIL",
  "password": "$ADMIN_PASSWORD"
}
JSON
LOGIN_OUT="$TMP_DIR/login.out.json"
STATUS=$(curl -sS -o "$LOGIN_OUT" -w "%{http_code}" -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  --data-binary "@$LOGIN_BODY")
[[ "$STATUS" == "200" ]] || { cat "$LOGIN_OUT" >&2; fail "login admin devolvio $STATUS"; }
TOKEN=$(jq -r '.token // empty' "$LOGIN_OUT")
[[ -n "$TOKEN" && "$TOKEN" != "null" ]] || fail "no se recibio token"
ok "admin token"

echo "3) Validar reportes ampliados..."
ENDPOINTS=(
  "/api/reportes/prestadoras-ranking"
  "/api/reportes/sla-ranking"
  "/api/reportes/reclamaciones-mensual"
  "/api/reportes/tiempo-promedio-respuesta"
  "/api/reportes/servicios-mas-reclamados"
  "/api/reportes/resoluciones-periodo"
  "/api/reportes/autorizaciones-estado"
  "/api/reportes/certificaciones-estado"
  "/api/reportes/licencias-vencimiento"
)

for endpoint in "${ENDPOINTS[@]}"; do
  OUT="$TMP_DIR/$(echo "$endpoint" | tr '/?' '__').json"
  request_get "$BASE_URL$endpoint" "$OUT"
  jq . "$OUT" >/dev/null
  ok "GET $endpoint"
done

echo "===================================="
echo "PRUEBA FASE 2D TERMINADA CORRECTAMENTE"
echo "REPORTES REGULATORIOS AMPLIADOS VALIDADOS"
echo "===================================="
