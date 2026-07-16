# Estado de implementación — Etapas 0 a 2

## Etapa 0 — Protección del trabajo

- Línea base local creada y confirmada en Git.
- Rama de respaldo definida: `integracion-core-web-api-caja-offline`.
- Proyecto consolidado utilizado como única fuente de trabajo.
- Se mantiene Caja en WPF/.NET 8.

## Etapa 1 — Matriz de cumplimiento

- Creada `MATRIZ_CUMPLIMIENTO_PROYECTO_FINAL.md`.
- Cada requisito se vincula con módulo, ruta, evidencia y estado.
- Los requisitos de Integración, Web y Caja que pertenecen a etapas posteriores permanecen marcados como pendientes o parciales.

## Etapa 2 — Core

### Cliente anónimo

- Campo `ClienteComercial.EsAnonimo`.
- Índice filtrado único para impedir más de un consumidor final.
- Registro de sistema `CF-0001 — Consumidor Final`.
- Endpoint `GET /api/clientes/anonimo`.
- Protección contra edición, desactivación o eliminación accidental.

### Sucursales y cajeros

- Entidades `Sucursal` y `UsuarioSucursal`.
- Cada usuario puede tener una sola asignación activa.
- `Caja.SucursalId` obligatorio.
- Claims JWT: `sucursalId`, `sucursalCodigo`, `sucursalNombre`.
- Un cajero o supervisor de caja sin sucursal no puede iniciar sesión.
- La Caja solo puede operar cajas pertenecientes a la sucursal del usuario.
- CRUD de sucursales y asignación administrativa de usuarios.

### Inventario formal

- Entidad `MovimientoInventario`.
- Tipos usados: `INICIAL`, `ENTRADA`, `AJUSTE`, `VENTA`, `ANULACION`.
- Operación cliente única para idempotencia.
- Endpoints de entrada, ajuste e historial.
- Las emisiones y anulaciones de facturas generan movimientos.
- Se bloquea la edición directa de existencia.

### Seguridad de contraseñas

- Servicio único `PasswordPolicyService`.
- Aplicado al registro, creación administrativa, cambio y restablecimiento.
- Requiere longitud, mayúscula, minúscula, número, símbolo y rechaza claves comunes.

### Auditoría HTTP

- Entidad `AuditoriaHttp`.
- Middleware automático para método, ruta, usuario, rol, sucursal, IP, tiempo, estado, petición, respuesta y correlation ID.
- Redacción de claves, tokens, cookies, secretos y datos de tarjetas.
- Endpoint administrativo `GET /api/auditorias/http`.

### CRUD y eliminación lógica

- Se añadieron eliminaciones lógicas o anulaciones controladas.
- La convención está documentada en `CRUD_BORRADO_LOGICO_CORE.md`.

### Base de datos

- Migración: `20260716230000_Etapas0A2RequisitosCore`.
- Snapshot actualizado.
- Seed de sucursal central, consumidor final y asignación del cajero de demostración.

### Pruebas añadidas

`CoreRequirementsStage2Tests.cs` cubre política de contraseñas, entidades nuevas, cliente anónimo único, asignación activa de sucursal, caja vinculada a sucursal, idempotencia de inventario y concurrencia.

## Validación pendiente

La validación estática terminó con cero errores estructurales. La compilación y ejecución de las pruebas deben realizarse con .NET 8. La GUI del Core y Caja requieren Windows Desktop SDK para su validación visual.
