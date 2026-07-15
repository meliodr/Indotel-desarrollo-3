# Checklist maestro de trabajo — Core INDOTEL y GUI operativa

Rama: `proyecto-revisado-funcionando`

Fecha inicial: 15/07/2026

Estado general: planificación. Las casillas se marcarán únicamente cuando exista código, prueba y evidencia.

## Convenciones

- `[ ]` pendiente.
- `[x]` completado y verificado.
- `P0` obligatorio antes de programar módulos nuevos.
- `P1` imprescindible para la entrega.
- `P2` importante para completar el flujo comercial.
- `P3` endurecimiento, defensa y evolución.

---

# 0. Reglas que no podemos violar

- [ ] **P0** No eliminar rutas actuales del Core.
- [ ] **P0** No renombrar rutas actuales.
- [ ] **P0** No cambiar la forma de las respuestas consumidas actualmente.
- [ ] **P0** No cambiar DTOs actuales sin una versión nueva y pruebas de compatibilidad.
- [ ] **P0** No cambiar los nombres de los roles existentes.
- [ ] **P0** No renombrar tablas o columnas actuales.
- [ ] **P0** No ejecutar migraciones destructivas.
- [ ] **P0** No reutilizar `/api/servicios` para servicios cobrables; esa ruta pertenece a `ServicioTelecom`.
- [ ] **P0** No conectar la nueva GUI directamente a SQL Server.
- [ ] **P0** No permitir que la GUI use `IndotelDbContext`.
- [ ] **P0** No modificar Caja durante este trabajo.
- [ ] **P0** No modificar la Web ciudadana durante este trabajo.
- [ ] **P0** No modificar Integración o Gateway durante este trabajo, salvo documentación de contratos futuros.
- [ ] **P0** No borrar físicamente usuarios, perfiles, facturas, pagos, cobros, recibos o cuentas por cobrar.
- [ ] **P0** No publicar tarifas como oficiales sin una fuente o resolución institucional validada.
- [ ] **P0** No afirmar que el sistema está listo para producción gubernamental real.

---

# 1. Línea base y protección del proyecto

- [ ] **P0** Registrar el commit exacto que servirá como línea base.
- [ ] **P0** Confirmar que la rama de trabajo parte de la versión consolidada correcta.
- [ ] **P0** Restaurar dependencias del Core.
- [ ] **P0** Compilar `Indotel.Core` en Debug.
- [ ] **P0** Compilar `Indotel.Core` en Release.
- [ ] **P0** Ejecutar las 20 pruebas existentes del Core.
- [ ] **P0** Guardar evidencia de las pruebas iniciales.
- [ ] **P0** Exportar el OpenAPI actual antes de agregar endpoints.
- [ ] **P0** Inventariar todas las rutas actuales.
- [ ] **P0** Identificar qué rutas consumen Caja, Web y Gateway.
- [ ] **P0** Respaldar la base de datos antes de cualquier migración.
- [ ] **P0** Confirmar los puertos actuales y evitar conflictos.
- [ ] **P0** Revisar que no existan secretos reales versionados.
- [ ] **P0** Confirmar `.gitignore` para `bin`, `obj`, `.vs`, bases locales, respaldos y secretos.
- [ ] **P0** Definir un procedimiento de rollback de código y base de datos.

## Criterio de salida

- [ ] El Core original compila, pasa sus pruebas y puede volver al estado inicial sin pérdida.

---

# 2. Arquitectura del Core ampliado

- [ ] **P1** Mantener el Core como API y fuente de verdad.
- [ ] **P1** Mantener la ampliación como monolito modular, sin reescribir todo el sistema.
- [ ] **P1** Crear carpetas o módulos separados para el dominio comercial.
- [ ] **P1** Separar controladores, DTOs, modelos, servicios y reglas del módulo comercial.
- [ ] **P1** Evitar que los nuevos controladores contengan cálculos financieros complejos.
- [ ] **P1** Crear servicios o casos de uso para operaciones comerciales.
- [ ] **P1** Crear transacciones explícitas para operaciones monetarias.
- [ ] **P1** No ampliar automáticamente el filtro transaccional actual a todas las rutas.
- [ ] **P1** Mantener el filtro existente de reclamaciones sin alterar su comportamiento.
- [ ] **P1** Definir contratos internos para auditoría, idempotencia y usuario actual.
- [ ] **P1** Preparar feature flags para activar módulos progresivamente.

Feature flags previstas:

```json
{
  "Features": {
    "CoreGui": true,
    "CommercialModule": false,
    "ApprovalWorkflow": false,
    "PaymentsModule": false
  }
}
```

---

# 3. Proyecto GUI obligatorio del Core

- [ ] **P1** Crear `core-indotel/Indotel.Core.Gui`.
- [ ] **P1** Agregar `Indotel.Core.Gui` a `Indotel.Core.sln`.
- [ ] **P1** Usar ASP.NET Core 8 MVC.
- [ ] **P1** Usar Razor Views.
- [ ] **P1** Configurar archivos estáticos.
- [ ] **P1** Configurar `IHttpClientFactory`.
- [ ] **P1** Configurar sesión del lado servidor.
- [ ] **P1** Configurar cookies `HttpOnly`, `Secure` y `SameSite`.
- [ ] **P1** Configurar protección antiforgery.
- [ ] **P1** Configurar manejo visual de `ProblemDetails`.
- [ ] **P1** Mostrar `correlationId` en errores y vistas de soporte.
- [ ] **P1** Ejecutar la GUI en un puerto independiente, inicialmente `http://localhost:5285`.
- [ ] **P1** Mantener el Core API en su puerto actual, inicialmente `http://localhost:5085`.
- [ ] **P1** Confirmar que detener la GUI no detiene el Core.
- [ ] **P1** Confirmar que detener el Core muestra un error controlado en la GUI.

## Estructura prevista

- [ ] Crear `Controllers/`.
- [ ] Crear `Clients/`.
- [ ] Crear `Models/ApiContracts/`.
- [ ] Crear `Models/ViewModels/`.
- [ ] Crear `Services/`.
- [ ] Crear `Views/`.
- [ ] Crear `ViewComponents/`.
- [ ] Crear `Filters/`.
- [ ] Crear `wwwroot/css/`.
- [ ] Crear `wwwroot/js/`.
- [ ] Crear `wwwroot/images/`.

---

# 4. Diseño visual de la GUI

- [ ] **P1** Crear identidad institucional propia; no copiar la imagen de un banco específico.
- [ ] **P1** Crear menú lateral.
- [ ] **P1** Crear encabezado superior.
- [ ] **P1** Mostrar nombre del usuario conectado.
- [ ] **P1** Mostrar rol del usuario.
- [ ] **P1** Mostrar ambiente: Development, Testing o Production.
- [ ] **P1** Agregar cierre de sesión visible.
- [ ] **P1** Crear dashboard con tarjetas de resumen.
- [ ] **P1** Crear tablas legibles, filtrables y paginadas.
- [ ] **P1** Crear etiquetas de estado con texto y color.
- [ ] **P1** Crear formularios divididos por secciones.
- [ ] **P1** Crear confirmaciones para acciones sensibles.
- [ ] **P1** Crear estados vacíos útiles.
- [ ] **P1** Crear pantalla de carga.
- [ ] **P1** Crear mensajes de éxito, advertencia y error.
- [ ] **P1** Crear vista de detalle con historial y auditoría.
- [ ] **P1** Hacer la interfaz responsive para escritorio y tablet.
- [ ] **P1** Revisar contraste, foco y navegación por teclado.
- [ ] **P2** Agregar iconografía consistente.
- [ ] **P2** Agregar impresión limpia para recibos y documentos.

## Navegación prevista

- [ ] Inicio.
- [ ] Operaciones pendientes.
- [ ] Usuarios.
- [ ] Perfiles.
- [ ] Clientes.
- [ ] Productos.
- [ ] Servicios cobrables.
- [ ] Órdenes.
- [ ] Cotizaciones.
- [ ] Facturas.
- [ ] Cuentas por cobrar.
- [ ] Pagos.
- [ ] Cobros y recibos.
- [ ] Auditoría.
- [ ] Salud del sistema.
- [ ] Configuración.

---

# 5. Autenticación y sesión de la GUI

- [ ] **P1** Crear pantalla de login.
- [ ] **P1** Validar correo o usuario.
- [ ] **P1** Validar contraseña.
- [ ] **P1** Consumir el endpoint real de autenticación del Core.
- [ ] **P1** Mostrar error de credenciales inválidas.
- [ ] **P1** Mostrar bloqueo temporal por intentos fallidos.
- [ ] **P1** Guardar JWT del lado servidor.
- [ ] **P1** Guardar refresh token del lado servidor.
- [ ] **P1** No guardar JWT en `localStorage`.
- [ ] **P1** No exponer refresh token al navegador.
- [ ] **P1** Renovar automáticamente el token cuando corresponda.
- [ ] **P1** Revocar refresh token al cerrar sesión.
- [ ] **P1** Agregar timeout por inactividad.
- [ ] **P1** Proteger todas las pantallas privadas.
- [ ] **P1** Enviar `X-Correlation-ID` en llamadas al Core.
- [ ] **P1** Mostrar menú según rol.
- [ ] **P1** Mantener validación definitiva de permisos en el Core.
- [ ] **P1** Probar sesiones de Administrador, AnalistaDAU y Auditor.
- [ ] **P2** Preparar perfiles comerciales y supervisores sin cambiar roles base hasta aprobar el diseño.

---

# 6. Dashboard del Core

- [ ] **P1** Crear endpoint o agregador seguro para resumen administrativo cuando sea necesario.
- [ ] **P1** Mostrar total de ciudadanos.
- [ ] **P1** Mostrar total de prestadoras.
- [ ] **P1** Mostrar reclamaciones abiertas.
- [ ] **P1** Mostrar reclamaciones vencidas.
- [ ] **P1** Mostrar resoluciones institucionales.
- [ ] **P1** Mostrar autorizaciones.
- [ ] **P1** Mostrar licencias próximas a vencer.
- [ ] **P1** Mostrar actividad reciente.
- [ ] **P1** Mostrar estado del Core.
- [ ] **P1** Mostrar estado de SQL Server.
- [ ] **P2** Mostrar cotizaciones pendientes cuando el módulo exista.
- [ ] **P2** Mostrar facturas pendientes y vencidas.
- [ ] **P2** Mostrar cuentas por cobrar.
- [ ] **P2** Mostrar pagos en conciliación.
- [ ] **P2** Mostrar cobros del día.
- [ ] **P2** Mostrar operaciones pendientes de aprobación.

---

# 7. Usuarios

- [ ] **P1** Reutilizar `/api/usuarios` sin cambiar sus contratos actuales.
- [ ] **P1** Crear listado de usuarios en la GUI.
- [ ] **P1** Agregar búsqueda.
- [ ] **P1** Agregar filtros por rol y estado.
- [ ] **P1** Crear vista de detalle.
- [ ] **P1** Crear usuario.
- [ ] **P1** Editar usuario.
- [ ] **P1** Activar usuario.
- [ ] **P1** Desactivar usuario.
- [ ] **P1** Cambiar clave administrativa.
- [ ] **P1** No mostrar `PasswordHash`.
- [ ] **P1** Auditar creación, edición, estado y cambio de clave.
- [ ] **P1** Impedir que un administrador se bloquee a sí mismo sin advertencia o control.
- [ ] **P2** Agregar paginación al endpoint si es necesario sin romper la respuesta actual; usar ruta nueva o parámetros compatibles.
- [ ] **P2** Agregar validación más fuerte de contraseñas de forma compatible.

---

# 8. Perfiles y roles

- [ ] **P1** Tratar `Perfil` como nombre funcional de `Rol`.
- [ ] **P1** No crear una entidad duplicada sin necesidad.
- [ ] **P1** Crear `/api/perfiles` como módulo administrativo nuevo.
- [ ] **P1** Listar perfiles.
- [ ] **P1** Consultar perfil por ID.
- [ ] **P1** Crear perfil.
- [ ] **P1** Editar perfil.
- [ ] **P1** Activar o desactivar perfil.
- [ ] **P1** Proteger perfiles base.
- [ ] **P1** Impedir eliminar físicamente perfiles.
- [ ] **P1** Impedir desactivar un perfil con uso crítico sin validación.
- [ ] **P1** Auditar todos los cambios.
- [ ] **P2** Definir permisos detallados si el alcance requiere más que roles.
- [ ] **P2** Definir perfil de operador comercial.
- [ ] **P2** Definir perfil de supervisor/aprobador.

---

# 9. Cliente comercial

- [ ] **P1** Crear entidad `ClienteComercial`.
- [ ] **P1** Mantener intactos `Ciudadano` y `Prestadora`.
- [ ] **P1** Permitir vínculo opcional con `Ciudadano`.
- [ ] **P1** Permitir vínculo opcional con `Prestadora`.
- [ ] **P1** Definir tipo de cliente.
- [ ] **P1** Definir tipo y número de documento.
- [ ] **P1** Garantizar unicidad razonable de documento.
- [ ] **P1** Guardar nombre o razón social.
- [ ] **P1** Guardar correo, teléfono y dirección.
- [ ] **P1** Guardar estado activo/inactivo.
- [ ] **P1** Crear `/api/clientes`.
- [ ] **P1** Crear CRUD lógico.
- [ ] **P1** Crear búsqueda por documento.
- [ ] **P1** Crear pantallas de listado, formulario y detalle.
- [ ] **P1** Auditar cambios.
- [ ] **P2** Relacionar exenciones autorizadas.
- [ ] **P2** Mostrar facturas, pagos y saldos relacionados.

---

# 10. Productos

- [ ] **P1** Crear entidad `Producto`.
- [ ] **P1** Definir código único.
- [ ] **P1** Definir nombre y descripción.
- [ ] **P1** Definir precio base.
- [ ] **P1** Definir moneda.
- [ ] **P1** Definir si maneja inventario.
- [ ] **P1** Definir existencia cuando aplique.
- [ ] **P1** Definir estado activo/inactivo.
- [ ] **P1** Crear `/api/productos`.
- [ ] **P1** Crear CRUD lógico.
- [ ] **P1** Crear listado, filtros y búsqueda.
- [ ] **P1** Crear formulario y detalle en la GUI.
- [ ] **P1** No permitir precios negativos.
- [ ] **P1** Usar `decimal(18,2)`.
- [ ] **P1** Auditar cambios de precio y estado.
- [ ] **P2** Exigir aprobación para cambiar precios vigentes.
- [ ] **P2** Crear historial de precios.

---

# 11. Servicios cobrables

- [ ] **P1** Crear entidad `ServicioCobrable` separada de `ServicioTelecom`.
- [ ] **P1** Crear `/api/servicios-cobrables`.
- [ ] **P1** Definir código único.
- [ ] **P1** Definir categoría.
- [ ] **P1** Definir descripción.
- [ ] **P1** Definir precio base configurable.
- [ ] **P1** Definir moneda.
- [ ] **P1** Definir vigencia de tarifa.
- [ ] **P1** Definir si permite exención.
- [ ] **P1** Definir si requiere prestadora.
- [ ] **P1** Definir si requiere resolución.
- [ ] **P1** Crear CRUD lógico.
- [ ] **P1** Crear pantallas en la GUI.
- [ ] **P1** No codificar precios en C#.
- [ ] **P1** No cargar precios oficiales sin fuente validada.
- [ ] **P2** Crear historial de tarifas.
- [ ] **P2** Exigir aprobación para modificar una tarifa vigente.

## Categorías iniciales de demostración

- [ ] Homologación e importación de equipos.
- [ ] Certificado de homologación.
- [ ] Carta de no objeción de aduanas.
- [ ] Solicitud o renovación de concesiones.
- [ ] Registros especiales.
- [ ] Licencias de radioaficionados.
- [ ] Derecho de uso del espectro.
- [ ] Asignación de recursos numéricos.
- [ ] Certificaciones administrativas.
- [ ] Duplicados de títulos, licencias o carnés.

---

# 12. Monedas y tasas de cambio

- [ ] **P1** Soportar inicialmente DOP y USD.
- [ ] **P1** Crear catálogo de monedas.
- [ ] **P1** Crear entidad de tasa de cambio.
- [ ] **P1** Guardar fecha de vigencia.
- [ ] **P1** Guardar fuente de la tasa.
- [ ] **P1** Guardar moneda original en documentos.
- [ ] **P1** Guardar monto original.
- [ ] **P1** Guardar tasa aplicada.
- [ ] **P1** Guardar equivalente calculado.
- [ ] **P1** No recalcular documentos históricos con tasas nuevas.
- [ ] **P1** Usar `decimal`, nunca `double`.
- [ ] **P2** Exigir aprobación para modificar tasas vigentes.
- [ ] **P2** Crear pantalla de tasas e historial.

---

# 13. Exenciones

- [ ] **P1** No aplicar exención únicamente por un booleano del cliente.
- [ ] **P1** Crear entidad `ExencionComercial`.
- [ ] **P1** Definir tipo de exención.
- [ ] **P1** Permitir porcentaje.
- [ ] **P1** Permitir monto máximo opcional.
- [ ] **P1** Permitir limitarla a un servicio cobrable.
- [ ] **P1** Guardar documento de soporte.
- [ ] **P1** Guardar vigencia.
- [ ] **P1** Guardar usuario aprobador.
- [ ] **P1** Guardar estado.
- [ ] **P1** Exigir aprobación antes de aplicarla.
- [ ] **P1** Mostrar monto exento en cotización y factura.
- [ ] **P1** Auditar aprobación, uso y revocación.
- [ ] **P1** No declarar categorías legalmente exentas sin validación normativa.

---

# 14. Órdenes

- [ ] **P2** Confirmar con el maestro si Orden debe aparecer explícitamente en el Core.
- [ ] **P2** Implementar Orden porque la Web futura necesita completar y consultar órdenes.
- [ ] **P2** Crear entidad `Orden`.
- [ ] **P2** Crear detalles de orden.
- [ ] **P2** Permitir productos y servicios cobrables.
- [ ] **P2** Definir estados controlados.
- [ ] **P2** Crear número único de orden.
- [ ] **P2** Crear `/api/ordenes`.
- [ ] **P2** Permitir crear, consultar, confirmar y cancelar.
- [ ] **P2** Permitir consultar órdenes del usuario autenticado.
- [ ] **P2** Crear simulación de pago sin registrar dinero real.
- [ ] **P2** Crear pantallas de listado, formulario y seguimiento.
- [ ] **P2** Auditar cambios de estado.

Estados previstos:

```text
BORRADOR
PENDIENTE_PAGO
PAGADA
EN_PROCESO
COMPLETADA
CANCELADA
VENCIDA
```

---

# 15. Cotizaciones

- [ ] **P1** Crear entidad `Cotizacion`.
- [ ] **P1** Crear entidad `CotizacionDetalle`.
- [ ] **P1** Generar número único.
- [ ] **P1** Relacionar cliente comercial.
- [ ] **P1** Permitir productos y servicios cobrables.
- [ ] **P1** Calcular subtotales en el Core.
- [ ] **P1** Calcular descuento en el Core.
- [ ] **P1** Calcular monto exento en el Core.
- [ ] **P1** Calcular total en el Core.
- [ ] **P1** Guardar moneda y tasa aplicada.
- [ ] **P1** Definir fecha de emisión y vencimiento.
- [ ] **P1** Crear estados y transiciones.
- [ ] **P1** Crear `/api/cotizaciones`.
- [ ] **P1** Crear, editar y cancelar mientras esté permitido.
- [ ] **P1** Emitir cotización.
- [ ] **P1** Aceptar cotización.
- [ ] **P1** Marcar vencida.
- [ ] **P1** Convertir a factura u orden según diseño final.
- [ ] **P1** Crear pantallas completas.
- [ ] **P1** Auditar operaciones.
- [ ] **P1** No editar libremente una cotización después de emitida.

Estados previstos:

```text
BORRADOR
EMITIDA
ACEPTADA
RECHAZADA
VENCIDA
CONVERTIDA
CANCELADA
```

---

# 16. Facturas

- [ ] **P1** Crear entidad `FacturaComercial`.
- [ ] **P1** Crear detalles de factura.
- [ ] **P1** Generar número único.
- [ ] **P1** Relacionar cliente.
- [ ] **P1** Relacionar cotización u orden cuando aplique.
- [ ] **P1** Copiar los importes históricos al emitir.
- [ ] **P1** Guardar moneda y tasa aplicada.
- [ ] **P1** Guardar subtotal, descuento, exención, impuestos y total.
- [ ] **P1** Guardar saldo pendiente.
- [ ] **P1** Crear fecha de emisión y vencimiento.
- [ ] **P1** Crear `/api/facturas`.
- [ ] **P1** Emitir desde cotización u orden.
- [ ] **P1** Consultar factura.
- [ ] **P1** Listar con filtros y paginación.
- [ ] **P1** Actualizar estado automáticamente por pagos.
- [ ] **P1** Anular con motivo, usuario y aprobación.
- [ ] **P1** No eliminar facturas.
- [ ] **P1** Crear pantallas de listado, detalle y emisión.
- [ ] **P1** Mostrar historial y auditoría.
- [ ] **P1** Preparar documento imprimible.

Estados previstos:

```text
BORRADOR
EMITIDA
PARCIALMENTE_PAGADA
PAGADA
VENCIDA
ANULADA
```

---

# 17. Cuentas por cobrar

- [ ] **P1** Crear entidad `CuentaPorCobrar`.
- [ ] **P1** Generarla automáticamente desde una factura con saldo.
- [ ] **P1** Guardar monto original.
- [ ] **P1** Guardar monto pagado.
- [ ] **P1** Guardar saldo pendiente.
- [ ] **P1** Guardar fecha de vencimiento.
- [ ] **P1** Guardar fecha del último pago.
- [ ] **P1** Crear estados controlados.
- [ ] **P1** Crear `/api/cuentas-por-cobrar`.
- [ ] **P1** Consultar por cliente.
- [ ] **P1** Consultar vencidas.
- [ ] **P1** Filtrar por estado y fechas.
- [ ] **P1** No permitir edición manual libre de saldos.
- [ ] **P1** Cambiar saldos únicamente por cobros, reversión o ajuste autorizado.
- [ ] **P1** Crear pantallas de listado y detalle.
- [ ] **P1** Mostrar aplicaciones de cobro.

Estados previstos:

```text
PENDIENTE
PARCIAL
PAGADA
VENCIDA
ANULADA
```

---

# 18. Pagos

- [ ] **P1** Crear entidad `Pago`.
- [ ] **P1** Separar pago de cobro.
- [ ] **P1** Crear `OperacionClienteId` único.
- [ ] **P1** Relacionar cliente.
- [ ] **P1** Relacionar orden, factura o cuenta cuando aplique.
- [ ] **P1** Definir método de pago.
- [ ] **P1** Guardar moneda, monto y tasa de cambio.
- [ ] **P1** Guardar referencia externa.
- [ ] **P1** Guardar canal: GUI, Caja, Web o Integración.
- [ ] **P1** Crear estados controlados.
- [ ] **P1** Crear `/api/pagos`.
- [ ] **P1** Crear `/api/pagos/simular`.
- [ ] **P1** Simular sin crear transacción financiera definitiva.
- [ ] **P1** Registrar pago.
- [ ] **P1** Aprobar o rechazar según método.
- [ ] **P1** Colocar depósitos y transferencias en conciliación cuando corresponda.
- [ ] **P1** Anular mediante operación controlada.
- [ ] **P1** Crear pantallas de pagos.
- [ ] **P1** Auditar todos los cambios.

Métodos previstos:

```text
EFECTIVO
TARJETA_SIMULADA
TRANSFERENCIA
DEPOSITO_BANCARIO
CHEQUE
COSTO_CERO
```

Estados previstos:

```text
PENDIENTE
APROBADO
RECHAZADO
ANULADO
EN_CONCILIACION
```

---

# 19. Cobros y aplicación

- [ ] **P1** Crear entidad `Cobro`.
- [ ] **P1** Crear entidad `AplicacionCobro`.
- [ ] **P1** Generar número de recibo único.
- [ ] **P1** Relacionar pago aprobado.
- [ ] **P1** Guardar monto recibido.
- [ ] **P1** Guardar monto aplicado.
- [ ] **P1** Guardar monto no aplicado.
- [ ] **P1** Permitir aplicar a una o varias facturas si el diseño lo aprueba.
- [ ] **P1** Permitir pago parcial.
- [ ] **P1** Impedir aplicar más que el saldo pendiente.
- [ ] **P1** Impedir aplicar dos veces el mismo cobro.
- [ ] **P1** Actualizar factura y cuenta por cobrar en la misma transacción.
- [ ] **P1** Crear recibo en la misma transacción.
- [ ] **P1** Registrar auditoría en la misma operación.
- [ ] **P1** Crear `/api/cobros`.
- [ ] **P1** Crear `/api/recibos`.
- [ ] **P1** Crear pantallas de aplicación y detalle.
- [ ] **P1** Reversar creando movimientos contrarios; no borrar originales.
- [ ] **P1** Exigir aprobación para reversión.

Estados previstos:

```text
APLICADO
PARCIAL
REVERSADO
```

---

# 20. Recibos e impresión

- [ ] **P1** Crear entidad o proyección persistente de recibo.
- [ ] **P1** Incluir número de recibo.
- [ ] **P1** Incluir fecha y hora.
- [ ] **P1** Incluir usuario u operador.
- [ ] **P1** Incluir cliente.
- [ ] **P1** Incluir conceptos.
- [ ] **P1** Incluir forma de pago.
- [ ] **P1** Incluir moneda y tasa.
- [ ] **P1** Incluir total recibido y aplicado.
- [ ] **P1** Incluir referencia externa.
- [ ] **P1** Incluir código o identificador de verificación.
- [ ] **P1** Crear vista imprimible HTML.
- [ ] **P2** Evaluar PDF generado por el Core.
- [ ] **P2** Preparar formato compatible con impresora térmica futura.
- [ ] **P2** No controlar físicamente la impresora desde el Core.

---

# 21. Soporte futuro para funciones de Caja

Aunque no modificaremos Caja, el Core debe ofrecer contratos para que posteriormente pueda implementar:

- [ ] **P2** Validación de usuario y clave mediante autenticación actual.
- [ ] **P2** Inicio de día o jornada de caja.
- [ ] **P2** Entrada de efectivo.
- [ ] **P2** Salida de efectivo.
- [ ] **P2** Pago de productos o servicios.
- [ ] **P2** Emisión de recibo.
- [ ] **P2** Cierre del día.
- [ ] **P2** Cuadre de transacciones.
- [ ] **P2** Consultas de resumen por método de pago.
- [ ] **P2** Total en DOP y USD.
- [ ] **P2** Anulaciones y reversos.
- [ ] **P2** Operaciones idempotentes para contingencia offline.

## Entidades potenciales

- [ ] `Caja`.
- [ ] `JornadaCaja`.
- [ ] `MovimientoCaja`.
- [ ] `EntradaEfectivo`.
- [ ] `SalidaEfectivo`.
- [ ] `CuadreCaja` o resumen calculado.

## Cálculo esperado de cierre

```text
Monto inicial
+ Entradas
+ Cobros en efectivo
- Salidas
= Efectivo esperado

Efectivo contado - Efectivo esperado = Diferencia
```

---

# 22. Idempotencia y operaciones fuera de línea

- [ ] **P1** Aceptar encabezado `Idempotency-Key` en operaciones monetarias.
- [ ] **P1** Aceptar `OperacionClienteId` en DTO cuando sea necesario.
- [ ] **P1** Crear entidad `OperacionIdempotente`.
- [ ] **P1** Guardar clave, usuario, ruta y método.
- [ ] **P1** Guardar hash de la solicitud.
- [ ] **P1** Guardar estado de procesamiento.
- [ ] **P1** Guardar código y respuesta original.
- [ ] **P1** Devolver el resultado original ante repetición idéntica.
- [ ] **P1** Devolver `409` si la misma clave trae contenido diferente.
- [ ] **P1** Evitar duplicar facturas, pagos, cobros o recibos.
- [ ] **P1** Probar concurrencia con dos solicitudes simultáneas.
- [ ] **P2** Preparar endpoint de sincronización por lotes.
- [ ] **P2** Definir resultados `APLICADA`, `YA_APLICADA`, `RECHAZADA`, `CONFLICTO` y `REQUIERE_REVISION`.
- [ ] **P2** Preparar contratos para que Caja e Integración los consuman en una fase posterior.

---

# 23. Conciliación bancaria

- [ ] **P2** No asumir que existe una API pública disponible de Banreservas.
- [ ] **P2** Crear interfaz `IConciliacionBancariaService`.
- [ ] **P2** Crear implementación simulada para el proyecto académico.
- [ ] **P2** Registrar depósitos o transferencias como `EN_CONCILIACION`.
- [ ] **P2** Validar referencia externa.
- [ ] **P2** Cambiar a `APROBADO` o `RECHAZADO`.
- [ ] **P2** Aplicar el cobro solo después de aprobación.
- [ ] **P2** Auditar conciliación.
- [ ] **P3** Preparar adaptador reemplazable para una integración real futura.

---

# 24. Doble autorización: operador y aprobador

- [ ] **P1** Crear entidad `SolicitudAprobacion`.
- [ ] **P1** Crear estados `PENDIENTE`, `APROBADA`, `RECHAZADA`, `CANCELADA`.
- [ ] **P1** Guardar solicitante.
- [ ] **P1** Guardar aprobador.
- [ ] **P1** Impedir que el solicitante apruebe su propia operación.
- [ ] **P1** Guardar resumen del cambio.
- [ ] **P1** Guardar hash del payload.
- [ ] **P1** Guardar comentario de decisión.
- [ ] **P1** Guardar `correlationId`.
- [ ] **P1** Crear pantalla Operaciones pendientes.
- [ ] **P1** Mostrar valores actuales y propuestos.
- [ ] **P1** Exigir comentario al rechazar.
- [ ] **P1** Auditar solicitud y decisión.

## Acciones que deben pasar por aprobación

- [ ] Modificar tarifa vigente.
- [ ] Aprobar exención.
- [ ] Anular factura.
- [ ] Reversar cobro.
- [ ] Cambiar tasa de cambio.
- [ ] Reabrir documento cerrado, si se permite.
- [ ] Activar perfil administrativo crítico.

---

# 25. Auditoría, trazabilidad y observabilidad

- [ ] **P1** Auditar todas las nuevas operaciones administrativas.
- [ ] **P1** Auditar login y logout de la GUI.
- [ ] **P1** Auditar cambios de usuarios y perfiles.
- [ ] **P1** Auditar cambios de catálogo y tarifas.
- [ ] **P1** Auditar cotizaciones, facturas y cuentas.
- [ ] **P1** Auditar pagos, cobros, aplicaciones y reversos.
- [ ] **P1** Guardar usuario, correo y rol.
- [ ] **P1** Guardar entidad e ID.
- [ ] **P1** Guardar acción.
- [ ] **P1** Guardar estado anterior y nuevo.
- [ ] **P1** Guardar método, ruta, IP y User-Agent.
- [ ] **P1** Guardar fecha UTC.
- [ ] **P1** Guardar `correlationId`.
- [ ] **P1** Crear filtros de auditoría en la GUI.
- [ ] **P2** Crear endpoint de salud ampliado.
- [ ] **P2** Mostrar versión y ambiente.
- [ ] **P2** No exponer secretos ni excepciones técnicas.
- [ ] **P3** Agregar logs estructurados.
- [ ] **P3** Agregar métricas de operaciones financieras.

---

# 26. Concurrencia y consistencia financiera

- [ ] **P1** Usar `decimal(18,2)` para dinero.
- [ ] **P1** Usar precisión adecuada para tasas de cambio.
- [ ] **P1** Agregar `rowversion` o token de concurrencia en documentos críticos.
- [ ] **P1** Detectar actualizaciones concurrentes.
- [ ] **P1** Devolver conflicto controlado.
- [ ] **P1** Ejecutar factura, cuenta, pago, cobro, aplicación, recibo y auditoría en transacción explícita.
- [ ] **P1** Revertir toda la operación si falla una parte.
- [ ] **P1** No reintentar ciegamente escrituras monetarias.
- [ ] **P1** Probar cobros parciales simultáneos.
- [ ] **P1** Probar duplicación de referencias.
- [ ] **P1** Probar reversión.
- [ ] **P1** Probar saldo exacto después de cada operación.

---

# 27. Swagger y documentación técnica

- [ ] **P1** Mantener Swagger como consola técnica.
- [ ] **P1** No presentarlo como la GUI final.
- [ ] **P1** Agrupar endpoints por módulos.
- [ ] **P1** Documentar JWT Bearer.
- [ ] **P1** Documentar roles requeridos.
- [ ] **P1** Agregar descripciones de operaciones.
- [ ] **P1** Agregar ejemplos de solicitudes y respuestas.
- [ ] **P1** Documentar estados permitidos.
- [ ] **P1** Documentar códigos de error de negocio.
- [ ] **P1** Documentar respuestas 400, 401, 403, 404, 409, 422 y 429.
- [ ] **P1** Documentar paginación.
- [ ] **P1** Documentar `Idempotency-Key`.
- [ ] **P1** Exportar OpenAPI después de cada fase.
- [ ] **P1** Comparar el contrato nuevo con la línea base.

---

# 28. Migraciones y datos

- [ ] **P1** Crear una migración por fase o conjunto coherente.
- [ ] **P1** Revisar SQL generado antes de aplicar.
- [ ] **P1** Mantener migraciones aditivas.
- [ ] **P1** No borrar columnas actuales.
- [ ] **P1** No renombrar tablas actuales.
- [ ] **P1** Agregar índices únicos para números de documentos y operaciones.
- [ ] **P1** Agregar índices para búsquedas y estados.
- [ ] **P1** Configurar relaciones y restricciones.
- [ ] **P1** Configurar comportamiento de borrado seguro.
- [ ] **P1** Respaldar antes de migrar.
- [ ] **P1** Probar migración en base nueva.
- [ ] **P1** Probar migración sobre una copia de la base existente.
- [ ] **P1** Probar rollback documentado.
- [ ] **P1** Crear datos semilla de demostración sin tarifas oficiales falsas.
- [ ] **P1** No incluir credenciales reales en datos semilla.

---

# 29. Pruebas del Core

- [ ] **P0** Mantener las 20 pruebas existentes pasando.
- [ ] **P1** Agregar pruebas de perfiles.
- [ ] **P1** Agregar pruebas de clientes.
- [ ] **P1** Agregar pruebas de productos.
- [ ] **P1** Agregar pruebas de servicios cobrables.
- [ ] **P1** Agregar pruebas de monedas y tasas.
- [ ] **P1** Agregar pruebas de exenciones.
- [ ] **P1** Agregar pruebas de cotizaciones.
- [ ] **P1** Agregar pruebas de facturas.
- [ ] **P1** Agregar pruebas de cuentas por cobrar.
- [ ] **P1** Agregar pruebas de pagos.
- [ ] **P1** Agregar pruebas de cobros.
- [ ] **P1** Agregar pruebas de recibos.
- [ ] **P1** Agregar pruebas de idempotencia.
- [ ] **P1** Agregar pruebas de concurrencia.
- [ ] **P1** Agregar pruebas de reversión.
- [ ] **P1** Agregar pruebas de doble autorización.
- [ ] **P1** Agregar pruebas de roles y acceso prohibido.
- [ ] **P1** Agregar pruebas de migraciones.
- [ ] **P1** Agregar pruebas de contratos actuales.
- [ ] **P1** Probar que no se exponen hashes, secretos o excepciones.

---

# 30. Pruebas de la GUI

- [ ] **P1** Probar login correcto.
- [ ] **P1** Probar login incorrecto.
- [ ] **P1** Probar bloqueo temporal.
- [ ] **P1** Probar refresh token.
- [ ] **P1** Probar logout.
- [ ] **P1** Probar expiración por inactividad.
- [ ] **P1** Probar menú por rol.
- [ ] **P1** Probar acceso directo a URL sin permiso.
- [ ] **P1** Probar validación antiforgery.
- [ ] **P1** Probar Core apagado.
- [ ] **P1** Probar error de base de datos controlado.
- [ ] **P1** Probar `correlationId` visible.
- [ ] **P1** Probar tablas, filtros y paginación.
- [ ] **P1** Probar formularios y validaciones.
- [ ] **P1** Probar confirmaciones sensibles.
- [ ] **P1** Probar diseño responsive.
- [ ] **P1** Probar navegación por teclado.
- [ ] **P2** Probar impresión.
- [ ] **P2** Probar contraste y accesibilidad básica.

---

# 31. Pruebas de no regresión de consumidores

Aunque no se modificarán, debemos demostrar que siguen funcionando:

- [ ] **P0** Ejecutar smoke de autenticación del Core.
- [ ] **P0** Ejecutar smoke de reclamaciones.
- [ ] **P0** Ejecutar pruebas de contratos actuales.
- [ ] **P0** Confirmar que Caja inicia sin cambios.
- [ ] **P0** Confirmar que Caja puede autenticarse como antes.
- [ ] **P0** Confirmar que Web inicia sin cambios.
- [ ] **P0** Confirmar que Web puede autenticarse como antes.
- [ ] **P0** Confirmar que Gateway inicia sin cambios.
- [ ] **P0** Confirmar que Integración continúa validando.
- [ ] **P0** Confirmar que las rutas actuales mantienen códigos y estructura.
- [ ] **P0** Confirmar que la nueva GUI puede detenerse sin afectar consumidores.

---

# 32. Seguridad

- [ ] **P1** Mantener JWT, refresh token y bloqueo actual.
- [ ] **P1** Mantener rate limiting.
- [ ] **P1** Mantener CORS por ambiente.
- [ ] **P1** Aplicar autorización por rol en endpoints nuevos.
- [ ] **P1** No confiar solo en botones ocultos.
- [ ] **P1** Proteger sesión de la GUI.
- [ ] **P1** Agregar encabezados de seguridad.
- [ ] **P1** No guardar tokens en navegador.
- [ ] **P1** No exponer cadenas de conexión.
- [ ] **P1** No exponer stack traces.
- [ ] **P1** Validar entradas y longitudes.
- [ ] **P1** Proteger subida de documentos cuando se agreguen soportes.
- [ ] **P1** Validar propiedad y rol.
- [ ] **P1** Auditar acciones sensibles.
- [ ] **P1** Probar IDOR en recursos comerciales.
- [ ] **P1** Probar acceso de usuario desactivado.
- [ ] **P2** Revisar política de contraseñas.
- [ ] **P3** Ejecutar análisis de dependencias y vulnerabilidades.

---

# 33. Documentación

- [x] Crear `PLAN_EVOLUCION_CORE_COMERCIAL_Y_GUI.md`.
- [x] Crear `ADR_GUI_ADMINISTRATIVA_CORE.md`.
- [x] Crear `PLAN_IMPLEMENTACION_GUI_CORE_OBLIGATORIA.md`.
- [x] Crear este checklist maestro.
- [ ] **P1** Actualizar arquitectura final.
- [ ] **P1** Actualizar manual técnico.
- [ ] **P1** Actualizar manual de usuario del Core GUI.
- [ ] **P1** Crear matriz de roles y permisos.
- [ ] **P1** Crear catálogo de códigos de error.
- [ ] **P1** Documentar entidades y relaciones.
- [ ] **P1** Documentar estados y transiciones.
- [ ] **P1** Documentar idempotencia.
- [ ] **P1** Documentar aprobación dual.
- [ ] **P1** Documentar migraciones y rollback.
- [ ] **P1** Documentar cómo iniciar API y GUI.
- [ ] **P1** Documentar credenciales únicamente de demostración.
- [ ] **P1** Documentar limitaciones académicas.
- [ ] **P1** Actualizar README en cada hito importante.

---

# 34. Primera demostración de la GUI

- [ ] Iniciar SQL Server.
- [ ] Iniciar Core API.
- [ ] Iniciar Core GUI.
- [ ] Mostrar que Swagger sigue disponible como herramienta técnica.
- [ ] Abrir la GUI del Core.
- [ ] Iniciar sesión como Administrador.
- [ ] Mostrar dashboard.
- [ ] Consultar usuarios.
- [ ] Crear usuario de prueba.
- [ ] Editar usuario.
- [ ] Desactivar usuario.
- [ ] Buscar las acciones en auditoría.
- [ ] Mostrar salud del Core y SQL Server.
- [ ] Mostrar error controlado con `correlationId`.
- [ ] Cerrar sesión.
- [ ] Confirmar que Caja, Web e Integración continúan funcionando.

---

# 35. Demostración final del módulo comercial

- [ ] Iniciar sesión con rol autorizado.
- [ ] Crear cliente comercial.
- [ ] Crear producto.
- [ ] Crear servicio cobrable.
- [ ] Configurar moneda o tasa de demostración.
- [ ] Crear cotización.
- [ ] Emitir cotización.
- [ ] Convertir a orden o factura según diseño final.
- [ ] Emitir factura.
- [ ] Mostrar cuenta por cobrar.
- [ ] Simular pago.
- [ ] Registrar pago.
- [ ] Aplicar cobro parcial o total.
- [ ] Mostrar nuevo saldo.
- [ ] Imprimir o visualizar recibo.
- [ ] Intentar repetir la operación con la misma clave idempotente.
- [ ] Demostrar que no se duplica.
- [ ] Solicitar reversión.
- [ ] Aprobarla con otro usuario.
- [ ] Mostrar auditoría completa.
- [ ] Mostrar que los documentos originales no fueron borrados.

---

# 36. Definición de terminado

El trabajo se considerará terminado únicamente cuando:

- [ ] La GUI forma parte de `Indotel.Core.sln`.
- [ ] La GUI tiene login real contra el Core.
- [ ] La GUI se ejecuta en puerto independiente.
- [ ] La GUI no accede a SQL Server.
- [ ] La GUI no utiliza `IndotelDbContext`.
- [ ] La GUI aplica roles y sesión segura.
- [ ] Existe dashboard, navegación y pantallas operativas.
- [ ] Usuarios y perfiles funcionan.
- [ ] Clientes, productos y servicios cobrables funcionan.
- [ ] Cotizaciones funcionan.
- [ ] Facturas funcionan.
- [ ] Cuentas por cobrar funcionan.
- [ ] Pagos y cobros funcionan.
- [ ] Recibos funcionan.
- [ ] Multi-moneda funciona.
- [ ] Exenciones son auditables.
- [ ] Idempotencia evita duplicados.
- [ ] Concurrencia está controlada.
- [ ] Acciones sensibles usan doble autorización.
- [ ] Swagger está actualizado.
- [ ] Migraciones son aditivas y probadas.
- [ ] Las pruebas nuevas pasan.
- [ ] Las 20 pruebas antiguas siguen pasando.
- [ ] Caja no fue modificada y continúa funcionando.
- [ ] Web no fue modificada y continúa funcionando.
- [ ] Gateway e Integración no fueron modificados y continúan funcionando.
- [ ] La documentación y el guion de defensa están actualizados.
- [ ] La demostración completa puede repetirse desde una instalación limpia.

---

# Orden obligatorio de ejecución

1. [ ] Línea base y pruebas.
2. [ ] Esqueleto de GUI.
3. [ ] Login, sesión y roles.
4. [ ] Dashboard, usuarios, auditoría y salud.
5. [ ] Perfiles.
6. [ ] Clientes comerciales.
7. [ ] Productos y servicios cobrables.
8. [ ] Monedas, tarifas y exenciones.
9. [ ] Órdenes, si se confirma su inclusión formal.
10. [ ] Cotizaciones.
11. [ ] Facturas y cuentas por cobrar.
12. [ ] Pagos, cobros y recibos.
13. [ ] Idempotencia y preparación offline.
14. [ ] Doble autorización.
15. [ ] Pruebas de regresión y consumidores.
16. [ ] Documentación y defensa.

> No se debe comenzar pagos o cobros antes de completar la línea base, la GUI inicial, el catálogo comercial, las reglas monetarias y las pruebas de compatibilidad.