SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

-- Se desactivan, no se eliminan, para conservar integridad referencial e historial.
UPDATE Prestadoras
SET Activa = 0
WHERE NombreComercial LIKE '%Prueba%'
   OR RazonSocial LIKE '%Prueba%';

UPDATE ServiciosTelecom
SET Activo = 0
WHERE Nombre LIKE '%Prueba%'
   OR Descripcion LIKE '%Prueba%';

SELECT Id, NombreComercial, Activa
FROM Prestadoras
ORDER BY NombreComercial;

SELECT Id, Nombre, Activo
FROM ServiciosTelecom
ORDER BY Nombre;

COMMIT TRANSACTION;
