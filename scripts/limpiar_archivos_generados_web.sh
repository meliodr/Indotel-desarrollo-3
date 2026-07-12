#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(git rev-parse --show-toplevel)"
cd "$REPO_ROOT"

git rm -r --cached --ignore-unmatch .vs
git rm -r --cached --ignore-unmatch INDOTEL.Web/bin INDOTEL.Web/obj
git rm --cached --ignore-unmatch INDOTEL.Web/INDOTEL.WEB.csproj.user

echo
echo "Archivos generados retirados del índice de Git."
echo "Revise el resultado con: git status"
echo "Luego confirme y publique el cambio con git commit y git push."
