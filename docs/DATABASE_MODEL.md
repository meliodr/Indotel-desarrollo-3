# Modelo de Base de Datos del Core

Este documento explica las tablas principales del MVP del Core INDOTEL.

## 1. Roles

Define los permisos generales del sistema.

Ejemplos:

- Administrador.
- AnalistaDAU.
- Prestadora.
- Ciudadano.
- Auditor.

## 2. Usuarios

Representa las cuentas que pueden iniciar sesion.

Campos importantes:

- NombreCompleto.
- Correo.
- PasswordHash.
- RolId.
- Activo.

## 3. Ciudadanos

Representa a los usuarios que presentan reclamaciones ante INDOTEL.

Campos importantes:

- Cedula.
- Nombres.
- Apellidos.
- Telefono.
- Correo.
- Direccion.

## 4. Prestadoras

Representa empresas de telecomunicaciones.

Ejemplos:

- Claro.
- Altice.
- Viva.
- Wind Telecom.

Campos importantes:

- Rnc.
- NombreComercial.
- RazonSocial.
- Representante.
- Telefono.
- Correo.

## 5. ServiciosTelecom

Representa el tipo de servicio reclamado.

Ejemplos:

- Internet.
- Telefonia movil.
- Telefonia fija.
- Telecable.

## 6. Reclamaciones

Es la tabla principal del flujo del sistema.

Guarda la queja del ciudadano contra una prestadora.

Campos importantes:

- NumeroExpediente.
- CiudadanoId.
- PrestadoraId.
- ServicioTelecomId.
- Titulo.
- Descripcion.
- Estado.
- FechaCreacion.
- FechaCierre.

## 7. DocumentosReclamacion

Guarda referencias a documentos o evidencias de una reclamacion.

Campos importantes:

- ReclamacionId.
- NombreArchivo.
- TipoContenido.
- RutaArchivo.

## 8. RespuestasPrestadora

Guarda la respuesta formal de una prestadora ante una reclamacion.

Campos importantes:

- ReclamacionId.
- PrestadoraId.
- Respuesta.
- DocumentoSoporte.
- FechaRespuesta.

## 9. HistorialReclamacion

Guarda los cambios de estado de una reclamacion.

Ejemplo:

RECIBIDA -> VALIDADA
VALIDADA -> ENVIADA_A_PRESTADORA
ENVIADA_A_PRESTADORA -> RESPONDIDA_POR_PRESTADORA

Campos importantes:

- ReclamacionId.
- EstadoAnterior.
- EstadoNuevo.
- Comentario.
- UsuarioId.
- FechaCambio.

## 10. Auditoria

Guarda acciones importantes realizadas dentro del sistema.

Ejemplos:

- Crear reclamacion.
- Cambiar estado.
- Registrar respuesta.
- Resolver caso.

Campos importantes:

- UsuarioId.
- Entidad.
- EntidadId.
- Accion.
- Detalle.
- Fecha.

## Nota

En esta fase se crearon modelos simples para asegurar que el proyecto compile y avance. Luego se agregaran validaciones, relaciones mas estrictas y datos semilla.
