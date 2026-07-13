# Manual técnico — Sistema Digital INDOTEL

## 1. Requisitos

### Desarrollo Linux

- Ubuntu 24.04 o equivalente.
- Git 2.40+.
- .NET SDK 8.0.
- Docker Engine y Docker Compose v2.
- curl y Python 3.

### Caja

- Windows 10/11 x64.
- .NET Desktop Runtime 8 cuando la publicación no sea self-contained.
- Acceso HTTPS al Gateway.

## 2. Repositorio y ramas

```text
core          Core ASP.NET y SQL
api-gateway   Gateway
web           Portal ciudadano
caja          WinForms interna
integracion   sistema consolidado, pruebas y release
main          punto inicial histórico; no usar para ejecutar el sistema final
```

## 3. Preparar integración

```bash
cd /home/jarry/Indotel-desarrollo-3
git fetch origin

git worktree add -B integracion \
  /home/jarry/indotel-prueba-integracion \
  origin/integracion

cd /home/jarry/indotel-prueba-integracion
bash scripts/preparar_sprint5_integracion.sh
git push origin integracion
```

## 4. Validación de código

```bash
bash scripts/validar_sprint5_integracion.sh
```

Con ejecución integrada:

```bash
RUN_RUNTIME_TESTS=1 \
INDOTEL_SQL_PASSWORD='CLAVE_SQL_LOCAL' \
bash scripts/validar_sprint5_integracion.sh
```

## 5. Configuración local por servicio

### Core

Variables principales:

```text
ConnectionStrings__DefaultConnection
Jwt__Issuer
Jwt__Audience
Jwt__Key
Jwt__AccessTokenMinutes
Jwt__RefreshTokenDays
SeedData__Enabled
SeedData__AdminEmail
SeedData__AdminPassword
Database__ApplyMigrationsOnStartup
Cors__AllowedOrigins__0
Security__ExposeResetTokenInResponse
```

Ejecución:

```bash
ASPNETCORE_ENVIRONMENT=Development \
ASPNETCORE_URLS=http://localhost:5085 \
ConnectionStrings__DefaultConnection='Server=localhost,1433;Database=IndotelCoreDb;User Id=sa;Password=***;TrustServerCertificate=True;Encrypt=False' \
Jwt__Key='CLAVE_ALEATORIA_DE_48_O_MAS' \
dotnet run --project core-indotel/Indotel.Core/Indotel.Core.csproj
```

### Gateway

```text
Gateway__CoreBaseUrl=http://localhost:5085
Cors__AllowedOrigins__0=http://localhost:5234
```

```bash
ASPNETCORE_ENVIRONMENT=Development \
ASPNETCORE_URLS=http://localhost:5185 \
Gateway__CoreBaseUrl=http://localhost:5085 \
dotnet run --project api-gateway/Indotel.ApiGateway/Indotel.ApiGateway.csproj
```

### Web

```text
ApiSettings__GatewayBaseUrl=http://localhost:5185
Security__DataProtectionKeysPath=.data-protection-keys
```

```bash
ASPNETCORE_ENVIRONMENT=Development \
ASPNETCORE_URLS=http://localhost:5234 \
ApiSettings__GatewayBaseUrl=http://localhost:5185 \
dotnet run --project INDOTEL.Web/INDOTEL.WEB.csproj
```

### Caja

Editar `INDOTEL_CAJA(REAL)/App.config` o el archivo de configuración publicado:

```xml
<add key="ApiBaseUrl" value="https://SERVIDOR_GATEWAY/" />
<add key="ApiTimeoutSeconds" value="20" />
<add key="ApiConnectTimeoutSeconds" value="5" />
```

## 6. Release Docker

Crear configuración real:

```bash
cp deploy/.env.release.example deploy/.env.release
chmod 600 deploy/.env.release
```

Generar claves aleatorias:

```bash
python3 - <<'PY'
import secrets
print('JWT_KEY=' + secrets.token_urlsafe(64))
print('SQL_PASSWORD=' + secrets.token_urlsafe(24) + 'Aa1!')
print('ADMIN_PASSWORD=' + secrets.token_urlsafe(20) + 'Aa1!')
PY
```

Copiar los valores al archivo sin publicarlos en Git, capturas o chats.

Validar estructura:

```bash
bash scripts/validar_sprint6_despliegue.sh
```

Validar imágenes y smoke:

```bash
BUILD_IMAGES=1 RUN_RELEASE_SMOKE=1 \
bash scripts/validar_sprint6_despliegue.sh
```

Desplegar:

```bash
bash scripts/desplegar_release.sh deploy/.env.release
```

URLs predeterminadas:

```text
Web:     https://localhost:8443/
Gateway: https://localhost:8443/api/...
Health:  https://localhost:8443/health
```

## 7. Certificado local

Caddy crea una CA interna. Para evitar advertencias, exportar e instalar su certificado raíz únicamente en las máquinas de demostración autorizadas.

```bash
docker cp indotel-release-caddy:/data/caddy/pki/authorities/local/root.crt ./artifacts/indotel-local-root.crt
```

En Windows, importar el certificado en “Entidades de certificación raíz de confianza”. No usar esa CA como certificado público de Internet.

## 8. Migraciones y seed

El primer inicio aplica migraciones porque el release configura:

```text
Database__ApplyMigrationsOnStartup=true
SeedData__Enabled=true
```

Después de crear el administrador inicial:

1. comprobar login;
2. guardar las credenciales en un gestor seguro;
3. cambiar `SEED_DATA_ENABLED=false`;
4. volver a ejecutar `docker compose up -d core`.

Antes de aplicar una versión con migraciones nuevas:

```bash
bash scripts/backup_sqlserver.sh deploy/.env.release
```

## 9. Backup

```bash
bash scripts/backup_sqlserver.sh deploy/.env.release
```

Se generan:

```text
backups/IndotelCoreDb-FECHA.bak
backups/IndotelCoreDb-FECHA.bak.sha256
```

Conservar al menos una copia fuera del servidor.

## 10. Restauración

```bash
CONFIRM_RESTORE=YES \
bash scripts/restore_sqlserver.sh \
  backups/IndotelCoreDb-FECHA.bak \
  deploy/.env.release
```

La operación detiene Web, Gateway y Core, restaura SQL y vuelve a iniciar los servicios.

## 11. Rollback

Las imágenes anteriores deben permanecer en el servidor.

```bash
CONFIRM_ROLLBACK=YES \
bash scripts/rollback_release.sh \
  TAG_ANTERIOR \
  backups/IndotelCoreDb-ANTES_DEL_RELEASE.bak \
  deploy/.env.release
```

## 12. Observabilidad

```bash
docker compose --env-file deploy/.env.release \
  -f deploy/docker-compose.release.yml ps

docker compose --env-file deploy/.env.release \
  -f deploy/docker-compose.release.yml logs -f --tail=200
```

Endpoints:

```text
/health             Gateway vivo
/health/live        Gateway vivo
/health/ready       Gateway y Core disponibles
/health/status      circuito y métricas del Gateway
/api/health         Core a través del Gateway
/api/health/db      SQL a través del Gateway
```

## 13. Diagnóstico

### Gateway devuelve 503

1. consultar `/health/status`;
2. revisar logs de Gateway y Core;
3. comprobar Core `/health` dentro de la red Docker;
4. comprobar SQL health;
5. usar el `correlationId` para unir registros.

### Web no inicia

- verificar `ApiSettings__GatewayBaseUrl`;
- verificar permisos del volumen de Data Protection;
- revisar que Gateway esté healthy.

### Caja no conecta

- verificar `ApiBaseUrl`;
- comprobar confianza del certificado;
- probar `/health` desde Windows;
- revisar firewall y DNS;
- consultar el código y `correlationId` mostrado.

## 14. Seguridad operativa

- No subir `.env.release`, `docker.env`, certificados privados ni respaldos.
- Rotar JWT y claves administrativas ante sospecha.
- No registrar tokens, contraseñas o documentos.
- Mantener SQL sin puerto público.
- Actualizar imágenes base después de pruebas.
- Probar restauración, no solo backup.
- Ejecutar la matriz de roles antes de cada entrega.
