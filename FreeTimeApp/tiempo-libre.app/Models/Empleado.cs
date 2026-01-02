using System;
using System.Collections.Generic;

namespace tiempo_libre.Models;

public partial class Empleado
{
    public int Nomina { get; set; }

    public string? Nombre { get; set; }

    public DateOnly? FechaAlta { get; set; }

    public int? CentroCoste { get; set; }

    public string? Posicion { get; set; }

    public string? UnidadOrganizativa { get; set; }

    public string? EncargadoRegistro { get; set; }

    public string? Rol { get; set; }

    public Empleado()
    {
    }
}
