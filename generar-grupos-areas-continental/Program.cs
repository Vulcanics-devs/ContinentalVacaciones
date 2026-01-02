using Microsoft.Data.SqlClient;

var connectionString = "Server=localhost;Database=Vacaciones;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True";

// Mapeo UnidadOrganizativa → Nombre General
var mapEquivalencias = new Dictionary<string, string?>
{
    { "80805 A 127 CONSTRUCCION LLANTAS RADIAL", "Construcción" },
    { "80805 A 101 Corte de Hule y Compuestos", "Banbury" },
    { "80805 A 111 PREPARACION DE MATERIALES", "Prep. De Materiales" },
    { "80805 A 132 Vulcanizado", "Vulcanización" },
    { "80805 A 141 ACABADO REPARACION INSPECCIO", "Acabado" },
    { "80805 A 105 TUBULADO", "Tubuladoras" },
    { "80805 A612 Taller de Moldes", "Lab. De Moldes" },
    { "80805 A 609 Mantenimiento Electrico II", "Mtto. B" },
    { "80805 A 607 MANTENIMIENTO AREA III", "Mtto. C Vulca" },
    { "80805 A 605 MANTENIMIENTO AREA I", "Mtto. A" },
    { "80805 A 611 Lava Moldes", "Moldes Vulca" },
    { "80805 A 609 Mantenimiento Electrico", "Mtto. A" },
    { "80805 A 606 Mantenimiento Area II", "Mtto. B" },
    { "80805 A 609 Mantenimiento Electrico III", "Mtto. C Vulca" },
    { "80805 A 601 Almacen Materias Primas", "AMP" },
    { "80805 A 800 Relaciones Laborales MX", "Sindicato" },
    { "80805 A_104 CALANDRIADO", "Calandria" },
    { "80805 A 800 Instructores Tecnicos", "Instructor Tecnico" },
    { "80805 A 790 Construccion de Bladders", "Bladders" },
    { "80805 A 612 Aire Vapor Vacio Agua", "Casa de Fuerza" },
    { "80805 A 610 Metrologia", "Metrologia" },
    { "80805 A 703 Almacen de Producto Terminad", "APT" },
    { "80805 A 102 Cementos", "Cementos" },
    { "80805 PLT Vulcanizacion MX", "Vulcanización MX" }
};

Console.WriteLine("=== Populating Areas and Grupos ===\n");

using var connection = new SqlConnection(connectionString);
await connection.OpenAsync();

using var transaction = connection.BeginTransaction();

try
{
    // 1. Read Empleados
    var empleados = new List<(int Nomina, string? Nombre, DateTime? FechaAlta, int? CentroCoste, string? Posicion, string? UnidadOrganizativa, string? Rol, string? NombreGeneral)>();

    using (var cmd = new SqlCommand("SELECT Nomina, Nombre, FechaAlta, CentroCoste, Posicion, UnidadOrganizativa, Rol FROM dbo.Empleados", connection, transaction))
    using (var reader = await cmd.ExecuteReaderAsync())
    {
        while (await reader.ReadAsync())
        {
            var uo = reader.IsDBNull(5) ? null : reader.GetString(5);
            var nombreGeneral = uo != null && mapEquivalencias.ContainsKey(uo) ? mapEquivalencias[uo] : null;

            empleados.Add((
                reader.GetInt32(0),
                reader.IsDBNull(1) ? null : reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                reader.IsDBNull(3) ? null : reader.GetInt32(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                uo,
                reader.IsDBNull(6) ? null : reader.GetString(6),
                nombreGeneral
            ));
        }
    }

    Console.WriteLine($"Found {empleados.Count} empleados");

    if (empleados.Count == 0)
    {
        Console.WriteLine("No empleados to process.");
        await transaction.CommitAsync();
        return;
    }

    // 2. Insert unique Areas
    var areaIdMap = new Dictionary<string, int>();
    var areasVistas = new HashSet<string>();

    foreach (var emp in empleados)
    {
        if (emp.UnidadOrganizativa == null || areasVistas.Contains(emp.UnidadOrganizativa))
            continue;

        areasVistas.Add(emp.UnidadOrganizativa);

        using var cmd = new SqlCommand(
            "INSERT INTO dbo.Areas (UnidadOrganizativaSap, NombreGeneral, Manning) VALUES (@uo, @ng, @manning); SELECT SCOPE_IDENTITY();",
            connection, transaction);
        cmd.Parameters.AddWithValue("@uo", emp.UnidadOrganizativa);
        cmd.Parameters.AddWithValue("@ng", (object?)emp.NombreGeneral ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@manning", 0.0m);

        var areaId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        areaIdMap[emp.UnidadOrganizativa] = areaId;
        Console.WriteLine($"Created Area: {emp.NombreGeneral ?? emp.UnidadOrganizativa} (ID: {areaId})");
    }

    // 3. Insert unique Grupos by (UnidadOrganizativa, Rol)
    var grupoIdMap = new Dictionary<string, int>();
    var gruposVistos = new HashSet<string>();

    foreach (var emp in empleados)
    {
        if (emp.UnidadOrganizativa == null)
            continue;

        var gkey = $"{emp.UnidadOrganizativa}|{emp.Rol}";
        if (gruposVistos.Contains(gkey))
            continue;

        gruposVistos.Add(gkey);

        var areaId = areaIdMap[emp.UnidadOrganizativa];

        using var cmd = new SqlCommand(
            "INSERT INTO dbo.Grupos (AreaId, Rol) VALUES (@areaId, @rol); SELECT SCOPE_IDENTITY();",
            connection, transaction);
        cmd.Parameters.AddWithValue("@areaId", areaId);
        cmd.Parameters.AddWithValue("@rol", (object?)emp.Rol ?? DBNull.Value);

        var grupoId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        grupoIdMap[gkey] = grupoId;
        Console.WriteLine($"Created Grupo: {emp.Rol} in {emp.NombreGeneral} (ID: {grupoId})");
    }

    await transaction.CommitAsync();
    Console.WriteLine("\n=== Carga completada! ===");
    Console.WriteLine($"Areas created: {areaIdMap.Count}");
    Console.WriteLine($"Grupos created: {grupoIdMap.Count}");
}
catch (Exception ex)
{
    await transaction.RollbackAsync();
    Console.WriteLine($"Error: {ex.Message}");
    throw;
}
