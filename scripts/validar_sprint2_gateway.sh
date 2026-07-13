#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
GATEWAY_PROJECT="$ROOT_DIR/api-gateway/Indotel.ApiGateway/Indotel.ApiGateway.csproj"
TEST_PROJECT="$ROOT_DIR/api-gateway/Indotel.ApiGateway.Tests/Indotel.ApiGateway.Tests.csproj"
PUBLISH_DIR="/tmp/indotel-api-gateway-sprint2"

printf '\n==> SDK de .NET\n'
dotnet --info

printf '\n==> Limpieza\n'
dotnet clean "$GATEWAY_PROJECT" --configuration Release

printf '\n==> Restauracion\n'
dotnet restore "$GATEWAY_PROJECT"
dotnet restore "$TEST_PROJECT"

printf '\n==> Compilacion Release\n'
dotnet build "$GATEWAY_PROJECT" --configuration Release --no-restore
dotnet build "$TEST_PROJECT" --configuration Release --no-restore

printf '\n==> Pruebas automaticas\n'
dotnet test "$TEST_PROJECT" --configuration Release --no-build --collect:"XPlat Code Coverage"

printf '\n==> Publicacion de comprobacion\n'
rm -rf "$PUBLISH_DIR"
dotnet publish "$GATEWAY_PROJECT" --configuration Release --no-restore --output "$PUBLISH_DIR"

printf '\nSprint 2 validado: restauracion, compilacion, pruebas y publicacion completadas.\n'
printf 'Salida publicada temporalmente en: %s\n' "$PUBLISH_DIR"
