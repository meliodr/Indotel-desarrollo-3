#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CAJA_PROJECT="$ROOT_DIR/INDOTEL_CAJA(REAL)/INDOTEL_CAJA(REAL).csproj"
TEST_PROJECT="$ROOT_DIR/INDOTEL_CAJA.Tests/INDOTEL_CAJA.Tests.csproj"
PUBLISH_DIR="/tmp/indotel-caja-sprint4-win-x64"

printf '\n==> SDK de .NET\n'
dotnet --info

printf '\n==> Limpieza fisica de artefactos\n'
find "$ROOT_DIR/INDOTEL_CAJA(REAL)" "$ROOT_DIR/INDOTEL_CAJA.Tests" \
  -type d \( -name bin -o -name obj -o -name TestResults \) \
  -prune -exec rm -rf {} + 2>/dev/null || true

printf '\n==> Restauracion\n'
dotnet restore "$CAJA_PROJECT"
dotnet restore "$TEST_PROJECT"

printf '\n==> Pruebas automaticas de infraestructura\n'
dotnet test "$TEST_PROJECT" \
  --configuration Release \
  --no-restore \
  --collect:"XPlat Code Coverage"

SDK_BASE_PATH="$(dotnet --info | sed -n 's/^[[:space:]]*Base Path:[[:space:]]*//p' | head -n 1)"
WINDOWS_DESKTOP_TARGETS="${SDK_BASE_PATH%/}/Sdks/Microsoft.NET.Sdk.WindowsDesktop/targets/Microsoft.NET.Sdk.WindowsDesktop.targets"
HOST_OS="$(uname -s 2>/dev/null || printf 'Unknown')"

CAN_BUILD_WINDOWS_DESKTOP=false
case "$HOST_OS" in
  MINGW*|MSYS*|CYGWIN*) CAN_BUILD_WINDOWS_DESKTOP=true ;;
esac

if [[ -f "$WINDOWS_DESKTOP_TARGETS" ]]; then
  CAN_BUILD_WINDOWS_DESKTOP=true
fi

if [[ "$CAN_BUILD_WINDOWS_DESKTOP" == "true" ]]; then
  printf '\n==> Compilacion Release de Caja\n'
  dotnet build "$CAJA_PROJECT" --configuration Release --no-restore

  printf '\n==> Publicacion win-x64 de comprobacion\n'
  rm -rf "$PUBLISH_DIR"
  dotnet publish "$CAJA_PROJECT" \
    --configuration Release \
    --runtime win-x64 \
    --self-contained false \
    --no-restore \
    --output "$PUBLISH_DIR"

  printf '\nSprint 4 validado: restauracion, compilacion, pruebas y publicacion completadas.\n'
  printf 'Caja requiere Windows para su ejecucion visual. Artefacto temporal: %s\n' "$PUBLISH_DIR"
  exit 0
fi

printf '\n==> Verificacion de configuracion WinForms\n'
grep -q '<TargetFramework>net8.0-windows</TargetFramework>' "$CAJA_PROJECT"
grep -q '<UseWindowsForms>true</UseWindowsForms>' "$CAJA_PROJECT"
grep -q '<EnableWindowsTargeting>true</EnableWindowsTargeting>' "$CAJA_PROJECT"

cat <<'EOF'

Validacion local Linux completada:
- restauracion de Caja: correcta;
- pruebas de infraestructura: correctas;
- configuracion net8.0-windows/WinForms: correcta.

Este SDK de Linux no contiene Microsoft.NET.Sdk.WindowsDesktop, por lo que no puede
compilar ni publicar WinForms en este equipo. La compilacion y publicacion completas
se ejecutan en GitHub Actions con windows-latest mediante el workflow Caja CI.
EOF
