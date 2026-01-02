using System.ComponentModel.DataAnnotations;

namespace tiempo_libre.Models;

public class Rol
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Display(Name = "Nombre del Rol")]
    public required string Name { get; set; }

    [MaxLength(500)]
    [Display(Name = "Descripción del Rol")]
    [DataType(DataType.MultilineText)]
    [DisplayFormat(ConvertEmptyStringToNull = true)]
    public required string Description { get; set; }

    [MaxLength(10)]
    [Display(Name = "Abreviación")]
    public required string Abreviation { get; set; }

    public Rol() { }
}
