using System;
using System.Collections.Generic;
namespace tiempo_libre.Models;

public partial class PermisosEincapacidade
{
    public int Id { get; set; }
    public int Nomina { get; set; }
    public string? Nombre { get; set; }
    public string? Posicion { get; set; }
    public DateOnly? Desde { get; set; }
    public DateOnly? Hasta { get; set; }
    public int? ClAbPre { get; set; }
    public string? ClaseAbsentismo { get; set; }
    public double? Dias { get; set; }
    public double? DiaNat { get; set; }
    public PermisosEincapacidade() { }
}
