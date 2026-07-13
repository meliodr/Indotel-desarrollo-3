# Arquitectura final — Sistema Digital INDOTEL

## Alcance

Prototipo académico avanzado para gestión de reclamaciones ciudadanas. No se presenta como plataforma gubernamental certificada ni como sustituto de sistemas institucionales reales.

## Diagrama lógico

```text
                              INTERNET / RED INTERNA
                                       |
                                  HTTPS 8443
                                       |
                                +--------------+
                                |    Caddy     |
                                | TLS / Proxy  |
                                +------+-------+
                                       |
                    +------------------+------------------+
                    |                                     |
              rutas Web                              /api y /health
                    |                                     |
             +------+-------+                     +-------+-------+
             | Portal Web   |                     | API Gateway   |
             | Ciudadano    |                     | :8080 interno |
             +--------------+                     +-------+-------+
                                                            |
                                                JWT + correlationId
                                                            |
                                                    +-------+-------+
                                                    | Core ASP.NET |
                                                    | :8080 interno|
                                                    +-------+-------+
                                                            |
                                                     EF Core / SQL
                                                            |
                                                    +-------+-------+
                                                    | SQL Server   |
                                                    | red backend  |
                                                    +---------------+

Caja WinForms (Windows) ---- HTTPS ----> Caddy / API Gateway
```

## Responsabilidades

### Core

- Autoridad de autenticación y autorización.
- Reglas de negocio y máquina de estados.
- Propiedad de ciudadanos, reclamaciones y documentos.
- JWT, refresh token, revocación y bloqueo.
- Persistencia, transacciones, auditoría y migraciones.
- Health de proceso y readiness de SQL Server.

### API Gateway

- URL estable para Web y Caja.
- Enrutamiento al Core.
- Timeout, circuit breaker y reintentos solo para consultas seguras.
- Rate limiting.
- Propagación de JWT y `X-Correlation-ID`.
- Conversión de caída del Core a HTTP 503 controlado.
- No contiene reglas de negocio ni acceso a base de datos.

### Web ciudadano

- Registro, login, reclamaciones y seguimiento.
- Tokens almacenados del lado servidor.
- Cookie segura de sesión.
- Manejo visual de 403, 404, 500 y 503.
- Conservación de sesión ante caída temporal durante refresh.
- No accede directamente al Core ni a SQL Server.

### Caja interna

- Cliente WinForms para Administrador, AnalistaDAU y Auditor.
- Auditor en solo lectura.
- Ciudadano y Prestadora rechazados.
- Búsqueda, registro, transición, resolución y cierre.
- Transiciones consultadas al Core.
- Manejo de timeout, caída, JSON inválido y errores HTTP.
- No accede directamente a SQL Server.

### Caddy

- Terminación HTTPS para el despliegue de demostración.
- Entrada única pública.
- Encabezados básicos de seguridad.
- Enrutamiento Web/Gateway.
- El certificado local usa la CA interna de Caddy.

### SQL Server

- Persistencia de usuarios, ciudadanos, reclamaciones, documentos, estados, auditorías y refresh tokens.
- No se expone al host en el Compose de release.
- Volumen persistente y volumen de respaldos.

## Redes Docker

- `frontend`: Caddy, Web y Gateway.
- `backend`: Gateway, Core y SQL Server.
- `backend` es interna y no publica puertos.
- Web no participa en `backend` y no puede alcanzar SQL Server.

## Flujos principales

### Ciudadano

```text
Navegador -> Caddy -> Web -> Gateway -> Core -> SQL
```

### Caja

```text
Caja Windows -> HTTPS Caddy -> Gateway -> Core -> SQL
```

### Error del Core

```text
Core apagado -> Gateway detecta conexión/timeout -> HTTP 503 ProblemDetails
              -> Web/Caja muestran mensaje controlado
```

### Sesión

```text
Login -> Core emite access token + refresh token
Web  -> tokens en cache del servidor; cookie contiene SessionId
Caja -> tokens únicamente en memoria
Refresh -> token anterior se revoca y rota
Reutilización -> Core revoca sesiones activas
```

## Controles de seguridad

- JWT con clave externa al repositorio.
- Refresh tokens almacenados por hash.
- Autorización por roles en Core.
- Comprobación de propiedad para ciudadanos.
- Archivos validados por extensión, tamaño y firma.
- SQL sin exposición pública en release.
- Contenedores .NET ejecutados con usuario no root.
- Secretos en `deploy/.env.release`, excluido de Git.
- HTTPS en el punto de entrada.
- Logs sin JWT, contraseñas ni contenido documental.
- Respuestas de error con código y `correlationId`.

## Limitaciones declaradas

- El release Docker utiliza el ambiente ASP.NET `Development` únicamente para evitar redirecciones HTTPS internas detrás de Caddy en este prototipo académico. La exposición externa sigue protegida por HTTPS. Antes de producción institucional real se debe habilitar soporte formal de proxy inverso/forwarded headers y ejecutar los servicios en `Production`.
- Caja requiere Windows para ejecución visual.
- El certificado `tls internal` es apropiado para laboratorio y demostración; un dominio público debe usar certificado de una CA pública.
- Se requiere revisión de seguridad independiente antes de uso institucional real.
