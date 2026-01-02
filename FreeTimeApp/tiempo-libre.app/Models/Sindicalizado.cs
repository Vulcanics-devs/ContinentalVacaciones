using System;
using System.Collections.Generic;

namespace tiempo_libre.Models;

public partial class Sindicalizado
{
    public int SindicalizadoId { get; set; }
    public int Nomina { get; set; }
    public string Nombre { get; set; } = null!;
    public DateOnly? FechaAlta { get; set; }
    public int? CentroCoste { get; set; }
    public string? Posicion { get; set; }
    public string? EncargadoRegistro { get; set; }
    public virtual ICollection<Grupo> Grupos { get; set; } = new List<Grupo>();
    public Sindicalizado() { }
}
