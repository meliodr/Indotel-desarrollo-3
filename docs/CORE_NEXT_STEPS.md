# Proximos pasos del Core INDOTEL

Proyecto: Sistema Digital INDOTEL
Modulo: Core Backend
Rama: `core`

## 1. Estado actual

El Core esta funcional y validado dentro del alcance academico definido.

Estado recomendado para presentacion:

```text
Core academico/demo: 100%
Core funcional probado: 100%
Core produccion academica/prototipo avanzado: 100%
```

No se declara como produccion gubernamental real certificada.

## 2. Objetivo de este documento

Este documento lista mejoras futuras que elevarian el Core desde prototipo institucional avanzado hacia una base productiva mas robusta.

Estas mejoras no son necesarias para la defensa actual y no deben implementarse apresuradamente antes de la entrega si pueden poner en riesgo la estabilidad ya probada.

## 3. Prioridad 1 - Mejoras que mas valor agregan

### 3.1 Pruebas automatizadas

Agregar un proyecto:

```text
Indotel.Core.Tests
```

Framework recomendado:

```text
xUnit
FluentAssertions
Microsoft.AspNetCore.Mvc.Testing
Testcontainers para SQL Server opcional
```

Pruebas recomendadas:

- Login correcto e incorrecto.
- Registro ciudadano.
- RBAC por roles.
- Dueno real ciudadano.
- Transiciones validas de reclamaciones.
- Transiciones invalidas de reclamaciones.
- SLA vencido.
- Resolucion y cierre.
- Bloqueo de documentos a usuario ajeno.
- Notificaciones protegidas.
- Auditoria generada.

### 3.2 Rate limiting

Agregar limite de peticiones a endpoints publicos o sensibles:

```text
POST /api/auth/login
POST /api/auth/register-ciudadano
POST /api/auth/forgot-password
POST /api/auth/reset-password
GET /api/health
```

Beneficio:

- Mitiga fuerza bruta.
- Reduce abuso de endpoints publicos.
- Mejora postura de seguridad.

### 3.3 Refresh token y logout real

Agregar:

```text
POST /api/auth/refresh-token
POST /api/auth/logout
```

Tablas posibles:

```text
RefreshTokens
RevokedTokens
```

Beneficio:

- Mejor experiencia de usuario.
- Revocacion real de sesiones.
- Menor riesgo si un token expira rapido.

### 3.4 Bloqueo por intentos fallidos

Agregar campos a Usuario:

```text
AccessFailedCount
LockoutEnd
LastLoginAt
LastFailedLoginAt
```

Regla sugerida:

```text
5 intentos fallidos -> bloqueo temporal de 15 minutos
```

## 4. Prioridad 2 - Mejoras arquitectonicas

### 4.1 Separacion Controllers -> Services -> Repositories

Estado actual:

```text
Controllers orquestan parte importante del flujo y usan DbContext directamente en varios casos.
```

Evolucion sugerida:

```text
Controllers
  ↓
Application Services
  ↓
Repositories / UnitOfWork
  ↓
DbContext
```

Ejemplos de servicios futuros:

```text
ReclamacionService
SlaService
DocumentoService
AuditoriaService
NotificacionService
ReporteService
```

### 4.2 Dominio mas expresivo

Agregar objetos o servicios de dominio:

```text
ReclamacionEstadoMachine
NumeroExpedienteGenerator
SlaCalculator
DocumentoAccessPolicy
NotificacionPolicy
```

Beneficio:

- Menos logica repetida.
- Mejor testabilidad.
- Mejor lectura para equipos futuros.

### 4.3 Validadores

Agregar validadores dedicados para requests complejos.

Opcion:

```text
FluentValidation
```

Validaciones recomendadas:

- Crear reclamacion.
- Cambiar estado.
- Resolver reclamacion.
- Cerrar reclamacion.
- Subir documento.
- Crear notificacion.

## 5. Prioridad 3 - Seguridad productiva

### 5.1 Auditoria append-only mas estricta

Mejorar auditoria para produccion real:

- Tabla protegida contra update/delete.
- Usuario SQL con permisos limitados.
- Politica append-only.
- Hash encadenado opcional para detectar manipulacion.

### 5.2 Almacenamiento documental externo o cifrado

Estado actual:

```text
Archivos almacenados localmente con ruta segura y control de acceso.
```

Produccion real:

```text
Azure Blob Storage
AWS S3
MinIO self-hosted
Cifrado en reposo
Hash SHA-256 por archivo
Antivirus/escaneo opcional
```

### 5.3 Logs estructurados y observabilidad

Agregar:

```text
Serilog
CorrelationId en todos los logs
Request logging
Metrica de errores
Metrica de latencia
Dashboard basico
```

Herramientas posibles:

```text
Seq local
Grafana + Loki
Application Insights en Azure si fuera produccion
```

## 6. Prioridad 4 - API y contratos

### 6.1 Versionado de API

Agregar versionado futuro:

```text
/api/v1/auth/login
/api/v1/reclamaciones
/api/v1/reportes
```

No cambiarlo antes de la defensa si puede romper scripts existentes.

### 6.2 Contrato OpenAPI formal

Exportar Swagger a un archivo:

```text
openapi.yaml
```

Uso:

- Documentacion para frontend.
- Contrato con integraciones futuras.
- Base para clientes HTTP tipados.

### 6.3 Paginacion estandar

Estandarizar respuestas paginadas:

```json
{
  "total": 100,
  "page": 1,
  "pageSize": 20,
  "totalPages": 5,
  "data": []
}
```

## 7. Prioridad 5 - Modulos futuros de INDOTEL

Estos modulos ampliarian el sistema, pero no forman parte del Core actual probado.

### 7.1 Espectro radioelectrico

- Inventario de frecuencias.
- Licencias de uso de frecuencia.
- Renovaciones.
- Vencimientos.
- Alertas regulatorias.

### 7.2 Sanciones y facturacion

- Multas a prestadoras.
- Generacion de deuda.
- Estado de pago.
- Integracion con caja/pagos.
- Cierre de sanciones pagadas.

### 7.3 Portal ciudadano

- Frontend web.
- Consulta de reclamaciones.
- Subida de evidencias.
- Notificaciones visuales.
- Descarga de resoluciones.

### 7.4 Portal prestadora

- Bandeja de reclamaciones recibidas.
- Respuestas formales.
- Adjuntar evidencias.
- Seguimiento de SLA.

## 8. Recomendacion antes de defensa

No implementar cambios grandes antes de la defensa.

Hacer solamente:

```text
1. Revisar que la API siga corriendo.
2. Ejecutar la prueba final.
3. Preparar discurso de defensa.
4. Tener listos los documentos de evidencia.
5. Mostrar limitaciones de forma honesta.
```

## 9. Frase recomendada

> El Core actual ya cumple el alcance funcional academico. Las mejoras listadas en este documento no son correcciones urgentes, sino una ruta de evolucion hacia produccion real.
