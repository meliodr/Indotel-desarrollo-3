# Contrato Web + Caja + Core INDOTEL

Este documento unifica lo que Web y Caja pueden consumir del Core, lo que ya existe y lo que falta implementar.

El objetivo es que ningun integrante invente rutas, duplique logica o se conecte directo a SQL Server.

## 1. Regla principal

```text
Web, Caja y Gateway consumen el Core por API HTTP/JSON.
Ningun modulo visual debe conectarse directo a la base de datos.
```

Flujo permitido en desarrollo:

```text
Web/Caja -> Core API -> SQL Server
```

Flujo recomendado en arquitectura final:

```text
Web/Caja -> API Gateway -> Core API -> SQL Server
```

## 2. Base local actual

Durante desarrollo, el Core corre en:

```text
http://localhost:5085
```

Swagger:

```text
http://localhost:5085/swagger
```

## 3. Autenticacion

### Endpoint actual

```text
POST /api/auth/login
```

Uso:

```text
Web: login ciudadano.
Caja: login personal interno.
Gateway: reenviar login si aplica.
```

Body ejemplo:

```json
{
  "correo": "admin@indotel.test",
  "password": "Admin123*"
}
```

Respuesta esperada:

```json
{
  "token": "TOKEN_JWT",
  "expiraEn": "fecha",
  "usuario": {
    "id": 1,
    "nombreCompleto": "Administrador INDOTEL",
    "correo": "admin@indotel.test",
    "rol": "Administrador"
  }
}
```

### Uso del token

Toda peticion protegida debe enviar:

```text
Authorization: Bearer TOKEN_JWT
```

La Web puede guardar el token en `sessionStorage` para la demo academica.

La Caja puede guardarlo en memoria durante la sesion de la aplicacion.

## 4. Estado de endpoints por modulo

Leyenda:

| Estado | Significado |
|---|---|
| ACTUAL | Ya existe en el Core. |
| ALIAS | Se agregara como ruta compatible, sin duplicar logica. |
| FALTA | No existe todavia. |
| FUTURO | No es obligatorio ahora. |

## 5. Endpoints compartidos por Web y Caja

| Necesidad | Metodo | Ruta | Estado | Observacion |
|---|---|---|---|---|
| Ver salud del Core | GET | /health | ACTUAL | Util para probar si el backend esta encendido |
| Login | POST | /api/auth/login | ACTUAL | Devuelve JWT |
| Usuario autenticado | GET | /api/auth/me | ACTUAL | Requiere token |
| Roles | GET | /api/catalogos/roles | ACTUAL | Solo lectura |
| Servicios | GET | /api/catalogos/servicios | ACTUAL | Ruta oficial actual |
| Prestadoras | GET | /api/catalogos/prestadoras | ACTUAL | Ruta oficial actual |
| Servicios alias | GET | /api/servicios | ALIAS | Para Web/Caja |
| Prestadoras alias | GET | /api/prestadoras | ALIAS | Para Web/Caja |

## 6. Contrato para Web / Portal Ciudadano

La Web representa al ciudadano. Debe enfocarse en registro, login, creacion de reclamacion y consulta de expediente.

### Funciones Web

| Funcion Web | Metodo | Ruta | Estado Core | Prioridad |
|---|---|---|---|---|
| Login ciudadano | POST | /api/auth/login | ACTUAL | Alta |
| Registrar cuenta ciudadana | POST | /api/auth/register-ciudadano | FALTA | Alta, despues de usuarios |
| Ver perfil ciudadano | GET | /api/ciudadanos/{id} | ACTUAL | Alta |
| Editar perfil ciudadano | PUT | /api/ciudadanos/{id} | FALTA | Alta |
| Cargar prestadoras | GET | /api/prestadoras | ALIAS | Alta |
| Cargar servicios | GET | /api/servicios | ALIAS | Alta |
| Crear reclamacion | POST | /api/reclamaciones | ACTUAL | Alta |
| Ver mis reclamaciones | GET | /api/ciudadanos/{id}/reclamaciones | FALTA | Alta |
| Ver detalle reclamacion | GET | /api/reclamaciones/{id} | ACTUAL | Alta |
| Ver historial | GET | /api/reclamaciones/{id}/historial | ACTUAL | Alta |
| Ver respuestas | GET | /api/reclamaciones/{id}/respuestas | ACTUAL | Alta |
| Consultar por expediente | GET | /api/reclamaciones/expediente/{numero} | FALTA | Alta |
| Subir documento | POST | /api/reclamaciones/{id}/documentos | FALTA | Media-alta |
| Listar documentos | GET | /api/reclamaciones/{id}/documentos | FALTA | Media-alta |

### Reglas Web

- La Web no debe mostrar reclamaciones de otros ciudadanos.
- La Web no debe inventar estados.
- La Web no debe conectarse a SQL Server.
- La Web debe mostrar el numero de expediente al crear la reclamacion.
- La Web debe manejar errores 400, 401, 403, 404, 409 y 500.
- La Web debe enviar token en toda peticion protegida.

### Flujo minimo Web

```text
Login/Registro -> Crear reclamacion -> Recibir expediente -> Consultar estado -> Ver historial/respuesta
```

## 7. Contrato para Caja / Ventanilla interna

Caja representa al personal interno de INDOTEL.

### Funciones Caja

| Funcion Caja | Metodo | Ruta | Estado Core | Prioridad |
|---|---|---|---|---|
| Login interno | POST | /api/auth/login | ACTUAL | Alta |
| Panel resumen | GET | /api/reportes/resumen | ACTUAL | Alta |
| Listar reclamaciones | GET | /api/reclamaciones | ACTUAL | Alta |
| Ver detalle reclamacion | GET | /api/reclamaciones/{id} | ACTUAL | Alta |
| Crear reclamacion desde ventanilla | POST | /api/reclamaciones | ACTUAL | Alta |
| Cambiar estado | PUT | /api/reclamaciones/{id}/estado | ACTUAL | Alta |
| Cambiar estado parcial | PATCH | /api/reclamaciones/{id}/estado | ALIAS | Alta |
| Registrar respuesta prestadora | POST | /api/reclamaciones/{id}/respuesta-prestadora | ACTUAL | Media |
| Ver historial | GET | /api/reclamaciones/{id}/historial | ACTUAL | Alta |
| Ver respuestas | GET | /api/reclamaciones/{id}/respuestas | ACTUAL | Alta |
| Reporte por estado | GET | /api/reportes/reclamaciones-por-estado | ACTUAL | Media |
| Reporte por prestadora | GET | /api/reportes/reclamaciones-por-prestadora | ACTUAL | Media |
| Buscar ciudadano por cedula | GET | /api/ciudadanos/cedula/{cedula} | FALTA | Alta |
| Editar ciudadano | PUT | /api/ciudadanos/{id} | FALTA | Alta |
| Gestionar usuarios | GET/POST/PUT | /api/usuarios | FALTA | Alta |
| Adjuntar documentos | POST | /api/reclamaciones/{id}/documentos | FALTA | Media-alta |
| Ver auditoria caso | GET | /api/auditoria/reclamacion/{id} | FALTA | Media |

### Reglas Caja

- Caja no debe conectarse directo a SQL Server.
- Caja debe usar token JWT.
- Caja debe mostrar botones segun rol y estado.
- Caja no debe permitir cerrar una reclamacion si el Core la rechaza.
- Caja debe mostrar mensajes claros cuando el Core devuelva 409 por regla de negocio.
- Caja debe usar filtros para bandeja cuando el Core los tenga.

### Flujo minimo Caja

```text
Login interno -> Panel -> Bandeja -> Detalle -> Cambiar estado -> Historial -> Reportes
```

## 8. Contrato para Gateway

Gateway no reemplaza al Core.

Responsabilidades posibles:

- Enrutar peticiones.
- Centralizar URL publica.
- Aplicar limites de uso.
- Validar token antes de reenviar.
- Registrar trazas tecnicas.
- Versionar rutas externas.

Gateway no debe:

- Saltarse reglas de negocio.
- Conectarse directo a SQL Server.
- Cambiar estados sin usar el Core.
- Duplicar logica de reclamaciones.

Ejemplo futuro:

```text
/gateway/auth/login -> /api/auth/login
/gateway/reclamaciones -> /api/reclamaciones
/gateway/catalogos/prestadoras -> /api/catalogos/prestadoras
```

## 9. Endpoints que deben implementarse primero

Para mejorar integracion con Web y Caja sin romper nada:

### Fase 1 - Compatibilidad rapida

- [ ] GET /api/prestadoras.
- [ ] GET /api/servicios.
- [ ] PATCH /api/reclamaciones/{id}/estado.

### Fase 2 - Ciudadanos y busqueda

- [ ] GET /api/ciudadanos/cedula/{cedula}.
- [ ] PUT /api/ciudadanos/{id}.
- [ ] GET /api/ciudadanos/{id}/reclamaciones.
- [ ] GET /api/reclamaciones/expediente/{numero}.

### Fase 3 - Usuarios y roles

- [ ] GET /api/usuarios.
- [ ] GET /api/usuarios/{id}.
- [ ] POST /api/usuarios.
- [ ] PUT /api/usuarios/{id}.
- [ ] PATCH /api/usuarios/{id}/estado.
- [ ] POST /api/auth/register-ciudadano.

### Fase 4 - Documentos y auditoria

- [ ] POST /api/reclamaciones/{id}/documentos.
- [ ] GET /api/reclamaciones/{id}/documentos.
- [ ] DELETE /api/documentos/{id}.
- [ ] GET /api/auditoria/reclamacion/{id}.

### Fase 5 - Workflow avanzado

- [ ] Maquina de estados estricta.
- [ ] POST /api/reclamaciones/{id}/enviar-prestadora.
- [ ] POST /api/reclamaciones/{id}/resolver.
- [ ] POST /api/reclamaciones/{id}/cerrar.
- [ ] POST /api/reclamaciones/{id}/rechazar.

## 10. Manejo de errores esperado

| Codigo | Uso | Que debe hacer Web/Caja |
|---|---|---|
| 200 | Consulta correcta | Mostrar datos |
| 201 | Registro creado | Mostrar confirmacion |
| 400 | Datos invalidos | Mostrar campos a corregir |
| 401 | No autenticado | Mandar a login |
| 403 | Sin permiso | Mostrar acceso denegado |
| 404 | No encontrado | Mostrar que no existe |
| 409 | Conflicto de negocio | Mostrar regla incumplida |
| 500 | Error interno | Mostrar mensaje general |

## 11. Reglas de version actual

- Version actual del contrato: 1.0 academica.
- Base de API actual: `/api`.
- Versionado `/api/v1` queda para etapa futura.
- No se eliminan rutas actuales.
- Toda ruta nueva debe aparecer aqui y en Swagger.

## 12. Definicion de contrato listo

Este contrato se considera listo cuando:

- [x] Web sabe que puede consumir hoy.
- [x] Caja sabe que puede consumir hoy.
- [x] Web sabe que falta pedir al Core.
- [x] Caja sabe que falta pedir al Core.
- [x] Las rutas actuales no se rompen.
- [x] Las rutas alias estan identificadas.
- [x] El orden de implementacion esta definido.

## 13. Decision final

Para la siguiente etapa se trabajara primero en compatibilidad de rutas y luego en usuarios, roles, maquina de estados, documentos y auditoria.
