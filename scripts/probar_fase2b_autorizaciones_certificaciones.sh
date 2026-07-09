#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:5085}"
ADMIN_EMAIL="${ADMIN_EMAIL:-admin@indotel.test}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:-Admin123*}"

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
echo "PRUEBA FASE 2B - AUTORIZACIONES Y CERTIFICACIONES"
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

echo "3) Crear solicitud de autorizacion..."
AUT_BODY="$TMP_DIR/aut.json"
cat > "$AUT_BODY" <<JSON
{
  "solicitanteNombre": "Prestadora Demo Fase 2B",
  "solicitanteRnc": "130999001",
  "prestadoraId": 1,
  "tipoAutorizacionId": 1,
  "descripcion": "Solicitud demo de autorizacion institucional."
}
JSON
AUT_OUT="$TMP_DIR/aut.out.json"
request POST "$BASE_URL/api/autorizaciones" 201 "$AUT_BODY" "$AUT_OUT"
AUT_ID=$(jq -r '.id // empty' "$AUT_OUT")
AUT_ESTADO=$(jq -r '.estado // empty' "$AUT_OUT")
[[ -n "$AUT_ID" && "$AUT_ESTADO" == "RECIBIDA" ]] || fail "autorizacion no fue creada en RECIBIDA"
ok "AUTORIZACION_ID=$AUT_ID"

echo "4) Pasar autorizacion a EN_REVISION..."
STATE_BODY="$TMP_DIR/state.json"
cat > "$STATE_BODY" <<JSON
{
  "estadoNuevo": "EN_REVISION",
  "comentario": "Solicitud tomada para revision."
}
JSON
STATE_OUT="$TMP_DIR/state.out.json"
request PATCH "$BASE_URL/api/autorizaciones/$AUT_ID/estado" 200 "$STATE_BODY" "$STATE_OUT"
[[ "$(jq -r '.estado // empty' "$STATE_OUT")" == "EN_REVISION" ]] || fail "autorizacion no paso a EN_REVISION"
ok "autorizacion en revision"

echo "5) Intentar renovar antes de aprobar, debe devolver 409..."
REN_BODY="$TMP_DIR/renovar.json"
cat > "$REN_BODY" <<JSON
{
  "comentario": "Intento invalido de renovacion."
}
JSON
REN_OUT_PRE="$TMP_DIR/renovar-pre.out.json"
request POST "$BASE_URL/api/autorizaciones/$AUT_ID/renovar" 409 "$REN_BODY" "$REN_OUT_PRE"
ok "renovacion antes de aprobar bloqueada -> 409"

echo "6) Aprobar autorizacion..."
APR_BODY="$TMP_DIR/aprobar.json"
cat > "$APR_BODY" <<JSON
{
  "estadoNuevo": "APROBADA",
  "comentario": "Autorizacion aprobada por revision tecnica."
}
JSON
APR_OUT="$TMP_DIR/aprobar.out.json"
request PATCH "$BASE_URL/api/autorizaciones/$AUT_ID/estado" 200 "$APR_BODY" "$APR_OUT"
[[ "$(jq -r '.estado // empty' "$APR_OUT")" == "APROBADA" ]] || fail "autorizacion no fue aprobada"
ok "autorizacion aprobada"

echo "7) Renovar autorizacion aprobada..."
REN_OUT="$TMP_DIR/renovar.out.json"
request POST "$BASE_URL/api/autorizaciones/$AUT_ID/renovar" 200 "$REN_BODY" "$REN_OUT"
[[ "$(jq -r '.estado // empty' "$REN_OUT")" == "RENOVADA" ]] || fail "autorizacion no fue renovada"
ok "autorizacion renovada"

echo "8) Crear solicitud de certificacion..."
CERT_BODY="$TMP_DIR/cert.json"
cat > "$CERT_BODY" <<JSON
{
  "solicitanteNombre": "Prestadora Demo Fase 2B",
  "solicitanteRnc": "130999001",
  "prestadoraId": 1,
  "tipoCertificacionId": 1,
  "descripcion": "Solicitud demo de certificacion institucional."
}
JSON
CERT_OUT="$TMP_DIR/cert.out.json"
request POST "$BASE_URL/api/certificaciones" 201 "$CERT_BODY" "$CERT_OUT"
CERT_ID=$(jq -r '.id // empty' "$CERT_OUT")
[[ -n "$CERT_ID" ]] || fail "certificacion no creada"
ok "CERTIFICACION_ID=$CERT_ID"

echo "9) Pasar certificacion a EN_REVISION y luego APROBADA..."
CERT_REV_OUT="$TMP_DIR/cert-rev.out.json"
request PATCH "$BASE_URL/api/certificaciones/$CERT_ID/estado" 200 "$STATE_BODY" "$CERT_REV_OUT"
CERT_APR_OUT="$TMP_DIR/cert-apr.out.json"
request PATCH "$BASE_URL/api/certificaciones/$CERT_ID/estado" 200 "$APR_BODY" "$CERT_APR_OUT"
[[ "$(jq -r '.estado // empty' "$CERT_APR_OUT")" == "APROBADA" ]] || fail "certificacion no fue aprobada"
ok "certificacion aprobada"

echo "10) Reportes de autorizaciones y certificaciones..."
REP_AUT="$TMP_DIR/rep-aut.json"
request GET "$BASE_URL/api/reportes/autorizaciones" 200 "" "$REP_AUT"
[[ "$(jq -r '.total // 0' "$REP_AUT")" -ge 1 ]] || fail "reporte autorizaciones sin datos"
ok "GET /api/reportes/autorizaciones"
REP_CERT="$TMP_DIR/rep-cert.json"
request GET "$BASE_URL/api/reportes/certificaciones" 200 "" "$REP_CERT"
[[ "$(jq -r '.total // 0' "$REP_CERT")" -ge 1 ]] || fail "reporte certificaciones sin datos"
ok "GET /api/reportes/certificaciones"

echo "11) Verificar auditoria Fase 2B..."
for PAIR in "SolicitudAutorizacion:CREAR_SOLICITUD_AUTORIZACION" "SolicitudAutorizacion:CAMBIO_ESTADO_AUTORIZACION" "SolicitudAutorizacion:RENOVAR_AUTORIZACION" "SolicitudCertificacion:CREAR_SOLICITUD_CERTIFICACION" "SolicitudCertificacion:CAMBIO_ESTADO_CERTIFICACION"; do
  ENTIDAD="${PAIR%%:*}"
  ACCION="${PAIR##*:}"
  AUD_OUT="$TMP_DIR/aud-$ACCION.json"
  request GET "$BASE_URL/api/auditorias?entidad=$ENTIDAD&accion=$ACCION&page=1&pageSize=20" 200 "" "$AUD_OUT"
  [[ "$(jq -r '.total // 0' "$AUD_OUT")" -ge 1 ]] || fail "auditoria no encontrada para $ACCION"
  ok "auditoria $ACCION encontrada"
done

echo "===================================="
echo "PRUEBA FASE 2B TERMINADA CORRECTAMENTE"
echo "AUTORIZACIONES Y CERTIFICACIONES VALIDADAS"
echo "AUTORIZACION_ID=$AUT_ID"
echo "CERTIFICACION_ID=$CERT_ID"
echo "===================================="
