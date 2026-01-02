-- Link Sindicalizados to Grupos based on matching:
-- 1. Empleado.UnidadOrganizativa = Area.UnidadOrganizativaSap
-- 2. Empleado.Rol = Grupo.Rol
-- 3. Grupo.AreaId = Area.AreaId

INSERT INTO SindicalizadosPorGrupo (SindicalizadoId, GrupoId)
SELECT DISTINCT
    s.SindicalizadoId,
    g.GrupoId
FROM Sindicalizados s
INNER JOIN Empleados e ON s.Nomina = e.Nomina
INNER JOIN Areas a ON e.UnidadOrganizativa = a.UnidadOrganizativaSap
INNER JOIN Grupos g ON g.AreaId = a.AreaId AND g.Rol = e.Rol
WHERE NOT EXISTS (
    SELECT 1
    FROM SindicalizadosPorGrupo spg
    WHERE spg.SindicalizadoId = s.SindicalizadoId
    AND spg.GrupoId = g.GrupoId
);

-- Verify the results
SELECT COUNT(*) as TotalLinksCreated FROM SindicalizadosPorGrupo;

-- Show some examples
SELECT TOP 10
    s.Nomina,
    s.Nombre,
    e.UnidadOrganizativa,
    e.Rol,
    a.NombreGeneral as AreaNombre,
    g.GrupoId
FROM Sindicalizados s
INNER JOIN SindicalizadosPorGrupo spg ON s.SindicalizadoId = spg.SindicalizadoId
INNER JOIN Grupos g ON spg.GrupoId = g.GrupoId
INNER JOIN Areas a ON g.AreaId = a.AreaId
INNER JOIN Empleados e ON s.Nomina = e.Nomina;
