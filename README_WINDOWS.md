# INDOTEL — ejecución completa en Windows

Este paquete ejecuta **Core, API Gateway, Web, Caja WPF, Core WinForms, SQL LocalDB y pagos simulados en una sola computadora Windows**.

## Inicio

1. `01_PREPARAR_TODO_EN_WINDOWS.bat`
2. `02_INICIAR_TODO_EN_WINDOWS.bat`
3. `05_PROBAR_TODO_EN_WINDOWS.bat`
4. `04_DETENER_TODO_EN_WINDOWS.bat`

No requiere Ubuntu ni Docker Desktop.

Consulte `docs/EJECUCION_100_POR_CIENTO_WINDOWS.md` para el detalle.

## Correcciones funcionales V4

La entrega V4 corrige instancias duplicadas de Caja/Core UI, retorno al login al cerrar sesión, menús por rol, USD persistente, seed externo configurable, navegación Web autenticada, creación de trámites y detalle de órdenes del autoservicio.

Los datos de demostración están en:

```text
core-indotel\Indotel.Core\Data\Seed\seed-development.json
```

Para compilar, ejecutar todas las pruebas y arrancar los módulos después de aplicar la corrección:

```text
08_APLICAR_Y_VALIDAR_CORRECCIONES_V4.bat
```

La evidencia se guarda en `VALIDACION_CORRECCIONES_V4.txt`.

La sincronización del seed es idempotente: no necesita borrar la base existente para agregar USD o completar los datos faltantes. Para reiniciar completamente el entorno de demostración use `ELIMINAR_DATOS_DEMO_WINDOWS.bat` y vuelva a ejecutar la preparación.
