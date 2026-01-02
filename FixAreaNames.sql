-- Fix Area names with proper Spanish characters

UPDATE Areas SET NombreGeneral = 'Preparación de Materiales' WHERE AreaId = 4;
UPDATE Areas SET NombreGeneral = 'Construcción Llantas Radial' WHERE AreaId = 5;
UPDATE Areas SET NombreGeneral = 'Acabado Reparación Inspección' WHERE AreaId = 7;
UPDATE Areas SET NombreGeneral = 'Almacén Materias Primas' WHERE AreaId = 8;
UPDATE Areas SET NombreGeneral = 'Mantenimiento Área I' WHERE AreaId = 9;
UPDATE Areas SET NombreGeneral = 'Mantenimiento Área II' WHERE AreaId = 10;
UPDATE Areas SET NombreGeneral = 'Mantenimiento Área III' WHERE AreaId = 11;
UPDATE Areas SET NombreGeneral = 'Mantenimiento Eléctrico' WHERE AreaId = 12;
UPDATE Areas SET NombreGeneral = 'Mantenimiento Eléctrico II' WHERE AreaId = 13;
UPDATE Areas SET NombreGeneral = 'Mantenimiento Eléctrico III' WHERE AreaId = 14;
UPDATE Areas SET NombreGeneral = 'Metrología' WHERE AreaId = 15;
UPDATE Areas SET NombreGeneral = 'Aire Vapor Vacío Agua' WHERE AreaId = 17;
UPDATE Areas SET NombreGeneral = 'Almacén de Producto Terminado' WHERE AreaId = 18;
UPDATE Areas SET NombreGeneral = 'Construcción de Bladders' WHERE AreaId = 19;
UPDATE Areas SET NombreGeneral = 'Instructores Técnicos' WHERE AreaId = 20;
UPDATE Areas SET NombreGeneral = 'Vulcanización MX' WHERE AreaId = 24;

-- Verify the updates
SELECT AreaId, NombreGeneral FROM Areas ORDER BY AreaId;
