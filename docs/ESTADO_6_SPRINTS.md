# Estado de los seis sprints

Actualizado: 13 de julio de 2026.

| Sprint | Módulo | Estado |
|---|---|---|
| 1 | Core | Implementado y validado automáticamente: compilación Release, 20/20 pruebas y publicación aprobadas sobre `6704e0a`. Pruebas integradas finales pasan al Sprint 5. |
| 2 | API Gateway | Implementado y validado automáticamente: compilación Release, 14/14 pruebas y publicación aprobadas. Prueba de Core apagado incluida en Sprint 5 runtime. |
| 3 | Web ciudadano | Implementado y validado automáticamente: 0 advertencias, 0 errores, 12/12 pruebas y publicación aprobadas sobre `7cf27ac`. Flujos manuales pasan al Sprint 5. |
| 4 | Caja interna | Implementado. Validación Linux aprobada sobre `0197a6a`: restauración, configuración WinForms y 15/15 pruebas. Compilación/publicación `win-x64` requiere evidencia del workflow Windows. |
| 5 | Integración y seguridad | Implementación preparada en `integracion`: consolidación de ramas, 61 pruebas base, revisión de secretos/arquitectura, E2E ciudadano, IDOR, reclamación, refresh reutilizado y Core apagado. Ejecución local/CI pendiente. |
| 6 | Despliegue y defensa | Implementación preparada en `integracion`: Dockerfiles, Compose, HTTPS Caddy, variables externas, backup, restore, rollback, CI, manuales y guion. Build, smoke, restauración y demo final pendientes. |

## Regla de cierre

Ningún sprint se marca como cerrado únicamente por tener código. Requiere:

1. compilación o validación compatible con su plataforma;
2. pruebas automáticas;
3. publicación;
4. evidencia de escenarios normales y de falla;
5. documentación actualizada;
6. ausencia de secretos y artefactos generados en Git.

## Siguiente ejecución obligatoria

### Preparar integración

```bash
cd /home/jarry/Indotel-desarrollo-3
git fetch origin

git worktree add -B integracion \
  /home/jarry/indotel-prueba-integracion \
  origin/integracion

cd /home/jarry/indotel-prueba-integracion
bash scripts/preparar_sprint5_integracion.sh
git push origin integracion
```

### Validar Sprint 5

```bash
RUN_RUNTIME_TESTS=1 \
INDOTEL_SQL_PASSWORD='CLAVE_SQL_LOCAL' \
bash scripts/validar_sprint5_integracion.sh
```

### Validar Sprint 6

```bash
BUILD_IMAGES=1 RUN_RELEASE_SMOKE=1 \
bash scripts/validar_sprint6_despliegue.sh
```

### Evidencia Windows

Confirmar que `.github/workflows/integration-release-ci.yml` o `.github/workflows/caja-ci.yml` completa:

- build de Caja;
- 15 pruebas;
- publicación `win-x64`;
- artefacto `indotel-caja-win-x64`.

## Definición de proyecto terminado

El proyecto queda listo para entrega cuando:

- las cuatro capas están integradas en `integracion`;
- los jobs Linux, Windows y Docker están verdes;
- runtime E2E está aprobado;
- backup y restauración fueron probados;
- Caja fue ejecutada en Windows limpio;
- la demostración completa puede repetirse siguiendo los manuales.
