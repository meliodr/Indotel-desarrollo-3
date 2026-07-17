# Corrección de migración del catálogo

La migración `20260716210000_CargaCatalogoServiciosIndotel` incluía por error `FechaActualizacion` al insertar la moneda DOP. La tabla `Monedas`, creada por `20260715120000_FaseComercialBaseGui`, no contiene esa columna.

La corrección elimina únicamente esa columna del `INSERT` de `Monedas`. Las columnas `FechaActualizacion` de `ServiciosCobrables` y `ClientesComerciales` se conservan porque sí existen.

La migración fallida no quedó registrada en `__EFMigrationsHistory`, por lo que puede volver a ejecutarse de forma segura después de aplicar esta corrección. EF Core ejecuta la migración dentro de una transacción.

También se actualizó `IndotelDbContextFactory` para aceptar tanto `INDOTEL_DESIGN_CONNECTION` como `ConnectionStrings__DefaultConnection`.

Paquete corregido: `INDOTEL_PROYECTO_ETAPAS_0_A_2_CORREGIDO.zip`.
SHA-256: `180bbe57b03019314e2be0c90177fa6a8e1a78d6bcca067683466582cffc7ceb`.
