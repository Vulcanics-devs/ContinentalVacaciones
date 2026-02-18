using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tiempo_libre.Models
{
    public class FestivosEmpleadosTrabajadosUpload
    {
        public string? Nomina { get; set; }
        public string? Nombre { get; set; }
        public string? FechaTrabajada { get; set; }
    }
}