using Microsoft.Data.SqlClient;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

var connString = "Server=localhost;Database=Vacaciones;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True";
var csvPath = @"C:\Users\Miltron\ProyectoConti\User list\USUARIOS APP DE VACACIONES .xlsx - Sheet1.csv";

Console.WriteLine("=== Importing Users from CSV ===\n");

// Read all lines manually
var lines = File.ReadAllLines(csvPath);
int imported = 0;
int skipped = 0;
int updated = 0;

using var conn = new SqlConnection(connString);
await conn.OpenAsync();

string? currentArea = null;

// First, ensure all roles exist
await EnsureRolesExist(conn);

for (int i = 4; i < lines.Length; i++) // Start from row 5 (index 4)
{
    var line = lines[i];
    var parts = line.Split(',');

    if (parts.Length < 6) continue;

    var col2 = parts[2]?.Trim(); // Area name
    var col3 = parts[3]?.Trim(); // Perfil/Role
    var col4 = parts[4]?.Trim(); // Name or Nomina
    var col5 = parts[5]?.Trim(); // Email or Name

    // Track current area
    if (!string.IsNullOrEmpty(col2))
    {
        currentArea = col2;
    }

    // Skip empty rows
    if (string.IsNullOrEmpty(col3) && string.IsNullOrEmpty(col4))
        continue;

    try
    {
        // Determine user type and create/update
        if (col3 == "Superusuario" || col3 == "IE")
        {
            await ProcessEngineeringUser(conn, col3, col4, col5);
            imported++;
        }
        else if (col3 == "Jefe de área" || col3 == "Lider de turno")
        {
            await ProcessAreaLeader(conn, currentArea, col3, col4, col5);
            imported++;
        }
        else if (col3 == "Comité Sindical")
        {
            await ProcessComiteSindical(conn, col4, col5);
            updated++;
        }

        if ((imported + updated) % 10 == 0)
            Console.WriteLine($"Processed {imported + updated} users...");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing line {i + 1}: {ex.Message}");
        skipped++;
    }
}

Console.WriteLine($"\n=== Import Complete! ===");
Console.WriteLine($"Users created: {imported}");
Console.WriteLine($"Users updated: {updated}");
Console.WriteLine($"Skipped: {skipped}");

static async Task EnsureRolesExist(SqlConnection conn)
{
    var roles = new[]
    {
        ("SuperUsuario", "Superusuario del sistema", "SU"),
        ("Ingeniero", "Ingeniero", "IE"),
        ("JefeArea", "Jefe de Área", "JA"),
        ("LiderGrupo", "Líder de Grupo/Turno", "LG"),
        ("ComiteSindical", "Comité Sindical", "CS")
    };

    foreach (var (name, desc, abbr) in roles)
    {
        var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Roles WHERE Name = @name", conn);
        checkCmd.Parameters.AddWithValue("@name", name);
        var count = (int)await checkCmd.ExecuteScalarAsync();

        if (count == 0)
        {
            var insertCmd = new SqlCommand(
                "INSERT INTO Roles (Name, Description, Abreviation) VALUES (@name, @desc, @abbr)",
                conn);
            insertCmd.Parameters.AddWithValue("@name", name);
            insertCmd.Parameters.AddWithValue("@desc", desc);
            insertCmd.Parameters.AddWithValue("@abbr", abbr);
            await insertCmd.ExecuteNonQueryAsync();
            Console.WriteLine($"Created role: {name}");
        }
    }
}

static async Task ProcessEngineeringUser(SqlConnection conn, string perfil, string? nombre, string? email)
{
    if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(email))
        return;

    var username = email.Split('@')[0];

    // Check if user exists
    var checkCmd = new SqlCommand("SELECT Id FROM Users WHERE Username = @username", conn);
    checkCmd.Parameters.AddWithValue("@username", username);
    var userId = await checkCmd.ExecuteScalarAsync();

    if (userId == null)
    {
        var roleName = perfil == "Superusuario" ? "SuperUsuario" : "Ingeniero";

        // Get role ID
        var getRoleCmd = new SqlCommand("SELECT Id FROM Roles WHERE Name = @name", conn);
        getRoleCmd.Parameters.AddWithValue("@name", roleName);
        var roleId = await getRoleCmd.ExecuteScalarAsync();

        if (roleId == null)
        {
            Console.WriteLine($"Warning: Role {roleName} not found");
            return;
        }

        // Hash password using BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Continental2024!");
        var passwordSalt = BCrypt.Net.BCrypt.GenerateSalt();

        // Insert user
        var insertCmd = new SqlCommand(@"
            INSERT INTO Users (Username, FullName, PasswordHash, PasswordSalt, Status, CreatedAt, CreatedBy)
            OUTPUT INSERTED.Id
            VALUES (@username, @fullname, @passwordHash, @passwordSalt, 0, @now, 1)",
            conn);
        insertCmd.Parameters.AddWithValue("@username", username);
        insertCmd.Parameters.AddWithValue("@fullname", nombre);
        insertCmd.Parameters.AddWithValue("@passwordHash", passwordHash);
        insertCmd.Parameters.AddWithValue("@passwordSalt", passwordSalt);
        insertCmd.Parameters.AddWithValue("@now", DateTime.UtcNow);

        var newUserId = await insertCmd.ExecuteScalarAsync();

        // Add role to user
        var addRoleCmd = new SqlCommand(
            "INSERT INTO UserRoles (RolesId, UserId) VALUES (@roleId, @userId)",
            conn);
        addRoleCmd.Parameters.AddWithValue("@roleId", roleId);
        addRoleCmd.Parameters.AddWithValue("@userId", newUserId);
        await addRoleCmd.ExecuteNonQueryAsync();

        Console.WriteLine($"Created {perfil}: {nombre}");
    }
    else
    {
        Console.WriteLine($"Already exists: {nombre}");
    }
}

static async Task ProcessAreaLeader(SqlConnection conn, string? areaName, string perfil, string? nombre, string? email)
{
    if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(areaName))
        return;

    var username = email.Split('@')[0];

    // Find area by name (fuzzy match)
    var findAreaCmd = new SqlCommand(
        "SELECT TOP 1 AreaId FROM Areas WHERE NombreGeneral LIKE '%' + @areaName + '%'",
        conn);
    findAreaCmd.Parameters.AddWithValue("@areaName", areaName);
    var areaId = await findAreaCmd.ExecuteScalarAsync();

    if (areaId == null)
    {
        Console.WriteLine($"Warning: Area not found for {areaName}");
        return;
    }

    // Check if user exists
    var checkCmd = new SqlCommand("SELECT Id, AreaId FROM Users WHERE Username = @username", conn);
    checkCmd.Parameters.AddWithValue("@username", username);

    using var reader = await checkCmd.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
        var userId = reader.GetInt32(0);
        var currentAreaId = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1);
        await reader.CloseAsync();

        // Update area if different
        if (currentAreaId != (int)areaId)
        {
            var updateCmd = new SqlCommand(
                "UPDATE Users SET AreaId = @areaId WHERE Id = @userId",
                conn);
            updateCmd.Parameters.AddWithValue("@areaId", areaId);
            updateCmd.Parameters.AddWithValue("@userId", userId);
            await updateCmd.ExecuteNonQueryAsync();
            Console.WriteLine($"Updated area for: {nombre}");
        }
        return;
    }
    await reader.CloseAsync();

    // Create new user
    var roleName = perfil == "Jefe de área" ? "JefeArea" : "LiderGrupo";

    // Get role ID
    var getRoleCmd = new SqlCommand("SELECT Id FROM Roles WHERE Name = @name", conn);
    getRoleCmd.Parameters.AddWithValue("@name", roleName);
    var roleId = await getRoleCmd.ExecuteScalarAsync();

    if (roleId == null)
    {
        Console.WriteLine($"Warning: Role {roleName} not found");
        return;
    }

    // Hash password
    var passwordHash = BCrypt.Net.BCrypt.HashPassword("Continental2024!");
    var passwordSalt = BCrypt.Net.BCrypt.GenerateSalt();

    // Insert user
    var insertCmd = new SqlCommand(@"
        INSERT INTO Users (Username, FullName, PasswordHash, PasswordSalt, Status, CreatedAt, CreatedBy, AreaId)
        OUTPUT INSERTED.Id
        VALUES (@username, @fullname, @passwordHash, @passwordSalt, 0, @now, 1, @areaId)",
        conn);
    insertCmd.Parameters.AddWithValue("@username", username);
    insertCmd.Parameters.AddWithValue("@fullname", nombre);
    insertCmd.Parameters.AddWithValue("@passwordHash", passwordHash);
    insertCmd.Parameters.AddWithValue("@passwordSalt", passwordSalt);
    insertCmd.Parameters.AddWithValue("@now", DateTime.UtcNow);
    insertCmd.Parameters.AddWithValue("@areaId", areaId);

    var newUserId = await insertCmd.ExecuteScalarAsync();

    // Add role to user
    var addRoleCmd = new SqlCommand(
        "INSERT INTO UserRoles (RolesId, UserId) VALUES (@roleId, @userId)",
        conn);
    addRoleCmd.Parameters.AddWithValue("@roleId", roleId);
    addRoleCmd.Parameters.AddWithValue("@userId", newUserId);
    await addRoleCmd.ExecuteNonQueryAsync();

    Console.WriteLine($"Created {perfil} in {areaName}: {nombre}");
}

static async Task ProcessComiteSindical(SqlConnection conn, string? nomina, string? nombre)
{
    if (string.IsNullOrEmpty(nomina) || string.IsNullOrEmpty(nombre))
        return;

    if (!int.TryParse(nomina, out var nominaNum))
        return;

    // Find user by nomina
    var findCmd = new SqlCommand("SELECT Id FROM Users WHERE Nomina = @nomina", conn);
    findCmd.Parameters.AddWithValue("@nomina", nominaNum);
    var userId = await findCmd.ExecuteScalarAsync();

    if (userId == null)
    {
        Console.WriteLine($"Warning: Comité Sindical user not found: {nomina} - {nombre}");
        return;
    }

    // Get ComiteSindical role
    var getRoleCmd = new SqlCommand("SELECT Id FROM Roles WHERE Name = @name", conn);
    getRoleCmd.Parameters.AddWithValue("@name", "ComiteSindical");
    var roleId = await getRoleCmd.ExecuteScalarAsync();

    if (roleId == null)
    {
        Console.WriteLine($"Warning: ComiteSindical role not found");
        return;
    }

    // Check if user already has this role
    var checkRoleCmd = new SqlCommand(
        "SELECT COUNT(*) FROM UserRoles WHERE RolesId = @roleId AND UserId = @userId",
        conn);
    checkRoleCmd.Parameters.AddWithValue("@roleId", roleId);
    checkRoleCmd.Parameters.AddWithValue("@userId", userId);
    var count = (int)await checkRoleCmd.ExecuteScalarAsync();

    if (count == 0)
    {
        // Add role to user
        var addRoleCmd = new SqlCommand(
            "INSERT INTO UserRoles (RolesId, UserId) VALUES (@roleId, @userId)",
            conn);
        addRoleCmd.Parameters.AddWithValue("@roleId", roleId);
        addRoleCmd.Parameters.AddWithValue("@userId", userId);
        await addRoleCmd.ExecuteNonQueryAsync();

        Console.WriteLine($"Added ComiteSindical role: {nombre} ({nomina})");
    }
    else
    {
        Console.WriteLine($"ComiteSindical role already exists: {nombre} ({nomina})");
    }
}
