# Sprint 4 — Caja interna

## Estado

**Implementación completada y validación local Linux aprobada en la rama `caja`.**

Commit validado localmente: `0197a6a`.

La validación ejecutada en Ubuntu 24.04 confirmó:

- restauración correcta del proyecto Caja;
- 15 pruebas de infraestructura ejecutadas y aprobadas;
- 0 errores y 0 pruebas omitidas;
- cobertura Cobertura generada;
- configuración `net8.0-windows`, WinForms y `EnableWindowsTargeting` correcta.

La compilación visual y publicación `win-x64` requieren Windows Desktop SDK. Esa etapa se ejecuta mediante GitHub Actions con `windows-latest` y debe conservarse como evidencia antes del cierre funcional definitivo.

## Objetivo

Convertir Caja en un cliente interno seguro y resiliente que consuma exclusivamente el API Gateway, respete los roles del Core y no se cierre ante fallos de red.

## Correcciones implementadas

### Plataforma

- Migración de .NET Framework 4.8.1 a `net8.0-windows`.
- Proyecto SDK-style con WinForms.
- Compilación cruzada declarada mediante `EnableWindowsTargeting`.
- SDK fijado en .NET 8.
- Eliminación de Entity Framework y paquetes heredados.
- Eliminación del almacenamiento de token duplicado.

### Comunicación

- URL centralizada en `App.config` mediante `ApiBaseUrl`.
- Dirección predeterminada: `http://localhost:5185/`.
- Caja consume el Gateway y no el puerto interno del Core.
- Un único `HttpClient` compartido.
- Timeout total y timeout de conexión configurables.
- Pool de conexiones reutilizable.
- Propagación de `X-Correlation-ID`.
- JWT agregado por solicitud sin almacenarlo en configuración.

### Resiliencia

- Captura de `HttpRequestException`.
- Captura de timeout.
- Captura de JSON inválido.
- Mensajes controlados para 400, 401, 403, 404, 409, 423, 429, 502, 503 y 504.
- Lectura de `ProblemDetails`, `mensaje`, `codigo` y `correlationId`.
- Ningún error de red debe cerrar la aplicación.
- Registro local sin tokens, contraseñas ni cuerpos de documentos.
- Manejo global de excepciones de interfaz y tareas.

### Sesión

- Refresh sincronizado para evitar renovaciones simultáneas.
- Credenciales incorrectas ya no se muestran como sesión expirada cuando el Core devuelve un mensaje específico.
- Logout limpia siempre la sesión local.
- Caída de Gateway durante logout produce advertencia, no bloqueo.
- Tokens permanecen únicamente en memoria.

### Roles

- Administrador: acceso completo.
- AnalistaDAU: gestión operativa.
- Auditor: consulta solamente.
- Ciudadano y Prestadora: acceso rechazado en Caja.
- Botones de creación, registro, cambio de estado y resolución se desactivan para Auditor.
- El Core conserva la autorización definitiva.

### Reclamaciones

- Búsqueda paginada mediante `/api/reclamaciones/buscar`.
- Búsqueda por cédula reactivada.
- Búsqueda por expediente.
- Transiciones consultadas mediante `/api/reclamaciones/{id}/transiciones`.
- La interfaz dejó de codificar una ruta lineal fija de estados.
- Operaciones de escritura desactivan el botón durante el envío.
- Resolución valida comentario y monto no negativo.
- El botón de respuesta de prestadora se retira del flujo de Caja, porque corresponde al módulo autorizado de la prestadora.

### Formularios

- Navegación modal para evitar ventanas ocultas acumuladas.
- Cancelar cierra el formulario actual sin crear otro panel.
- Validación de cédula numérica de 11 dígitos.
- Validación de correo.
- Validación de provincia, municipio, título y descripción.
- Mensajes con referencia de correlación cuando está disponible.

### Pruebas y entrega

- Proyecto `INDOTEL_CAJA.Tests`.
- Pruebas de roles, cédula y referencias de error.
- CI en `windows-latest`.
- Publicación automática `win-x64`.
- Artefacto descargable desde GitHub Actions.

## Configuración

`INDOTEL_CAJA(REAL)/App.config`:

```xml
<add key="ApiBaseUrl" value="http://localhost:5185/" />
<add key="ApiTimeoutSeconds" value="20" />
<add key="ApiConnectTimeoutSeconds" value="5" />
```

En producción debe utilizarse la URL HTTPS real del Gateway.

## Validación local Linux

```bash
cd /home/jarry/indotel-prueba-caja
git fetch origin
git reset --hard origin/caja
bash scripts/validar_sprint4_caja.sh
```

Resultado validado:

```text
Validacion local Linux completada:
- restauracion de Caja: correcta;
- pruebas de infraestructura: correctas;
- configuracion net8.0-windows/WinForms: correcta.
```

## Validación Windows pendiente

El workflow `.github/workflows/caja-ci.yml` debe confirmar:

1. restauración;
2. compilación Release;
3. 15 pruebas de infraestructura;
4. publicación `win-x64`;
5. generación del artefacto `indotel-caja-win-x64`.

## Pruebas manuales pendientes

1. Login de Administrador.
2. Login de AnalistaDAU.
3. Login de Auditor y comprobación de solo lectura.
4. Rechazo de Ciudadano y Prestadora.
5. Gateway apagado durante login.
6. Core apagado con Gateway activo.
7. Logout sin conexión.
8. Búsqueda por cédula y expediente.
9. Transiciones dinámicas por estado.
10. Doble clic en operaciones de escritura.
11. Registro de ciudadano.
12. Registro, resolución y cambio de estado de reclamación.
13. Ejecución del artefacto en Windows limpio.
