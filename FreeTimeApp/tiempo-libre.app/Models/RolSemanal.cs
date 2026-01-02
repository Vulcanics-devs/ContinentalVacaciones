
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace tiempo_libre.Models;

public partial class RolSemanal
{
    [Key]
    public int RolSemanalId { get; set; }

    [Required]
    [MaxLength(25)]
    public string Rol { get; set; } = null!;
    [Required]
    public int IndiceSemana { get; set; }
    public int IdRegla { get; set; }
    public virtual Regla Regla { get; set; } = null!;
}