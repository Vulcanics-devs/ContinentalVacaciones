using System.ComponentModel.DataAnnotations;

namespace tiempo_libre.DTOs
{
    public class SolicitarCodigoRequest
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;
    }

    public class SolicitarCodigoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? MinutosExpiracion { get; set; }
    }

    public class CambiarPasswordRequest
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código de verificación es requerido")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "El código debe tener 5 dígitos")]
        public string CodigoVerificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        public string NuevaPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
        [Compare("NuevaPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }

    public class CambiarPasswordResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ValidarCodigoRequest
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código de verificación es requerido")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "El código debe tener 5 dígitos")]
        public string CodigoVerificacion { get; set; } = string.Empty;
    }

    public class ValidarCodigoResponse
    {
        public bool Valido { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? IntentosRestantes { get; set; }
    }
}