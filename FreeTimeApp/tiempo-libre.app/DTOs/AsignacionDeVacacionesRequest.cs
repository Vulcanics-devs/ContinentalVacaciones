using System;
using System.ComponentModel.DataAnnotations;

namespace tiempo_libre.DTOs
{
    public class AsignacionDeVacacionesRequest
    {
        [Required]
        public DateTime FechaInicio { get; set; }
        [Required]
        public DateTime FechaFinal { get; set; }
        [Required]
        public DateTime FechaInicioReservaciones { get; set; }
    }
}
