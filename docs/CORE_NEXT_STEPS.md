# Proximos pasos del Core INDOTEL

Proyecto: Sistema Digital INDOTEL
Modulo: Core Backend
Rama: `core`

## 1. Estado actual

El Core esta funcional y validado dentro del alcance academico definido.

Estado recomendado para presentacion:

```text
Core principal de reclamaciones: 100%
Fase 2A - Resoluciones institucionales: 100%
Fase 2B - Autorizaciones y certificaciones: 100%
Fase 2C - Espectro radioelectrico y licencias tecnicas: 100%
Fase 2D - Reportes regulatorios ampliados: 100%
Fase 3 - Hardening basico de autenticacion: 100%
```

No se declara como produccion gubernamental real certificada.

## 2. Objetivo de este documento

Este documento lista mejoras futuras que elevarian el Core desde prototipo institucional avanzado hacia una base productiva mas robusta.

Varias mejoras que antes estaban como pendientes ya fueron implementadas en Fase 3:

```text
Refresh token.
Logout real.
Revocacion de refresh token.
Bloqueo por intentos fallidos.
Rate limiting basico en Auth.
```

Por eso, lo que sigue ahora no es agregar mas funciones rapidamente, sino cerrar, defender, probar y preparar una evolucion mas limpia.

## 3. Prioridad 1 - Cierre de entrega

Antes de cualquier nueva funcion:

```text
1. Verificar git status.
2. Subir migraciones pendientes si existen.
3. Ejecutar prueba final del Core.
4. Ejecutar scripts de Fase 2A, 2B, 2C, 2D y Fase 3.
5. No tocar mas codigo si todo pasa.
6. Preparar defensa.
```

Comandos sugeridos:

```bash
git status --short
bash /tmp/probar_final_100_indotel.sh
bash scripts/probar_fase2a_resoluciones.sh
bash scripts/probar_fase2b_autorizaciones_certificaciones.sh
ADMIN_PASSWORD='***' bash scripts/probar_fase2c_espectro_licencias.sh
ADMIN_PASSWORD='***' bash scripts/probar_fase2d_reportes_ampliados.sh
ADMIN_PASSWORD='***' bash scripts/probar_fase3_hardening_auth.sh
```

## 4. Prioridad 2 - Pruebas automatizadas formales

Actualmente existen scripts funcionales end-to-end. El siguiente paso profesional seria convertir parte de esa cobertura en pruebas automatizadas dentro del proyecto.

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
- Refresh token.
- Logout.
- Bloqueo por intentos fallidos.
- Registro ciudadano.
- RBAC por roles.
- Dueno real ciudadano.
- Transiciones validas de reclamaciones.
- Transiciones invalidas de reclamaciones.
- SLA vencido.
- Resolucion y cierre.
- Bloqueo de documentos a usuario ajeno.
- Resoluciones institucionales.
- Autorizaciones.
- Certificaciones.
- Espectro.
- Licencias tecnicas.
- Notificaciones protegidas.
- Auditoria generada.

## 5. Prioridad 3 - Mejoras arquitectonicas

### 5.1 Separacion Controllers -> Services -> Repositories

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
ResolucionInstitucionalService
AutorizacionService
CertificacionService
EspectroService
LicenciaTecnicaService
SlaService
DocumentoService
AuditoriaService
NotificacionService
ReporteService
AuthSessionService
```

### 5.2 Dominio mas expresivo

Agregar objetos o servicios de dominio:

```text
ReclamacionEstadoMachine
ResolucionEstadoMachine
SolicitudInstitucionalEstadoMachine
LicenciaTecnicaEstadoMachine
NumeroExpedienteGenerator
NumeroResolucionGenerator
SlaCalculator
DocumentoAccessPolicy
NotificacionPolicy
```

Beneficio:

- Menos logica repetida.
- Mejor testabilidad.
- Mejor lectura para equipos futuros.

### 5.3 Validadores

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
- Crear resolucion.
- Crear autorizacion.
- Crear certificacion.
- Crear frecuencia.
- Crear licencia tecnica.
- Crear notificacion.

## 6. Prioridad 4 - Seguridad y operacion productiva

### 6.1 Auditoria append-only mas estricta

Mejorar auditoria para produccion real:

- Tabla protegida contra update/delete.
- Usuario SQL con permisos limitados.
- Politica append-only.
- Hash encadenado opcional para detectar manipulacion.

### 6.2 Almacenamiento documental externo o cifrado

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

### 6.3 Logs estructurados y observabilidad

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

### 6.4 Politicas por ambiente

Ajustar para produccion:

```text
CORS estricto.
Secretos fuera de appsettings.
Rotacion de secretos.
HTTPS obligatorio.
Headers de seguridad.
Rate limiting por IP/usuario mas granular.
```

## 7. Prioridad 5 - API y contratos

### 7.1 Versionado de API

Agregar versionado futuro:

```text
/api/v1/auth/login
/api/v1/reclamaciones
/api/v1/reportes
```

No cambiarlo antes de la defensa si puede romper scripts existentes.

### 7.2 Contrato OpenAPI formal

Exportar Swagger a un archivo:

```text
openapi.yaml
```

Uso:

- Documentacion para frontend.
- Contrato con integraciones futuras.
- Base para clientes HTTP tipados.

### 7.3 Paginacion estandar

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

## 8. Modulos futuros fuera del alcance actual

Estos modulos ampliarian el sistema, pero no deben implementarse antes de la defensa.

### 8.1 Sanciones y facturacion

- Multas a prestadoras.
- Generacion de deuda.
- Estado de pago.
- Integracion con caja/pagos.
- Cierre de sanciones pagadas.

### 8.2 Portal ciudadano

- Frontend web.
- Consulta de reclamaciones.
- Subida de evidencias.
- Notificaciones visuales.
- Descarga de resoluciones.

### 8.3 Portal prestadora

- Bandeja de reclamaciones recibidas.
- Respuestas formales.
- Adjuntar evidencias.
- Seguimiento de SLA.

### 8.4 Integraciones reales

- Correo/SMS real.
- Firma digital real.
- Sistemas externos gubernamentales.
- Pagos.
- Consulta IMEI/GSMA.

## 9. Recomendacion antes de defensa

No implementar cambios grandes antes de la defensa.

Hacer solamente:

```text
1. Revisar que la API siga corriendo.
2. Ejecutar la prueba final.
3. Preparar discurso de defensa.
4. Tener listos los documentos de evidencia.
5. Mostrar limitaciones de forma honesta.
```

## 10. Frase recomendada

> El Core actual ya cumple el alcance funcional academico. Las mejoras restantes no son correcciones urgentes, sino una ruta de evolucion hacia produccion real y mantenimiento a largo plazo.
