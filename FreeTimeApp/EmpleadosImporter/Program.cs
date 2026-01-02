using Microsoft.Data.SqlClient;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

var connString = "Server=localhost;Database=Vacaciones;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True";
var csvPath = @"C:\Users\Miltron\ProyectoConti\New Data\Listado-Sindicalizados-Final.csv";

Console.WriteLine("=== Importing Empleados from CSV ===\n");

var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    HasHeaderRecord = true,
};

using var reader = new StreamReader(csvPath);
using var csv = new CsvReader(reader, config);

var records = csv.GetRecords<dynamic>().ToList();
Console.WriteLine($"Found {records.Count} records in CSV\n");

using var conn = new SqlConnection(connString);
await conn.OpenAsync();

int imported = 0;
int skipped = 0;

foreach (var record in records)
{
    try
    {
        var dict = (IDictionary<string, object>)record;

        var nomina = int.Parse(dict["Nomina"].ToString()!);
        var nombre = dict["Nombre"].ToString();
        var alta = dict["Alta"].ToString();
        var centroCoste = dict.ContainsKey("CentroCoste") && !string.IsNullOrWhiteSpace(dict["CentroCoste"].ToString())
            ? int.Parse(dict["CentroCoste"].ToString()!) : (int?)null;
        var posicion = dict.ContainsKey("Posicion") ? dict["Posicion"].ToString() : null;
        var unidadOrg = dict.ContainsKey("UnidadOrganizativa") ? dict["UnidadOrganizativa"].ToString() : null;
        var encargado = dict.ContainsKey("EncargadoRegistro") ? dict["EncargadoRegistro"].ToString() : null;
        var rol = dict.ContainsKey("Regla ") ? dict["Regla "].ToString() : null;

        // Parse fecha
        DateTime? fechaAlta = null;
        if (!string.IsNullOrEmpty(alta))
        {
            fechaAlta = DateTime.ParseExact(alta!, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        var cmd = new SqlCommand(@"
            INSERT INTO Empleados (Nomina, Nombre, FechaAlta, CentroCoste, Posicion, UnidadOrganizativa, EncargadoRegistro, Rol)
            VALUES (@nomina, @nombre, @fechaAlta, @centroCoste, @posicion, @unidadOrg, @encargado, @rol)", conn);

        cmd.Parameters.AddWithValue("@nomina", nomina);
        cmd.Parameters.AddWithValue("@nombre", (object?)nombre ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@fechaAlta", (object?)fechaAlta ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@centroCoste", (object?)centroCoste ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@posicion", (object?)posicion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@unidadOrg", (object?)unidadOrg ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@encargado", (object?)encargado ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@rol", (object?)rol ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
        imported++;

        if (imported % 50 == 0)
            Console.WriteLine($"Imported {imported} employees...");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error importing record: {ex.Message}");
        skipped++;
    }
}

Console.WriteLine($"\n=== Import Complete! ===");
Console.WriteLine($"Successfully imported: {imported}");
Console.WriteLine($"Skipped: {skipped}");
