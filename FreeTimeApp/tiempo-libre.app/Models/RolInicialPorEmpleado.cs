using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace tiempo_libre.Models;

public class RolInicialPorEmpleado
{
    public int Id { get; set; }
    [Required]
    public int Nomina { get; set; }
    [Required]
    public DateOnly Fecha { get; set; }
    [Required]
    public string RolSemanal { get; set; } = null!;
}