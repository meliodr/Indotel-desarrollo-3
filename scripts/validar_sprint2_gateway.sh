#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
GATEWAY_DIR="$ROOT_DIR/api-gateway/Indotel.ApiGateway"
TEST_DIR="$ROOT_DIR/api-gateway/Indotel.ApiGateway.Tests"
GATEWAY_PROJECT="$GATEWAY_DIR/Indotel.ApiGateway.csproj"
TEST_PROJECT="$TEST_DIR/Indotel.ApiGateway.Tests.csproj"
PUBLISH_DIR="/tmp/indotel-api-gateway-sprint2"

printf '\n==> SDK de .NET\n'
dotnet --info

printf '\n==> Limpieza fisica de artefactos\n'
rm -rf \
  "$GATEWAY_DIR/bin" \
  "$GATEWAY_DIR/obj" \
  "$TEST_DIR/bin" \
  "$TEST_DIR/obj" \
  "$TEST_DIR/TestResults" \
  "$PUBLISH_DIR"

printf '\n==> Restauracion\n'
dotnet restore "$GATEWAY_PROJECT"
dotnet restore "$TEST_PROJECT"

printf '\n==> Compilacion Release\n'
dotnet build "$GATEWAY_PROJECT" --configuration Release --no-restore
dotnet build "$TEST_PROJECT" --configuration Release --no-restore

printf '\n==> Pruebas automaticas\n'
dotnet test "$TEST_PROJECT" --configuration Release --no-build --collect:"XPlat Code Coverage"

printf '\n==> Publicacion de comprobacion\n'
dotnet publish "$GATEWAY_PROJECT" --configuration Release --no-restore --output "$PUBLISH_DIR"

printf '\nSprint 2 validado: restauracion, compilacion, pruebas y publicacion completadas.\n'
printf 'Salida publicada temporalmente en: %s\n' "$PUBLISH_DIR"
