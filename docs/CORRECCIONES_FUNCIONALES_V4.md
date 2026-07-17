# Correcciones funcionales V4 — Windows

Fecha: 17 de julio de 2026.

## Incidencias corregidas

1. **Caja duplicada**
   - Mutex de instancia única en Caja WPF.
   - Bloqueo contra doble envío del formulario de acceso.
   - El lanzador no vuelve a iniciar Caja ni Core UI si ya están abiertos.
   - Las suscripciones del `MainViewModel` se liberan al cerrar la ventana.

2. **Core UI se cerraba al cerrar sesión**
   - El cierre de sesión regresa a `FrmLogin`.
   - Cerrar la ventana del dashboard sí termina la aplicación.
   - Core UI también usa instancia única.

3. **Menús incoherentes en Core UI**
   - La navegación se filtra por rol.
   - Cajero y SupervisorCaja solo acceden a las áreas financieras correspondientes.
   - Catálogos comerciales, monedas, tasas, auditoría y salud respetan sus perfiles.
   - Se unificó la categoría `ESPECTRO_RADIOELECTRICO`.

4. **USD no aparecía**
   - El seed ahora hace upsert por código; agrega USD aunque DOP ya exista.
   - Se agregan tasas DOP/USD y USD/DOP, un servicio USD, un producto USD y una factura USD.

5. **Datos de prueba hardcodeados**
   - Los datos de demostración se movieron a `Data/Seed/seed-development.json`.
   - El código de seed solo interpreta y sincroniza el archivo externo.
   - Hay datos representativos para usuarios, roles, prestadoras, servicios telecom, reclamaciones, clientes, productos, servicios cobrables, monedas, tasas, exenciones, aprobaciones, sucursal, caja, facturas y cuentas por cobrar.

6. **Inicio público mostraba acceso aunque había sesión**
   - La portada cambia a `Iniciar trámite` e `Ir a mi panel` cuando el ciudadano está autenticado.
   - Los visitantes conservan `Iniciar sesión` y registro.

7. **Iniciar trámite fallaba**
   - Se consolidó el flujo en `Ciudadano/NuevaReclamacion`.
   - El formulario anterior redirige al flujo canónico y no crea registros duplicados.
   - Los catálogos se consultan por `/api/catalogos/prestadoras` y `/api/catalogos/servicios`.
   - Los errores de catálogo ya no se ocultan.
   - El botón queda bloqueado después del primer envío válido.

8. **Autoservicio / EstadoOrden fallaba**
   - `EstadoOrden` ahora renderiza la vista existente `DetalleOrden.cshtml`.
   - Se agregó protección de doble envío en órdenes.
   - El navegador impide combinar DOP y USD en una misma orden antes de enviarla.

## Duplicaciones localizadas

- Inicio repetido de Caja/Core UI desde el lanzador.
- Doble evento clic/Enter en el acceso de Caja.
- Reapertura de ventanas por eventos de sesión acumulados.
- Dos flujos de inicio de sesión Web (`Auth` y `Comercial`).
- Dos flujos de creación de reclamaciones (`Ciudadano` y `Reclamacion`).
- Doble clic en completar orden o presentar reclamación.

Todos quedaron bloqueados o consolidados.

## Validación en Windows

Ejecutar:

```text
08_APLICAR_Y_VALIDAR_CORRECCIONES_V4.bat
```

El script detiene procesos, valida el seed externo, compila, ejecuta pruebas, inicia todos los módulos y comprueba los cuatro endpoints de salud. La evidencia queda en `VALIDACION_CORRECCIONES_V4.txt`.
