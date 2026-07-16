# Línea base — Etapas 0 a 2

Fecha: 2026-07-16

## Rama de integración

`integracion-core-web-api-caja-offline`

## Punto de restauración local

El proyecto consolidado fue inicializado como repositorio Git local antes de comenzar los requisitos finales.

- Commit: `aad3a4d08c13e91a8fdb6814631f9bafef904f97`
- Mensaje: `Checkpoint antes de completar requisitos finales`
- Componentes protegidos: Core, GUI del Core, API Gateway, Web, Caja WPF, Docker, scripts, activos y documentación.

## Regla de trabajo

1. No modificar `main` directamente.
2. Cada bloque funcional debe tener código, migración, endpoint, prueba y evidencia.
3. Los documentos financieros no se eliminan físicamente; se cancelan, anulan o desactivan de forma auditable.
4. Caja conserva WPF/.NET 8.
5. Web y Caja consumen el Gateway; no acceden a SQL Server.

## Restauración

Para volver a la línea base local:

```bash
git reset --hard aad3a4d08c13e91a8fdb6814631f9bafef904f97
```

Este comando destruye cambios posteriores no confirmados; debe usarse solamente como recuperación.
