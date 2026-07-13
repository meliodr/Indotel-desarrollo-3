# Plan maestro de cierre — Sistema Digital INDOTEL

## Objetivo

Completar el proyecto en seis sprints grandes, dejando Core, API/Gateway, Web y Caja integrados, probados, documentados y listos para demostración y entrega.

## Regla de cierre

Una tarea no se considera terminada solo porque el código exista. Debe cumplir:

1. Código implementado.
2. Compilación correcta.
3. Pruebas automáticas aprobadas.
4. Prueba manual o E2E con evidencia.
5. Documentación actualizada.
6. Sin secretos ni archivos generados en Git.

---

# Sprint 1 — Core seguro y estable

## Objetivo

Convertir el Core en la autoridad segura y resiliente del sistema.

## Alcance

- Corregir acceso entre ciudadanos y propiedad de recursos.
- Centralizar identidad, roles y comprobación de propiedad.
- Estandarizar errores con ProblemDetails, código y correlationId.
- Separar health de vida y disponibilidad.
- Devolver HTTP 503 cuando SQL Server no esté disponible.
- Agregar reintentos transitorios para SQL Server.
- Fortalecer JWT, refresh token, reutilización y revocación.
- Revocar sesiones al cambiar o restablecer contraseña.
- Reducir duración configurable del access token.
- Evitar token de recuperación en producción.
- Evitar credenciales de seed fijas en producción.
- Hacer atómicas las operaciones de reclamaciones.
- Publicar transiciones válidas por estado y rol.
- Validar contenido real de documentos.
- Proteger descarga, listado y carga por propiedad.
- Aplicar CORS por lista explícita fuera de desarrollo.
- Aplicar rate limiting particionado por IP.
- Agregar pruebas unitarias y CI.

## Criterio de aceptación

- Core compila y las pruebas pasan.
- Un ciudadano no puede consultar ni modificar datos de otro.
- SQL apagado produce readiness 503.
- Todos los errores contienen `mensaje`, `codigo` y `correlationId`.
- La reutilización de refresh token revoca sesiones activas.
- Documentos falsamente renombrados son rechazados.
- Las operaciones de cambio de estado, resolución y cierre son atómicas.

---

# Sprint 2 — API y Gateway

## Objetivo

Crear una entrada única, segura y resiliente para Web y Caja.

## Alcance

- Gateway con una URL estable.
- Enrutamiento completo hacia Core.
- HTTPS y certificados por ambiente.
- Health y readiness propios.
- Respuesta 503 cuando Core no responda.
- Timeout, circuit breaker y reintentos solo para métodos seguros.
- Propagación de Authorization y X-Correlation-ID.
- Rate limiting por IP, usuario y ruta.
- CORS explícito.
- Límites de cuerpo y archivos.
- Logs estructurados y métricas.
- Swagger/OpenAPI controlado.
- Pruebas de caída, lentitud y JWT inválido.

## Criterio de aceptación

- Web y Caja consumen únicamente el Gateway.
- Core apagado produce 503 controlado.
- No se reintentan POST, PATCH, PUT ni cargas de archivo automáticamente.
- CorrelationId atraviesa Gateway y Core.

---

# Sprint 3 — Web del ciudadano

## Objetivo

Completar el portal ciudadano con manejo uniforme de sesión y fallos.

## Alcance

- Cliente central para Gateway.
- Manejo global de timeout, 503, JSON inválido y caída de red.
- Conservar sesión ante caída temporal durante refresh.
- Cerrar sesión solo ante refresh rechazado o expirado.
- Almacenar tokens de manera protegida del lado servidor.
- Completar detalle, historial, documentos y notificaciones.
- Evitar doble envío.
- Paginación y estados de carga.
- Páginas 403, 404, 500 y servicio no disponible.
- Pruebas unitarias, integración y E2E.

## Criterio de aceptación

- Flujo registro → login → reclamación → documento → seguimiento → logout completo.
- Administrador y roles internos no entran al portal ciudadano.
- Gateway/Core apagado no tumba la Web.

---

# Sprint 4 — Caja interna

## Objetivo

Convertir Caja en una aplicación interna completa, estable y controlada por roles.

## Alcance

- URL configurable apuntando al Gateway.
- Un único cliente HTTP reutilizable.
- Manejo de red, timeout, certificado, 503 y JSON inválido.
- Health inicial y botón Reintentar.
- Logout local aunque el servicio no responda.
- Renovación de token sincronizada.
- Acceso por rol: Administrador, AnalistaDAU y Auditor de solo lectura.
- Bloqueo de Ciudadano y Prestadora en Caja interna.
- Transiciones consultadas desde Core.
- Historial, documentos, resolución y cierre formal.
- Bandeja con filtros y paginación.
- Prevención de doble clic y navegación sin formularios ocultos acumulados.
- Migración a net8.0-windows cuando sea viable.
- CI en windows-latest e instalador.

## Criterio de aceptación

- Caja no se cierra ni muestra excepción cuando Gateway/Core está apagado.
- Cada rol ve solo lo permitido.
- Flujo interno completo hasta cierre de expediente.

---

# Sprint 5 — Integración, seguridad y pruebas finales

## Objetivo

Comprobar el sistema completo bajo escenarios normales y de falla.

## Alcance

- Pruebas E2E Web → Gateway → Core → SQL.
- Pruebas E2E Caja → Gateway → Core → SQL.
- Matriz completa de roles.
- Pruebas de propiedad y acceso indebido.
- Pruebas de concurrencia y doble envío.
- Pruebas de Core, Gateway y SQL apagados.
- Pruebas de expiración y refresh token.
- Pruebas de archivos maliciosos, grandes o inválidos.
- Pruebas de carga básica y tiempos de respuesta.
- Revisión de secretos, logs y dependencias.
- Corrección de regresiones.

## Criterio de aceptación

- Todos los pipelines pasan.
- No existen vulnerabilidades críticas conocidas.
- Los flujos de ciudadano, operador, auditor y prestadora tienen evidencia.

---

# Sprint 6 — Despliegue, documentación y defensa

## Objetivo

Dejar el sistema reproducible, entregable y defendible.

## Alcance

- Configuración por variables de entorno.
- Despliegue reproducible con contenedores o servidor definido.
- Migraciones controladas.
- HTTPS y certificados.
- Backup y restauración de SQL Server probados.
- Procedimiento de rollback.
- Manual técnico y manual de usuario.
- Arquitectura y diagramas.
- Catálogo de endpoints y errores.
- Usuarios y datos de demostración.
- Guion de defensa y flujo de demo.
- Lista final de evidencias.

## Criterio de aceptación

- Una computadora o servidor limpio puede desplegar el sistema siguiendo la guía.
- Existe respaldo restaurable.
- La demostración completa puede ejecutarse sin improvisación.

---

# Orden obligatorio

1. Core.
2. API/Gateway.
3. Web.
4. Caja.
5. Integración y seguridad.
6. Despliegue y documentación final.

# Estado actual

- Sprint 1: implementación completada; restauración, compilación Release, 20 pruebas automáticas y publicación de comprobación aprobadas sobre el commit `6704e0a`. Permanecen las pruebas manuales de integración para el cierre funcional definitivo.
- Sprints 2 a 6: pendientes.
