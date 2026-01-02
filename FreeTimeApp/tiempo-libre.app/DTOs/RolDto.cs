namespace tiempo_libre.DTOs
{
    // DTO para listado de roles
    public class RolListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Abreviation { get; set; } = string.Empty;
    }

    // DTO para editar rol
    public class EditRolRequestDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Abreviation { get; set; }
    }
}
