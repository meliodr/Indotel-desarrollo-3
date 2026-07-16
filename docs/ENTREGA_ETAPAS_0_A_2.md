# Entrega — Etapas 0 a 2

Fecha: 2026-07-16

## Control de versiones local

- Línea base: `aad3a4d08c13e91a8fdb6814631f9bafef904f97`
- Implementación funcional: `850a52d`
- Documentación de entrega: `9a0682c`
- Rama de respaldo en GitHub: `integracion-core-web-api-caja-offline`

## Artefacto

- Archivo: `INDOTEL_PROYECTO_ETAPAS_0_A_2.zip`
- SHA-256: `fe1c244865c484ac9462537ddf2950ec4ff012d01b8080b4004a0495d104b5c9`
- Archivos incluidos: 593
- Prueba de compresión: correcta

## Contenido

- Etapa 0: línea base Git y reglas de protección.
- Etapa 1: matriz de cumplimiento enlazada con la rúbrica.
- Etapa 2: cliente anónimo, sucursales y cajeros, inventario formal, borrado lógico, política de contraseñas y auditoría HTTP automática.

## Validación disponible

La validación estática se ejecuta con:

```bash
python scripts/validar_etapas_0_2.py
```

Resultado durante la preparación: 247 archivos C# y 240 rutas API revisadas, con cero errores estructurales.

La compilación y las pruebas reales deben ejecutarse con .NET 8:

```bash
dotnet restore core-indotel/Indotel.Core.Tests/Indotel.Core.Tests.csproj
dotnet build core-indotel/Indotel.Core/Indotel.Core.csproj -c Release -warnaserror
dotnet build core-indotel/Indotel.Core.Tests/Indotel.Core.Tests.csproj -c Release -warnaserror
dotnet test core-indotel/Indotel.Core.Tests/Indotel.Core.Tests.csproj -c Release --no-build
```

## Estado de GitHub

Los checkpoints, matriz, estado y manifiesto quedaron guardados en esta rama. El ZIP completo es la fuente consolidada de las modificaciones; el árbol completo aún debe copiarse a un clon local y confirmarse en GitHub, porque el conector no permite una carga masiva confiable de cientos de archivos.
