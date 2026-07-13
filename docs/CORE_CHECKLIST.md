# Checklist del Core INDOTEL

Este checklist resume lo que se debe construir en el Core.

## 1. Preparacion del proyecto

- [ ] Crear carpeta core-indotel.
- [ ] Crear proyecto ASP.NET Core Web API.
- [ ] Configurar Swagger.
- [ ] Configurar Entity Framework Core.
- [ ] Configurar SQL Server Express.
- [ ] Crear endpoint GET /health.

## 2. Base de datos

- [ ] Crear modelo Usuario.
- [ ] Crear modelo Rol.
- [ ] Crear modelo Ciudadano.
- [ ] Crear modelo Prestadora.
- [ ] Crear modelo ServicioTelecom.
- [ ] Crear modelo Reclamacion.
- [ ] Crear modelo DocumentoReclamacion.
- [ ] Crear modelo RespuestaPrestadora.
- [ ] Crear modelo HistorialReclamacion.
- [ ] Crear modelo Auditoria.
- [ ] Crear DbContext.
- [ ] Crear migracion inicial.
- [ ] Crear datos semilla.

## 3. Seguridad

- [ ] Crear login.
- [ ] Crear JWT.
- [ ] Crear roles basicos.
- [ ] Validar usuario activo.
- [ ] Proteger endpoints por rol.

## 4. Ciudadanos

- [ ] Registrar ciudadano.
- [ ] Listar ciudadanos.
- [ ] Buscar ciudadano por cedula.
- [ ] Ver reclamaciones de un ciudadano.

## 5. Prestadoras

- [ ] Registrar prestadora.
- [ ] Listar prestadoras.
- [ ] Editar prestadora.
- [ ] Ver reclamaciones asignadas.

## 6. Servicios de telecomunicaciones

- [ ] Crear servicios: Internet, Telefonia Movil, Telefonia Fija, Telecable, Otros.
- [ ] Listar servicios.
- [ ] Activar o desactivar servicios.

## 7. Reclamaciones

- [ ] Crear reclamacion.
- [ ] Generar numero de expediente.
- [ ] Listar reclamaciones.
- [ ] Ver detalle de reclamacion.
- [ ] Cambiar estado.
- [ ] Adjuntar documento.
- [ ] Enviar a prestadora.
- [ ] Registrar respuesta de prestadora.
- [ ] Resolver reclamacion.
- [ ] Cerrar reclamacion.
- [ ] Ver historial de la reclamacion.

## 8. Auditoria

- [ ] Registrar creacion de reclamacion.
- [ ] Registrar cambio de estado.
- [ ] Registrar respuesta de prestadora.
- [ ] Registrar resolucion.
- [ ] Consultar auditoria general.
- [ ] Consultar auditoria por reclamacion.

## 9. Reportes

- [ ] Total de reclamaciones.
- [ ] Reclamaciones por estado.
- [ ] Reclamaciones por prestadora.
- [ ] Reclamaciones por servicio.
- [ ] Casos abiertos.
- [ ] Casos cerrados.

## 10. Pruebas de demo

- [ ] Probar login.
- [ ] Crear reclamacion como ciudadano.
- [ ] Validar reclamacion como analista.
- [ ] Enviar a prestadora.
- [ ] Responder como prestadora.
- [ ] Resolver como analista.
- [ ] Cerrar caso.
- [ ] Revisar auditoria.
- [ ] Revisar reportes.

## 11. Documentacion final

- [ ] README del Core.
- [ ] Instrucciones para instalar.
- [ ] Instrucciones para correr migraciones.
- [ ] Usuarios de prueba.
- [ ] Endpoints principales.
