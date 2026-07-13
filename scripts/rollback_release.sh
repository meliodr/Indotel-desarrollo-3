#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

PREVIOUS_TAG="${1:-}"
BACKUP_FILE="${2:-}"
ENV_FILE="${3:-deploy/.env.release}"
COMPOSE_FILE="deploy/docker-compose.release.yml"

fail() {
  printf '\nERROR: %s\n' "$1" >&2
  exit 1
}

[[ -n "$PREVIOUS_TAG" ]] || fail "Uso: bash scripts/rollback_release.sh TAG_ANTERIOR [respaldo.bak] [env_file]"
[[ -f "$ENV_FILE" ]] || fail "No existe $ENV_FILE."
[[ "${CONFIRM_ROLLBACK:-}" == "YES" ]] || fail "Defina CONFIRM_ROLLBACK=YES para autorizar el rollback."

for image in core gateway web; do
  docker image inspect "indotel/$image:$PREVIOUS_TAG" >/dev/null 2>&1 \
    || fail "No existe la imagen indotel/$image:$PREVIOUS_TAG en este servidor."
done

cp "$ENV_FILE" "$ENV_FILE.before-rollback-$(date -u +%Y%m%dT%H%M%SZ)"
python3 - "$ENV_FILE" "$PREVIOUS_TAG" <<'PY'
import pathlib, re, sys
path = pathlib.Path(sys.argv[1])
tag = sys.argv[2]
text = path.read_text(encoding='utf-8')
if re.search(r'^RELEASE_TAG=', text, flags=re.M):
    text = re.sub(r'^RELEASE_TAG=.*$', f'RELEASE_TAG={tag}', text, flags=re.M)
else:
    text = f'RELEASE_TAG={tag}\n' + text
path.write_text(text, encoding='utf-8')
PY

printf '\n==> Revirtiendo servicios a %s\n' "$PREVIOUS_TAG"
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d --no-build --remove-orphans

if [[ -n "$BACKUP_FILE" ]]; then
  [[ -f "$BACKUP_FILE" ]] || fail "No existe el respaldo $BACKUP_FILE."
  CONFIRM_RESTORE=YES bash scripts/restore_sqlserver.sh "$BACKUP_FILE" "$ENV_FILE"
fi

printf '\nRollback aplicado a la version %s. Valide health, login y flujo de reclamaciones.\n' "$PREVIOUS_TAG"
