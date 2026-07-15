# ADR: GUI operativa obligatoria del Core como aplicación separada

Estado: aceptada y obligatoria para la entrega académica

Fecha: 15/07/2026

## Contexto

El docente confirmó que el Core debe tener una interfaz gráfica propia y mostró como referencia una interfaz de sistema bancario. Por tanto, Swagger no satisface por sí solo el requisito de GUI.

El proyecto actual `Indotel.Core` es una ASP.NET Core Web API consumida por Caja, Web y API Gateway. Integrar vistas, acceso a datos y lógica visual dentro de ese mismo ejecutable aumentaría el riesgo de romper consumidores y mezclar responsabilidades.

## Decisión

Crear un proyecto nuevo dentro de la misma solución del Core:

```text
core-indotel/
├── Indotel.Core.sln
├── Indotel.Core/
├── Indotel.Core.Tests/
└── Indotel.Core.Gui/
```

`Indotel.Core.Gui` será la interfaz oficial y demostrable del Core.

La aplicación:

- usará ASP.NET Core 8 MVC y Razor Views;
- consumirá `Indotel.Core` mediante HTTP;
- no referenciará `IndotelDbContext`;
- no consultará SQL Server directamente;
- no duplicará reglas de negocio;
- almacenará JWT y refresh token del lado servidor;
- utilizará una cookie segura como identificador de sesión;
- aplicará permisos y roles emitidos por el Core;
- mostrará errores mediante ProblemDetails y `correlationId`;
- se ejecutará en un puerto independiente;
- podrá detenerse sin afectar la API;
- no requerirá cambios en Caja, Web ni Integración.

## Por qué sigue siendo GUI del Core

La separación es técnica, no funcional. Ambos proyectos forman parte de la solución y del entregable del Core:

```text
Core API = reglas, seguridad, persistencia y contratos
Core GUI = operación visual de esas reglas y contratos
```

La GUI no es otra Web ciudadana ni una modificación de Caja. Es la terminal administrativa del Core.

## Patrón visual y operativo

La GUI seguirá un patrón de aplicación bancaria interna:

- inicio de sesión;
- dashboard;
- navegación lateral por módulos;
- listados filtrables;
- formularios estructurados;
- vistas de detalle con historial;
- estados visibles;
- auditoría;
- salud del sistema;
- cola de operaciones pendientes;
- confirmación de acciones sensibles;
- doble control operador/aprobador.

No se copiará la imagen de un banco específico. El diseño será institucional, inspirado en Fluent 2 y adaptado a INDOTEL.

## Primera tecnología

```text
ASP.NET Core 8 MVC
Razor Views
Bootstrap 5
CSS institucional inspirado en Fluent
IHttpClientFactory
Cookies seguras
Sesión del lado servidor
Protección antiforgery
Políticas de autorización
```

MVC se adopta porque separa presentación, entrada y modelos, mantiene el desarrollo principal en C# y permite pruebas sin agregar una cadena de herramientas JavaScript separada.

## Módulos visuales

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

Los módulos se activarán progresivamente mediante feature flags.

## Doble autorización

Las acciones sensibles usarán un patrón operador/aprobador:

```text
Operador solicita -> PENDIENTE_APROBACION
Supervisor revisa -> APROBADA o RECHAZADA
```

El solicitante no podrá aprobar su propia operación.

Se aplicará inicialmente a:

- modificación de tarifas;
- aprobación de exenciones;
- anulación de facturas;
- reversión de cobros;
- modificación de tasas de cambio;
- activación de perfiles críticos.

## Reglas de seguridad

La GUI no debe:

- almacenar JWT en `localStorage`;
- enviar secretos al navegador;
- confiar únicamente en botones ocultos;
- acceder al `DbContext`;
- calcular saldos por su cuenta;
- permitir edición manual de saldos;
- borrar documentos financieros;
- exponer excepciones técnicas.

El Core seguirá siendo responsable de autorización, validación, transacciones, concurrencia, idempotencia y auditoría.

## Compatibilidad

La implementación será aditiva:

- no se eliminan o renombran rutas;
- no se cambian DTOs existentes;
- no se modifica `ServicioTelecom`;
- no se cambian roles actuales;
- no se realizan migraciones destructivas;
- no se modifica Caja;
- no se modifica Web;
- no se modifica Integración.

Antes y después de cada fase deben aprobarse las pruebas existentes y los smoke tests de consumidores.

## Despliegue inicial

```text
Core API: http://localhost:5085
Core GUI: http://localhost:5285
```

La GUI se ejecutará inicialmente de forma local e independiente. La integración con Caddy o el despliegue consolidado será una fase posterior y no modificará rutas actuales.

## Alternativas descartadas

### Usar solamente Swagger

Descartada porque Swagger es una consola técnica para probar endpoints, no una interfaz operativa completa.

### Agregar páginas dentro del mismo proyecto API

Descartada por acoplamiento, riesgo de despliegue y mezcla de responsabilidades.

### Conectar la GUI directamente a SQL Server

Descartada porque evitaría las reglas, autorización, auditoría y contratos del Core.

### Convertir la Web ciudadana en GUI del Core

Descartada porque cambiaría el alcance y podría romper la experiencia existente.

### Modificar Caja para representar el Core

Descartada porque Caja tiene un alcance propio y debe permanecer independiente.

## Consecuencias

Positivas:

- cumple explícitamente el requisito del docente;
- reduce el riesgo para los consumidores actuales;
- mantiene una arquitectura API-first;
- permite una interfaz bancaria y administrativa real;
- facilita pruebas, seguridad y evolución.

Negativas:

- agrega un proyecto adicional;
- requiere administrar sesión y clientes HTTP;
- exige mantener DTOs visuales y contratos;
- añade trabajo de diseño y pruebas de interfaz.

Las consecuencias negativas son aceptables y forman parte del alcance requerido.

## Documentación relacionada

```text
docs/PLAN_EVOLUCION_CORE_COMERCIAL_Y_GUI.md
docs/PLAN_IMPLEMENTACION_GUI_CORE_OBLIGATORIA.md
```

## Resultado

El Core INDOTEL tendrá una GUI propia, operativa y visible. La GUI será parte de la solución del Core, consumirá su API y permitirá demostrar usuarios, perfiles, administración, auditoría y los futuros módulos comerciales y financieros sin romper Caja, Web o Integración.