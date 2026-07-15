# Plan de implementación de la GUI obligatoria del Core INDOTEL

Rama de trabajo: `proyecto-revisado-funcionando`

Fecha: 15/07/2026

Estado: decisión aprobada para implementación incremental.

## 1. Decisión principal

El Core INDOTEL tendrá una interfaz gráfica propia y demostrable.

La interfaz no será Swagger. Swagger continuará como herramienta técnica para desarrolladores.

La GUI del Core será una aplicación web interna incluida dentro de la solución del Core:

```text
core-indotel/
├── Indotel.Core.sln
├── Indotel.Core/          API y reglas del Core
├── Indotel.Core.Tests/    pruebas existentes
└── Indotel.Core.Gui/      interfaz operativa del Core
```

La GUI se presentará académicamente como parte del módulo Core, aunque técnicamente permanezca separada de la API para proteger las reglas y los contratos existentes.

## 2. Objetivo de la GUI

Permitir que personal interno autorizado opere y supervise el Core mediante pantallas, sin depender de Swagger ni escribir JSON manualmente.

La GUI permitirá:

- validar usuario y contraseña;
- visualizar un dashboard del Core;
- administrar usuarios y perfiles;
- consultar auditorías y salud del sistema;
- operar los nuevos módulos comerciales;
- consultar clientes, productos y servicios cobrables;
- crear y revisar cotizaciones;
- emitir y consultar facturas;
- consultar cuentas por cobrar;
- registrar, aprobar, aplicar o reversar pagos y cobros según permisos;
- mostrar recibos;
- mostrar historial y trazabilidad;
- gestionar una cola de aprobaciones para acciones sensibles.

## 3. Principio de arquitectura

```text
Navegador
    ↓
Indotel.Core.Gui
    ↓ HTTP + JWT
Indotel.Core API
    ↓
Servicios y reglas del Core
    ↓
Entity Framework Core
    ↓
SQL Server
```

Reglas obligatorias:

1. `Indotel.Core.Gui` no referencia `IndotelDbContext`.
2. La GUI no consulta SQL Server directamente.
3. La GUI no calcula saldos, totales, exenciones ni estados financieros.
4. Toda operación pasa por endpoints del Core.
5. El Core vuelve a validar permisos aunque la GUI oculte botones.
6. La GUI solo presenta datos y captura instrucciones del usuario.
7. Caja, Web y Gateway no se modifican durante esta fase.

## 4. Tecnología elegida

La primera versión usará:

```text
ASP.NET Core 8 MVC
Razor Views
Bootstrap 5
CSS propio inspirado en Fluent 2
IHttpClientFactory
Cookie segura para la sesión de la GUI
JWT y refresh token del Core almacenados del lado servidor
DataAnnotations y validación del lado cliente y servidor
Protección antiforgery
Políticas de autorización
```

Se elige MVC con Razor porque:

- mantiene todo el desarrollo principal en C# y .NET;
- funciona bien en Visual Studio Code;
- permite separar Models, Views y Controllers;
- es sencillo de probar;
- evita agregar React, Angular, Node.js y otra cadena de compilación;
- permite construir rápidamente formularios, tablas, filtros y vistas de detalle;
- facilita proteger los tokens del Core fuera de `localStorage`.

No se usará Blazor Server en la primera entrega para evitar dependencia permanente de SignalR y estado de conexión. No se usará React o Angular en la primera entrega para no aumentar la complejidad académica y operativa.

## 5. Identidad visual

La GUI tendrá estilo institucional y operativo, inspirado en aplicaciones bancarias internas:

- fondo claro y limpio;
- barra lateral oscura o azul institucional;
- encabezado superior con usuario, rol, ambiente y cierre de sesión;
- tarjetas de resumen;
- tablas densas, legibles y filtrables;
- etiquetas de estado con color y texto;
- formularios divididos en secciones;
- confirmaciones para acciones sensibles;
- mensajes de error no técnicos;
- `correlationId` visible para soporte;
- modo responsive para escritorio y tablet;
- contraste y navegación por teclado.

No se copiará la identidad visual de un banco específico.

## 6. Navegación principal

```text
Inicio
Operaciones pendientes
Usuarios
Perfiles
Clientes
Productos
Servicios cobrables
Cotizaciones
Facturas
Cuentas por cobrar
Pagos
Cobros y recibos
Auditoría
Salud del sistema
Configuración
```

Los módulos no implementados permanecerán ocultos mediante feature flags hasta que sus endpoints y pruebas estén terminados.

## 7. Pantallas de la primera entrega

La primera entrega debe demostrar una GUI real sin introducir todavía riesgo financiero.

### 7.1 Inicio de sesión

Campos:

- correo;
- contraseña;
- botón Iniciar sesión;
- mensaje de credenciales inválidas;
- mensaje de bloqueo temporal;
- indicador de ambiente de desarrollo.

Flujo:

```text
GUI -> POST /api/auth/login -> Core
Core devuelve JWT + refresh token
GUI crea sesión segura del lado servidor
Navegador recibe solo cookie de sesión
```

### 7.2 Dashboard inicial

Datos obtenidos desde endpoints existentes:

- total de ciudadanos;
- total de prestadoras;
- reclamaciones abiertas;
- reclamaciones vencidas;
- resoluciones;
- autorizaciones;
- licencias por vencer;
- actividad reciente;
- estado de la API y base de datos.

### 7.3 Usuarios

Operaciones:

- listar;
- buscar;
- ver detalle;
- crear;
- editar;
- activar o desactivar;
- cambiar contraseña administrativa.

La GUI consumirá los endpoints existentes de `/api/usuarios` sin cambiar sus contratos.

### 7.4 Perfiles

Primero se mostrará una consulta de roles existentes.

Después de crear los endpoints de perfiles se habilitarán:

- crear perfil;
- editar nombre y descripción;
- activar o desactivar;
- proteger perfiles base;
- impedir desactivar perfiles utilizados de forma crítica.

### 7.5 Auditoría

La GUI mostrará:

- fecha;
- usuario;
- rol;
- acción;
- entidad;
- estado anterior;
- estado nuevo;
- ruta;
- IP;
- `correlationId`;
- nivel.

### 7.6 Salud del sistema

Mostrará:

- estado del Core;
- estado de SQL Server;
- ambiente;
- versión;
- hora del servidor;
- último error conocido sin exponer información sensible.

## 8. Patrón bancario: operador y aprobador

Para acciones sensibles se implementará un flujo de doble control:

```text
Operador crea solicitud
        ↓
PENDIENTE_APROBACION
        ↓
Supervisor revisa
        ├── APROBADA -> Core ejecuta operación
        └── RECHAZADA -> no se modifica el documento financiero
```

Una misma persona no podrá crear y aprobar la misma operación.

Acciones candidatas:

- modificar una tarifa vigente;
- aprobar una exención;
- anular una factura;
- reversar un cobro;
- cambiar una tasa de cambio;
- reabrir un documento cerrado;
- activar un perfil administrativo crítico.

Entidad prevista:

```text
SolicitudAprobacion
- Id
- TipoOperacion
- Entidad
- EntidadId
- PayloadHash
- Resumen
- Estado
- UsuarioSolicitanteId
- UsuarioAprobadorId
- FechaSolicitud
- FechaDecision
- ComentarioDecision
- CorrelationId
```

Este flujo se implementará después del esqueleto de la GUI y antes de habilitar operaciones financieras sensibles.

## 9. Módulos comerciales que soportará la GUI

### Clientes comerciales

- listado;
- detalle;
- creación y edición;
- vinculación opcional con ciudadano o prestadora;
- estado activo/inactivo;
- exenciones autorizadas.

### Productos

- catálogo;
- código;
- precio;
- moneda;
- inventario cuando aplique;
- estado.

### Servicios cobrables

- categoría;
- precio configurable;
- moneda;
- vigencia;
- requisito de prestadora o resolución;
- permiso de exención.

### Cotizaciones

- crear encabezado;
- agregar detalles;
- calcular totales en el Core;
- emitir;
- aceptar;
- vencer;
- convertir a factura.

### Facturas

- emitir desde cotización;
- consultar detalle;
- mostrar saldo;
- mostrar historial;
- anular mediante aprobación;
- no eliminar.

### Cuentas por cobrar

- consultar pendientes;
- filtrar por cliente, estado y vencimiento;
- ver aplicaciones de cobro;
- no editar saldos manualmente.

### Pagos y cobros

- simular pago;
- registrar pago;
- revisar pagos pendientes o en conciliación;
- aplicar cobro;
- aplicar pagos parciales;
- generar recibo;
- reversar con doble autorización;
- usar idempotencia.

## 10. Diseño de cada tipo de pantalla

### Listados

- título y descripción;
- botón principal;
- búsqueda;
- filtros;
- paginación;
- tabla;
- estado vacío;
- acción Ver detalle;
- acciones secundarias según permiso.

### Formularios

- datos agrupados;
- campos obligatorios visibles;
- validación junto al campo;
- botones Guardar y Cancelar;
- resumen de la operación;
- confirmación para operaciones sensibles.

### Detalle

- número o identificador;
- estado actual;
- datos principales;
- montos;
- relaciones;
- historial;
- auditoría;
- acciones permitidas;
- `correlationId` de operaciones recientes.

### Operaciones pendientes

- quién solicitó;
- tipo de operación;
- entidad afectada;
- valores actuales y propuestos;
- fecha;
- motivo;
- Aprobar;
- Rechazar;
- comentario obligatorio.

## 11. Seguridad de la GUI

1. Cookie `HttpOnly`, `Secure` y `SameSite` apropiado.
2. Protección antiforgery en formularios.
3. JWT y refresh token almacenados del lado servidor.
4. Renovación automática de token mediante el Core.
5. Cierre de sesión que revoque refresh token.
6. Timeout de sesión por inactividad.
7. Menú y botones por rol.
8. Autorización definitiva siempre en el Core.
9. No mostrar excepciones, cadenas de conexión o secretos.
10. Auditoría de login, logout y acciones administrativas.
11. Encabezado `X-Correlation-ID` en las llamadas al Core.
12. Política de contenido y encabezados de seguridad.

## 12. Estrategia para no romper nada

### Contratos

- no eliminar rutas actuales;
- no renombrar rutas actuales;
- no cambiar DTOs actuales;
- no cambiar respuestas actuales;
- no cambiar roles actuales;
- no reutilizar `/api/servicios` para servicios cobrables.

### Base de datos

- migraciones solamente aditivas;
- nuevas tablas y nuevos índices;
- ninguna eliminación o renombrado de columnas existentes;
- datos semilla nuevos controlados;
- respaldo antes de aplicar migraciones.

### Ejecución

Puertos de desarrollo propuestos:

```text
Core API:  http://localhost:5085
Core GUI:  http://localhost:5285
```

La GUI podrá iniciarse o detenerse sin afectar la API.

### Feature flags

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

### Compatibilidad

Antes y después de cada fase deben pasar:

- compilación del Core;
- 20 pruebas existentes del Core;
- pruebas de contratos de endpoints actuales;
- smoke de autenticación;
- smoke de reclamaciones;
- ejecución de Caja sin cambios;
- ejecución de Web sin cambios;
- validación de Integración sin cambios.

## 13. Organización del proyecto GUI

```text
Indotel.Core.Gui/
├── Controllers/
│   ├── AccountController.cs
│   ├── DashboardController.cs
│   ├── UsuariosController.cs
│   ├── PerfilesController.cs
│   └── AuditoriaController.cs
├── Clients/
│   ├── CoreApiClient.cs
│   ├── AuthApiClient.cs
│   └── TokenRefreshHandler.cs
├── Models/
│   ├── ViewModels/
│   └── ApiContracts/
├── Services/
│   ├── CoreSessionService.cs
│   └── PermissionService.cs
├── Views/
├── ViewComponents/
├── Filters/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── images/
├── Program.cs
└── appsettings.json
```

No se compartirán entidades EF directamente con la GUI. Se usarán contratos HTTP o DTOs de presentación.

## 14. Plan de trabajo por fases

### Fase 0 — Línea base

- congelar commit de referencia;
- compilar Core;
- ejecutar 20 pruebas;
- exportar OpenAPI;
- registrar endpoints actuales;
- respaldar base;
- documentar puertos y credenciales de demostración.

Criterio de salida: línea base reproducible y sin cambios funcionales.

### Fase 1 — Esqueleto de la GUI

- crear `Indotel.Core.Gui`;
- agregarlo a `Indotel.Core.sln`;
- configurar MVC, estáticos, sesión y `HttpClientFactory`;
- crear layout institucional;
- crear navegación lateral y encabezado;
- crear página de error y estado vacío;
- agregar feature flag.

Criterio de salida: la GUI abre de forma independiente y no cambia el Core.

### Fase 2 — Autenticación y sesión

- pantalla de login;
- consumo de `/api/auth/login`;
- sesión segura;
- refresh token;
- logout;
- protección de rutas;
- menú por rol.

Criterio de salida: Administrador, AnalistaDAU y Auditor ven funciones distintas sin cambiar el login del Core.

### Fase 3 — Dashboard y módulos actuales

- dashboard;
- usuarios;
- roles en modo consulta;
- auditoría;
- salud;
- actividad reciente.

Criterio de salida: el maestro puede operar y consultar funciones reales del Core desde pantallas.

### Fase 4 — Perfiles y base comercial

- endpoints de perfiles;
- clientes comerciales;
- productos;
- servicios cobrables;
- pantallas correspondientes;
- migración aditiva;
- pruebas.

### Fase 5 — Cotizaciones y facturación

- cotizaciones y detalles;
- factura comercial;
- cuentas por cobrar;
- estados;
- pantallas;
- pruebas de cálculo y concurrencia.

### Fase 6 — Pagos y cobros

- simulación;
- pago;
- cobro;
- aplicación;
- recibo;
- reversión;
- idempotencia;
- transacciones explícitas;
- pantallas.

### Fase 7 — Doble autorización

- solicitudes pendientes;
- operador y supervisor;
- aprobación o rechazo;
- auditoría;
- pantallas de comparación.

### Fase 8 — Endurecimiento y defensa

- pruebas de seguridad;
- pruebas de accesibilidad;
- pruebas de contratos;
- capturas y manual;
- guion de demostración;
- documentación final.

## 15. Primera demostración al maestro

La primera demostración debe mostrar:

1. iniciar Core API;
2. iniciar Core GUI;
3. iniciar sesión como Administrador;
4. visualizar dashboard;
5. consultar usuarios;
6. crear un usuario de prueba;
7. desactivarlo;
8. consultar auditoría de la acción;
9. abrir salud del sistema;
10. cerrar sesión;
11. demostrar que Caja, Web e Integración siguen funcionando igual.

Esta demostración prueba que el Core posee una GUI real antes de introducir los módulos financieros.

## 16. Definición de terminado

La GUI del Core se considera terminada cuando:

- forma parte de `Indotel.Core.sln`;
- se ejecuta en puerto independiente;
- posee login real contra el Core;
- no almacena JWT en el navegador;
- aplica roles;
- tiene dashboard, navegación y pantallas operativas;
- consume únicamente endpoints HTTP;
- no accede a SQL Server;
- las acciones quedan auditadas;
- los errores muestran `correlationId`;
- los módulos comerciales y financieros tienen pruebas;
- las operaciones sensibles usan aprobación;
- las pruebas antiguas continúan pasando;
- Caja, Web e Integración no requieren modificaciones.

## 17. Decisión final

El Core tendrá una GUI propia y visible. La GUI será parte del entregable académico del Core, pero estará desacoplada técnicamente de la API y la base de datos. Esta decisión satisface el requisito del maestro y mantiene una arquitectura segura, demostrable y ampliable.