#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

if [[ -n "$(git status --porcelain)" ]]; then
  echo "El arbol de trabajo tiene cambios sin guardar. Confirme o descarte esos cambios antes de integrar." >&2
  exit 1
fi

CURRENT_BRANCH="$(git branch --show-current)"
if [[ "$CURRENT_BRANCH" != "integracion" ]]; then
  echo "Este script debe ejecutarse desde la rama integracion. Rama actual: $CURRENT_BRANCH" >&2
  exit 1
fi

git fetch origin core api-gateway web caja

git config user.name "INDOTEL Integration Bot"
git config user.email "integration@indotel.local"

resolve_conflicts() {
  local component="$1"
  local unresolved
  unresolved="$(git diff --name-only --diff-filter=U || true)"

  while IFS= read -r path; do
    [[ -z "$path" ]] && continue

    case "$component:$path" in
      web:INDOTEL.Web/*|web:INDOTEL.Web.Tests/*|web:.github/workflows/web-ci.yml|web:docs/SPRINT_3_WEB_CIUDADANO.md|web:scripts/validar_sprint3_web.sh)
        git checkout --theirs -- "$path"
        git add "$path"
        ;;
      caja:INDOTEL_CAJA\(REAL\)/*|caja:INDOTEL_CAJA.Tests/*|caja:.github/workflows/caja-ci.yml|caja:docs/SPRINT_4_CAJA.md|caja:scripts/validar_sprint4_caja.sh)
        git checkout --theirs -- "$path"
        git add "$path"
        ;;
      *:.gitignore|*:global.json|*:README.md)
        git checkout --ours -- "$path"
        git add "$path"
        ;;
      *)
        echo "Conflicto no reconocido: $path" >&2
        ;;
    esac
  done <<< "$unresolved"

  unresolved="$(git diff --name-only --diff-filter=U || true)"
  if [[ -n "$unresolved" ]]; then
    echo "Quedan conflictos que requieren revision:" >&2
    echo "$unresolved" >&2
    git merge --abort || true
    exit 1
  fi
}

merge_component() {
  local component="$1"
  local ref="origin/$component"

  if git merge-base --is-ancestor "$ref" HEAD; then
    echo "El componente $component ya esta integrado."
    return
  fi

  echo "==> Integrando $component"
  if ! git merge --no-ff --no-commit "$ref"; then
    resolve_conflicts "$component"
  fi

  git add -A
  git commit -m "Sprint 5: integra componente $component"
}

merge_component web
merge_component caja

cat > .gitignore <<'EOF'
# IDE
.vs/
**/.vs/
.vscode/
*.user
*.suo
*.userosscache
*.sln.docstates

# Compilacion y pruebas
**/bin/
**/obj/
**/TestResults/
**/coverage/
*.pdb
*.cache

# Dependencias restaurables
packages/
**/packages/

# Configuracion local y secretos
.env
.env.*
!.env.example
appsettings.Development.json
appsettings.*.local.json
docker.env
*.pfx
*.snk

# Datos persistentes
uploads/
.data-protection-keys/
backups/
artifacts/

# Logs y temporales
*.log
*.tmp
.DS_Store
Thumbs.db
*.rar
EOF

cat > global.json <<'EOF'
{
  "sdk": {
    "version": "8.0.128",
    "rollForward": "latestPatch",
    "allowPrerelease": false
  }
}
EOF

git add .gitignore global.json
if ! git diff --cached --quiet; then
  git commit -m "Sprint 5: consolida configuracion compartida"
fi

echo
echo "Integracion preparada. Componentes presentes:"
for path in core-indotel/Indotel.Core/Indotel.Core.csproj \
            api-gateway/Indotel.ApiGateway/Indotel.ApiGateway.csproj \
            INDOTEL.Web/INDOTEL.WEB.csproj \
            'INDOTEL_CAJA(REAL)/INDOTEL_CAJA(REAL).csproj'; do
  if [[ -f "$path" ]]; then
    echo "  OK  $path"
  else
    echo "  FALTA  $path" >&2
    exit 1
  fi
done

echo
echo "Sprint 5 preparado en la rama integracion. Ejecute scripts/validar_sprint5_integracion.sh."
