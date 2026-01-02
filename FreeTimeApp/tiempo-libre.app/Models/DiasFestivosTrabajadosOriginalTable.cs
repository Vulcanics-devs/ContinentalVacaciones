using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models
{
	public class DiasFestivosTrabajadosOriginalTable
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public int Nomina { get; set; }

		[Required]
		[MaxLength(150)]
		public string Nombre { get; set; } = string.Empty;

		[Required]
		public DateOnly FestivoTrabajado { get; set; }
	}
}
