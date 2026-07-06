# Plan del Core INDOTEL

Este documento define lo que se hara en el modulo Core del proyecto universitario Sistema Digital INDOTEL.

## 1. Que es el Core

El Core es el motor principal del sistema. Guarda los datos, aplica las reglas de negocio y expone los endpoints que usaran el middleware, la app interna y el portal web.

El Core no debe ser consumido directamente por las pantallas. El flujo correcto es:

```text
Portal o App Interna -> Middleware -> Core -> Base de Datos
```

## 2. Objetivo del Core

Construir una API backend sencilla, clara y defendible para gestionar el flujo principal de INDOTEL:

```text
Ciudadano crea una reclamacion
INDOTEL valida la reclamacion
La reclamacion se envia a la prestadora
La prestadora responde
INDOTEL analiza y resuelve
El caso se cierra
```

## 3. Alcance universitario

El proyecto no busca copiar todo INDOTEL real. Se hara una version academica enfocada en:

- Usuarios y roles.
- Ciudadanos.
- Prestadoras.
- Servicios de telecomunicaciones.
- Reclamaciones.
- Respuestas de prestadoras.
- Historial de estados.
- Auditoria.
- Reportes basicos.

Los modulos grandes como espectro, certificaciones, autorizaciones y firma digital pueden quedar en version basica o como datos de apoyo si el tiempo alcanza.

## 4. Tecnologias

- C#.
- ASP.NET Core Web API.
- Entity Framework Core.
- SQL Server Express.
- Swagger.
- JWT para login.

## 5. Carpetas sugeridas

```text
core-indotel/
  Indotel.Core/
    Controllers/
    Data/
    DTOs/
    Models/
    Services/
    Program.cs
    appsettings.json
```

## 6. Tablas principales

### Obligatorias

1. Usuarios
2. Roles
3. Ciudadanos
4. Prestadoras
5. ServiciosTelecom
6. Reclamaciones
7. DocumentosReclamacion
8. RespuestasPrestadora
9. HistorialReclamacion
10. Auditoria

### Secundarias si el tiempo alcanza

11. Autorizaciones
12. Certificaciones
13. SolicitudesFirmaDigital
14. Radioaficionados
15. Frecuencias
16. Interferencias
17. Inspecciones
18. ActasInspeccion
19. Controversias
20. Resoluciones
21. Notificaciones
22. ParametrosSistema
23. LogsSistema

## 7. Roles minimos

- Administrador: gestiona usuarios, roles y datos generales.
- AnalistaDAU: valida, observa, envia y resuelve reclamaciones.
- Prestadora: responde reclamaciones asignadas.
- Ciudadano: crea y consulta sus reclamaciones.
- Auditor: consulta informacion sin modificar.

## 8. Estados de reclamacion

```text
RECIBIDA
VALIDADA
OBSERVADA
ENVIADA_A_PRESTADORA
RESPONDIDA_POR_PRESTADORA
EN_ANALISIS_INDOTEL
RESUELTA
CERRADA
RECHAZADA
ARCHIVADA
```

## 9. Reglas de negocio

- Una reclamacion nueva inicia en estado RECIBIDA.
- Una reclamacion no puede enviarse a la prestadora si no esta VALIDADA.
- Una prestadora solo puede responder reclamaciones que le pertenecen.
- Una reclamacion no puede cerrarse sin estar RESUELTA.
- Una reclamacion CERRADA no debe modificarse.
- Todo cambio de estado debe guardarse en HistorialReclamacion.
- Toda accion importante debe guardarse en Auditoria.
- Los ciudadanos solo ven sus propias reclamaciones.
- Los usuarios inactivos no pueden usar el sistema.

## 10. Endpoints obligatorios

### Salud

```text
GET /health
```

### Autenticacion

```text
POST /api/auth/login
POST /api/auth/register-ciudadano
GET  /api/usuarios/{id}/estado
```

### Ciudadanos

```text
GET  /api/ciudadanos
GET  /api/ciudadanos/{id}
GET  /api/ciudadanos/cedula/{cedula}
POST /api/ciudadanos
PUT  /api/ciudadanos/{id}
GET  /api/ciudadanos/{id}/reclamaciones
```

### Prestadoras

```text
GET  /api/prestadoras
GET  /api/prestadoras/{id}
POST /api/prestadoras
PUT  /api/prestadoras/{id}
GET  /api/prestadoras/{id}/reclamaciones
```

### Servicios

```text
GET  /api/servicios
POST /api/servicios
PUT  /api/servicios/{id}
```

### Reclamaciones

```text
POST  /api/reclamaciones
GET   /api/reclamaciones
GET   /api/reclamaciones/{id}
PATCH /api/reclamaciones/{id}/estado
POST  /api/reclamaciones/{id}/documentos
POST  /api/reclamaciones/{id}/enviar-prestadora
POST  /api/reclamaciones/{id}/respuesta-prestadora
POST  /api/reclamaciones/{id}/resolver
POST  /api/reclamaciones/{id}/cerrar
GET   /api/reclamaciones/{id}/historial
```

### Auditoria

```text
GET /api/auditoria
GET /api/auditoria/reclamacion/{id}
```

### Reportes basicos

```text
GET /api/reportes/resumen
GET /api/reportes/reclamaciones-por-prestadora
GET /api/reportes/reclamaciones-por-estado
```

## 11. Datos semilla para la demo

El Core debe incluir datos de prueba:

- 1 administrador.
- 1 analista DAU.
- 1 usuario prestadora.
- 1 ciudadano.
- 4 prestadoras.
- 5 servicios de telecomunicaciones.
- 10 reclamaciones en diferentes estados.

## 12. Prioridad de desarrollo

### Fase 1

- Crear proyecto.
- Swagger.
- Conexion a SQL Server.
- Modelos principales.
- DbContext.
- Endpoint /health.

### Fase 2

- Login.
- JWT.
- Usuarios.
- Roles.
- Ciudadanos.
- Prestadoras.

### Fase 3

- Reclamaciones.
- Estados.
- Documentos.
- Respuesta de prestadora.
- Historial.

### Fase 4

- Auditoria.
- Reportes.
- Datos semilla.
- Pruebas.
- Documentacion.

## 13. Casos de prueba minimos

1. Login correcto.
2. Usuario inactivo no puede acceder.
3. Ciudadano crea reclamacion.
4. Analista valida reclamacion.
5. Analista envia reclamacion a prestadora.
6. Prestadora responde.
7. Analista resuelve.
8. Analista cierra.
9. Auditoria registra cambios.
10. Reportes muestran conteos.

## 14. Entregables del Core

- Proyecto backend funcionando.
- Swagger activo.
- Base de datos creada.
- Datos semilla.
- Endpoints principales.
- Auditoria basica.
- Reportes basicos.
- README de instalacion.
