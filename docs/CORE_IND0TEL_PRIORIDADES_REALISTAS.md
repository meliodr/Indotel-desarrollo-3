# Prioridades realistas basadas en INDOTEL

Proyecto: Sistema Digital INDOTEL
Modulo actual: Core Backend de reclamaciones
Rama: `core`

## 1. Criterio de seleccion

INDOTEL real tiene muchas funciones institucionales. No conviene intentar copiarlas todas dentro del proyecto academico.

La decision correcta es conservar el Core actual como modulo de reclamaciones y definir solo algunas extensiones futuras que agreguen valor regulatorio sin romper el alcance.

Criterios usados para elegir:

```text
1. Que sea importante para INDOTEL real.
2. Que tenga relacion directa con nuestro Core actual.
3. Que pueda modelarse como modulo backend sin depender de pantallas complejas.
4. Que ayude a defender que el sistema puede evolucionar.
5. Que no obligue a rehacer el Core probado.
```

## 2. Lo que ya cubre nuestro Core

El Core actual cubre muy bien el eje de Atencion al Usuario:

```text
Reclamaciones contra prestadoras.
Prestadoras.
Servicios telecom.
SLA regulatorio.
Documentos/evidencias.
Auditoria institucional.
Resolucion y cierre.
Reportes.
Notificaciones.
Seguridad por roles y dueno real.
```

Por eso no se recomienda agregar mas funcionalidades de reclamaciones antes de la defensa. Esa parte ya esta solida y probada.

## 3. Prioridades futuras seleccionadas

### Prioridad 1 - Gestion de autorizaciones y certificaciones

Esta es la extension mas importante para acercarse al INDOTEL real sin cambiar el centro del sistema.

INDOTEL maneja autorizaciones y certificaciones institucionales. En nuestro sistema podria ser un modulo futuro para gestionar solicitudes de empresas o entidades reguladas.

Alcance recomendado:

```text
Solicitudes de autorizacion.
Solicitudes de certificacion.
Entidad solicitante.
Tipo de autorizacion.
Estado de solicitud.
Documentos requeridos.
Analista responsable.
Resolucion administrativa.
Fecha de vencimiento.
Renovacion.
```

Estados sugeridos:

```text
RECIBIDA
EN_REVISION
DOCUMENTACION_INCOMPLETA
APROBADA
RECHAZADA
VENCIDA
RENOVADA
```

Por que importa:

```text
Conecta con funciones reales de INDOTEL.
Reutiliza nuestro motor de documentos.
Reutiliza auditoria.
Reutiliza notificaciones.
Reutiliza reportes.
No rompe el motor de reclamaciones.
```

### Prioridad 2 - Espectro radioelectrico y licencias tecnicas

Este es el modulo mas regulatorio de INDOTEL. No debe implementarse completo ahora, pero debe quedar como fase futura principal.

Alcance recomendado:

```text
Inventario de frecuencias.
Rango de frecuencia.
Region o provincia.
Prestadora o entidad asignada.
Tipo de servicio: radio, TV, internet, telefonia, radioaficionado.
Estado: disponible, asignada, reservada, vencida.
Licencia asociada.
Fecha de inicio.
Fecha de vencimiento.
Alertas de renovacion.
```

Estados sugeridos para licencia:

```text
SOLICITADA
EN_EVALUACION_TECNICA
APROBADA
ACTIVA
POR_VENCER
VENCIDA
CANCELADA
```

Por que importa:

```text
Es una funcion central de un regulador telecom.
Diferencia el proyecto de un simple sistema de quejas.
Permite reportes tecnicos.
Permite alertas por vencimiento.
Puede compartir auditoria y notificaciones.
```

### Prioridad 3 - Resoluciones institucionales del Consejo Directivo

Nuestro Core ya tiene resolucion de reclamaciones, pero no resoluciones institucionales oficiales.

Este modulo seria pequeno y muy defendible.

Alcance recomendado:

```text
Numero de resolucion.
Fecha de aprobacion.
Tipo de resolucion.
Entidad relacionada.
Resumen.
Documento oficial.
Estado de publicacion.
Relacion con reclamacion, autorizacion, prestadora o licencia.
```

Estados sugeridos:

```text
BORRADOR
APROBADA
PUBLICADA
ARCHIVADA
```

Por que importa:

```text
Da apariencia institucional real.
Conecta con auditoria y documentos.
Permite cerrar casos con soporte formal.
Es facil de explicar en defensa.
No requiere integraciones externas.
```

### Prioridad 4 - Estadisticas regulatorias del sector

Nuestro Core ya tiene reportes de reclamaciones. Como fase futura, se puede ampliar a estadisticas del sector telecom.

Alcance recomendado:

```text
Indicadores por prestadora.
Indicadores por servicio.
Indicadores por provincia.
Cantidad de reclamaciones por periodo.
Tiempo promedio de respuesta.
Prestadoras con mas incumplimientos SLA.
Ranking de servicios mas reclamados.
```

No se recomienda iniciar con estadisticas nacionales complejas de lineas moviles, internet fijo o mercado completo, porque eso exigiria datos externos grandes.

Por que importa:

```text
Aprovecha datos que ya tenemos.
No requiere nuevos modulos complejos.
Mejora los reportes regulatorios.
Ayuda mucho en la presentacion.
```

## 4. Funciones que NO tomaremos ahora

Estas funciones existen o pueden existir alrededor de INDOTEL, pero no conviene agregarlas al alcance actual:

```text
Consulta IMEI / GSMA.
Consulta de lineas prepago por cedula.
Firma digital completa.
Comercio electronico.
Sandbox regulatorio.
Proyectos de conectividad e infraestructura.
Buzon institucional interno de sugerencias.
Licitaciones.
Portal de transparencia.
Noticias y contenido publico.
```

Motivo:

```text
Aumentan demasiado el alcance.
Requieren integraciones externas.
No fortalecen directamente el Core ya probado.
Pueden distraer de la defensa del modulo principal.
```

## 5. Recomendacion final

Para la defensa, decir:

> El Core actual cubre el eje de Atencion al Usuario y Reclamaciones, que es una de las funciones visibles de INDOTEL. Para evolucionar el sistema, priorizamos tres lineas realistas: autorizaciones/certificaciones, espectro radioelectrico/licencias tecnicas y resoluciones institucionales. No intentamos copiar todo INDOTEL porque eso romperia el alcance academico.

## 6. Orden sugerido de implementacion futura

```text
Fase 2A: Resoluciones institucionales.
Fase 2B: Autorizaciones y certificaciones.
Fase 2C: Espectro radioelectrico y licencias tecnicas.
Fase 2D: Estadisticas regulatorias ampliadas.
```

La razon de este orden es simple:

```text
Resoluciones es pequeño y conecta con lo actual.
Autorizaciones reutiliza documentos, auditoria y notificaciones.
Espectro es importante pero mas tecnico.
Estadisticas ampliadas se alimentan de los datos anteriores.
```
