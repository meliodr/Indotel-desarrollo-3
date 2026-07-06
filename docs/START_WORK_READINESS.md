# Verificacion Final para Iniciar el Core

Estado: LISTO PARA EMPEZAR

## 1. Rama de trabajo

La rama oficial para el Core es:

```text
core
```

No se trabajara directo en main.

## 2. Alcance confirmado

El Core tendra dos niveles:

### MVP obligatorio

- Auth.
- Usuarios y roles.
- Ciudadanos.
- Prestadoras.
- Servicios de telecomunicaciones.
- Reclamaciones.
- Respuesta de prestadora.
- Historial.
- Auditoria.
- Reportes basicos.

### Core academico completo

- Autorizaciones.
- Certificaciones.
- Firma digital.
- Radioaficionados.
- Espectro y frecuencias.
- Interferencias.
- Inspecciones.
- Controversias.
- Resoluciones.
- Consultas publicas.

## 3. Prioridad de construccion

Primero se construye el MVP completo de reclamaciones.

Despues se agregan versiones simples de los modulos regulatorios.

## 4. Regla de documentacion

Cada modulo importante debe explicar:

- Que hace.
- Para que sirve.
- Que archivos usa.
- Que endpoints tiene.
- Como se prueba.
- Que reglas de negocio aplica.

## 5. Primer bloque de codigo a crear

La primera tarea de desarrollo sera crear:

```text
core-indotel/Indotel.Core
```

Con:

- Proyecto ASP.NET Core Web API.
- Swagger.
- GET /health.
- Estructura Controllers, Models, DTOs, Data, Services y Helpers.
- README inicial del Core.

## 6. Definicion de listo

El primer bloque estara listo cuando:

- El proyecto compile.
- dotnet run funcione.
- Swagger abra.
- GET /health responda OK.
- Exista README explicando como correrlo.

## 7. Confirmacion

Con esta verificacion, ya se puede empezar a programar el Core.
