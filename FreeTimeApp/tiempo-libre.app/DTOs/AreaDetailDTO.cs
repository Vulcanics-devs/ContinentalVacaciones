using System.Collections.Generic;

namespace tiempo_libre.DTOs
{
    public class AreaDetailDTO
    {
        public int AreaId { get; set; }
        public string? NombreGeneral { get; set; }
        public string? UnidadOrganizativaSap { get; set; }
        public int? Manning { get; set; }
        public List<GrupoDetailDTO> Grupos { get; set; } = new();
    }

    public class GrupoDetailDTO
    {
        public int GrupoId { get; set; }
        public string? Rol { get; set; }
        public string? IdentificadorSAP { get; set; }
        public int PersonasPorTurno { get; set; }
        public int DuracionDeturno { get; set; }
        public int? LiderId { get; set; }
    }
}
