-- Update Users with their AreaId and GrupoId based on Empleados and Grupos

UPDATE u
SET
    u.AreaId = a.AreaId,
    u.GrupoId = g.GrupoId
FROM Users u
INNER JOIN Empleados e ON u.Nomina = e.Nomina
INNER JOIN Areas a ON e.UnidadOrganizativa = a.UnidadOrganizativaSap
INNER JOIN Grupos g ON g.AreaId = a.AreaId AND g.Rol = e.Rol
WHERE u.Nomina IS NOT NULL;

-- Verify the updates
SELECT
    COUNT(*) as TotalUsersUpdated
FROM Users
WHERE AreaId IS NOT NULL AND GrupoId IS NOT NULL;

-- Show some examples
SELECT TOP 10
    u.Id,
    u.FullName,
    u.Nomina,
    a.NombreGeneral as AreaNombre,
    g.GrupoId,
    g.Rol as GrupoRol
FROM Users u
INNER JOIN Areas a ON u.AreaId = a.AreaId
INNER JOIN Grupos g ON u.GrupoId = g.GrupoId
WHERE u.Nomina IS NOT NULL;
