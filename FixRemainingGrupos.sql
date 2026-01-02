-- Fix remaining grupos with 0 values

-- Vulcanización R0144 groups (6 personas per group from CSV)
UPDATE g SET PersonasPorTurno = 6, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral LIKE 'Vulcanizaci%n' AND a.NombreGeneral NOT LIKE '%MX' AND g.Rol LIKE 'R0144%';

-- Set default PersonasPorTurno = 1 for all N0439 administrative roles
UPDATE g SET PersonasPorTurno = 1
FROM Grupos g
WHERE g.Rol LIKE 'N0439%' AND g.PersonasPorTurno = 0;

-- Set default PersonasPorTurno = 1 for specialty roles
UPDATE g SET PersonasPorTurno = 1, DuracionDeturno = 8
FROM Grupos g
WHERE g.Rol IN ('R0229', 'R0135', 'R0133', 'R0133_02', 'R0135_02', 'R0154_02', 'R0267_02', 'N0A01', 'R0144_04', 'R0229_04')
AND g.PersonasPorTurno = 0;

-- Update Vulcanización MX
UPDATE g SET PersonasPorTurno = 1
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Vulcanización MX' AND g.Rol LIKE 'N0439%';

-- Verify - show all grupos that still have 0 values
SELECT
    a.NombreGeneral AS Area,
    g.Rol AS Grupo,
    g.PersonasPorTurno,
    g.DuracionDeturno
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE g.PersonasPorTurno = 0 OR g.DuracionDeturno = 0
ORDER BY a.NombreGeneral, g.Rol;
