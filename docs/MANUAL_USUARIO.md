# Manual de usuario — Sistema Digital INDOTEL

## 1. Portal ciudadano

### Acceso

Abrir la dirección entregada por el administrador, por ejemplo:

```text
https://localhost:8443/
```

En laboratorio puede aparecer una advertencia de certificado hasta instalar la CA local autorizada.

### Registrarse

1. Seleccionar **Registrarse**.
2. Completar cédula, nombres, apellidos, teléfono, correo y dirección.
3. Crear una contraseña que incluya mayúscula, minúscula, número y símbolo.
4. Enviar el formulario una sola vez.
5. El sistema inicia la sesión o permite entrar con el correo registrado.

No compartir la contraseña ni el código de referencia de una sesión.

### Iniciar sesión

1. Escribir correo y contraseña.
2. Seleccionar **Entrar**.
3. Si el servicio central no está disponible, esperar y usar **Reintentar**. Los datos del formulario no deben enviarse repetidamente.
4. Después de varios intentos incorrectos, la cuenta puede bloquearse temporalmente.

### Crear una reclamación

1. Abrir **Nueva reclamación**.
2. Elegir prestadora y servicio.
3. Indicar provincia y municipio.
4. Escribir un título claro.
5. Describir el problema, fechas y gestiones previas.
6. Revisar la información.
7. Seleccionar **Enviar** una sola vez.
8. Guardar el número de expediente mostrado.

### Adjuntar documentos

Formatos permitidos:

```text
PDF
PNG
JPG/JPEG
```

El sistema comprueba extensión, tamaño y contenido real. Un archivo renombrado con una extensión falsa será rechazado.

No subir contraseñas, tarjetas, información médica innecesaria ni documentos ajenos al caso.

### Consultar seguimiento

En **Mis reclamaciones** se puede:

- consultar número de expediente;
- ver estado actual;
- abrir detalle;
- revisar historial;
- consultar documentos;
- descargar documentos autorizados;
- revisar notificaciones.

Un ciudadano solo puede acceder a sus propios datos y expedientes.

### Estados principales

```text
RECIBIDA
OBSERVADA
VALIDADA
ENVIADA_A_PRESTADORA
RESPONDIDA_POR_PRESTADORA
EN_REVISION_INDOTEL
RESUELTA
CERRADA
```

También pueden existir estados finales o regulatorios como `RECHAZADA`, `ARCHIVADA` o `VENCIDA`.

### Notificaciones

1. Abrir el área de notificaciones.
2. Leer el aviso.
3. Marcar como leído cuando corresponda.
4. Abrir el expediente relacionado desde el enlace disponible.

### Cerrar sesión

Seleccionar **Cerrar sesión**. La sesión local se elimina incluso cuando el servicio central está temporalmente apagado.

## 2. Caja interna

### Usuarios autorizados

- **Administrador:** administración y operación completa.
- **AnalistaDAU:** gestión operativa de ciudadanos y reclamaciones.
- **Auditor:** consulta de solo lectura.

Ciudadano y Prestadora no deben entrar en Caja.

### Iniciar Caja

1. Ejecutar `INDOTEL_CAJA_REAL.exe` en Windows.
2. Verificar que la dirección del Gateway esté configurada.
3. Iniciar sesión.
4. Si aparece “servicio no disponible”, comprobar red y usar **Reintentar**.

### Buscar ciudadano

1. Abrir **Buscar ciudadano**.
2. Digitar cédula de 11 números.
3. Seleccionar **Buscar**.
4. Revisar que nombre y correo correspondan a la persona presente.

### Registrar ciudadano

Disponible para roles operativos:

1. Abrir **Registrar ciudadano**.
2. Completar datos obligatorios.
3. Validar cédula y correo.
4. Confirmar una sola vez.

Auditor no puede registrar.

### Crear reclamación en ventanilla

1. Seleccionar el ciudadano.
2. Abrir **Nueva reclamación**.
3. Seleccionar prestadora y servicio.
4. Completar ubicación, título y descripción.
5. Confirmar.
6. Entregar o anotar el número de expediente.

### Bandeja de reclamaciones

Permite:

- buscar por expediente;
- buscar por cédula;
- aplicar filtros;
- navegar por páginas;
- abrir detalle;
- consultar historial;
- consultar transiciones permitidas.

### Cambiar estado

1. Abrir el expediente.
2. Seleccionar **Cambiar estado**.
3. Elegir únicamente entre las transiciones devueltas por el Core.
4. Escribir comentario justificativo.
5. Confirmar una sola vez.

No es posible:

- cerrar antes de resolver;
- enviar a prestadora antes de validar;
- modificar un expediente cerrado;
- saltar estados no autorizados.

### Resolver y cerrar

1. Verificar historial y respuesta de prestadora.
2. Completar comentario de resolución.
3. Registrar monto cuando corresponda; nunca negativo.
4. Pasar a `RESUELTA`.
5. Revisar el expediente.
6. Ejecutar el cierre formal para pasar a `CERRADA`.

### Auditor

El Auditor puede consultar información, pero no debe encontrar habilitados botones de:

- registrar ciudadano;
- crear reclamación;
- cambiar estado;
- resolver;
- cerrar.

### Errores y referencia

Cuando Caja muestre un error, copiar:

```text
Código
Mensaje
CorrelationId o referencia
Hora aproximada
Operación realizada
```

No copiar tokens, contraseñas ni contenido confidencial.

## 3. Buenas prácticas

- Usar una cuenta individual.
- No compartir credenciales.
- Cerrar sesión al terminar.
- Confirmar identidad antes de modificar datos.
- No hacer doble clic repetido en operaciones.
- No intentar cambiar estados fuera del flujo.
- Reportar errores con el `correlationId`.
- No guardar documentos descargados en equipos públicos.

## 4. Mensajes frecuentes

### Servicio temporalmente no disponible

El Gateway o el Core no responde. La aplicación no debe cerrarse. Esperar, comprobar conexión y reintentar.

### Acceso denegado

La cuenta no tiene permiso para esa función o intenta acceder a información ajena.

### Sesión expirada

Volver a iniciar sesión. Si ocurre repetidamente, reportar hora y referencia.

### Archivo inválido

Comprobar formato, tamaño y que el archivo sea realmente PDF, PNG o JPEG.

### Transición no permitida

Revisar el estado actual. Caja solo debe mostrar las transiciones autorizadas por el Core.

## 5. Soporte

Al reportar un incidente incluir:

- módulo: Web o Caja;
- usuario y rol, sin contraseña;
- fecha y hora;
- expediente, cuando aplique;
- pasos realizados;
- código de error;
- `correlationId`;
- captura sin datos sensibles.
