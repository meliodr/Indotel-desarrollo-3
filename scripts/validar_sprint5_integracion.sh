#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

fail() {
  printf '\nERROR: %s\n' "$1" >&2
  exit 1
}

printf '\n==> Verificacion de rama y componentes\n'
CURRENT_BRANCH="$(git branch --show-current)"
[[ "$CURRENT_BRANCH" == "integracion" ]] || fail "Este script debe ejecutarse desde la rama integracion. Rama actual: $CURRENT_BRANCH"

COMPONENTS=(
  "core-indotel/Indotel.Core/Indotel.Core.csproj"
  "api-gateway/Indotel.ApiGateway/Indotel.ApiGateway.csproj"
  "INDOTEL.Web/INDOTEL.WEB.csproj"
  "INDOTEL_CAJA(REAL)/INDOTEL_CAJA(REAL).csproj"
)

for component in "${COMPONENTS[@]}"; do
  [[ -f "$component" ]] || fail "Falta el componente $component. Ejecute scripts/preparar_sprint5_integracion.sh"
  printf 'OK  %s\n' "$component"
done

printf '\n==> Validacion automatica del Core\n'
bash scripts/validar_sprint1_core.sh

printf '\n==> Validacion automatica del API Gateway\n'
bash scripts/validar_sprint2_gateway.sh

printf '\n==> Validacion automatica de la Web\n'
bash scripts/validar_sprint3_web.sh

printf '\n==> Validacion automatica de Caja\n'
bash scripts/validar_sprint4_caja.sh

printf '\n==> Revision de archivos generados rastreados\n'
TRACKED_GENERATED="$(git ls-files | grep -E '(^|/)(bin|obj|TestResults|coverage|packages|\.vs)/' || true)"
[[ -z "$TRACKED_GENERATED" ]] || {
  printf '%s\n' "$TRACKED_GENERATED" >&2
  fail "Hay artefactos generados rastreados por Git."
}

printf '\n==> Revision de secretos y configuracion local rastreada\n'
TRACKED_SECRETS="$(git ls-files | grep -E '(^|/)(\.env|docker\.env|appsettings\.Development\.json)$|\.(pfx|p12|key)$' || true)"
[[ -z "$TRACKED_SECRETS" ]] || {
  printf '%s\n' "$TRACKED_SECRETS" >&2
  fail "Hay configuracion local o secretos rastreados por Git."
}

PRIVATE_KEYS="$(git grep -n -E 'BEGIN (RSA |EC |OPENSSH )?PRIVATE KEY' -- . ':!docs/**' || true)"
[[ -z "$PRIVATE_KEYS" ]] || {
  printf '%s\n' "$PRIVATE_KEYS" >&2
  fail "Se detecto una clave privada en archivos rastreados."
}

printf '\n==> Revision de arquitectura de comunicaciones\n'
DIRECT_CORE_WEB="$(grep -R -n --include='*.cs' --include='*.json' --include='*.config' 'localhost:5085' INDOTEL.Web 2>/dev/null || true)"
[[ -z "$DIRECT_CORE_WEB" ]] || {
  printf '%s\n' "$DIRECT_CORE_WEB" >&2
  fail "La Web contiene referencias directas al puerto interno del Core."
}

DIRECT_CORE_CAJA="$(grep -R -n --include='*.cs' --include='*.json' --include='*.config' 'localhost:5085' 'INDOTEL_CAJA(REAL)' 2>/dev/null || true)"
[[ -z "$DIRECT_CORE_CAJA" ]] || {
  printf '%s\n' "$DIRECT_CORE_CAJA" >&2
  fail "Caja contiene referencias directas al puerto interno del Core."
}

grep -R -q --include='*.json' --include='*.cs' 'localhost:5185' INDOTEL.Web \
  || fail "No se encontro la URL local del Gateway en la configuracion Web."
grep -R -q --include='*.config' --include='*.cs' 'localhost:5185' 'INDOTEL_CAJA(REAL)' \
  || fail "No se encontro la URL local del Gateway en la configuracion de Caja."

DIRECT_SQL_CAJA="$(grep -R -n -E --include='*.cs' --include='*.config' 'SqlConnection|Initial Catalog=|Server=.*Database=' 'INDOTEL_CAJA(REAL)' 2>/dev/null || true)"
[[ -z "$DIRECT_SQL_CAJA" ]] || {
  printf '%s\n' "$DIRECT_SQL_CAJA" >&2
  fail "Caja contiene acceso directo a SQL Server."
}

printf '\n==> Revision de cambios sin guardar\n'
if [[ -n "$(git status --porcelain)" ]]; then
  git status --short
  fail "La validacion genero o encontro cambios sin guardar."
fi

if [[ "${RUN_RUNTIME_TESTS:-0}" == "1" ]]; then
  printf '\n==> Pruebas integradas de ejecucion\n'
  bash scripts/probar_sprint5_runtime.sh
else
  cat <<'EOF'

Las pruebas de compilacion, unitarias, arquitectura y secretos fueron aprobadas.
Para ejecutar tambien Web -> Gateway -> Core -> SQL y las pruebas de falla:

  RUN_RUNTIME_TESTS=1 INDOTEL_SQL_PASSWORD='***' bash scripts/validar_sprint5_integracion.sh
EOF
fi

printf '\nSprint 5 validado automaticamente: 61 pruebas base, arquitectura y revision de repositorio completadas.\n'
