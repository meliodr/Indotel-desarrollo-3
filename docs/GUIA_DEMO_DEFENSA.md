# Guía de demostración y defensa

## Objetivo de la exposición

Demostrar que el sistema gestiona una reclamación de extremo a extremo, aplica seguridad por roles, protege la propiedad del ciudadano y responde de forma controlada ante fallos.

Duración recomendada: 12 a 15 minutos.

## 1. Preparación previa

Un día antes:

1. Ejecutar Sprint 5 con runtime.
2. Ejecutar Sprint 6 con build y smoke.
3. Confirmar Caja en Windows.
4. Instalar y confiar la CA local de Caddy.
5. Crear un backup.
6. Probar restauración en un entorno separado.
7. Preparar cuentas de demostración.
8. Limpiar datos de pruebas visibles.
9. Guardar capturas de pipelines y pruebas.
10. Desactivar notificaciones o aplicaciones que puedan interrumpir.

Comandos:

```bash
cd /home/jarry/indotel-prueba-integracion

RUN_RUNTIME_TESTS=1 \
INDOTEL_SQL_PASSWORD='***' \
bash scripts/validar_sprint5_integracion.sh

BUILD_IMAGES=1 RUN_RELEASE_SMOKE=1 \
bash scripts/validar_sprint6_despliegue.sh

bash scripts/desplegar_release.sh deploy/.env.release
```

## 2. Evidencias que deben estar abiertas

- Resultado Core: 20/20.
- Resultado Gateway: 14/14.
- Resultado Web: 12/12.
- Resultado Caja: 15/15.
- Workflow Windows de Caja aprobado.
- Resultado runtime integrado.
- Arquitectura final.
- Página `/health/status`.
- Backup con archivo `.sha256`.

## 3. Guion

### Minuto 0–1 — Problema y solución

> El proyecto centraliza reclamaciones de telecomunicaciones. Separamos el portal ciudadano, la aplicación interna, el Gateway, el Core y la base de datos para que cada componente tenga una responsabilidad clara.

Aclaración:

> Es un prototipo académico avanzado. No afirmamos que sea una plataforma gubernamental certificada.

### Minuto 1–3 — Arquitectura

Mostrar `docs/ARQUITECTURA_FINAL.md`.

> Web y Caja nunca acceden directamente a SQL Server. Ambos consumen el Gateway. El Core es la autoridad de seguridad y reglas de negocio. El Gateway maneja disponibilidad, timeout y circuit breaker. Caddy ofrece la entrada HTTPS.

Puntos clave:

- una URL pública;
- SQL en red interna;
- JWT y roles en Core;
- `correlationId` de extremo a extremo;
- Caja sin acceso SQL.

### Minuto 3–6 — Flujo ciudadano

1. Abrir Web.
2. Registrar ciudadano de demostración.
3. Iniciar sesión.
4. Crear reclamación.
5. Mostrar expediente.
6. Adjuntar documento válido.
7. Consultar historial y notificación.

Frase:

> El ciudadano solo ve sus expedientes. Esta restricción no depende de ocultar botones; el Core verifica identidad y propiedad.

### Minuto 6–9 — Flujo interno

1. Abrir Caja.
2. Login como AnalistaDAU.
3. Buscar por cédula o expediente.
4. Abrir detalle.
5. Mostrar transiciones permitidas consultadas al Core.
6. Cambiar de `RECIBIDA` a `VALIDADA`.
7. Mostrar historial.
8. Resolver y cerrar siguiendo el flujo.

Frase:

> Caja no inventa la máquina de estados. Consulta al Core y el Core vuelve a validar la transición.

### Minuto 9–10 — Auditor

1. Cerrar sesión.
2. Entrar como Auditor.
3. Mostrar que puede consultar.
4. Mostrar botones de escritura deshabilitados.

Frase:

> La interfaz aplica solo lectura para mejorar la experiencia, pero la autorización definitiva permanece en el Core.

### Minuto 10–12 — Resiliencia

1. Mostrar `/health/status`.
2. Detener Core:

```bash
docker stop indotel-release-core
```

3. Consultar `/api/health` a través del Gateway.
4. Mostrar HTTP 503 con `codigo` y `correlationId`.
5. Mostrar que Caja/Web presentan mensaje controlado.
6. Reiniciar Core:

```bash
docker start indotel-release-core
```

Frase:

> La disponibilidad no es responsabilidad exclusiva de Caja. El Gateway detecta la caída y devuelve 503, mientras Web y Caja deben mostrar el fallo sin cerrarse.

### Minuto 12–14 — Seguridad y pruebas

Mostrar los resultados:

```text
Core:     20 pruebas
Gateway:  14 pruebas
Web:      12 pruebas
Caja:     15 pruebas
Total:    61 pruebas base
```

Mencionar:

- refresh token rotado y almacenado por hash;
- detección de reutilización;
- archivos validados por firma;
- SQL no expuesto públicamente;
- secretos fuera de Git;
- backup, restauración y rollback documentados.

### Cierre

> El resultado es un sistema modular, reproducible y defendible. Las reglas sensibles se concentran en el Core, la disponibilidad se controla en el Gateway y cada cliente maneja correctamente la experiencia del usuario.

## 4. Preguntas probables

### ¿Por qué existe un Gateway si ya hay una API?

Porque ofrece una dirección estable, limita tráfico, propaga correlación, aplica timeout y circuit breaker y evita que cada cliente conozca el puerto interno del Core.

### ¿Caja puede conectarse directamente a SQL?

No. Caja solo consume HTTPS del Gateway. El acceso a datos pertenece al Core.

### ¿Quién controla los roles?

El Core. Web y Caja también adaptan botones y navegación, pero eso no sustituye la autorización del servidor.

### ¿Qué ocurre si el Core se apaga?

El Gateway responde 503 estructurado. Web y Caja muestran un mensaje controlado y no deben cerrarse.

### ¿Se reintentan las operaciones?

Solo consultas seguras como GET, HEAD y OPTIONS sin cuerpo. No se reintentan automáticamente POST, PUT, PATCH, DELETE ni documentos.

### ¿Cómo evitan que un ciudadano vea otro expediente?

El Core obtiene la identidad del JWT y compara el ciudadano asociado con el propietario del recurso.

### ¿Cómo protegen los documentos?

Se valida autorización, tamaño, extensión y firma real del archivo. La descarga también verifica propiedad o rol.

### ¿Qué significa `correlationId`?

Es una referencia única que atraviesa clientes, Gateway y Core para rastrear un error sin mostrar detalles sensibles.

### ¿Está listo para producción gubernamental?

No se afirma eso. Está listo como prototipo académico reproducible. Producción institucional requeriría infraestructura, certificados públicos, monitoreo, pruebas de penetración y gobierno de datos oficiales.

## 5. Plan de contingencia

Si falla Internet:

- la demostración funciona en localhost;
- tener repositorio y artefacto de Caja descargados;
- conservar capturas de pipelines.

Si falla Caja:

- mostrar las 15 pruebas y el workflow Windows;
- explicar el flujo con capturas;
- continuar con Web y API.

Si falla SQL:

- mostrar readiness 503;
- explicar backup y restauración;
- restaurar desde un respaldo probado si hay tiempo.

Si una cuenta se bloquea:

- usar una cuenta de demostración alternativa;
- no improvisar cambios directos en SQL durante la defensa.

## 6. Lista final de comprobación

- [ ] Todos los contenedores healthy.
- [ ] Web abre sin advertencia.
- [ ] Caja conecta por HTTPS.
- [ ] Cuentas y roles comprobados.
- [ ] Reclamación de demostración preparada.
- [ ] Documento permitido disponible.
- [ ] Archivo falso disponible para demostrar rechazo.
- [ ] Backup reciente y checksum.
- [ ] Terminales con comandos preparados.
- [ ] Resultados 61 pruebas disponibles.
- [ ] Arquitectura abierta.
- [ ] Plan de contingencia disponible.
