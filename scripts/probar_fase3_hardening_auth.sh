#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:5085}"
ADMIN_EMAIL="${ADMIN_EMAIL:-admin@indotel.test}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:?Debe definir ADMIN_PASSWORD}"

TMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TMP_DIR"' EXIT

ok() { echo "OK: $1"; }
fail() { echo "ERROR: $1" >&2; exit 1; }

echo "===================================="
echo "PRUEBA FASE 3 - HARDENING AUTH"
echo "===================================="

echo "1) Health check..."
curl -sS "$BASE_URL/api/health" | jq . >/dev/null
ok "GET /api/health"

echo "2) Login admin y validar refresh token..."
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
REFRESH_TOKEN=$(jq -r '.refreshToken // empty' "$LOGIN_OUT")
[[ -n "$TOKEN" && "$TOKEN" != "null" ]] || fail "no se recibio token"
[[ -n "$REFRESH_TOKEN" && "$REFRESH_TOKEN" != "null" ]] || fail "no se recibio refresh token"
ok "login devuelve access token y refresh token"

echo "3) Usar refresh token..."
REFRESH_BODY="$TMP_DIR/refresh.json"
cat > "$REFRESH_BODY" <<JSON
{
  "refreshToken": "$REFRESH_TOKEN"
}
JSON
REFRESH_OUT="$TMP_DIR/refresh.out.json"
STATUS=$(curl -sS -o "$REFRESH_OUT" -w "%{http_code}" -X POST "$BASE_URL/api/auth/refresh-token" \
  -H "Content-Type: application/json" \
  --data-binary "@$REFRESH_BODY")
[[ "$STATUS" == "200" ]] || { cat "$REFRESH_OUT" >&2; fail "refresh-token devolvio $STATUS"; }
TOKEN_2=$(jq -r '.token // empty' "$REFRESH_OUT")
REFRESH_TOKEN_2=$(jq -r '.refreshToken // empty' "$REFRESH_OUT")
[[ -n "$TOKEN_2" && "$TOKEN_2" != "null" ]] || fail "refresh no devolvio access token"
[[ -n "$REFRESH_TOKEN_2" && "$REFRESH_TOKEN_2" != "null" ]] || fail "refresh no devolvio nuevo refresh token"
ok "refresh token renovado"

echo "4) Reusar refresh token anterior debe fallar..."
REUSE_OUT="$TMP_DIR/reuse.out.json"
STATUS=$(curl -sS -o "$REUSE_OUT" -w "%{http_code}" -X POST "$BASE_URL/api/auth/refresh-token" \
  -H "Content-Type: application/json" \
  --data-binary "@$REFRESH_BODY")
[[ "$STATUS" == "401" ]] || { cat "$REUSE_OUT" >&2; fail "refresh token viejo esperaba 401 y devolvio $STATUS"; }
ok "refresh token anterior revocado -> 401"

echo "5) Logout revoca refresh token nuevo..."
LOGOUT_BODY="$TMP_DIR/logout.json"
cat > "$LOGOUT_BODY" <<JSON
{
  "refreshToken": "$REFRESH_TOKEN_2"
}
JSON
LOGOUT_OUT="$TMP_DIR/logout.out.json"
STATUS=$(curl -sS -o "$LOGOUT_OUT" -w "%{http_code}" -X POST "$BASE_URL/api/auth/logout" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN_2" \
  --data-binary "@$LOGOUT_BODY")
[[ "$STATUS" == "200" ]] || { cat "$LOGOUT_OUT" >&2; fail "logout devolvio $STATUS"; }
ok "logout correcto"

REVOKED_BODY="$TMP_DIR/revoked.json"
cat > "$REVOKED_BODY" <<JSON
{
  "refreshToken": "$REFRESH_TOKEN_2"
}
JSON
REVOKED_OUT="$TMP_DIR/revoked.out.json"
STATUS=$(curl -sS -o "$REVOKED_OUT" -w "%{http_code}" -X POST "$BASE_URL/api/auth/refresh-token" \
  -H "Content-Type: application/json" \
  --data-binary "@$REVOKED_BODY")
[[ "$STATUS" == "401" ]] || { cat "$REVOKED_OUT" >&2; fail "refresh revocado esperaba 401 y devolvio $STATUS"; }
ok "refresh token revocado por logout -> 401"

echo "6) Registrar ciudadano temporal para probar bloqueo por intentos fallidos..."
STAMP=$(date +%s%N)
TEST_EMAIL="lockout-$STAMP@test.local"
REGISTER_BODY="$TMP_DIR/register.json"
cat > "$REGISTER_BODY" <<JSON
{
  "cedula": "9${STAMP:0:10}",
  "nombres": "Usuario",
  "apellidos": "Lockout",
  "telefono": "8090000000",
  "correo": "$TEST_EMAIL",
  "direccion": "Demo",
  "password": "Ciudadano123*"
}
JSON
REGISTER_OUT="$TMP_DIR/register.out.json"
STATUS=$(curl -sS -o "$REGISTER_OUT" -w "%{http_code}" -X POST "$BASE_URL/api/auth/register-ciudadano" \
  -H "Content-Type: application/json" \
  --data-binary "@$REGISTER_BODY")
[[ "$STATUS" == "200" ]] || { cat "$REGISTER_OUT" >&2; fail "registro ciudadano devolvio $STATUS"; }
ok "ciudadano temporal registrado"

echo "7) Forzar 5 intentos fallidos y esperar bloqueo 423..."
BAD_BODY="$TMP_DIR/bad-login.json"
cat > "$BAD_BODY" <<JSON
{
  "correo": "$TEST_EMAIL",
  "password": "ClaveIncorrecta123*"
}
JSON
for i in 1 2 3 4; do
  BAD_OUT="$TMP_DIR/bad-$i.json"
  STATUS=$(curl -sS -o "$BAD_OUT" -w "%{http_code}" -X POST "$BASE_URL/api/auth/login" \
    -H "Content-Type: application/json" \
    --data-binary "@$BAD_BODY")
  [[ "$STATUS" == "401" ]] || { cat "$BAD_OUT" >&2; fail "intento $i esperaba 401 y devolvio $STATUS"; }
  ok "intento fallido $i -> 401"
done

BAD_OUT_5="$TMP_DIR/bad-5.json"
STATUS=$(curl -sS -o "$BAD_OUT_5" -w "%{http_code}" -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  --data-binary "@$BAD_BODY")
[[ "$STATUS" == "423" ]] || { cat "$BAD_OUT_5" >&2; fail "intento 5 esperaba 423 y devolvio $STATUS"; }
ok "quinto intento bloquea usuario -> 423"

GOOD_BODY="$TMP_DIR/good-locked.json"
cat > "$GOOD_BODY" <<JSON
{
  "correo": "$TEST_EMAIL",
  "password": "Ciudadano123*"
}
JSON
GOOD_OUT="$TMP_DIR/good-locked.out.json"
STATUS=$(curl -sS -o "$GOOD_OUT" -w "%{http_code}" -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  --data-binary "@$GOOD_BODY")
[[ "$STATUS" == "423" ]] || { cat "$GOOD_OUT" >&2; fail "login correcto bloqueado esperaba 423 y devolvio $STATUS"; }
ok "usuario bloqueado no puede entrar aunque use clave correcta"

echo "===================================="
echo "PRUEBA FASE 3 TERMINADA CORRECTAMENTE"
echo "REFRESH TOKEN LOGOUT LOCKOUT RATE LIMIT CONFIGURADO"
echo "===================================="
