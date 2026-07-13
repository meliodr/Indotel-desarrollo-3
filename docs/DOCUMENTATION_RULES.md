# Reglas de Documentacion del Core

Este proyecto debe quedar documentado para que cualquier integrante pueda leerlo y entender que hace cada parte.

## 1. Regla principal

No se debe crear una parte importante del Core sin documentarla.

Cada modulo debe responder:

- Que hace.
- Para que sirve.
- Que archivos usa.
- Que endpoints expone.
- Como se prueba.
- Que reglas de negocio aplica.

## 2. Documentos obligatorios

Dentro de docs se mantendran estos documentos:

- CORE_MASTER_PLAN.md: vision general del Core.
- CORE_CHECKLIST.md: lista de tareas.
- CORE_REFERENCE_ANALYSIS.md: comparacion con el proyecto anterior.
- ENDPOINTS.md: lista explicada de endpoints.
- DATABASE_MODEL.md: explicacion de tablas y relaciones.
- BUSINESS_RULES.md: reglas de negocio.
- DEMO_FLOW.md: pasos para presentar el sistema.
- INSTALL_CORE.md: como instalar y ejecutar el Core.

## 3. README del Core

La carpeta core-indotel debe tener su propio README con:

- Que es el Core.
- Como instalar dependencias.
- Como configurar la base de datos.
- Como ejecutar el proyecto.
- Como abrir Swagger.
- Usuarios de prueba.

## 4. Comentarios en codigo

Los comentarios deben usarse solo cuando ayuden a entender una regla importante.

Ejemplos de comentarios utiles:

- Por que una reclamacion no puede cerrarse sin estar resuelta.
- Por que se genera un numero de expediente.
- Por que se registra auditoria.
- Por que un ciudadano no debe ver casos ajenos.

No se deben comentar cosas obvias.

## 5. Cada controlador debe explicar su funcion

Cada controlador debe tener una descripcion breve en el README o en ENDPOINTS.md.

Ejemplo:

```text
ReclamacionesController
Gestiona el ciclo completo de una reclamacion desde que se crea hasta que se cierra.
```

## 6. Cada tabla debe explicarse

Cada tabla principal debe tener:

- Proposito.
- Campos principales.
- Relaciones.
- Ejemplo de uso.

## 7. Cada endpoint importante debe tener ejemplo

Cada endpoint debe documentarse con:

- Metodo HTTP.
- Ruta.
- Quien lo usa.
- Que recibe.
- Que devuelve.
- Ejemplo de prueba en Swagger.

## 8. Diario de cambios

Cada avance importante debe quedar anotado en docs/CHANGELOG_CORE.md.

Formato sugerido:

```text
Fecha:
Cambio realizado:
Archivos afectados:
Como probarlo:
Pendiente:
```

## 9. Regla de aprendizaje

Antes de avanzar a un modulo nuevo, se debe poder explicar el modulo anterior en palabras sencillas.

## 10. Regla de entrega

Nada debe considerarse terminado si no cumple estas condiciones:

- Funciona.
- Esta en la rama core.
- Tiene documentacion basica.
- Puede probarse en Swagger.
- Tiene datos de prueba si aplica.
