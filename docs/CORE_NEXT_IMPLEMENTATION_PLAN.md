# Plan de implementacion siguiente del Core INDOTEL

Este documento define el orden exacto para volver al codigo sin improvisar.

Ya se documentaron:

- `CORE_AUDIT_MAP.md`.
- `CORE_ROLE_MATRIX.md`.
- `CORE_ROUTE_DECISIONS.md`.
- `CORE_STATE_MACHINE.md`.
- `CORE_WEB_CAJA_CONTRACT.md`.

Ahora se define que se programa primero, que se prueba y cuando se considera terminado.

## 1. Estado actual

```text
Core funcional para demostracion academica: 95%
Core preparado para crecer: 70%
Core listo para produccion real: 60%
```

El Core actual ya tiene:

- Login JWT.
- Swagger.
- SQL Server Docker.
- EF Core.
- Migracion inicial.
- Datos semilla.
- Catalogos.
- Ciudadanos basicos.
- Reclamaciones basicas.
- Cambio de estado basico.
- Respuesta de prestadora.
- Historial.
- Reportes basicos.

## 2. Regla de trabajo

No se debe programar una mejora si no cumple estas condiciones:

```text
1. Esta documentada.
2. Tiene endpoint definido.
3. Tiene rol permitido definido.
4. Tiene prueba en Swagger definida.
5. Se sabe si requiere migracion o no.
6. No rompe rutas existentes.
```

## 3. Orden general recomendado

| Orden | Bloque | Objetivo | Riesgo | Requiere migracion |
|---|---|---|---|---|
| 1 | Compatibilidad de rutas | Ayudar a Web/Caja sin romper nada | Bajo | No |
| 2 | Ciudadanos ampliado | Buscar, editar y listar reclamaciones del ciudadano | Bajo/medio | No |
| 3 | Consulta por expediente | Permitir busqueda publica/controlada por numero | Bajo | No |
| 4 | Usuarios base | Crear usuarios reales por rol | Medio | Posible |
| 5 | RBAC real | Aplicar permisos por rol | Medio/alto | No |
| 6 | Maquina de estados | Evitar saltos incorrectos | Medio | No |
| 7 | Documentos/evidencias | Subir y consultar archivos | Medio | No si se usa modelo actual |
| 8 | Auditoria automatica | Registrar acciones importantes | Medio | No si se usa modelo actual |
| 9 | Filtros y paginacion | Mejorar bandeja de Caja | Bajo/medio | No |
| 10 | SLA y notificaciones simuladas | Vencimientos y alertas | Medio/alto | Si, probablemente |

## 4. Bloque 1 - Compatibilidad de rutas

### Objetivo

Agregar rutas alias para que Web y Caja puedan consumir endpoints con nombres simples.

### Endpoints a crear

```text
GET /api/prestadoras
GET /api/servicios
PATCH /api/reclamaciones/{id}/estado
```

### Regla

Estas rutas no deben duplicar logica.

Deben reutilizar lo que ya existe:

```text
GET /api/catalogos/prestadoras
GET /api/catalogos/servicios
PUT /api/reclamaciones/{id}/estado
```

### Archivos probables

```text
Controllers/CatalogosController.cs
Controllers/ReclamacionesController.cs
```

### Pruebas Swagger

- [ ] GET /api/prestadoras debe devolver 200.
- [ ] GET /api/servicios debe devolver 200.
- [ ] PATCH /api/reclamaciones/{id}/estado debe cambiar estado igual que PUT.
- [ ] Las rutas antiguas deben seguir funcionando.

### Definicion de terminado

- [ ] Build correcto.
- [ ] Swagger muestra rutas nuevas.
- [ ] Rutas viejas siguen funcionando.
- [ ] No requiere migracion.

## 5. Bloque 2 - Ciudadanos ampliado

### Objetivo

Completar lo que Web y Caja necesitan de ciudadanos.

### Endpoints a crear

```text
GET /api/ciudadanos/cedula/{cedula}
PUT /api/ciudadanos/{id}
GET /api/ciudadanos/{id}/reclamaciones
```

### Uso

Caja:

```text
Buscar ciudadano por cedula.
Editar ciudadano desde ventanilla.
Ver casos anteriores del ciudadano.
```

Web:

```text
Ver mis reclamaciones.
Actualizar perfil.
```

### Archivos probables

```text
Controllers/CiudadanosController.cs
DTOs/CiudadanoUpdateDto.cs
```

### Pruebas Swagger

- [ ] Buscar cedula existente debe dar 200.
- [ ] Buscar cedula inexistente debe dar 404.
- [ ] Editar ciudadano debe dar 200.
- [ ] Listar reclamaciones del ciudadano debe dar 200.

### Definicion de terminado

- [ ] Web puede implementar Mis reclamaciones.
- [ ] Caja puede buscar ciudadano por cedula.
- [ ] No rompe POST /api/ciudadanos.

## 6. Bloque 3 - Consulta por expediente

### Objetivo

Permitir consultar una reclamacion por numero de expediente.

### Endpoint a crear

```text
GET /api/reclamaciones/expediente/{numero}
```

### Uso

Web:

```text
Consulta publica/controlada del expediente.
```

Caja:

```text
Busqueda rapida de caso.
```

### Archivos probables

```text
Controllers/ReclamacionesController.cs
```

### Pruebas Swagger

- [ ] Expediente existente debe dar 200.
- [ ] Expediente inexistente debe dar 404.

## 7. Bloque 4 - Usuarios base

### Objetivo

Dejar de depender solo del usuario administrador semilla.

### Endpoints a crear

```text
GET /api/usuarios
GET /api/usuarios/{id}
POST /api/usuarios
PUT /api/usuarios/{id}
PATCH /api/usuarios/{id}/estado
PUT /api/usuarios/{id}/password
```

### Roles involucrados

- Administrador.

### Reglas

- Solo Administrador puede crear usuarios.
- La clave debe guardarse con PasswordHasher.
- No se debe devolver PasswordHash en respuestas.
- El correo debe ser unico.
- El rol debe existir.

### Posibles campos nuevos

Se debe evaluar si el modelo Usuario necesita:

```text
CiudadanoId nullable
PrestadoraId nullable
```

Esto permitiria vincular:

```text
Usuario ciudadano -> Ciudadano
Usuario prestadora -> Prestadora
```

Si se agregan esos campos, requiere migracion.

### Archivos probables

```text
Controllers/UsuariosController.cs
DTOs/UsuarioCreateDto.cs
DTOs/UsuarioUpdateDto.cs
DTOs/UsuarioResponseDto.cs
DTOs/CambiarPasswordUsuarioDto.cs
Models/Usuario.cs
Data/IndotelDbContext.cs
```

### Pruebas Swagger

- [ ] Crear usuario AnalistaDAU.
- [ ] Crear usuario Auditor.
- [ ] Crear usuario RepresentantePrestadora.
- [ ] Crear usuario Ciudadano.
- [ ] Login con usuario creado.
- [ ] Desactivar usuario y validar que no pueda operar.

## 8. Bloque 5 - RBAC real

### Objetivo

Aplicar permisos reales por rol.

### Roles iniciales

```text
Administrador
AnalistaDAU
RepresentantePrestadora
Ciudadano
Auditor
```

### Cambios principales

- [ ] Aplicar `[Authorize(Roles = "Administrador")]` en UsuariosController.
- [ ] Permitir cambio de estado a Administrador y AnalistaDAU.
- [ ] Permitir reportes a Administrador, AnalistaDAU y Auditor.
- [ ] Permitir respuesta prestadora a RepresentantePrestadora y Administrador.
- [ ] Filtrar reclamaciones segun rol.
- [ ] Evitar que Ciudadano vea casos ajenos.
- [ ] Evitar que Prestadora vea casos de otra prestadora.

### Pruebas Swagger

- [ ] Admin puede ver todo.
- [ ] Analista puede cambiar estado.
- [ ] Auditor no puede cambiar estado.
- [ ] Ciudadano no puede listar todos los ciudadanos.
- [ ] Prestadora no puede responder caso de otra prestadora.

## 9. Bloque 6 - Maquina de estados estricta

### Objetivo

Impedir saltos incorrectos.

### Archivos probables

```text
Services/ReclamacionEstadoService.cs
Constants/ReclamacionEstados.cs
```

### Reglas iniciales

Permitidas:

```text
RECIBIDA -> VALIDADA
VALIDADA -> ENVIADA_A_PRESTADORA
ENVIADA_A_PRESTADORA -> RESPONDIDA_POR_PRESTADORA
RESPONDIDA_POR_PRESTADORA -> EN_REVISION_INDOTEL
EN_REVISION_INDOTEL -> RESUELTA
RESUELTA -> CERRADA
```

Prohibidas:

```text
RECIBIDA -> CERRADA
VALIDADA -> CERRADA
RESPONDIDA_POR_PRESTADORA -> CERRADA
CERRADA -> VALIDADA
```

### Error esperado

```text
409 Conflict
```

### Pruebas Swagger

- [ ] Transiciones correctas deben dar 200.
- [ ] Transiciones incorrectas deben dar 409.
- [ ] Todo cambio debe registrar historial.

## 10. Bloque 7 - Documentos y evidencias

### Objetivo

Permitir adjuntar evidencia a reclamaciones.

### Endpoints

```text
POST /api/reclamaciones/{id}/documentos
GET /api/reclamaciones/{id}/documentos
DELETE /api/documentos/{id}
```

### Reglas

- Solo PDF, JPG, JPEG, PNG.
- Maximo 5MB.
- Guardar ruta o nombre del archivo.
- Vincular documento a ReclamacionId.
- No permitir documentos en reclamaciones cerradas.

### Pruebas Swagger

- [ ] Subir PDF valido.
- [ ] Rechazar extension invalida.
- [ ] Rechazar archivo mayor a 5MB.
- [ ] Listar documentos de la reclamacion.

## 11. Bloque 8 - Auditoria automatica

### Objetivo

Registrar acciones importantes.

### Servicio sugerido

```text
Services/AuditoriaService.cs
```

### Acciones a auditar

- Login exitoso.
- Creacion de ciudadano.
- Edicion de ciudadano.
- Creacion de reclamacion.
- Cambio de estado.
- Respuesta de prestadora.
- Carga de documento.
- Creacion/desactivacion de usuario.

### Endpoint

```text
GET /api/auditoria/reclamacion/{id}
```

### Datos

- UsuarioId.
- IP.
- Entidad.
- EntidadId.
- Accion.
- Detalle.
- Fecha.

## 12. Bloque 9 - Filtros y paginacion

### Objetivo

Preparar bandeja de Caja y consultas grandes.

### Mejoras

```text
GET /api/reclamaciones?estado=VALIDADA
GET /api/reclamaciones?prestadoraId=1
GET /api/reclamaciones?ciudadanoId=1
GET /api/reclamaciones?desde=2026-01-01&hasta=2026-12-31
GET /api/reclamaciones?page=1&pageSize=10
```

### Pruebas Swagger

- [ ] Filtrar por estado.
- [ ] Filtrar por prestadora.
- [ ] Filtrar por ciudadano.
- [ ] Paginacion devuelve cantidad correcta.

## 13. Bloque 10 - SLA y notificaciones simuladas

### Objetivo

Dar apariencia institucional mas completa.

### SLA

- Agregar FechaLimiteRespuesta.
- Crear estado VENCIDA.
- Marcar vencida si la prestadora no responde.

### Notificaciones

- Crear NotificationService simulado.
- Registrar notificacion en tabla futura o logs.
- Enviar correo simulado por consola/log.

### Requiere evaluacion

Este bloque probablemente requiere cambios de modelo y migracion.

## 14. Porcentaje estimado por avance

| Punto | Porcentaje academico estimado |
|---|---:|
| Estado actual | 95% |
| Compatibilidad de rutas | 96% |
| Ciudadanos ampliado | 97% |
| Consulta por expediente | 97.5% |
| Usuarios base | 98% |
| RBAC real | 99% |
| Maquina de estados estricta | 99.5% |
| Documentos/evidencias | 100% academico fuerte |

Para produccion real, aun faltarian MFA, SSO, recovery, auditoria avanzada, SLA robusto, notificaciones reales, logs profesionales, monitoreo y pruebas automatizadas.

## 15. Comandos de prueba despues de cada bloque

```bash
cd /home/jarry/Indotel-desarrollo-3
 git pull
cd core-indotel/Indotel.Core
 dotnet build
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

Luego en Swagger:

```text
POST /api/auth/login
Authorize con token
Probar endpoints nuevos
```

## 16. Definicion de listo para volver a programar

Ya se puede volver a codigo cuando existan estos documentos:

- [x] Auditoria y alcance.
- [x] Roles y permisos.
- [x] Decisiones de rutas.
- [x] Maquina de estados.
- [x] Contrato Web/Caja/Core.
- [x] Plan de implementacion.

## 17. Decision final

El primer bloque de codigo que se implementara sera:

```text
Bloque 1 - Compatibilidad de rutas
```

Incluye:

```text
GET /api/prestadoras
GET /api/servicios
PATCH /api/reclamaciones/{id}/estado
```

Este bloque es seguro porque no modifica base de datos, no rompe rutas actuales y ayuda de inmediato a Web y Caja.
