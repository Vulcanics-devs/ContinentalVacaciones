using System;
using System.Collections.Generic;
using tiempo_libre.Models.Enums;

namespace tiempo_libre.Tests
{
    /// <summary>
    /// Clase de prueba simple para validar la lógica del calendario de grupo
    /// </summary>
    public static class TestCalendarioGrupo
    {
        // Reglas definidas basadas en el código TypeScript
        private static readonly Dictionary<string, string[]> REGLAS = new()
        {
            ["R0144"] = new[] { "1", "1", "1", "1", "1", "D", "D", "D", "3", "3", "3", "3", "3", "D", "2", "2", "D", "D", "2", "2", "3", "3", "D", "2", "2", "D", "1", "1" },
            ["N0439"] = new[] { "1", "1", "1", "1", "1", "D", "D" },
            ["R0135"] = new[] { "1", "1", "1", "1", "1", "D", "D", "D", "1", "1", "1", "1", "1", "D" },
            ["R0229"] = new[] { "D", "1", "1", "1", "1", "1", "D", "1", "1", "D", "D", "1", "1", "1", "1", "D", "1", "1", "D", "1", "1", "2", "2", "2", "2", "2", "D", "D" },
            ["R0154"] = new[] { "2", "2", "2", "2", "2", "D", "D", "D", "1", "1", "1", "1", "1", "D" },
            ["R0267"] = new[] { "2", "2", "D", "2", "2", "2", "D", "1", "1", "1", "1", "1", "D", "D", "D", "3", "3", "3", "D", "1", "1" },
            ["R0130"] = new[] { "1", "1", "1", "1", "1", "D", "D", "D", "3", "3", "D", "2", "2", "2", "2", "2", "D", "3", "3", "3", "3", "3", "D", "2", "2", "D", "1", "1" },
            ["N0440"] = new[] { "2", "2", "2", "2", "2", "D", "D" },
            ["N0A01"] = new[] { "1", "1", "1", "D", "1", "1", "D" },
            ["R0133"] = new[] { "1", "1", "1", "1", "1", "D", "D", "2", "2", "2", "2", "2", "D", "D" }
        };

        public static void TestCrearRol()
        {
            Console.WriteLine("=== Test CrearRol ===");
            
            // Test con R0144 grupo 2
            var rol = CrearRol("R0144", 2);
            Console.WriteLine($"R0144 Grupo 2 - Primeros 14 días: {string.Join(", ", rol[0..14])}");
            
            // Test con N0439 grupo 1
            var rolN0439 = CrearRol("N0439", 1);
            Console.WriteLine($"N0439 Grupo 1 - Todos los días: {string.Join(", ", rolN0439)}");
        }

        public static void TestGenerarCalendario()
        {
            Console.WriteLine("\n=== Test GenerarCalendario ===");
            
            var fechaInicio = new DateTime(2025, 9, 15);
            var fechaFin = new DateTime(2025, 9, 21); // Una semana
            
            var calendario = GenerarCalendario(fechaInicio, fechaFin, "R0144", 2);
            
            Console.WriteLine($"Calendario R0144 Grupo 2 del {fechaInicio:yyyy-MM-dd} al {fechaFin:yyyy-MM-dd}:");
            foreach (var dia in calendario)
            {
                Console.WriteLine($"{dia.Fecha:yyyy-MM-dd}: {dia.Turno} - {dia.Tipo}");
            }
        }

        public static void TestTipoCalendarioEnum()
        {
            Console.WriteLine("\n=== Test TipoCalendarioEnum ===");
            
            var tipos = new[] { "1", "2", "3", "D", "P", "PD", "V", "VA", "G", "E", "A", "M", "R", "S", "PP" };
            
            foreach (var tipo in tipos)
            {
                var enumValue = TipoCalendarioExtensions.FromShortString(tipo);
                var description = enumValue.ToDescription();
                var backToString = enumValue.ToShortString();
                
                Console.WriteLine($"{tipo} -> {enumValue} -> {description} -> {backToString}");
            }
        }

        private static string[] CrearRol(string reglaRef, int gpoRef)
        {
            if (!REGLAS.ContainsKey(reglaRef))
                return new string[0];

            var regla = REGLAS[reglaRef];
            var cantSemanas = regla.Length / 7;
            var rol = new string[cantSemanas * 7];
            var dia = (gpoRef - 1) * 7;

            for (int i = 0; i < cantSemanas * 7; i++, dia++)
            {
                rol[i] = regla[dia % (cantSemanas * 7)];
            }

            return rol;
        }

        private static List<(DateTime Fecha, string Turno, string Tipo)> GenerarCalendario(DateTime fechaInicio, DateTime fechaFin, string regla, int numeroGrupo)
        {
            var resultado = new List<(DateTime Fecha, string Turno, string Tipo)>();
            
            if (!REGLAS.ContainsKey(regla))
                return resultado;

            var patronRegla = REGLAS[regla];
            var cantSemanas = patronRegla.Length / 7;
            
            // Crear el rol específico para este grupo
            var rol = CrearRol(regla, numeroGrupo);
            
            // Fecha de referencia: 15 de septiembre de 2025
            var fechaRef = new DateTime(2025, 9, 15);
            
            var fecha = new DateTime(fechaRef.Ticks);
            var i = 0;
            
            // Generar desde la fecha de referencia hasta la fecha fin
            while (fecha <= fechaFin)
            {
                if (fecha >= fechaInicio)
                {
                    var turnoCode = rol[i % (cantSemanas * 7)];
                    var tipoCalendario = TipoCalendarioExtensions.FromShortString(turnoCode);
                    
                    resultado.Add((fecha, turnoCode, tipoCalendario.ToDescription()));
                }
                
                fecha = fecha.AddDays(1);
                i++;
            }

            return resultado;
        }

        public static void RunAllTests()
        {
            Console.WriteLine("Ejecutando pruebas del CalendarioGrupo...\n");
            
            TestTipoCalendarioEnum();
            TestCrearRol();
            TestGenerarCalendario();
            
            Console.WriteLine("\n¡Pruebas completadas!");
        }
    }
}
