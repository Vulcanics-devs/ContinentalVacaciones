using System;
using System.Collections.Generic;

namespace tiempo_libre.Models;

public partial class RolesEmpleado
{
    public int Id { get; set; }
    public int? Nomina { get; set; }
    public string? Nombre { get; set; }
    public DateOnly? Fecha { get; set; }
    public string? Dia { get; set; }
    public string? Phtd { get; set; }
    public string? TextoPlanHrTrDia { get; set; }
    public int? CentroDeCoste { get; set; }
    public string? EncTiempos { get; set; }
    public RolesEmpleado() { }
}
