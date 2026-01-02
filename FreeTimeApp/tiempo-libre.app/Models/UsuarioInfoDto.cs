namespace tiempo_libre.Models
{
    public class UsuarioInfoDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string UnidadOrganizativaSap { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public DateOnly? FechaIngreso { get; set; }
        public int? Nomina { get; set; }
        public AreaInfoDto? Area { get; set; }
        public GrupoInfoDto? Grupo { get; set; }
    }

    public class AreaInfoDto
    {
        public int AreaId { get; set; }
        public string NombreGeneral { get; set; } = string.Empty;
        public string UnidadOrganizativaSap { get; set; } = string.Empty;
    }

    public class GrupoInfoDto
    {
        public int GrupoId { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string IdentificadorSAP { get; set; } = string.Empty;
        public int PersonasPorTurno { get; set; }
        public int DuracionDeturno { get; set; }
    }
}
