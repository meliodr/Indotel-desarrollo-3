# ADR: GUI administrativa del Core como aplicación separada

Estado: aceptada para implementación futura

Fecha: 15/07/2026

## Contexto

El proyecto requiere mejorar el llamado "GUI del Core" y agregar módulos administrativos para usuarios, perfiles, productos, clientes, servicios cobrables, cotizaciones, facturas, cuentas por cobrar, pagos y cobros.

El proyecto actual `Indotel.Core` es una ASP.NET Core Web API. Swagger es una interfaz técnica para explorar endpoints, pero no es una aplicación de operaciones administrativas.

Incorporar páginas, acceso directo a Entity Framework y lógica visual dentro de la API aumentaría el acoplamiento y podría afectar a Caja, Web, Gateway e Integración.

## Decisión

Crear una aplicación independiente dentro de la solución:

```text
Indotel.Core.Admin
```

La aplicación administrativa:

- consumirá `Indotel.Core` mediante HTTP;
- no referenciará `IndotelDbContext`;
- no consultará SQL Server directamente;
- no duplicará reglas de negocio;
- almacenará la sesión y los tokens del Core del lado servidor;
- utilizará permisos y roles emitidos por el Core;
- mostrará errores usando el contrato ProblemDetails y correlationId;
- podrá ejecutarse o desplegarse sin modificar Caja, Web o Gateway.

## Motivación empresarial

Las plataformas grandes suelen separar la API de las interfaces operativas. Los dashboards empresariales organizan recursos por módulos, ofrecen búsquedas y filtros, aplican permisos por rol y muestran transacciones, clientes, productos, facturas, reportes y registros de integración sin permitir que la pantalla sustituya las reglas del backend.

Para INDOTEL se aplicarán estos principios:

1. API-first y contratos estables.
2. Interfaz administrativa separada.
3. Navegación modular.
4. Acciones visibles según rol, pero siempre verificadas por el Core.
5. Historial y auditoría disponibles en la vista de detalle.
6. Operaciones financieras inmutables o reversables, no borrables.
7. Diseño consistente mediante un sistema de componentes.
8. Entorno de demostración separado de datos reales.

## Tecnología

Primera versión:

- ASP.NET Core 8;
- Razor Pages o MVC;
- Bootstrap 5;
- estilos y tokens visuales inspirados en Fluent;
- `IHttpClientFactory`;
- cookie segura para la sesión del portal;
- tokens JWT y refresh token almacenados en servidor;
- protección CSRF;
- autorización por política;
- configuración de URL del Core por ambiente.

Esta decisión reduce la curva de aprendizaje porque mantiene C# y .NET en toda la solución. No impide migrar el frontend a React, Angular o Blazor en el futuro, porque el contrato seguirá siendo la API.

## Estructura visual propuesta

### Navegación lateral

```text
Inicio
Usuarios
Perfiles
Clientes
Productos
Servicios cobrables
Cotizaciones
Facturas
Cuentas por cobrar
Pagos y cobros
Auditoría
Salud del sistema
Configuración
```

### Dashboard

- tarjetas de totales;
- facturas pendientes y vencidas;
- cuentas por cobrar;
- pagos pendientes de conciliación;
- cobros del día;
- actividad reciente;
- alertas del sistema;
- acceso a auditoría por correlationId.

### Listados

- búsqueda;
- filtros;
- paginación;
- columnas configuradas por módulo;
- etiquetas de estado;
- acciones contextuales;
- exportación futura;
- estados vacíos y mensajes claros.

### Formularios

- validación en cliente para usabilidad;
- validación definitiva en Core;
- mensajes de error junto al campo;
- confirmación para acciones sensibles;
- resumen antes de emitir factura, aplicar cobro o reversar.

### Vista de detalle

- encabezado con estado y número;
- información principal;
- detalles monetarios;
- relaciones;
- historial;
- auditoría;
- acciones permitidas.

## Seguridad

La GUI no debe:

- almacenar JWT en `localStorage`;
- enviar secretos al navegador;
- confiar solamente en botones ocultos;
- mostrar datos financieros sin autorización;
- permitir edición libre de saldos;
- permitir borrado de documentos financieros;
- exponer excepciones técnicas.

El Core seguirá siendo responsable de autorización, validación, transacciones, concurrencia, idempotencia y auditoría.

## Swagger

Swagger se mantendrá y mejorará como portal técnico:

- contratos;
- ejemplos;
- roles;
- errores;
- estados;
- idempotencia;
- pruebas manuales de endpoints.

No se intentará transformar Swagger en el dashboard administrativo.

## Despliegue seguro

La GUI se agregará inicialmente sin modificar el despliegue actual.

Etapas:

1. crear proyecto y pruebas;
2. ejecutar localmente en puerto independiente;
3. validar contra Core local;
4. mantener `CoreAdminUi=false` por defecto;
5. integrar al despliegue solo después de pruebas;
6. agregar ruta de proxy en una fase posterior, sin cambiar las rutas existentes.

## Consecuencias positivas

- menor riesgo para Core actual;
- no rompe consumidores;
- interfaz reemplazable;
- contratos reutilizables;
- mejor seguridad;
- pruebas más sencillas;
- separación entre experiencia de usuario y reglas financieras.

## Consecuencias negativas

- se agrega un proyecto adicional;
- requiere administrar sesión del portal;
- requiere DTOs y clientes HTTP bien definidos;
- algunas operaciones necesitan llamadas adicionales a la API.

Estas desventajas son aceptables frente al riesgo de mezclar UI, reglas y persistencia.

## Alternativas descartadas

### Páginas dentro del proyecto API

Descartada por acoplamiento, aumento del riesgo de despliegue y mezcla de responsabilidades.

### GUI conectada directamente a SQL Server

Descartada porque evita reglas, autorización, auditoría y contratos del Core.

### Modificar la Web ciudadana actual para convertirla en panel administrativo

Descartada porque podría romper el alcance y la experiencia de la Web existente.

### Usar únicamente Swagger

Descartada porque Swagger es una herramienta técnica, no una aplicación operativa para usuarios administrativos.

## Resultado

La GUI del Core será una aplicación administrativa separada y API-first. El Core continuará siendo headless desde el punto de vista del dominio y conservará toda la lógica de negocio.
