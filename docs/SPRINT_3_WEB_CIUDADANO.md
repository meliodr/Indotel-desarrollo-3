# Sprint 3 — Web del ciudadano

## Estado

**Implementación y validación automática completadas en la rama `web`.**

Commit validado: `7cf27ac`.

La aceptación funcional definitiva conserva pendiente el flujo integrado Web → Gateway → Core → SQL Server y las pruebas manuales de caída, sesión y roles.

## Evidencia de validación automática

Validación ejecutada en Ubuntu 24.04 mediante `scripts/validar_sprint3_web.sh` con SDK .NET 8.0.128.

Resultados:

- Restauración: correcta.
- Compilación Release de Web: correcta, 0 advertencias y 0 errores.
- Compilación Release de pruebas: correcta, 0 advertencias y 0 errores.
- Pruebas automáticas: 12 ejecutadas, 12 correctas, 0 errores, 0 omitidas.
- Publicación de comprobación: correcta.
- Cobertura Cobertura generada localmente.

Mensaje final obtenido:

```text
Sprint 3 validado: restauracion, compilacion, pruebas y publicacion completadas.
```

Durante las pruebas aparecieron advertencias de Data Protection indicando que las claves locales se almacenan sin un cifrador XML configurado. No impidieron la validación y se resolverán en el Sprint 6 de despliegue y endurecimiento de secretos.

## Objetivo

Completar el portal ciudadano con manejo uniforme de sesión, indisponibilidad, seguridad de tokens y experiencia controlada ante fallos.

## Correcciones implementadas

### Consumo exclusivo del Gateway

- La Web apunta por defecto a `http://localhost:5185`.
- La URL se configura mediante `ApiSettings:GatewayBaseUrl` o la variable:

```text
ApiSettings__GatewayBaseUrl
```

- Los clientes HTTP existentes conservan compatibilidad, pero todos utilizan la misma URL del Gateway.
- La Web ya no debe apuntar directamente al puerto interno del Core.

### Fallos de red y disponibilidad

- `GatewayTransportHandler` centraliza:
  - conexión rechazada;
  - timeout;
  - HTTP 502;
  - HTTP 503;
  - HTTP 504;
  - propagación de `X-Correlation-ID`.
- Los fallos se convierten en `GatewayUnavailableException`.
- `HomeController` presenta una página amigable con estado HTTP 503.
- La página indica que los datos no fueron enviados y ofrece Reintentar.
- Se agregaron páginas uniformes para 404, 500 y otros códigos HTTP.

### Sesión y tokens

- JWT y refresh token ya no se guardan como claims dentro de la cookie.
- La cookie contiene únicamente identidad, rol y un identificador aleatorio de sesión.
- Los tokens se almacenan del lado servidor mediante `IDistributedCache`.
- La implementación actual utiliza memoria distribuida local; para varias instancias se debe sustituir por Redis o SQL distribuido.
- La renovación de token se sincroniza por sesión.
- Una caída temporal durante refresh conserva la sesión y muestra 503.
- Un refresh rechazado, inválido o expirado elimina la sesión.
- Logout elimina siempre la sesión local aunque Gateway o Core no respondan.
- Las claves de Data Protection se persisten en una carpeta excluida de Git.

### Cookies

- Cookie `HttpOnly`.
- `SameSite=Lax`.
- `Secure=Always` fuera de Development.
- Duración alineada con el refresh token.
- Claves de cifrado persistentes mediante Data Protection.

### Funcionalidad

- Panel con paginación de diez reclamaciones.
- Totales globales independientes de la página mostrada.
- Paginación de notificaciones preparada mediante parámetros de consulta.
- Historial, documentos, descarga y notificaciones existentes se mantienen.
- Formularios desactivan sus botones después de un envío válido para evitar doble clic.
- No se agregaron reintentos automáticos de POST, PUT, PATCH o carga de documentos.

### Errores y observabilidad

- El correlationId del Gateway se propaga a la respuesta Web.
- Los logs registran ruta, método y correlationId, sin registrar JWT, refresh tokens ni cuerpos de documentos.
- Errores inesperados regresan una página 500 sin detalles técnicos sensibles.

## Pruebas automáticas

Además de las seis pruebas existentes, se agregaron pruebas para:

- página 503;
- página 404;
- almacenamiento de tokens del lado servidor;
- eliminación de sesión del almacén;
- respuesta 503 convertida en excepción controlada;
- error de red convertido en excepción controlada.

## Validación automática

```bash
cd /home/jarry/indotel-prueba-web
git fetch origin
git reset --hard origin/web
bash scripts/validar_sprint3_web.sh
```

## Ejecución integrada

Orden recomendado:

```bash
# Terminal 1: Core
cd /home/jarry/indotel-prueba-core
dotnet run --project core-indotel/Indotel.Core/Indotel.Core.csproj --urls http://localhost:5085

# Terminal 2: Gateway
cd /home/jarry/indotel-prueba-api-gateway
dotnet run --project api-gateway/Indotel.ApiGateway/Indotel.ApiGateway.csproj --urls http://localhost:5185

# Terminal 3: Web
cd /home/jarry/indotel-prueba-web
dotnet run --project INDOTEL.Web/INDOTEL.WEB.csproj --urls http://localhost:5234
```

## Pruebas manuales obligatorias

1. Registro ciudadano.
2. Login ciudadano.
3. Rechazo de Administrador en Web.
4. Crear reclamación.
5. Consultar panel paginado.
6. Consultar detalle e historial.
7. Subir y descargar documento.
8. Consultar y marcar notificación.
9. Logout con servicios disponibles.
10. Logout con Gateway apagado.
11. Gateway apagado durante navegación: página 503.
12. Core apagado con Gateway activo: página 503.
13. Expirar access token y comprobar refresh.
14. Apagar Core durante refresh y comprobar que la cookie no se elimina.
15. Reactivar Core y comprobar que la sesión puede continuar.

## Criterio de cierre

- Compilación Release aprobada.
- Pruebas automáticas aprobadas.
- Publicación aprobada.
- Flujo ciudadano completo aprobado.
- Gateway/Core apagados no tumban la Web.
- Caída temporal durante refresh no elimina la sesión.
- Roles internos no entran al portal ciudadano.
