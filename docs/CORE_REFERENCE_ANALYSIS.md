# Analisis del Core de referencia: SistemaGomas

Este documento resume lo encontrado en el Core del proyecto anterior y como se usara para medir el alcance del Core INDOTEL.

## 1. Tecnologias del Core anterior

- ASP.NET Web API sobre .NET Framework 4.7.2.
- Entity Framework 6.
- SQL Server LocalDB.
- Swagger con Swashbuckle.
- log4net.
- NServiceBus aparece en el proyecto, pero no esta completamente integrado.

## 2. Controladores encontrados

El Core anterior tiene estos controladores principales:

- UsuariosController.
- ProductosController.
- ServiciosController.
- InventarioController.
- FacturacionController.
- CajaController.
- VehiculosController.
- SucursalesController.

## 3. Alcance real del Core anterior

Aunque el proyecto parece grande, el Core esta concentrado en estas funciones:

- Login y registro de usuarios.
- Catalogo de productos.
- Catalogo de servicios.
- Inventario.
- Facturacion.
- Caja.
- Vehiculos.
- Sucursales.

Para INDOTEL, el equivalente sera:

- Usuarios y login.
- Ciudadanos.
- Prestadoras.
- Servicios telecom.
- Reclamaciones.
- Respuestas de prestadoras.
- Auditoria.
- Reportes basicos.

## 4. Lo bueno que debemos imitar

- Tiene controladores separados por modulo.
- Usa Swagger.
- Usa transacciones en procesos importantes.
- Tiene logging con log4net.
- Tiene DTOs de entrada y salida.
- Tiene rutas claras con RoutePrefix.
- Tiene flujo de negocio completo en facturacion.

## 5. Lo que debemos mejorar

- No usar tokens simulados. INDOTEL debe usar JWT real.
- No duplicar rutas.
- No mezclar demasiado SQL crudo dentro de controladores.
- No poner DTOs dentro de controladores grandes si crecen demasiado.
- No subir bin, obj, .vs ni archivos temporales.
- No depender de archivos MDF como unica forma de base de datos.
- No dejar una capa de integracion incompleta.
- No conectar pantallas directamente a la base de datos.

## 6. Problemas encontrados en el Core anterior

- Login devuelve un TOKEN-SIMULADO, no un JWT real.
- FacturacionController tiene rutas duplicadas: historial/{idCliente} y detalle/{idFactura}.
- GomasContext solo declara algunos DbSet; muchas tablas se usan con SQL directo.
- El hash de password usa SHA256 simple, sin salt.
- El proyecto incluye archivos binarios y carpetas que no deben ir al repositorio final.
- NServiceBus aparece en el Core, pero la mensajeria no esta terminada.

## 7. Medida de alcance para nuestro Core

El Core anterior tiene cerca de 8 controladores funcionales. Para que INDOTEL sea mejor, nuestro Core debe tener al menos:

- AuthController.
- UsuariosController.
- CiudadanosController.
- PrestadorasController.
- ServiciosController.
- ReclamacionesController.
- AuditoriaController.
- ReportesController.

Si el tiempo alcanza, se agregan:

- CertificacionesController.
- AutorizacionesController.
- EspectroController.
- InspeccionesController.

## 8. Prioridad real

Para superar el Core anterior sin pasarnos del alcance universitario, el minimo fuerte sera:

1. JWT real.
2. CRUDs principales bien organizados.
3. Reclamaciones con flujo completo.
4. Auditoria de cambios.
5. Reportes basicos.
6. Swagger limpio.
7. Base de datos por migraciones o scripts SQL.

## 9. Decision para INDOTEL

El Core INDOTEL no intentara hacer todos los 12 modulos completos desde el inicio. Primero se hara el flujo de reclamaciones completo, porque ese es el corazon del sistema y permite demostrar integracion con portal, app interna y middleware.
