#!/usr/bin/env bash
set -Eeuo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SOLUTION="$ROOT_DIR/core-indotel/Indotel.Core.sln"
CORE_PROJECT="$ROOT_DIR/core-indotel/Indotel.Core/Indotel.Core.csproj"
TEST_PROJECT="$ROOT_DIR/core-indotel/Indotel.Core.Tests/Indotel.Core.Tests.csproj"

printf '\n==> SDK de .NET\n'
dotnet --info

printf '\n==> Limpieza\n'
dotnet clean "$SOLUTION" --configuration Release

printf '\n==> Restauración\n'
dotnet restore "$SOLUTION"

printf '\n==> Compilación Release\n'
dotnet build "$SOLUTION" --configuration Release --no-restore --warnaserror

printf '\n==> Pruebas automáticas\n'
dotnet test "$TEST_PROJECT" \
  --configuration Release \
  --no-build \
  --logger "console;verbosity=normal" \
  --collect "XPlat Code Coverage"

printf '\n==> Publicación de comprobación\n'
PUBLISH_DIR="${TMPDIR:-/tmp}/indotel-core-sprint1-publish"
rm -rf "$PUBLISH_DIR"
dotnet publish "$CORE_PROJECT" \
  --configuration Release \
  --no-build \
  --output "$PUBLISH_DIR"

printf '\nSprint 1 validado: restauración, compilación, pruebas y publicación completadas.\n'
printf 'Salida publicada temporalmente en: %s\n' "$PUBLISH_DIR"
