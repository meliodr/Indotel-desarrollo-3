#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:5085}"
ADMIN_EMAIL="${ADMIN_EMAIL:-admin@indotel.test}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:?Debe definir ADMIN_PASSWORD}"

TMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TMP_DIR"' EXIT

ok() { echo "OK: $1"; }
fail() { echo "ERROR: $1" >&2; exit 1; }

request() {
  local method="$1"
  local url="$2"
  local expected="$3"
  local body_file="${4:-}"
  local out_file="$5"
  local status

  if [[ -n "$body_file" ]]; then
    status=$(curl -sS -o "$out_file" -w "%{http_code}" -X "$method" "$url" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $TOKEN" \
      --data-binary "@$body_file")
  else
    status=$(curl -sS -o "$out_file" -w "%{http_code}" -X "$method" "$url" \
      -H "Authorization: Bearer $TOKEN")
  fi

  [[ "$status" == "$expected" ]] || {
    echo "Respuesta:" >&2
    cat "$out_file" >&2 || true
    fail "$method $url esperaba $expected y devolvio $status"
  }
}

echo "===================================="
echo "PRUEBA FASE 2C - ESPECTRO Y LICENCIAS TECNICAS"
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

RANDOM_BASE=$((RANDOM + 3000))
INICIO="${RANDOM_BASE}.100"
FIN="${RANDOM_BASE}.200"

echo "3) Crear frecuencia radioelectrica..."
FREC_BODY="$TMP_DIR/frecuencia.json"
cat > "$FREC_BODY" <<JSON
{
  "rangoInicioMHz": $INICIO,
  "rangoFinMHz": $FIN,
  "banda": "Banda demo Fase 2C",
  "servicioUso": "Internet fijo inalambrico",
  "provincia": "Peravia",
  "region": "Sur",
  "observacion": "Frecuencia demo para prueba Fase 2C."
}
JSON
FREC_OUT="$TMP_DIR/frecuencia.out.json"
request POST "$BASE_URL/api/espectro/frecuencias" 201 "$FREC_BODY" "$FREC_OUT"
FRECUENCIA_ID=$(jq -r '.id // empty' "$FREC_OUT")
[[ -n "$FRECUENCIA_ID" ]] || fail "frecuencia no creada"
[[ "$(jq -r '.estado // empty' "$FREC_OUT")" == "DISPONIBLE" ]] || fail "frecuencia no inicio DISPONIBLE"
ok "FRECUENCIA_ID=$FRECUENCIA_ID"

echo "4) Consultar frecuencia por ID..."
FREC_GET="$TMP_DIR/frecuencia-get.json"
request GET "$BASE_URL/api/espectro/frecuencias/$FRECUENCIA_ID" 200 "" "$FREC_GET"
ok "GET /api/espectro/frecuencias/{id}"

echo "5) Crear asignacion de frecuencia..."
ASIG_BODY="$TMP_DIR/asignacion.json"
cat > "$ASIG_BODY" <<JSON
{
  "frecuenciaRadioelectricaId": $FRECUENCIA_ID,
  "prestadoraId": 1,
  "entidadAsignada": "Prestadora demo Fase 2C",
  "usoAutorizado": "Uso tecnico demo Fase 2C",
  "provincia": "Peravia",
  "region": "Sur",
  "observacion": "Asignacion demo."
}
JSON
ASIG_OUT="$TMP_DIR/asignacion.out.json"
request POST "$BASE_URL/api/espectro/asignaciones" 201 "$ASIG_BODY" "$ASIG_OUT"
ASIGNACION_ID=$(jq -r '.id // empty' "$ASIG_OUT")
[[ -n "$ASIGNACION_ID" ]] || fail "asignacion no creada"
ok "ASIGNACION_ID=$ASIGNACION_ID"

echo "6) Intentar duplicar asignacion, debe devolver 409..."
ASIG_DUP_OUT="$TMP_DIR/asignacion-dup.out.json"
request POST "$BASE_URL/api/espectro/asignaciones" 409 "$ASIG_BODY" "$ASIG_DUP_OUT"
ok "asignacion duplicada bloqueada -> 409"

echo "7) Crear licencia tecnica..."
LIC_BODY="$TMP_DIR/licencia.json"
cat > "$LIC_BODY" <<JSON
{
  "prestadoraId": 1,
  "entidadAsignada": "Prestadora demo Fase 2C",
  "frecuenciaRadioelectricaId": $FRECUENCIA_ID
}
JSON
LIC_OUT="$TMP_DIR/licencia.out.json"
request POST "$BASE_URL/api/licencias-tecnicas" 201 "$LIC_BODY" "$LIC_OUT"
LICENCIA_ID=$(jq -r '.id // empty' "$LIC_OUT")
[[ -n "$LICENCIA_ID" ]] || fail "licencia no creada"
[[ "$(jq -r '.estado // empty' "$LIC_OUT")" == "SOLICITADA" ]] || fail "licencia no inicio SOLICITADA"
ok "LICENCIA_ID=$LICENCIA_ID"

echo "8) Cambiar licencia SOLICITADA -> EN_EVALUACION_TECNICA -> APROBADA -> ACTIVA..."
for ESTADO in EN_EVALUACION_TECNICA APROBADA ACTIVA; do
  STATE_BODY="$TMP_DIR/lic-$ESTADO.json"
  cat > "$STATE_BODY" <<JSON
{
  "estadoNuevo": "$ESTADO",
  "comentario": "Cambio a $ESTADO por prueba Fase 2C."
}
JSON
  STATE_OUT="$TMP_DIR/lic-$ESTADO.out.json"
  request PATCH "$BASE_URL/api/licencias-tecnicas/$LICENCIA_ID/estado" 200 "$STATE_BODY" "$STATE_OUT"
  [[ "$(jq -r '.estado // empty' "$STATE_OUT")" == "$ESTADO" ]] || fail "licencia no paso a $ESTADO"
  ok "licencia en $ESTADO"
done

echo "9) Reportes de espectro y licencias tecnicas..."
REP_ESP="$TMP_DIR/rep-espectro.json"
request GET "$BASE_URL/api/reportes/espectro" 200 "" "$REP_ESP"
[[ "$(jq -r '.totalFrecuencias // 0' "$REP_ESP")" -ge 1 ]] || fail "reporte espectro sin datos"
ok "GET /api/reportes/espectro"
REP_LIC="$TMP_DIR/rep-licencias.json"
request GET "$BASE_URL/api/reportes/licencias-tecnicas" 200 "" "$REP_LIC"
[[ "$(jq -r '.total // 0' "$REP_LIC")" -ge 1 ]] || fail "reporte licencias sin datos"
ok "GET /api/reportes/licencias-tecnicas"

echo "10) Verificar auditoria Fase 2C..."
for PAIR in "FrecuenciaRadioelectrica:CREAR_FRECUENCIA" "AsignacionFrecuencia:ASIGNAR_FRECUENCIA" "LicenciaTecnica:CREAR_LICENCIA_TECNICA" "LicenciaTecnica:CAMBIO_ESTADO_LICENCIA_TECNICA"; do
  ENTIDAD="${PAIR%%:*}"
  ACCION="${PAIR##*:}"
  AUD_OUT="$TMP_DIR/aud-$ACCION.json"
  request GET "$BASE_URL/api/auditorias?entidad=$ENTIDAD&accion=$ACCION&page=1&pageSize=20" 200 "" "$AUD_OUT"
  [[ "$(jq -r '.total // 0' "$AUD_OUT")" -ge 1 ]] || fail "auditoria no encontrada para $ACCION"
  ok "auditoria $ACCION encontrada"
done

echo "===================================="
echo "PRUEBA FASE 2C TERMINADA CORRECTAMENTE"
echo "ESPECTRO Y LICENCIAS TECNICAS VALIDADOS"
echo "FRECUENCIA_ID=$FRECUENCIA_ID"
echo "ASIGNACION_ID=$ASIGNACION_ID"
echo "LICENCIA_ID=$LICENCIA_ID"
echo "===================================="
