

namespace tiempo_libre.DTOs
{
    
    public class EliminarVacacionesPorFechaRequest

    {

        public int EmpleadoId { get; set; }
        public List<DateOnly> Fechas { get; set; } = new();
    }
}
