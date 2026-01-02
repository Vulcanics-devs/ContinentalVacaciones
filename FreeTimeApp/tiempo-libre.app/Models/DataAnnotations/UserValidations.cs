using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace tiempo_libre.Models.DataAnnotations;

public class UsernameAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is null) return false;
        var username = value.ToString();

        // Validar si es entero
        if (int.TryParse(username, out _))
            return true;

        // Validar si es correo electrónico
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        return emailRegex.IsMatch(username!);
    }
}

