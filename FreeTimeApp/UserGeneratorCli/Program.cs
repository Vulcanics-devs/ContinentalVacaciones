using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using tiempo_libre.Models;

var connectionString = "Server=localhost;Database=Vacaciones;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True";

Console.WriteLine("=== Generating Users from Empleados ===\n");

var optionsBuilder = new DbContextOptionsBuilder<FreeTimeDbContext>();
optionsBuilder.UseSqlServer(connectionString);

using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<GenericUsersGenerator>();

using var db = new FreeTimeDbContext(optionsBuilder.Options);

var generator = new GenericUsersGenerator(db, logger);

var created = await generator.GenerateUsersFromEmpleadosAsync();

Console.WriteLine($"\n=== Users Generation Complete! ===");
Console.WriteLine($"Users created: {created}");
