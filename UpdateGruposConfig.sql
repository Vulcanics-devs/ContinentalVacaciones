-- Update Grupos configuration from CSV data
-- Mapping: CSV Area Name -> Database Area Name -> PersonasPorTurno

-- Materia Prima = AMP
UPDATE g SET PersonasPorTurno = 1, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'AMP' AND g.Rol LIKE 'R0144%';

-- Mezclado = Banbury
UPDATE g SET PersonasPorTurno = 9, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Banbury' AND g.Rol LIKE 'R0144%';

-- Tubuladoras
UPDATE g SET PersonasPorTurno = 4, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Tubuladoras' AND g.Rol LIKE 'R0144%';

-- Cementos
UPDATE g SET PersonasPorTurno = 1, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Cementos' AND g.Rol LIKE 'R0144%';

-- Calandrias = Calandria
UPDATE g SET PersonasPorTurno = 2, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Calandria' AND g.Rol LIKE 'R0267%';

-- Mtto A
UPDATE g SET PersonasPorTurno = 3, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Mtto. A' AND g.Rol LIKE 'R0144%';

-- Prep 1 + Prep 2 = Prep. De Materiales (combined: 5 personas x grupo each)
UPDATE g SET PersonasPorTurno = 5, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Prep. De Materiales' AND g.Rol LIKE 'R0144%';

-- Construcción 1 (3 personas) + Construcción 2 (12 personas) = Construcción
-- Using average or larger value
UPDATE g SET PersonasPorTurno = 12, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Construcción' AND g.Rol LIKE 'R0144%';

-- Mtto B
UPDATE g SET PersonasPorTurno = 4, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Mtto. B' AND g.Rol LIKE 'R0144%';

-- Vulcanización
UPDATE g SET PersonasPorTurno = 6, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Vulcanización' AND g.Rol LIKE 'R0144%';

UPDATE g SET PersonasPorTurno = 6, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Vulcanización MX' AND g.Rol LIKE 'N0439%';

-- AF = Acabado
UPDATE g SET PersonasPorTurno = 7, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Acabado' AND g.Rol LIKE 'R0144%';

-- Lab Moldes
UPDATE g SET PersonasPorTurno = 2, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Lab. De Moldes' AND g.Rol LIKE 'R0229%';

-- Moldes = Moldes Vulca
UPDATE g SET PersonasPorTurno = 3, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Moldes Vulca' AND g.Rol LIKE 'R0144%';

UPDATE g SET PersonasPorTurno = 3, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Moldes Vulca' AND g.Rol LIKE 'R0135%';

-- Mantto Vulca = Mtto. C Vulca
UPDATE g SET PersonasPorTurno = 4, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Mtto. C Vulca' AND g.Rol LIKE 'R0144%';

-- Mantto AF (not clear which area this is, skipping for now)

-- APT
UPDATE g SET PersonasPorTurno = 1, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'APT' AND g.Rol LIKE 'R0144%';

-- Metrología
UPDATE g SET PersonasPorTurno = 1, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Metrologia' AND (g.Rol LIKE 'R0228%' OR g.Rol LIKE 'R0133%' OR g.Rol LIKE 'N0439%');

-- Casa de Fuerza
UPDATE g SET PersonasPorTurno = 1, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Casa de Fuerza' AND (g.Rol LIKE 'R0130%' OR g.Rol LIKE 'R0135%' OR g.Rol LIKE 'N0439%');

-- Bladders
UPDATE g SET PersonasPorTurno = 1, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Bladders' AND g.Rol LIKE 'R0144%';

-- Sindicato
UPDATE g SET PersonasPorTurno = 1, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Sindicato' AND (g.Rol LIKE 'N0439%' OR g.Rol LIKE 'R0154%');

-- Instructores Tecnicos = Instructor Tecnico
UPDATE g SET PersonasPorTurno = 1, DuracionDeturno = 6
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
WHERE a.NombreGeneral = 'Instructor Tecnico' AND g.Rol LIKE 'N0439%';

-- Update all N0439 roles (administrative, single shift) to 8 hours
UPDATE g SET DuracionDeturno = 8
FROM Grupos g
WHERE g.Rol LIKE 'N0439%';

-- Update all R0135 roles (2-shift system) to 12 hours where not already set
UPDATE g SET DuracionDeturno = 12
FROM Grupos g
WHERE g.Rol LIKE 'R0135%' AND PersonasPorTurno > 0;

-- Update all R0228, R0229, R0130, R0133, R0154 roles to 8 hours (administrative/specialty)
UPDATE g SET DuracionDeturno = 8
FROM Grupos g
WHERE (g.Rol LIKE 'R0228%' OR g.Rol LIKE 'R0229%' OR g.Rol LIKE 'R0130%' OR g.Rol LIKE 'R0133%' OR g.Rol LIKE 'R0154%' OR g.Rol LIKE 'R0267%' OR g.Rol LIKE 'N0A01%') AND PersonasPorTurno > 0;

-- Verify results
SELECT
    a.NombreGeneral AS Area,
    g.Rol AS Grupo,
    g.PersonasPorTurno,
    g.DuracionDeturno
FROM Grupos g
INNER JOIN Areas a ON g.AreaId = a.AreaId
ORDER BY a.NombreGeneral, g.Rol;
