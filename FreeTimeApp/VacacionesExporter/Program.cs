using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ClosedXML.Excel;

namespace VacacionesExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   Exportador de Vacaciones Programadas                    ║");
            Console.WriteLine("║   Continental Vacation Management System                  ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            try
            {
                // Cargar configuración
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");

                // Obtener año desde argumentos o usar año actual
                int? year = null;
                if (args.Length > 0 && int.TryParse(args[0], out var parsedYear))
                {
                    year = parsedYear;
                    Console.WriteLine($"→ Exportando vacaciones del año: {year}");
                }
                else
                {
                    Console.WriteLine("→ Exportando todas las vacaciones (sin filtro de año)");
                    Console.WriteLine("  Tip: Para filtrar por año, ejecute: dotnet run <año>");
                }
                Console.WriteLine();

                // Configurar DbContext
                var optionsBuilder = new DbContextOptionsBuilder<VacacionesDbContext>();
                optionsBuilder.UseSqlServer(connectionString);

                using var context = new VacacionesDbContext(optionsBuilder.Options);

                // Obtener datos
                Console.WriteLine("📊 Consultando base de datos...");
                var query = context.VacacionesProgramadas
                    .Include(v => v.Empleado)
                        .ThenInclude(e => e.Grupo)
                    .Include(v => v.Empleado)
                        .ThenInclude(e => e.Area)
                    .AsQueryable();

                if (year.HasValue)
                {
                    var startDate = new DateOnly(year.Value, 1, 1);
                    var endDate = new DateOnly(year.Value, 12, 31);
                    query = query.Where(v => v.FechaVacacion >= startDate && v.FechaVacacion <= endDate);
                }

                var vacaciones = query
                    .OrderBy(v => v.Empleado.Nomina)
                    .ThenBy(v => v.FechaVacacion)
                    .ToList();

                Console.WriteLine($"✓ Se encontraron {vacaciones.Count} registros de vacaciones");
                Console.WriteLine();

                if (vacaciones.Count == 0)
                {
                    Console.WriteLine("⚠ No hay datos para exportar.");
                    return;
                }

                // Agrupar vacaciones por área
                var vacacionesPorArea = vacaciones
                    .GroupBy(v => new {
                        AreaId = v.Empleado?.AreaId ?? 0,
                        AreaNombre = v.Empleado?.Area?.NombreGeneral ?? "Sin Área"
                    })
                    .OrderBy(g => g.Key.AreaNombre)
                    .ToList();

                Console.WriteLine($"📊 Agrupando por área: {vacacionesPorArea.Count} áreas encontradas");
                Console.WriteLine();

                // Crear archivo Excel
                Console.WriteLine("📝 Generando archivo Excel con tabs por área...");
                using var workbook = new XLWorkbook();

                // Configurar encabezados (reutilizable para cada hoja)
                var headers = new[]
                {
                    "ID", "Nómina", "Nombre Completo", "Área", "Grupo",
                    "Fecha Vacación", "Tipo Vacación", "Origen Asignación",
                    "Estado Vacación", "Periodo Programación", "Fecha Programación",
                    "Puede ser Intercambiada", "Observaciones", "Fecha Creación",
                    "Creado Por", "Última Actualización", "Actualizado Por"
                };

                // Rastrear nombres de hojas usados para evitar duplicados
                var usedSheetNames = new HashSet<string>();

                // Crear una hoja por área
                foreach (var areaGroup in vacacionesPorArea)
                {
                    var areaNombre = areaGroup.Key.AreaNombre;
                    var vacacionesArea = areaGroup.OrderBy(v => v.Empleado?.Nomina).ThenBy(v => v.FechaVacacion).ToList();

                    // Sanitizar nombre de hoja (máximo 31 caracteres, sin caracteres inválidos)
                    var sheetName = SanitizeSheetName(areaNombre);

                    // Manejar duplicados
                    var uniqueSheetName = sheetName;
                    var counter = 1;
                    while (usedSheetNames.Contains(uniqueSheetName))
                    {
                        var suffix = $" ({counter})";
                        var maxLength = 31 - suffix.Length;
                        uniqueSheetName = sheetName.Substring(0, Math.Min(sheetName.Length, maxLength)) + suffix;
                        counter++;
                    }
                    usedSheetNames.Add(uniqueSheetName);

                    var worksheet = workbook.Worksheets.Add(uniqueSheetName);

                    Console.WriteLine($"  → {areaNombre}: {vacacionesArea.Count} registros");

                    // Configurar encabezados
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = worksheet.Cell(1, i + 1);
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    // Llenar datos
                    int row = 2;
                    foreach (var vacacion in vacacionesArea)
                    {
                        worksheet.Cell(row, 1).Value = vacacion.Id;
                        worksheet.Cell(row, 2).Value = vacacion.Empleado?.Nomina?.ToString() ?? "";
                        worksheet.Cell(row, 3).Value = vacacion.Empleado?.FullName ?? "";
                        worksheet.Cell(row, 4).Value = vacacion.Empleado?.Area?.NombreGeneral ?? "";
                        worksheet.Cell(row, 5).Value = vacacion.Empleado?.Grupo?.Rol ?? "";
                        worksheet.Cell(row, 6).Value = vacacion.FechaVacacion.ToString("yyyy-MM-dd");
                        worksheet.Cell(row, 7).Value = vacacion.TipoVacacion;
                        worksheet.Cell(row, 8).Value = vacacion.OrigenAsignacion;
                        worksheet.Cell(row, 9).Value = vacacion.EstadoVacacion;
                        worksheet.Cell(row, 10).Value = vacacion.PeriodoProgramacion;
                        worksheet.Cell(row, 11).Value = vacacion.FechaProgramacion.ToString("yyyy-MM-dd HH:mm:ss");
                        worksheet.Cell(row, 12).Value = vacacion.PuedeSerIntercambiada ? "Sí" : "No";
                        worksheet.Cell(row, 13).Value = vacacion.Observaciones ?? "";
                        worksheet.Cell(row, 14).Value = vacacion.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                        worksheet.Cell(row, 15).Value = vacacion.CreatedBy?.ToString() ?? "";
                        worksheet.Cell(row, 16).Value = vacacion.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
                        worksheet.Cell(row, 17).Value = vacacion.UpdatedBy?.ToString() ?? "";
                        row++;
                    }

                    // Ajustar columnas
                    worksheet.Columns().AdjustToContents();

                    // Aplicar filtros
                    worksheet.RangeUsed().SetAutoFilter();

                    // Congelar primera fila
                    worksheet.SheetView.FreezeRows(1);
                }

                Console.WriteLine();

                // Guardar archivo
                var timestamp = DateTime.Now;
                var fileName = year.HasValue
                    ? $"VacacionesProgramadas_{year}_{timestamp:yyyyMMdd_HHmmss}.xlsx"
                    : $"VacacionesProgramadas_Todas_{timestamp:yyyyMMdd_HHmmss}.xlsx";

                workbook.SaveAs(fileName);

                Console.WriteLine($"✓ Archivo generado exitosamente: {fileName}");
                Console.WriteLine();

                // Mostrar resumen
                Console.WriteLine("📋 RESUMEN DE EXPORTACIÓN");
                Console.WriteLine("═══════════════════════════════════════");
                Console.WriteLine($"Total de registros:     {vacaciones.Count}");
                Console.WriteLine($"Empleados únicos:       {vacaciones.Select(v => v.EmpleadoId).Distinct().Count()}");

                var porTipo = vacaciones.GroupBy(v => v.TipoVacacion)
                    .Select(g => new { Tipo = g.Key, Count = g.Count() })
                    .ToList();
                Console.WriteLine();
                Console.WriteLine("Por Tipo de Vacación:");
                foreach (var tipo in porTipo)
                {
                    Console.WriteLine($"  • {tipo.Tipo,-30} : {tipo.Count,5}");
                }

                var porOrigen = vacaciones.GroupBy(v => v.OrigenAsignacion)
                    .Select(g => new { Origen = g.Key, Count = g.Count() })
                    .ToList();
                Console.WriteLine();
                Console.WriteLine("Por Origen de Asignación:");
                foreach (var origen in porOrigen)
                {
                    Console.WriteLine($"  • {origen.Origen,-30} : {origen.Count,5}");
                }

                var porEstado = vacaciones.GroupBy(v => v.EstadoVacacion)
                    .Select(g => new { Estado = g.Key, Count = g.Count() })
                    .ToList();
                Console.WriteLine();
                Console.WriteLine("Por Estado:");
                foreach (var estado in porEstado)
                {
                    Console.WriteLine($"  • {estado.Estado,-30} : {estado.Count,5}");
                }

                Console.WriteLine();
                Console.WriteLine("═══════════════════════════════════════");
                Console.WriteLine($"📁 Ruta completa: {Path.GetFullPath(fileName)}");
                Console.WriteLine();
                Console.WriteLine("✓ Exportación completada exitosamente!");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("❌ ERROR DURANTE LA EXPORTACIÓN");
                Console.WriteLine("═══════════════════════════════════════");
                Console.WriteLine($"Mensaje: {ex.Message}");
                Console.WriteLine($"Tipo: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Error interno: {ex.InnerException.Message}");
                }
                Console.WriteLine();
                Console.WriteLine("Stack trace:");
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Sanitiza el nombre de una hoja de Excel para cumplir con las restricciones:
        /// - Máximo 31 caracteres
        /// - No puede contener: : \ / ? * [ ]
        /// </summary>
        private static string SanitizeSheetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Sin Nombre";
            }

            // Remover caracteres inválidos para nombres de hojas de Excel
            var invalidChars = new[] { ':', '\\', '/', '?', '*', '[', ']' };
            var sanitized = name;
            foreach (var c in invalidChars)
            {
                sanitized = sanitized.Replace(c.ToString(), "");
            }

            // Truncar a 31 caracteres (límite de Excel)
            if (sanitized.Length > 31)
            {
                sanitized = sanitized.Substring(0, 31);
            }

            return string.IsNullOrWhiteSpace(sanitized) ? "Hoja" : sanitized;
        }
    }

    // Clases de modelo simplificadas
    public class VacacionesDbContext : DbContext
    {
        public VacacionesDbContext(DbContextOptions<VacacionesDbContext> options) : base(options) { }

        public DbSet<VacacionesProgramadas> VacacionesProgramadas { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Grupo> Grupos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VacacionesProgramadas>()
                .ToTable("VacacionesProgramadas");

            modelBuilder.Entity<User>()
                .ToTable("Users");

            modelBuilder.Entity<Area>(entity =>
            {
                entity.ToTable("Areas");
                entity.HasKey(e => e.AreaId);
            });

            modelBuilder.Entity<Grupo>(entity =>
            {
                entity.ToTable("Grupos");
                entity.HasKey(e => e.GrupoId);
            });
        }
    }

    public class VacacionesProgramadas
    {
        public int Id { get; set; }
        public int EmpleadoId { get; set; }
        public DateOnly FechaVacacion { get; set; }
        public string TipoVacacion { get; set; } = string.Empty;
        public string OrigenAsignacion { get; set; } = "Manual";
        public string EstadoVacacion { get; set; } = "Activa";
        public string PeriodoProgramacion { get; set; } = string.Empty;
        public DateTime FechaProgramacion { get; set; }
        public bool PuedeSerIntercambiada { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public string? Observaciones { get; set; }

        // Navigation properties
        public virtual User? Empleado { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public int? Nomina { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int? AreaId { get; set; }
        public int? GrupoId { get; set; }

        // Navigation properties
        public virtual Area? Area { get; set; }
        public virtual Grupo? Grupo { get; set; }
    }

    public class Area
    {
        public int AreaId { get; set; }
        public string NombreGeneral { get; set; } = string.Empty;
    }

    public class Grupo
    {
        public int GrupoId { get; set; }
        public string IdentificadorSAP { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}
