# Roadmap Detallado del Core

Este roadmap organiza el trabajo para construir el Core en orden logico.

## Fase 1: Fundacion

Objetivo: dejar el proyecto corriendo.

Tareas:

- Crear proyecto ASP.NET Core Web API.
- Configurar Swagger.
- Crear endpoint GET /health.
- Crear estructura de carpetas.
- Crear appsettings.json.
- Crear appsettings.Development.json.example.

Resultado esperado:

- El proyecto corre.
- Swagger abre.
- /health responde OK.

## Fase 2: Base de datos

Objetivo: crear el modelo principal de datos.

Tareas:

- Crear modelos principales.
- Crear DbContext.
- Configurar relaciones.
- Crear migracion inicial o script SQL.
- Crear datos semilla.

Tablas del MVP:

- Roles.
- Usuarios.
- Ciudadanos.
- Prestadoras.
- ServiciosTelecom.
- Reclamaciones.
- DocumentosReclamacion.
- RespuestasPrestadora.
- HistorialReclamacion.
- Auditoria.

Resultado esperado:

- La base de datos se crea.
- Existen datos iniciales para demo.

## Fase 3: Seguridad

Objetivo: permitir acceso controlado al sistema.

Tareas:

- Login.
- Roles.
- Token JWT.
- Validar usuarios activos.
- Proteger endpoints importantes.

Resultado esperado:

- El usuario puede iniciar sesion.
- El sistema devuelve token.
- Los endpoints protegidos funcionan segun rol.

## Fase 4: Catalogos

Objetivo: crear las entidades base.

Tareas:

- CRUD de ciudadanos.
- CRUD de prestadoras.
- CRUD de servicios.
- Consulta de usuarios.

Resultado esperado:

- El sistema tiene datos para crear reclamaciones.

## Fase 5: Reclamaciones

Objetivo: construir el corazon del proyecto.

Tareas:

- Crear reclamacion.
- Generar numero de expediente.
- Listar reclamaciones.
- Ver detalle.
- Cambiar estado.
- Guardar historial.

Resultado esperado:

- Se puede crear y dar seguimiento a una reclamacion.

## Fase 6: Flujo completo

Objetivo: cerrar el proceso institucional.

Tareas:

- Validar reclamacion.
- Enviar a prestadora.
- Registrar respuesta.
- Resolver.
- Cerrar.
- Auditar cada accion.

Resultado esperado:

- El flujo completo se puede presentar en Swagger.

## Fase 7: Reportes

Objetivo: mostrar informacion de resumen.

Tareas:

- Reporte general.
- Reclamaciones por estado.
- Reclamaciones por prestadora.
- Casos abiertos y cerrados.

Resultado esperado:

- El maestro puede ver indicadores del sistema.

## Fase 8: Cierre

Objetivo: dejar el proyecto presentable.

Tareas:

- Revisar Swagger.
- Corregir errores.
- Completar README.
- Completar manual de instalacion.
- Completar flujo de demo.
- Limpiar archivos innecesarios.

Resultado esperado:

- Core listo para entregar y defender.
