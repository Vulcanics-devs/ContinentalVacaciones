# Configuración SMTP - Sistema de Vacaciones Continental

## Configuración de Variable de Entorno

El sistema está configurado para usar las credenciales SMTP de Continental. La contraseña debe establecerse mediante una variable de entorno por seguridad.

### Windows (PowerShell como Administrador):
```powershell
# Establecer variable de entorno permanente para el usuario
[System.Environment]::SetEnvironmentVariable("SMTP_PASSWORD", "TU_CONTRASEÑA_AQUI", [System.EnvironmentVariableTarget]::User)

# O establecer variable de entorno temporal (solo para la sesión actual)
$env:SMTP_PASSWORD = "TU_CONTRASEÑA_AQUI"
```

### Windows (Command Prompt como Administrador):
```cmd
# Establecer variable de entorno permanente
setx SMTP_PASSWORD "TU_CONTRASEÑA_AQUI"

# O establecer variable temporal (solo para la sesión actual)
set SMTP_PASSWORD=TU_CONTRASEÑA_AQUI
```

### Linux/macOS (bash/zsh):
```bash
# Agregar a ~/.bashrc o ~/.zshrc para hacerlo permanente
export SMTP_PASSWORD="TU_CONTRASEÑA_AQUI"

# Recargar el archivo de configuración
source ~/.bashrc  # o source ~/.zshrc
```

### Docker:
Si usas Docker, puedes pasar la variable de entorno al contenedor:
```bash
docker run -e SMTP_PASSWORD="TU_CONTRASEÑA_AQUI" tu-imagen
```

O en docker-compose.yml:
```yaml
environment:
  - SMTP_PASSWORD=TU_CONTRASEÑA_AQUI
```

## Configuración SMTP Actual

- **Servidor SMTP**: smtpHubEu.contiwan.com
- **Puerto**: 587
- **SSL**: Habilitado
- **Usuario**: uih35795
- **Email de envío**: mats.ti_sl_fa@conti.com.mx
- **Nombre del remitente**: Sistema de Vacaciones - Continental

## Uso del Servicio de Email

El servicio de email ya está registrado en el contenedor de dependencias. Para usarlo en cualquier controlador o servicio:

```csharp
public class MiControlador : ControllerBase
{
    private readonly IEmailService _emailService;

    public MiControlador(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("enviar-notificacion")]
    public async Task<IActionResult> EnviarNotificacion()
    {
        var enviado = await _emailService.SendEmailAsync(
            "destinatario@conti.com.mx",
            "Asunto del correo",
            "<h1>Contenido HTML del correo</h1><p>Este es el mensaje</p>",
            isHtml: true
        );

        if (enviado)
            return Ok("Correo enviado exitosamente");
        else
            return BadRequest("Error al enviar el correo");
    }
}
```

## Verificación de Configuración

Para verificar que la variable de entorno está configurada correctamente:

### Windows PowerShell:
```powershell
[System.Environment]::GetEnvironmentVariable("SMTP_PASSWORD", [System.EnvironmentVariableTarget]::User)
```

### Windows CMD:
```cmd
echo %SMTP_PASSWORD%
```

### Linux/macOS:
```bash
echo $SMTP_PASSWORD
```

## Notas Importantes

1. **Nunca** incluyas la contraseña en los archivos de configuración (appsettings.json)
2. La contraseña debe mantenerse segura y no compartirse en repositorios
3. Si no se establece la variable de entorno, el servicio registrará una advertencia y no podrá enviar correos
4. Después de establecer la variable de entorno, reinicia la aplicación o el IDE para que tome los cambios

## Troubleshooting

Si los correos no se envían:

1. Verifica que la variable de entorno SMTP_PASSWORD está configurada
2. Revisa los logs de la aplicación para mensajes de error específicos
3. Asegúrate de que el servidor puede conectarse a smtpHubEu.contiwan.com:587
4. Verifica que las credenciales son correctas
5. Comprueba que no hay firewalls bloqueando el puerto 587