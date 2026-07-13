# Plan Maestro del Core INDOTEL

Este documento define el plan de trabajo del Core del Sistema Digital INDOTEL.

## Vision

El Core sera el motor principal del sistema. Su responsabilidad sera guardar los datos, aplicar las reglas de negocio y exponer endpoints para que los demas modulos trabajen.

## Meta

El proyecto debe servir para dos cosas:

1. Lograr una calificacion excelente.
2. Sentar una base real de aprendizaje sobre backend, APIs, base de datos y seguridad.

## Flujo principal

```text
Ciudadano crea reclamacion
Analista valida
Se envia a prestadora
Prestadora responde
INDOTEL resuelve
Caso se cierra
Auditoria registra todo
```

## Modulos obligatorios

- Autenticacion.
- Usuarios.
- Roles.
- Ciudadanos.
- Prestadoras.
- Servicios de telecomunicaciones.
- Reclamaciones.
- Documentos de reclamacion.
- Respuestas de prestadoras.
- Historial de reclamacion.
- Auditoria.
- Reportes basicos.

## Modulos secundarios

Si el MVP queda estable, se agregaran versiones basicas de:

- Certificaciones.
- Autorizaciones.
- Espectro y frecuencias.
- Inspecciones.
- Resoluciones.

## Estructura del proyecto

```text
core-indotel/
  Indotel.Core/
    Controllers/
    Data/
    DTOs/
    Models/
    Services/
    Helpers/
    Program.cs
    appsettings.json
```

## Etapas

1. Fundacion del proyecto.
2. Base de datos.
3. Seguridad.
4. Catalogos principales.
5. Reclamaciones.
6. Flujo institucional completo.
7. Auditoria y reportes.
8. Documentacion final.

## Criterio de exito

El Core sera exitoso si en Swagger podemos demostrar:

1. Login.
2. Crear ciudadano.
3. Crear prestadora.
4. Crear servicio.
5. Crear reclamacion.
6. Validar reclamacion.
7. Enviar a prestadora.
8. Registrar respuesta.
9. Resolver caso.
10. Cerrar caso.
11. Ver auditoria.
12. Ver reportes.
