#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:5085}"
ADMIN_EMAIL="${ADMIN_EMAIL:-admin@indotel.test}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:-Admin123*}"

TMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TMP_DIR"' EXIT

ok() { echo "OK: $1"; }
fail() { echo "ERROR: $1" >&2; exit 1; }

require_status() {
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
echo "PRUEBA FASE 2A - RESOLUCIONES"
echo "===================================="

echo "1) Health checks..."
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
[[ -n "$TOKEN" && "$TOKEN" != "null" ]] || fail "no se recibio token admin"
ok "admin token"

echo "3) Crear resolucion institucional en BORRADOR..."
CREATE_BODY="$TMP_DIR/resolucion-create.json"
cat > "$CREATE_BODY" <<JSON
{
  "titulo": "Resolucion institucional Fase 2A",
  "resumen": "Resolucion de prueba para validar el modulo institucional de Fase 2A.",
  "tipoResolucionId": 1
}
JSON

CREATE_OUT="$TMP_DIR/resolucion-create.out.json"
require_status POST "$BASE_URL/api/resoluciones" 201 "$CREATE_BODY" "$CREATE_OUT"
RESOLUCION_ID=$(jq -r '.id // empty' "$CREATE_OUT")
NUMERO_RESOLUCION=$(jq -r '.numeroResolucion // empty' "$CREATE_OUT")
ESTADO=$(jq -r '.estado // empty' "$CREATE_OUT")
[[ -n "$RESOLUCION_ID" ]] || fail "no se recibio ID de resolucion"
[[ "$ESTADO" == "BORRADOR" ]] || fail "estado inicial esperado BORRADOR, llego $ESTADO"
ok "RESOLUCION_ID=$RESOLUCION_ID"
ok "NUMERO_RESOLUCION=$NUMERO_RESOLUCION"

echo "4) Intentar publicar sin aprobar, debe devolver 409..."
PUBLICAR_OUT_PRE="$TMP_DIR/publicar-pre.out.json"
require_status PATCH "$BASE_URL/api/resoluciones/$RESOLUCION_ID/publicar" 409 "" "$PUBLICAR_OUT_PRE"
ok "publicacion sin aprobar bloqueada -> 409"

echo "5) Aprobar resolucion..."
APROBAR_OUT="$TMP_DIR/aprobar.out.json"
require_status PATCH "$BASE_URL/api/resoluciones/$RESOLUCION_ID/aprobar" 200 "" "$APROBAR_OUT"
ESTADO=$(jq -r '.estado // empty' "$APROBAR_OUT")
[[ "$ESTADO" == "APROBADA" ]] || fail "estado esperado APROBADA, llego $ESTADO"
ok "resolucion aprobada"

echo "6) Publicar resolucion..."
PUBLICAR_OUT="$TMP_DIR/publicar.out.json"
require_status PATCH "$BASE_URL/api/resoluciones/$RESOLUCION_ID/publicar" 200 "" "$PUBLICAR_OUT"
ESTADO=$(jq -r '.estado // empty' "$PUBLICAR_OUT")
[[ "$ESTADO" == "PUBLICADA" ]] || fail "estado esperado PUBLICADA, llego $ESTADO"
ok "resolucion publicada"

echo "7) Adjuntar URL de documento oficial..."
DOC_BODY="$TMP_DIR/doc.json"
cat > "$DOC_BODY" <<JSON
{
  "urlDocumentoOficial": "https://indotel.local/resoluciones/$NUMERO_RESOLUCION.pdf"
}
JSON
DOC_OUT="$TMP_DIR/doc.out.json"
require_status POST "$BASE_URL/api/resoluciones/$RESOLUCION_ID/documento" 200 "$DOC_BODY" "$DOC_OUT"
URL_DOC=$(jq -r '.urlDocumentoOficial // empty' "$DOC_OUT")
[[ -n "$URL_DOC" ]] || fail "no se adjunto URL de documento oficial"
ok "documento oficial adjuntado"

echo "8) Consultar resolucion por ID..."
GET_OUT="$TMP_DIR/get.out.json"
require_status GET "$BASE_URL/api/resoluciones/$RESOLUCION_ID" 200 "" "$GET_OUT"
GET_ID=$(jq -r '.id // empty' "$GET_OUT")
[[ "$GET_ID" == "$RESOLUCION_ID" ]] || fail "GET por ID no devolvio la resolucion esperada"
ok "GET /api/resoluciones/{id}"

echo "9) Consultar listado filtrado por PUBLICADA..."
LIST_OUT="$TMP_DIR/list.out.json"
require_status GET "$BASE_URL/api/resoluciones?estado=PUBLICADA&page=1&pageSize=20" 200 "" "$LIST_OUT"
FOUND=$(jq --argjson id "$RESOLUCION_ID" '[.data[] | select(.id == $id)] | length' "$LIST_OUT")
[[ "$FOUND" -ge 1 ]] || fail "resolucion publicada no aparece en listado"
ok "listado filtrado funcionando"

echo "10) Reporte de resoluciones..."
REP_OUT="$TMP_DIR/reporte.out.json"
require_status GET "$BASE_URL/api/reportes/resoluciones" 200 "" "$REP_OUT"
TOTAL_REP=$(jq -r '.total // 0' "$REP_OUT")
[[ "$TOTAL_REP" -ge 1 ]] || fail "reporte de resoluciones no incluye datos"
ok "GET /api/reportes/resoluciones"

echo "11) Verificar auditoria..."
for ACCION in CREAR_RESOLUCION_INSTITUCIONAL APROBAR_RESOLUCION_INSTITUCIONAL PUBLICAR_RESOLUCION_INSTITUCIONAL ADJUNTAR_DOCUMENTO_RESOLUCION; do
  AUD_OUT="$TMP_DIR/aud-$ACCION.json"
  require_status GET "$BASE_URL/api/auditorias?entidad=ResolucionInstitucional&accion=$ACCION&page=1&pageSize=20" 200 "" "$AUD_OUT"
  TOTAL=$(jq -r '.total // 0' "$AUD_OUT")
  [[ "$TOTAL" -ge 1 ]] || fail "auditoria no encontrada para $ACCION"
  ok "auditoria $ACCION encontrada"
done

echo "===================================="
echo "PRUEBA FASE 2A TERMINADA CORRECTAMENTE"
echo "RESOLUCIONES INSTITUCIONALES VALIDADAS"
echo "RESOLUCION_ID=$RESOLUCION_ID"
echo "NUMERO_RESOLUCION=$NUMERO_RESOLUCION"
echo "===================================="
