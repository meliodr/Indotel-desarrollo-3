# Estado de los seis sprints

Actualizado: 13 de julio de 2026.

| Sprint | Módulo | Estado |
|---|---|---|
| 1 | Core | Implementado y validado automáticamente: compilación Release, 20/20 pruebas y publicación aprobadas. Integración manual final pendiente para Sprint 5. |
| 2 | API/Gateway | Implementado en `api-gateway`. Pendiente ejecutar `scripts/validar_sprint2_gateway.sh` y prueba integrada con Core encendido y apagado. |
| 3 | Web ciudadano | Implementado en `web`. Pendiente ejecutar `scripts/validar_sprint3_web.sh` y flujo Web → Gateway → Core → SQL. |
| 4 | Caja interna | Pendiente. |
| 5 | Integración y seguridad | Pendiente. |
| 6 | Despliegue, documentación y defensa | Pendiente. |

## Regla

Ningún sprint se marca como cerrado únicamente por tener código. Requiere compilación, pruebas automáticas, publicación, evidencia de escenarios normales y de falla, y documentación actualizada.

## Próximas validaciones

### Sprint 2

```bash
cd /home/jarry/indotel-prueba-api-gateway
git fetch origin
git reset --hard origin/api-gateway
bash scripts/validar_sprint2_gateway.sh
```

### Sprint 3

```bash
cd /home/jarry/indotel-prueba-web
git fetch origin
git reset --hard origin/web
bash scripts/validar_sprint3_web.sh
```
