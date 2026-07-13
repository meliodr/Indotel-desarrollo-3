# Web: correcciones y pruebas

## Estado actual

La Web funciona como Portal Ciudadano y consume los servicios HTTP de la API central. No accede directamente a la base de datos.

## Correcciones aplicadas

### Prioridad crítica

- Autorización restringida al rol `Ciudadano` en panel, reclamaciones y notificaciones.
- Renovación automática del JWT mediante refresh token con rotación.
- Cierre de sesión remoto y local, incluyendo revocación del refresh token.
- Eliminación de la advertencia de redirección HTTPS durante el desarrollo local.

### Prioridad alta

- Carga y descarga real de documentos del expediente.
- Validación de archivos en la Web: PDF, JPG, JPEG o PNG, máximo 5 MB.
- Acción para marcar notificaciones como leídas.
- Acceso directo desde una notificación hacia el expediente relacionado.
- Bloqueo de perfiles internos en el Portal Ciudadano.

### Prioridad media

- Página de acceso no autorizado.
- Navegación adaptada al rol y al estado de la sesión.
- Solución actualizada para incluir el proyecto de pruebas.
- Flujo de CI actualizado para compilar y ejecutar pruebas automáticamente.

## Pruebas automatizadas incluidas

- Página pública para usuario anónimo.
- Página de privacidad en español.
- Redirección del panel al login cuando no existe sesión.
- Validación de contraseña mínima durante el registro.
- Cálculo del estado leído de una notificación.
- Validación de la estructura de la respuesta de autenticación.

## Verificación manual recomendada

1. Registrar un ciudadano.
2. Iniciar sesión.
3. Crear una reclamación.
4. Abrir el detalle del expediente.
5. Subir un PDF o una imagen menor de 5 MB.
6. Descargar el documento.
7. Abrir una notificación y marcarla como leída.
8. Cerrar sesión.
9. Intentar entrar al panel sin sesión y confirmar la redirección.
10. Intentar iniciar sesión con un perfil interno y confirmar el bloqueo.

## Comandos de verificación

```bash
dotnet restore INDOTEL.Web/INDOTEL.WEB.csproj
dotnet restore INDOTEL.Web.Tests/INDOTEL.Web.Tests.csproj
dotnet build INDOTEL.Web/INDOTEL.WEB.csproj
dotnet test INDOTEL.Web.Tests/INDOTEL.Web.Tests.csproj
```
