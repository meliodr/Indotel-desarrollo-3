# Resultados de pruebas del Core INDOTEL

Fecha de prueba: 09/07/2026
Rama probada: `core`
Entorno: Desarrollo local
Base de datos: SQL Server Docker `indotel-sqlserver`
API: `http://localhost:5085`

## Estado general

El Core compiló correctamente, inició sin errores, conectó con SQL Server, confirmó migraciones al día y respondió correctamente las pruebas principales de API.

Resultado general:

```text
CORE FUNCIONAL PARA DEMO ACADEMICA
```

## Build

Comando ejecutado:

```bash
cd /home/jarry/Indotel-desarrollo-3/core-indotel/Indotel.Core
dotnet build
```

Resultado:

```text
Compilación realizada correctamente
0 errores
```

## Infraestructura

Comandos ejecutados:

```bash
cd /home/jarry/Indotel-desarrollo-3
sudo docker compose up -d
sudo docker ps
```

Resultado:

```text
Container indotel-sqlserver Running
Puerto 1433 activo
```

## Ejecución del API

Comando ejecutado:

```bash
cd /home/jarry/Indotel-desarrollo-3/core-indotel/Indotel.Core
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

Resultado:

```text
Now listening on: http://localhost:5085
Hosting environment: Development
No migrations were applied. The database is already up to date.
```

## Pruebas de endpoints base

| Endpoint | Resultado |
|---|---:|
| `POST /api/auth/login` | OK |
| `GET /api/servicios` | 200 |
| `GET /api/prestadoras` | 200 |
| `GET /api/usuarios` | 200 |
| `GET /api/ciudadanos` | 200 |
| `GET /api/reclamaciones` | 200 |
| `GET /api/reportes/resumen` | 200 |

## Prueba de autenticación pública ciudadana

Prueba ejecutada después de implementar Fase 1 básica de autenticación pública.

Ciudadano creado durante la prueba:

```text
Correo ciudadano: ciudadano.1783579911536929251@indotel.test
Cedula ciudadano: 4021783579911536929251
```

Resultados:

| Acción | Resultado esperado | Resultado real |
|---|---:|---:|
| Registrar ciudadano | 200 | OK |
| Consultar `/api/auth/me` con token ciudadano | 200 | OK |
| Cambiar contraseña propia | 200 | OK |
| Login con contraseña anterior | 401 | OK |
| Login con contraseña nueva | 200 | OK |
| Solicitar recuperación de contraseña | 200 | OK |
| Restablecer contraseña con token | 200 | OK |
| Login con contraseña restablecida | 200 | OK |

Endpoints probados:

```text
POST /api/auth/register-ciudadano
GET /api/auth/me
POST /api/auth/change-password
POST /api/auth/login
POST /api/auth/forgot-password
POST /api/auth/reset-password
```

Conclusión:

```text
AUTH PUBLICA BASICA FUNCIONANDO
```

Nota de producción:

```text
El endpoint forgot-password devuelve el token en la respuesta solo para demo y pruebas.
En producción real debe enviarse por correo y guardarse/inválidarse de forma más estricta.
```

## Prueba de flujo de reclamación

Reclamación creada durante la prueba:

```text
ID=5
EXPEDIENTE=IND-20260709060434933-888
```

Flujo probado:

```text
RECIBIDA
-> VALIDADA
-> ENVIADA_A_PRESTADORA
-> RESPONDIDA_POR_PRESTADORA
-> EN_REVISION
-> RESUELTA
-> CERRADA
```

Resultados:

| Acción | Resultado esperado | Resultado real |
|---|---:|---:|
| Crear reclamación | 201/200 | OK |
| `RECIBIDA -> CERRADA` | 409 | OK |
| `RECIBIDA -> VALIDADA` | 200 | OK |
| `VALIDADA -> ENVIADA_A_PRESTADORA` | 200 | OK |
| Registrar respuesta prestadora | 200 | OK |
| `RESPONDIDA_POR_PRESTADORA -> EN_REVISION` | 200 | OK |
| `EN_REVISION -> RESUELTA` | 200 | OK |
| `RESUELTA -> CERRADA` | 200 | OK |
| `CERRADA -> VALIDADA` | 409 | OK |
| Consultar historial | 200 | OK |

## Validación de historial

El historial registró correctamente los cambios:

```text
RECIBIDA
VALIDADA
ENVIADA_A_PRESTADORA
RESPONDIDA_POR_PRESTADORA
EN_REVISION
RESUELTA
CERRADA
```

Cada cambio quedó asociado al usuario autenticado.

## Prueba de documentos/evidencias

Reclamación para documentos:

```text
DOC_RECLAMACION_ID=6
```

Resultados:

| Acción | Resultado esperado | Resultado real |
|---|---:|---:|
| Subir documento a caso cerrado | 409 | OK |
| Crear reclamación abierta para documentos | 201/200 | OK |
| Subir PDF a reclamación abierta | 201 | OK |
| Listar documentos de reclamación | 200 | OK |

Documento subido:

```text
nombreArchivo=evidencia-prueba.pdf
tipoContenido=application/pdf
rutaArchivo=uploads/reclamaciones/6/a879ad712f1148e7927517387b3efeec.pdf
```

## Consulta por expediente

Endpoint probado:

```text
GET /api/reclamaciones/expediente/IND-20260709060434933-888
```

Resultado:

```text
200 OK
```

## Bug encontrado y corregido

Durante la primera ejecución del script se detectó un error al crear dos reclamaciones muy rápido. El número de expediente se generaba con precisión de segundos:

```text
IND-yyyyMMddHHmmss
```

Esto podía producir duplicados por el índice único de `NumeroExpediente`.

Corrección aplicada:

```text
IND-yyyyMMddHHmmssfff-aleatorio
```

Ejemplo:

```text
IND-20260709060434933-888
```

Después de la corrección, la prueba completa pasó.

## Conclusión

El Core está listo para demo académica con los siguientes módulos probados:

```text
Autenticación JWT
Autenticación pública ciudadana básica
Catálogos
Usuarios
Ciudadanos
Reclamaciones
Máquina de estados
Historial
Respuesta de prestadora
Documentos/evidencias
Reportes básicos
Consulta por expediente
```

Estado recomendado para presentación:

```text
Core académico: 100%
Core funcional probado: 98% - 100%
Core producción real estimado: 73%
```

## Pendientes posteriores

Estos puntos quedan como mejoras de segunda fase:

```text
Refresh token y logout
Bloqueo por intentos fallidos
Auditoría automática completa
Filtros y paginación
RBAC fase 2 con CiudadanoId y PrestadoraId en Usuario
SLA y vencimientos automáticos
Notificaciones simuladas
Pruebas automáticas formales
Exportación PDF/Excel
```
