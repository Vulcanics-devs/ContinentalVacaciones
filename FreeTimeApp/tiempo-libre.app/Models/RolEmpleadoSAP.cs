using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models;

[Table("RolesEmpleadosSAP")]
public class RolEmpleadoSAP
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Nomina { get; set; }

    public string? Nombre { get; set; }
    public DateOnly? Alta { get; set; }
    public string? CentroCoste { get; set; }
    public string? Posición { get; set; }
    public string? UnidadOrganizativa { get; set; }
    public string? EncargadoRegistro { get; set; }
    public string? Regla { get; set; }
    public string? Turno { get; set; }
}