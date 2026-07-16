# Matriz de cumplimiento — Proyecto final de ventas de bienes y servicios

Fuente: especificaciones del proyecto final entregadas por el docente.

Estados:

- **IMPLEMENTADO:** código y contrato disponibles.
- **PARCIAL:** existe una parte, pero falta evidencia o integración completa.
- **PENDIENTE:** corresponde a una etapa futura.
- **VALIDAR:** implementado, pendiente de compilación/prueba final.

## Requisitos generales

| Requisito | Módulo | Evidencia principal | Estado |
|---|---|---|---|
| Seguridad en todos los módulos | Todos | JWT, roles, autorización, cookies, políticas | PARCIAL |
| Logs para auditoría | Core/Gateway | `Auditoria`, `AuditoriaHttp`, correlation ID | VALIDAR |
| Transacciones | Core | servicios financieros e idempotencia | PARCIAL |

## Core Tienda

| Requisito | Endpoint/archivo | Prueba/evidencia | Estado |
|---|---|---|---|
| Usuario y clave | `/api/auth/login` | bloqueo e hash de claves | IMPLEMENTADO |
| Complejidad de clave | `PasswordPolicyService` | `CoreRequirementsStage2Tests` | VALIDAR |
| Roles de consulta, mantenimiento y administración | `AuthorizationPolicies`, roles seed | pruebas de autorización pendientes | PARCIAL |
| Cliente anónimo | `/api/clientes/anonimo` | índice único y seed `CF-0001` | VALIDAR |
| Guardar todas las peticiones y respuestas | `HttpAuditMiddleware`, `/api/auditorias/http` | prueba de integración pendiente | VALIDAR |
| Rastro de acciones de usuarios | `AuditoriaService` | tablas `Auditorias` y `AuditoriasHttp` | IMPLEMENTADO |
| CRUD usuarios | `/api/usuarios` | borrado lógico documentado | VALIDAR |
| CRUD perfiles | `/api/perfiles` | borrado lógico y perfiles base protegidos | VALIDAR |
| CRUD productos | `/api/productos` | borrado lógico | VALIDAR |
| CRUD clientes | `/api/clientes` | borrado lógico; anónimo protegido | VALIDAR |
| CRUD servicios | `/api/servicios-cobrables` | cierre de vigencia lógico | VALIDAR |
| CRUD cotizaciones | `/api/cotizaciones` | cancelación lógica | VALIDAR |
| CRUD facturas | `/api/facturas` | anulación mediante doble aprobación | VALIDAR |
| CRUD cuentas por cobrar | `/api/cuentas-por-cobrar` | baja lógica restringida | VALIDAR |
| Pagar cuenta | `/api/caja-operaciones/cobrar` y pagos/cobros | pruebas financieras existentes | IMPLEMENTADO |
| Aumentar mercancía | `/api/productos/{id}/entradas-inventario` | movimientos e idempotencia | VALIDAR |
| Historial de inventario | `/api/productos/{id}/movimientos` | `MovimientoInventario` | VALIDAR |
| Cajero asociado a sucursal | JWT y `UsuarioSucursal` | login bloquea cajero sin sucursal | VALIDAR |

## Aplicación Web

| Requisito | Evidencia | Estado |
|---|---|---|
| Validación de usuario y clave | controlador de autenticación | PARCIAL |
| Consulta productos y servicios | catálogo por Gateway/Core | PARCIAL |
| Completar orden | módulo Comercial | PARCIAL |
| Simular pago | módulo Comercial | PARCIAL |
| Revisar estado de orden | módulo Comercial | PARCIAL |

La validación integral del Web corresponde a una etapa posterior.

## Aplicación de Caja

| Requisito | Evidencia | Estado |
|---|---|---|
| Login del cajero | Caja WPF + Core | PARCIAL |
| Cajero pertenece a sucursal | Etapa 2 Core; adaptación WPF posterior | PARCIAL |
| Inicio del día y monto inicial | módulo Caja | PARCIAL |
| Entrada/salida de efectivo | módulo Caja | PARCIAL |
| Pago de productos o servicios | API financiera | PARCIAL |
| Imprimir recibo | WPF PrintDialog | PARCIAL |
| Operar offline | cola local actual | PARCIAL |
| Aplicar al reconectar | sincronización actual | PARCIAL |
| Cierre y cuadre | módulo Caja | PARCIAL |
| Consumir Integración | configuración Gateway | PARCIAL |

## Integración

| Requisito | Estado |
|---|---|
| Enlace entre Core, Caja y Web | PARCIAL |
| Exponer servicios a los canales | PARCIAL |
| Responder con catálogo propio al caer Core | PENDIENTE |
| Recibir transacciones con Core caído | PENDIENTE |
| Aplicarlas al reconectar | PENDIENTE |
| Registrar todas las peticiones/respuestas | PENDIENTE |
| Refrescar inventario local | PENDIENTE |
| Consumir integración externa | PENDIENTE |
| Publicar servicios para Caja y Web | PARCIAL |

## Regla para cambiar a CUMPLIDO

Un requisito solo se marca como cumplido cuando existe:

`Código + migración (si aplica) + endpoint/pantalla + prueba ejecutada + evidencia`.
