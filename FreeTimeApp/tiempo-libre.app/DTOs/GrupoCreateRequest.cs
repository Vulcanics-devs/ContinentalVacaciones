namespace tiempo_libre.DTOs
{
    public class GrupoCreateRequest
    {
    public string? Rol { get; set; }
    public string? IdentificadorSAP { get; set; }
        public int PersonasPorTurno { get; set; } = 3;
        public int DuracionDeturno { get; set; } = 24;
        public int? LiderId { get; set; }
    }
}
