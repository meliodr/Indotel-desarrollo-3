# Sprint 5 — Integración, seguridad y pruebas finales

## Estado

**Implementación preparada en la rama `integracion`.**

El cierre requiere integrar las ramas `web` y `caja`, ejecutar la validación consolidada y conservar la salida como evidencia.

## Objetivo

Comprobar que Core, API Gateway, Web, Caja y SQL Server funcionan como un único sistema, incluyendo escenarios normales, controles de seguridad y caídas controladas.

## Componentes

```text
Web ciudadano ───────┐
                     ├──> API Gateway ───> Core ───> SQL Server
Caja interna ────────┘
```

Caja nunca accede directamente a SQL Server. Web y Caja consumen exclusivamente el Gateway.

## Preparación de la rama integrada

Desde el repositorio principal:

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

El script incorpora las ramas `web` y `caja`, conserva Core y Gateway y consolida `.gitignore` y `global.json`.

## Validación automática consolidada

```bash
cd /home/jarry/indotel-prueba-integracion
bash scripts/validar_sprint5_integracion.sh
```

La validación ejecuta:

1. Sprint 1 Core: compilación, 20 pruebas y publicación.
2. Sprint 2 Gateway: compilación, 14 pruebas y publicación.
3. Sprint 3 Web: compilación, 12 pruebas y publicación.
4. Sprint 4 Caja: 15 pruebas y validación WinForms en Linux; build completo en Windows CI.
5. Revisión de artefactos generados rastreados.
6. Revisión de secretos y claves privadas.
7. Confirmación de que Web y Caja no apuntan al puerto interno del Core.
8. Confirmación de que Caja no contiene acceso directo a SQL Server.
9. Confirmación de árbol Git limpio.

Total base esperado: **61 pruebas automáticas aprobadas**.

## Prueba integrada de ejecución

Requiere Docker, Docker Compose, .NET 8, curl y Python 3.

```bash
cd /home/jarry/indotel-prueba-integracion

RUN_RUNTIME_TESTS=1 \
INDOTEL_SQL_PASSWORD='CLAVE_SQL_LOCAL' \
bash scripts/validar_sprint5_integracion.sh
```

También puede ejecutarse directamente:

```bash
INDOTEL_SQL_PASSWORD='CLAVE_SQL_LOCAL' \
bash scripts/probar_sprint5_runtime.sh
```

La prueba levanta temporalmente Core, Gateway y Web y valida:

- Core conectado a SQL Server;
- readiness de base de datos;
- Gateway conectado al Core;
- Web disponible;
- página 404 controlada;
- registro de dos ciudadanos a través del Gateway;
- perfil ciudadano autenticado;
- bloqueo de acceso a datos de otro ciudadano;
- carga de catálogos;
- creación de reclamación;
- acceso del propietario a su reclamación;
- rechazo del segundo ciudadano sobre la reclamación ajena;
- rotación de refresh token;
- detección de reutilización de refresh token;
- Core apagado y Gateway respondiendo HTTP 503 con `correlationId`.

Los procesos iniciados por la prueba se cierran mediante `trap`. Los logs quedan en `/tmp/indotel-sprint5-runtime`.

## Matriz manual de roles

| Acción | Administrador | AnalistaDAU | Auditor | Prestadora | Ciudadano |
|---|---:|---:|---:|---:|---:|
| Acceder a Caja | Sí | Sí | Solo lectura | No | No |
| Acceder a Web ciudadano | No | No | No | No | Sí |
| Crear reclamación | Sí | Sí | No | No | Propia |
| Cambiar estado | Sí | Sí | No | Limitado por Core | No |
| Resolver y cerrar | Sí | Sí | No | No | No |
| Consultar reportes/auditoría | Sí | Según política | Sí | No | No |
| Consultar reclamación | Todas | Todas | Todas | Asignadas | Propias |

La interfaz oculta o deshabilita acciones no permitidas, pero el Core mantiene la autorización definitiva.

## Escenarios manuales pendientes

1. SQL Server apagado: `/api/health/db` debe responder 503.
2. Gateway apagado: Web y Caja deben mostrar error controlado.
3. Core apagado con Gateway activo: Gateway debe devolver 503.
4. Auditor en Caja: ninguna operación de escritura habilitada.
5. Ciudadano y Prestadora rechazados por Caja.
6. Carga de PDF, PNG y JPEG válidos.
7. Rechazo de archivo renombrado, extensión prohibida y archivo demasiado grande.
8. Dos operadores intentando la misma transición de estado.
9. Doble clic en formularios de escritura.
10. Flujo completo hasta cierre formal del expediente.
11. Logout con servicios disponibles y apagados.
12. Ejecución de Caja publicada en Windows limpio.

## Criterio de cierre

- 61 pruebas base aprobadas.
- Prueba runtime integrada aprobada.
- Compilación Windows de Caja aprobada.
- Matriz manual de roles aprobada.
- No existen secretos ni artefactos generados rastreados.
- Web y Caja utilizan únicamente el Gateway.
- Las fallas de Core, Gateway y SQL se presentan de forma controlada.
- La evidencia de ejecución queda guardada en la documentación final.
