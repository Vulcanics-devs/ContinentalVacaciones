-- Script to create Grupos with proper configuration based on personas por grupo.xlsx data
-- This creates the 4-shift groups (R0144, R0144_02, R0144_03, R0144_04) for each area

-- Areas with their personnel configuration (from CSV)
-- PersonasPorTurno = "Personas x día" from CSV
-- DuracionDeturno = 6 hours (24 hours / 4 shifts)

USE FreeTime;
GO

-- Helper: Get AreaId by name (case-insensitive, partial match)
DECLARE @MaterialPrima INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%Materia%Prima%');
DECLARE @Mezclado INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%Corte%Hule%');
DECLARE @Tubulado INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%TUBULADO%');
DECLARE @Cementos INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%Cementos%');
DECLARE @Calandriado INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%CALANDRIADO%');
DECLARE @MttoA INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%AREA I%');
DECLARE @Prep1 INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%PREPARACION%MATERIALES%');
DECLARE @Construccion INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%CONSTRUCCION%RADIAL%');
DECLARE @MttoB INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%Area II%');
DECLARE @Vulcanizado INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%Vulcanizado%');
DECLARE @AF INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%ACABADO%REPARACION%');
DECLARE @LabMoldes INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%Lava%Moldes%');
DECLARE @Moldes INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%de Moldes%');
DECLARE @MttoVulca INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%AREA III%');
DECLARE @MttoAF INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%Electrico II%');
DECLARE @APT INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%Producto Terminad%');
DECLARE @Metrologia INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%Metrologia%');
DECLARE @CasaFuerza INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%Vapor%Agua%');
DECLARE @Bladders INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%Bladders%');
DECLARE @Sindicato INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%Relaciones%Laborales%');
DECLARE @Instructores INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%Instructores%');

-- Create groups for Almacén Materias Primas (4 personas por turno)
IF @MaterialPrima IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0144', @MaterialPrima, 4, 6),
    ('R0144_02', @MaterialPrima, 4, 6),
    ('R0144_03', @MaterialPrima, 4, 6),
    ('R0144_04', @MaterialPrima, 4, 6),
    ('N0439', @MaterialPrima, 0, 8); -- Admin/support group
    PRINT 'Grupos created for Almacén Materias Primas';
END

-- Create groups for Corte Hule/Mezclado (36 personas por turno)
IF @Mezclado IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0144', @Mezclado, 36, 6),
    ('R0144_02', @Mezclado, 36, 6),
    ('R0144_03', @Mezclado, 36, 6),
    ('R0144_04', @Mezclado, 36, 6),
    ('N0439', @Mezclado, 0, 8);
    PRINT 'Grupos created for Corte de Hule y Compuestos';
END

-- Create groups for Tubulado (16 personas por turno)
IF @Tubulado IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0144', @Tubulado, 16, 6),
    ('R0144_02', @Tubulado, 16, 6),
    ('R0144_03', @Tubulado, 16, 6),
    ('R0144_04', @Tubulado, 16, 6),
    ('R0267_02', @Tubulado, 0, 8), -- Special shift
    ('N0439', @Tubulado, 0, 8);
    PRINT 'Grupos created for Tubulado';
END

-- Create groups for Cementos (1 persona por turno)
IF @Cementos IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0144_03', @Cementos, 1, 6);
    PRINT 'Grupos created for Cementos';
END

-- Create groups for Calandriado (6 personas por turno)
IF @Calandriado IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0267', @Calandriado, 6, 8), -- 3-shift operation (24/3 = 8 hours)
    ('R0267_02', @Calandriado, 6, 8),
    ('R0267_03', @Calandriado, 6, 8),
    ('N0A01', @Calandriado, 0, 8);
    PRINT 'Grupos created for Calandriado';
END

-- Create groups for Mantenimiento Área I (9 personas por turno)
IF @MttoA IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0144', @MttoA, 9, 6),
    ('R0144_02', @MttoA, 9, 6),
    ('R0144_03', @MttoA, 9, 6),
    ('R0144_04', @MttoA, 9, 6),
    ('R0229', @MttoA, 0, 8),
    ('R0135', @MttoA, 0, 8),
    ('N0439', @MttoA, 0, 8);
    PRINT 'Grupos created for Mantenimiento Área I';
END

-- Create groups for Preparación de Materiales - Avg 17 personas por turno
IF @Prep1 IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0144', @Prep1, 17, 6),
    ('R0144_02', @Prep1, 17, 6),
    ('R0144_03', @Prep1, 17, 6),
    ('R0144_04', @Prep1, 17, 6),
    ('R0154_02', @Prep1, 0, 8),
    ('N0439', @Prep1, 0, 8);
    PRINT 'Grupos created for Preparación de Materiales';
END

-- Create groups for Construcción - Avg 11-46 personas por turno (using 12 avg for smaller, 46 for radial)
IF @Construccion IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0144', @Construccion, 46, 6),
    ('R0144_02', @Construccion, 46, 6),
    ('R0144_03', @Construccion, 46, 6),
    ('R0144_04', @Construccion, 46, 6),
    ('N0439', @Construccion, 0, 8);
    PRINT 'Grupos created for Construcción Llantas Radial';
END

-- Create groups for Mantenimiento Área II (13 personas por turno)
IF @MttoB IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0144', @MttoB, 13, 6),
    ('R0144_02', @MttoB, 13, 6),
    ('R0144_03', @MttoB, 13, 6),
    ('R0144_04', @MttoB, 13, 6),
    ('R0135', @MttoB, 0, 8),
    ('R0135_02', @MttoB, 0, 8),
    ('R0133', @MttoB, 0, 8),
    ('R0133_02', @MttoB, 0, 8),
    ('N0439', @MttoB, 0, 8);
    PRINT 'Grupos created for Mantenimiento Área II';
END

-- Create groups for Vulcanizado (21 personas por turno)
IF @Vulcanizado IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0144', @Vulcanizado, 21, 6),
    ('R0144_02', @Vulcanizado, 21, 6),
    ('R0144_03', @Vulcanizado, 21, 6),
    ('R0144_04', @Vulcanizado, 21, 6),
    ('N0439', @Vulcanizado, 0, 8);
    PRINT 'Grupos created for Vulcanizado';
END

-- Create groups for Acabado Reparación Inspección (28 personas por turno)
IF @AF IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0144', @AF, 28, 6),
    ('R0144_02', @AF, 28, 6),
    ('R0144_03', @AF, 28, 6),
    ('R0144_04', @AF, 28, 6),
    ('N0439', @AF, 0, 8);
    PRINT 'Grupos created for Acabado Reparación Inspección';
END

-- Create groups for Lava Moldes (5 personas por turno)
IF @LabMoldes IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0144', @LabMoldes, 5, 6),
    ('R0144_02', @LabMoldes, 5, 6),
    ('R0144_03', @LabMoldes, 5, 6),
    ('R0144_04', @LabMoldes, 5, 6),
    ('R0135', @LabMoldes, 0, 8),
    ('R0135_02', @LabMoldes, 0, 8),
    ('R0229_04', @LabMoldes, 0, 8),
    ('N0439', @LabMoldes, 0, 8);
    PRINT 'Grupos created for Lava Moldes';
END

-- Create groups for Taller de Moldes (9 personas por turno)
IF @Moldes IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0229', @Moldes, 9, 8),
    ('R0229_03', @Moldes, 9, 8),
    ('R0229_04', @Moldes, 9, 8),
    ('R0144_04', @Moldes, 0, 6),
    ('N0439', @Moldes, 0, 8);
    PRINT 'Grupos created for Taller de Moldes';
END

-- Create groups for Mantenimiento Área III (14 personas por turno)
IF @MttoVulca IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0144', @MttoVulca, 14, 6),
    ('R0144_02', @MttoVulca, 14, 6),
    ('R0144_03', @MttoVulca, 14, 6),
    ('R0144_04', @MttoVulca, 14, 6),
    ('N0439', @MttoVulca, 0, 8);
    PRINT 'Grupos created for Mantenimiento Área III';
END

-- Create groups for Mantenimiento Eléctrico II (5 personas por turno)
IF @MttoAF IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0144', @MttoAF, 5, 6),
    ('R0144_02', @MttoAF, 5, 6),
    ('R0144_03', @MttoAF, 5, 6),
    ('R0144_04', @MttoAF, 5, 6),
    ('R0135', @MttoAF, 0, 8),
    ('N0439', @MttoAF, 0, 8);
    PRINT 'Grupos created for Mantenimiento Eléctrico II';
END

-- Create groups for Almacén Producto Terminado (3 personas por turno)
IF @APT IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0144', @APT, 3, 6),
    ('R0144_02', @APT, 3, 6),
    ('R0144_03', @APT, 3, 6),
    ('R0144_04', @APT, 3, 6),
    ('N0439', @APT, 0, 8);
    PRINT 'Grupos created for Almacén de Producto Terminado';
END

-- Create groups for Metrología (1 persona por turno)
IF @Metrologia IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0228', @Metrologia, 1, 8),
    ('R0133', @Metrologia, 0, 8),
    ('N0439', @Metrologia, 0, 8);
    PRINT 'Grupos created for Metrología';
END

-- Create groups for Casa de Fuerza/Aire Vapor Vacío Agua (3 personas por turno)
IF @CasaFuerza IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0130', @CasaFuerza, 3, 8),
    ('R0130_03', @CasaFuerza, 3, 8),
    ('R0130_04', @CasaFuerza, 3, 8),
    ('R0135_02', @CasaFuerza, 0, 8),
    ('N0439', @CasaFuerza, 0, 8);
    PRINT 'Grupos created for Aire Vapor Vacío Agua';
END

-- Create groups for Construcción de Bladders (4 personas por turno)
IF @Bladders IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0144', @Bladders, 4, 6),
    ('R0144_02', @Bladders, 4, 6),
    ('R0144_03', @Bladders, 4, 6),
    ('R0144_04', @Bladders, 4, 6);
    PRINT 'Grupos created for Construcción de Bladders';
END

-- Create groups for Sindicato/Relaciones Laborales (2 personas por turno)
IF @Sindicato IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('N0439', @Sindicato, 2, 8),
    ('R0154_02', @Sindicato, 0, 8);
    PRINT 'Grupos created for Relaciones Laborales MX';
END

-- Create groups for Instructores Técnicos (1 persona por turno)
IF @Instructores IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('N0439', @Instructores, 1, 8);
    PRINT 'Grupos created for Instructores Técnicos';
END

-- Additional support areas
DECLARE @MttoElectrico INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%Electrico%' AND NombreGeneral NOT LIKE '%II%' AND NombreGeneral NOT LIKE '%III%');
DECLARE @MttoElectricoIII INT = (SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%Electrico III%');

IF @MttoElectrico IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0144', @MttoElectrico, 5, 6),
    ('R0144_02', @MttoElectrico, 5, 6),
    ('R0144_03', @MttoElectrico, 5, 6),
    ('R0144_04', @MttoElectrico, 5, 6),
    ('N0439', @MttoElectrico, 0, 8);
    PRINT 'Grupos created for Mantenimiento Eléctrico';
END

IF @MttoElectricoIII IS NOT NULL
BEGIN
    INSERT INTO Grupos (Rol, AreaId, PersonasPorTurno, DuracionDeturno) VALUES
    ('R0144', @MttoElectricoIII, 5, 6),
    ('R0144_02', @MttoElectricoIII, 5, 6),
    ('R0144_03', @MttoElectricoIII, 5, 6),
    ('R0144_04', @MttoElectricoIII, 5, 6),
    ('N0439', @MttoElectricoIII, 0, 8);
    PRINT 'Grupos created for Mantenimiento Eléctrico III';
END

-- Verify results
SELECT 'Total Grupos Created' as Result, COUNT(*) as Total FROM Grupos;
SELECT 'Grupos by Area' as Info, a.NombreGeneral, COUNT(g.GrupoId) as TotalGrupos
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
GROUP BY a.NombreGeneral
ORDER BY a.NombreGeneral;

PRINT 'Script completed successfully!';
GO
