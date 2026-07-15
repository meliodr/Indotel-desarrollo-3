# Plan de evolución comercial y GUI administrativa del Core INDOTEL

Rama de trabajo: `proyecto-revisado-funcionando`

Fecha de decisión: 15/07/2026

Estado: planificación aprobada para implementación incremental. Este documento no declara las funciones comerciales como implementadas todavía.

## 1. Objetivo

Ampliar exclusivamente el Core INDOTEL para incorporar las funciones académicas solicitadas de usuarios, perfiles, productos, clientes, servicios cobrables, cotizaciones, facturas, cuentas por cobrar, pagos y cobros, sin romper los contratos actuales utilizados por Caja, Web, API Gateway o Integración.

La ampliación también contempla una GUI administrativa del Core, pero la GUI no se mezclará con la API existente. Se creará como un proyecto independiente dentro de la solución del Core y consumirá los endpoints HTTP del Core.

## 2. Línea base que no se puede romper

El sistema actual ya tiene:

- autenticación JWT y refresh token;
- usuarios y roles;
- ciudadanos y prestadoras;
- servicios de telecomunicaciones asociados a reclamaciones;
- reclamaciones, documentos, historial, SLA y auditoría;
- resoluciones, autorizaciones, certificaciones, espectro y licencias;
- respuestas de error estructuradas;
- políticas de autorización;
- Core consumido por Web y Caja a través del Gateway en la arquitectura integrada.

Reglas de compatibilidad:

1. No eliminar ni renombrar rutas actuales.
2. No cambiar la forma de las respuestas actuales.
3. No renombrar tablas o columnas actuales.
4. No reutilizar `/api/servicios` para el catálogo comercial, porque esa ruta ya representa `ServicioTelecom`.
5. No cambiar los nombres de roles existentes.
6. No hacer eliminaciones físicas de usuarios, facturas, pagos, cobros o cuentas por cobrar.
7. No activar migraciones destructivas.
8. Cada fase debe compilar y probarse antes de habilitar la siguiente.

## 3. Decisión sobre la arquitectura

El Core seguirá siendo un monolito modular ASP.NET Core, pero los módulos comerciales se organizarán por responsabilidad.

```text
Indotel.Core API
├── Reclamaciones actuales
├── Regulación actual
├── Comercial
│   ├── Clientes
│   ├── Productos
│   ├── Servicios cobrables
│   ├── Cotizaciones
│   ├── Facturación
│   ├── Cuentas por cobrar
│   ├── Pagos
│   └── Cobros
├── Seguridad
├── Auditoría
└── Persistencia EF Core

Indotel.Core.Admin
├── Dashboard
├── Usuarios y perfiles
├── Catálogo comercial
├── Clientes
├── Cotizaciones
├── Facturas
├── Cuentas por cobrar
├── Pagos y cobros
├── Auditoría
└── Salud del sistema
```

La GUI no accederá directamente a `IndotelDbContext`. Utilizará `HttpClient` y los contratos HTTP del Core. Esto obliga a que la interfaz y los demás clientes vean exactamente las mismas reglas.

## 4. GUI administrativa: patrón empresarial adoptado

Las grandes plataformas empresariales separan el motor transaccional de la interfaz operativa. Un ejemplo visible es Stripe: su Dashboard administra transacciones, clientes, productos, facturas, reportes, miembros y registros de API, mientras la API permanece como contrato independiente.

Para INDOTEL se adopta el mismo principio:

- API como fuente de verdad;
- dashboard administrativo separado;
- navegación por módulos;
- permisos por rol;
- tablas filtrables y paginadas;
- vista de detalle con historial;
- acciones sensibles con confirmación;
- estados visibles mediante etiquetas;
- auditoría y correlationId accesibles;
- sin lógica comercial duplicada en la pantalla.

### Tecnología propuesta

Crear un proyecto nuevo:

```text
core-indotel/Indotel.Core.Admin
```

Tecnología inicial:

- ASP.NET Core 8 Razor Pages o MVC;
- autenticación de la GUI mediante cookie segura;
- tokens del Core almacenados en servidor, no en `localStorage`;
- `HttpClientFactory` para consumir el Core;
- Bootstrap 5 con diseño visual inspirado en Fluent;
- componentes accesibles y responsive;
- tablas con búsqueda, filtro, orden y paginación;
- configuración `CoreApi:BaseUrl` por ambiente.

Razor Pages/MVC se elige para mantener el desarrollo dentro de C# y .NET, disminuir dependencias y facilitar el mantenimiento del equipo. La API y la GUI seguirán siendo proyectos independientes.

### La GUI no debe hacer

- consultar SQL Server directamente;
- usar `IndotelDbContext`;
- calcular totales financieros por su cuenta;
- decidir transiciones de estado;
- guardar contraseñas o tokens en el navegador;
- duplicar reglas de exención, moneda o aplicación de cobros;
- sustituir Swagger como documentación técnica.

## 5. OpenAPI y Swagger

Swagger seguirá siendo la consola técnica de desarrolladores, no el GUI administrativo final.

Se mejorará para incluir:

- agrupación por módulo;
- descripción de cada operación;
- ejemplos de solicitudes y respuestas;
- respuestas 400, 401, 403, 404, 409 y 422;
- códigos de error de negocio;
- requisitos de rol;
- estados permitidos;
- encabezado de idempotencia en operaciones monetarias;
- esquema de paginación común.

La GUI administrativa será para operaciones de negocio. Swagger será para probar y documentar la API.

## 6. Modelo comercial propuesto

### 6.1 Perfiles

No se creará una entidad duplicada. Para este proyecto, `Perfil` será el nombre funcional de `Rol`.

Se conserva la tabla actual `Roles` y se agrega un controlador administrativo:

```text
GET    /api/perfiles
GET    /api/perfiles/{id}
POST   /api/perfiles
PUT    /api/perfiles/{id}
PATCH  /api/perfiles/{id}/estado
```

Reglas:

- no eliminar roles con usuarios asociados;
- no eliminar físicamente roles;
- proteger los roles base del sistema;
- auditar creación, edición y cambio de estado.

### 6.2 Cliente comercial

No se sustituirán `Ciudadano` ni `Prestadora`.

Nueva entidad:

```text
ClienteComercial
- Id
- TipoCliente
- TipoDocumento
- NumeroDocumento
- NombreRazonSocial
- Correo
- Telefono
- Direccion
- CiudadanoId opcional
- PrestadoraId opcional
- EsExento
- Activo
- FechaCreacion
- FechaActualizacion
```

Ruta pública del módulo:

```text
/api/clientes
```

La tabla se llamará `ClientesComerciales` para evitar confusiones internas.

### 6.3 Productos

```text
Producto
- Id
- Codigo
- Nombre
- Descripcion
- PrecioBase
- Moneda
- ManejaInventario
- Existencia
- Activo
- FechaCreacion
- FechaActualizacion
```

Ruta:

```text
/api/productos
```

### 6.4 Servicios cobrables

`ServicioTelecom` se mantiene intacto para reclamaciones.

Nueva entidad:

```text
ServicioCobrable
- Id
- Codigo
- Nombre
- Categoria
- Descripcion
- PrecioBase
- Moneda
- PermiteExencion
- RequierePrestadora
- RequiereResolucion
- Activo
- FechaInicioVigencia
- FechaFinVigencia
```

Ruta:

```text
/api/servicios-cobrables
```

Categorías iniciales de demostración, sin afirmar tarifas oficiales:

- homologación e importación;
- concesiones, licencias y registros;
- espectro y recursos numéricos;
- certificaciones administrativas y duplicados.

Los precios se almacenarán como datos configurables. No se codificarán montos en C# y no se cargarán tarifas oficiales sin resolución o fuente institucional validada.

### 6.5 Cotizaciones

```text
Cotizacion
- Id
- NumeroCotizacion
- ClienteComercialId
- Estado
- Moneda
- Subtotal
- Descuento
- MontoExento
- Total
- FechaEmision
- FechaVencimiento
- UsuarioCreacionId
```

```text
CotizacionDetalle
- Id
- CotizacionId
- TipoItem
- ProductoId opcional
- ServicioCobrableId opcional
- Descripcion
- Cantidad
- PrecioUnitario
- MontoExento
- Subtotal
```

Estados:

```text
BORRADOR -> EMITIDA -> ACEPTADA -> CONVERTIDA
BORRADOR/EMITIDA -> CANCELADA
EMITIDA -> VENCIDA
```

### 6.6 Facturas

```text
FacturaComercial
- Id
- NumeroFactura
- ClienteComercialId
- CotizacionId opcional
- Estado
- Moneda
- Subtotal
- Descuento
- MontoExento
- Impuestos
- Total
- SaldoPendiente
- FechaEmision
- FechaVencimiento
- UsuarioEmisionId
```

Los detalles de factura se copian de la cotización o se crean de forma controlada. La factura emitida conserva sus importes históricos aunque cambien los precios del catálogo.

Estados:

```text
BORRADOR
EMITIDA
PARCIALMENTE_PAGADA
PAGADA
VENCIDA
ANULADA
```

No se elimina una factura. Se anula con motivo, usuario y auditoría.

### 6.7 Cuentas por cobrar

```text
CuentaPorCobrar
- Id
- FacturaComercialId
- ClienteComercialId
- MontoOriginal
- MontoPagado
- SaldoPendiente
- Estado
- FechaVencimiento
- FechaUltimoPago
```

Se genera a partir de una factura emitida con saldo. Sus montos no se editan mediante un CRUD libre: cambian por aplicación de cobros, anulación o ajuste autorizado.

### 6.8 Pagos y cobros

Se separan dos conceptos:

- `Pago`: intento o medio presentado por un cliente, que puede estar pendiente, aprobado o rechazado.
- `Cobro`: aplicación contable confirmada de uno o varios pagos sobre facturas o cuentas por cobrar.

```text
Pago
- Id
- OperacionClienteId
- ClienteComercialId
- MetodoPago
- Moneda
- Monto
- TasaCambio
- ReferenciaExterna
- Canal
- Estado
- FechaCreacion
- FechaConfirmacion
```

```text
Cobro
- Id
- NumeroRecibo
- PagoId
- ClienteComercialId
- Moneda
- MontoRecibido
- MontoAplicado
- MontoNoAplicado
- Estado
- UsuarioAplicacionId
- Fecha
```

```text
AplicacionCobro
- Id
- CobroId
- FacturaComercialId
- CuentaPorCobrarId
- MontoAplicado
```

Estados de pago:

```text
PENDIENTE
APROBADO
RECHAZADO
ANULADO
EN_CONCILIACION
```

Estados de cobro:

```text
APLICADO
PARCIAL
REVERSADO
```

Reglas financieras:

1. usar `decimal`, nunca `double`;
2. precisión monetaria `decimal(18,2)`;
3. una aplicación no puede superar el saldo pendiente;
4. un cobro no puede aplicarse dos veces;
5. toda reversión crea un movimiento contrario, no borra el original;
6. factura, cuenta por cobrar, pago, cobro, aplicaciones, recibo y auditoría se guardan en una transacción explícita;
7. toda operación monetaria acepta una clave de idempotencia.

## 7. Idempotencia y soporte futuro de Caja fuera de línea

Aunque Caja no se modifique en esta fase, el Core debe estar preparado para recibir operaciones reenviadas.

Encabezado propuesto:

```text
Idempotency-Key: GUID
```

También se aceptará `OperacionClienteId` dentro del DTO cuando el cliente no pueda enviar encabezados.

Nueva entidad:

```text
OperacionIdempotente
- Id
- Clave
- UsuarioId
- Ruta
- Metodo
- HashSolicitud
- Estado
- CodigoRespuesta
- RespuestaSerializada
- FechaCreacion
- FechaFinalizacion
```

Comportamiento:

- primera solicitud: procesa y registra;
- misma clave y mismo contenido: devuelve el resultado original;
- misma clave con contenido diferente: devuelve `409`;
- operación en proceso: devuelve estado controlado;
- no se crean dos pagos, cobros o facturas por reintentos.

Esto permite que Integración y Caja agreguen sincronización posteriormente sin cambiar el dominio financiero.

## 8. Multi-moneda y exenciones

Se soportarán inicialmente `DOP` y `USD`.

Cada documento financiero conservará:

- moneda original;
- monto original;
- tasa aplicada;
- equivalente calculado;
- fecha de la tasa.

No se recalcularán documentos históricos con una tasa nueva.

Las exenciones se modelarán como autorización auditable, no como un simple booleano aplicado automáticamente.

```text
ExencionComercial
- Id
- ClienteComercialId
- Tipo
- Porcentaje
- MontoMaximo opcional
- ServicioCobrableId opcional
- DocumentoSoporte
- FechaInicio
- FechaFin
- Estado
- UsuarioAprobacionId
```

No se afirmará en el sistema que una categoría está legalmente exenta hasta que exista una regla normativa validada para el proyecto.

## 9. Endpoints nuevos previstos

```text
/api/perfiles
/api/clientes
/api/productos
/api/servicios-cobrables
/api/cotizaciones
/api/facturas
/api/cuentas-por-cobrar
/api/pagos
/api/pagos/simular
/api/cobros
/api/recibos
/api/admin/resumen
```

Todas son rutas nuevas. Ninguna sustituye rutas actuales.

## 10. Organización del código

Sin convertir todavía el proyecto a Clean Architecture completa, se organizará el código nuevo de forma modular:

```text
Models/Commercial/
DTOs/Commercial/
Services/Commercial/
Controllers/Commercial/
Constants/Commercial/
Data/Configurations/Commercial/
```

Los controladores comerciales serán delgados. Las reglas monetarias no se escribirán directamente en los controladores.

Servicios previstos:

```text
IClienteComercialService
IProductoService
IServicioCobrableService
ICotizacionService
IFacturacionService
ICuentaPorCobrarService
IPagoService
ICobroService
IIdempotencyService
ITasaCambioService
IExencionService
```

## 11. Estrategia de base de datos

Cada fase tendrá una migración aditiva independiente.

Ejemplo:

```text
AddCommercialCustomersAndCatalog
AddQuotesAndDetails
AddInvoicesAndReceivables
AddPaymentsCollectionsAndIdempotency
```

Prohibido en estas migraciones:

- `DropTable` sobre tablas actuales;
- `DropColumn` sobre tablas actuales;
- renombrar claves existentes;
- cambiar datos de reclamaciones;
- hacer obligatoria una relación nueva sobre datos actuales sin valor por defecto o nulabilidad.

Antes de cada migración:

1. respaldo de base de datos;
2. generar script SQL;
3. revisar operaciones destructivas;
4. aplicar en base de prueba;
5. ejecutar pruebas existentes y nuevas;
6. validar rollback o restauración.

## 12. Transacciones

El filtro transaccional actual está limitado a rutas de reclamaciones. No se ampliará globalmente de forma automática.

Pagos, cobros, facturación y aplicaciones usarán transacciones explícitas en servicios comerciales. Esto permite controlar:

- nivel de aislamiento;
- concurrencia;
- idempotencia;
- actualización de saldos;
- reversión completa;
- respuesta de conflicto.

## 13. Control de concurrencia

Las entidades financieras incluirán un token de concurrencia, preferiblemente `rowversion`.

Cuando dos operaciones intenten aplicar dinero al mismo saldo:

- una confirma;
- la segunda detecta que el saldo cambió;
- el Core responde `409 CONFLICTO_CONCURRENCIA`;
- no se permite saldo negativo.

## 14. Autorización propuesta

Nuevas políticas, sin cambiar las actuales:

```text
ComercialAdministracion  -> Administrador
ComercialConsulta        -> Administrador, AnalistaDAU, Auditor
FacturacionOperacion     -> Administrador, AnalistaDAU
CobrosOperacion          -> Administrador, AnalistaDAU
AuditoriaFinanciera      -> Administrador, Auditor
```

La GUI ocultará acciones no permitidas, pero el Core siempre volverá a validar el permiso.

## 15. Auditoría

Acciones mínimas:

```text
CREAR_CLIENTE_COMERCIAL
ACTUALIZAR_CLIENTE_COMERCIAL
CREAR_PRODUCTO
ACTUALIZAR_PRECIO_PRODUCTO
CREAR_SERVICIO_COBRABLE
ACTUALIZAR_TARIFA_SERVICIO
CREAR_COTIZACION
EMITIR_COTIZACION
CONVERTIR_COTIZACION
EMITIR_FACTURA
ANULAR_FACTURA
REGISTRAR_PAGO
CONFIRMAR_PAGO
APLICAR_COBRO
REVERSAR_COBRO
APROBAR_EXENCION
```

La auditoría financiera debe conservar usuario, rol, IP, ruta, método, correlationId, estados anterior/nuevo y detalle no sensible.

## 16. Recibos

El Core generará un modelo de recibo inmutable y una representación imprimible.

```text
NumeroRecibo
Fecha
Cliente
Conceptos
Moneda
Monto
Método de pago
Referencia
Usuario
Código de verificación
```

La impresión física seguirá siendo responsabilidad de Caja o de una GUI cliente. El Core entrega los datos o un documento generado; no controla la impresora.

## 17. Pruebas para no romper el sistema

Antes de modificar código se conservará una línea base de las pruebas actuales.

Nuevas pruebas:

### Unitarias

- cálculo de cotizaciones;
- cálculo de factura;
- exenciones;
- conversión de moneda;
- estados de factura;
- aplicación parcial y total;
- reversión;
- idempotencia;
- concurrencia;
- generación de recibo.

### Integración

- migraciones sobre base vacía;
- migraciones sobre una copia con datos actuales;
- autenticación y roles;
- CRUD de catálogos;
- cotización a factura;
- factura a cuenta por cobrar;
- pago a cobro;
- doble envío con la misma clave;
- dos cobros concurrentes;
- respuestas ProblemDetails.

### Regresión

- ejecutar todas las pruebas existentes del Core;
- validar endpoints consumidos por Web y Caja;
- comparar OpenAPI antes y después para confirmar que las rutas actuales no desaparecieron ni cambiaron.

## 18. Feature flags

La ampliación se habilitará gradualmente:

```json
{
  "Features": {
    "CommercialModule": false,
    "CoreAdminUi": false
  }
}
```

Durante desarrollo y pruebas se habilitará por ambiente. Las funciones actuales continúan operando aunque el módulo comercial esté desactivado.

## 19. Fases de implementación

### Fase 0 — Línea base

- compilar Core actual;
- ejecutar pruebas actuales;
- exportar OpenAPI actual;
- respaldar base de datos;
- registrar contratos usados por Caja, Web y Gateway.

### Fase 1 — Catálogo y perfiles

- CRUD administrativo de perfiles sobre `Rol`;
- clientes comerciales;
- productos;
- servicios cobrables;
- monedas, tarifas y exenciones;
- migración y pruebas.

### Fase 2 — Cotizaciones

- cotización y detalles;
- estados;
- cálculos;
- emisión y vencimiento;
- pruebas.

### Fase 3 — Facturación y cuentas por cobrar

- factura y detalles;
- emisión desde cotización;
- saldo;
- cuenta por cobrar;
- anulación controlada;
- pruebas.

### Fase 4 — Pagos y cobros

- simulación de pago;
- registro de pago;
- aplicación de cobro;
- pagos parciales;
- recibos;
- reversión;
- idempotencia;
- concurrencia;
- pruebas.

### Fase 5 — GUI administrativa

- proyecto `Indotel.Core.Admin`;
- login;
- dashboard;
- módulos CRUD;
- cotizaciones, facturas y cobros;
- auditoría y salud;
- pruebas de autorización y usabilidad.

### Fase 6 — Preparación para consumidores

- documentación OpenAPI completa;
- guía para Caja, Web e Integración;
- ejemplos de DTOs;
- pruebas de contrato;
- módulo comercial habilitado en ambiente de integración.

## 20. Criterios de aceptación

La ampliación se considera aceptable cuando:

1. todas las pruebas anteriores siguen aprobando;
2. ninguna ruta existente cambia;
3. las migraciones son aditivas;
4. no se duplican pagos con reintentos;
5. no se producen saldos negativos;
6. los documentos financieros no se borran;
7. las acciones sensibles quedan auditadas;
8. Swagger documenta los contratos nuevos;
9. la GUI usa la API y no el DbContext;
10. el módulo puede deshabilitarse mediante configuración.

## 21. Fuera de alcance inicial

- conexión bancaria real;
- procesamiento real de tarjetas;
- NCF fiscal real;
- contabilidad general completa;
- inventario avanzado;
- sustitución de Caja o Web;
- tarifas oficiales no verificadas;
- producción gubernamental certificada.

Estas integraciones se diseñarán mediante interfaces y simuladores académicos, sin afirmar conexión real.

## 22. Decisión final

Se ampliará el Core por adición, no por reemplazo. La prioridad es construir primero contratos, reglas financieras, persistencia, auditoría e idempotencia. La GUI se desarrollará después como cliente administrativo separado.

Esta estrategia permite evolucionar el proyecto sin obligar a Caja, Web o Integración a cambiar hasta que decidan consumir las rutas nuevas.
