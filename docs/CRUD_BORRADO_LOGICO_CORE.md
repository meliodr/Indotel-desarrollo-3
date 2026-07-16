# CRUD y borrado lógico en el Core

La rúbrica solicita CRUD para usuarios, perfiles, productos, clientes, servicios, cotizaciones, facturas y cuentas por cobrar. En un sistema financiero no es correcto eliminar físicamente documentos con valor histórico.

## Convención

| Recurso | Lectura/creación/actualización | Eliminación implementada |
|---|---|---|
| Usuarios | CRUD administrativo | `DELETE` desactiva el usuario y termina su asignación de sucursal. |
| Perfiles | CRUD administrativo | `DELETE` desactiva perfiles no base y sin usuarios activos. |
| Productos | CRUD comercial | `DELETE` desactiva; inventario se modifica mediante movimientos formales. |
| Clientes | CRUD comercial | `DELETE` desactiva; el cliente anónimo está protegido. |
| Servicios | CRUD comercial | `DELETE` desactiva y cierra vigencia. |
| Cotizaciones | CRUD documental | `DELETE` cambia a `CANCELADA` cuando el estado lo permite. |
| Facturas | Creación, consulta y ciclo financiero | `DELETE` genera una solicitud de anulación con doble autorización. |
| Cuentas por cobrar | Consulta y actualización por cobros | `DELETE` solo desactiva una cuenta sin pagos cuya factura ya fue anulada. |

## Razón

- Preserva auditoría y trazabilidad.
- Evita perder referencias de pagos, recibos e inventario.
- Permite explicar el cumplimiento del CRUD como eliminación lógica.
- Se alinea con el principio de no destruir información financiera.
