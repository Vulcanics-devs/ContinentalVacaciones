using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using tiempo_libre.DTOs;
using tiempo_libre.Models;

namespace tiempo_libre.Services
{
    public interface IRecuperacionPasswordService
    {
        Task<SolicitarCodigoResponse> GenerarCodigoRecuperacionAsync(string email, string? ipAddress, string? userAgent);
        Task<ValidarCodigoResponse> ValidarCodigoAsync(string email, string codigo);
        Task<CambiarPasswordResponse> CambiarPasswordConCodigoAsync(CambiarPasswordRequest request);
        Task LimpiarCodigosExpiradosAsync();
    }

    public class RecuperacionPasswordService : IRecuperacionPasswordService
    {
        private readonly FreeTimeDbContext _db;
        private readonly IEmailService _emailService;
        private readonly ILogger<RecuperacionPasswordService> _logger;
        private const int MINUTOS_EXPIRACION = 15;
        private const int MAX_INTENTOS = 3;

        public RecuperacionPasswordService(
            FreeTimeDbContext db,
            IEmailService emailService,
            ILogger<RecuperacionPasswordService> logger)
        {
            _db = db;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<SolicitarCodigoResponse> GenerarCodigoRecuperacionAsync(string email, string? ipAddress, string? userAgent)
        {
            try
            {
                // Buscar usuario por email
                var usuario = await _db.Users
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == email.ToLower());

                if (usuario == null)
                {
                    // Por seguridad, no revelamos si el email existe o no
                    _logger.LogWarning("Intento de recuperación para email no registrado: {Email}", email);
                    return new SolicitarCodigoResponse
                    {
                        Success = true, // Decimos que fue exitoso aunque no exista
                        Message = "Si el correo está registrado, recibirás un código de verificación.",
                        MinutosExpiracion = MINUTOS_EXPIRACION
                    };
                }

                // Invalidar códigos anteriores no utilizados
                var codigosAnteriores = await _db.CodigosVerificacion
                    .Where(c => c.UsuarioId == usuario.Id
                        && c.TipoCodigo == "CambioPassword"
                        && !c.Usado
                        && c.FechaExpiracion > DateTime.Now)
                    .ToListAsync();

                foreach (var codigoAnterior in codigosAnteriores)
                {
                    codigoAnterior.FechaExpiracion = DateTime.Now;
                }

                // Generar nuevo código de 5 dígitos
                var codigo = GenerarCodigo5Digitos();

                // Crear registro del código
                var nuevoCodigoVerificacion = new CodigoVerificacion
                {
                    UsuarioId = usuario.Id,
                    Codigo = codigo,
                    TipoCodigo = "CambioPassword",
                    FechaCreacion = DateTime.Now,
                    FechaExpiracion = DateTime.Now.AddMinutes(MINUTOS_EXPIRACION),
                    Usado = false,
                    IntentosRestantes = MAX_INTENTOS,
                    IpSolicitud = ipAddress,
                    UserAgent = userAgent
                };

                _db.CodigosVerificacion.Add(nuevoCodigoVerificacion);
                await _db.SaveChangesAsync();

                // Enviar email con el código
                var asunto = "Código de Verificación - Sistema de Vacaciones";
                var cuerpo = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f5f5f5;'>
                            <div style='background-color: white; padding: 30px; border-radius: 10px;'>
                                <h2 style='color: #333;'>Recuperación de Contraseña</h2>
                                <p>Hola {usuario.FullName},</p>
                                <p>Has solicitado un código para cambiar tu contraseña.</p>
                                <div style='background-color: #f0f0f0; padding: 20px; text-align: center; margin: 20px 0; border-radius: 5px;'>
                                    <h1 style='color: #007bff; letter-spacing: 5px; margin: 0;'>{codigo}</h1>
                                </div>
                                <p><strong>Este código expirará en {MINUTOS_EXPIRACION} minutos.</strong></p>
                                <p style='color: #666; font-size: 14px;'>Si no solicitaste este cambio, puedes ignorar este correo.</p>
                                <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
                                <p style='color: #999; font-size: 12px;'>Este es un correo automático del Sistema de Vacaciones - Continental</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                var emailEnviado = await _emailService.SendEmailAsync(usuario.Username, asunto, cuerpo, true);

                if (!emailEnviado)
                {
                    _logger.LogError("No se pudo enviar el email con código a {Email}", email);
                    return new SolicitarCodigoResponse
                    {
                        Success = false,
                        Message = "Error al enviar el correo. Por favor, intente más tarde."
                    };
                }

                _logger.LogInformation("Código de recuperación generado para usuario {UserId}", usuario.Id);

                return new SolicitarCodigoResponse
                {
                    Success = true,
                    Message = "Se ha enviado un código de verificación a tu correo.",
                    MinutosExpiracion = MINUTOS_EXPIRACION
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar código de recuperación");
                return new SolicitarCodigoResponse
                {
                    Success = false,
                    Message = "Error al procesar la solicitud. Por favor, intente más tarde."
                };
            }
        }

        public async Task<ValidarCodigoResponse> ValidarCodigoAsync(string email, string codigo)
        {
            try
            {
                var usuario = await _db.Users
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == email.ToLower());

                if (usuario == null)
                {
                    return new ValidarCodigoResponse
                    {
                        Valido = false,
                        Message = "Código inválido"
                    };
                }

                var codigoVerificacion = await _db.CodigosVerificacion
                    .Where(c => c.UsuarioId == usuario.Id
                        && c.Codigo == codigo
                        && c.TipoCodigo == "CambioPassword"
                        && !c.Usado)
                    .OrderByDescending(c => c.FechaCreacion)
                    .FirstOrDefaultAsync();

                if (codigoVerificacion == null)
                {
                    return new ValidarCodigoResponse
                    {
                        Valido = false,
                        Message = "Código inválido"
                    };
                }

                if (codigoVerificacion.FechaExpiracion < DateTime.Now)
                {
                    return new ValidarCodigoResponse
                    {
                        Valido = false,
                        Message = "El código ha expirado"
                    };
                }

                if (codigoVerificacion.IntentosRestantes <= 0)
                {
                    return new ValidarCodigoResponse
                    {
                        Valido = false,
                        Message = "Se han agotado los intentos para este código"
                    };
                }

                return new ValidarCodigoResponse
                {
                    Valido = true,
                    Message = "Código válido",
                    IntentosRestantes = codigoVerificacion.IntentosRestantes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar código");
                return new ValidarCodigoResponse
                {
                    Valido = false,
                    Message = "Error al validar el código"
                };
            }
        }

        public async Task<CambiarPasswordResponse> CambiarPasswordConCodigoAsync(CambiarPasswordRequest request)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var usuario = await _db.Users
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Email.ToLower());

                if (usuario == null)
                {
                    return new CambiarPasswordResponse
                    {
                        Success = false,
                        Message = "Datos inválidos"
                    };
                }

                var codigoVerificacion = await _db.CodigosVerificacion
                    .Where(c => c.UsuarioId == usuario.Id
                        && c.Codigo == request.CodigoVerificacion
                        && c.TipoCodigo == "CambioPassword"
                        && !c.Usado)
                    .OrderByDescending(c => c.FechaCreacion)
                    .FirstOrDefaultAsync();

                if (codigoVerificacion == null)
                {
                    return new CambiarPasswordResponse
                    {
                        Success = false,
                        Message = "Código inválido"
                    };
                }

                if (codigoVerificacion.FechaExpiracion < DateTime.Now)
                {
                    return new CambiarPasswordResponse
                    {
                        Success = false,
                        Message = "El código ha expirado"
                    };
                }

                if (codigoVerificacion.IntentosRestantes <= 0)
                {
                    return new CambiarPasswordResponse
                    {
                        Success = false,
                        Message = "Se han agotado los intentos para este código"
                    };
                }

                // Decrementar intentos si el código es incorrecto
                codigoVerificacion.IntentosRestantes--;

                // Si el código es correcto, cambiar la contraseña
                var newSalt = Guid.NewGuid().ToString();
                var newHash = PasswordHasher.HashPassword(request.NuevaPassword, newSalt);
                usuario.PasswordSalt = newSalt;
                usuario.PasswordHash = newHash;
                usuario.UpdatedAt = DateTime.Now;

                // Marcar el código como usado
                codigoVerificacion.Usado = true;
                codigoVerificacion.FechaUso = DateTime.Now;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                // Enviar email de confirmación
                var asunto = "Contraseña Cambiada - Sistema de Vacaciones";
                var cuerpo = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <h2>Contraseña Actualizada</h2>
                            <p>Hola {usuario.FullName},</p>
                            <p>Tu contraseña ha sido cambiada exitosamente.</p>
                            <p>Si no realizaste este cambio, contacta inmediatamente al administrador del sistema.</p>
                            <p style='color: #666;'>Fecha y hora del cambio: {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                        </div>
                    </body>
                    </html>";

                await _emailService.SendEmailAsync(usuario.Username, asunto, cuerpo, true);

                _logger.LogInformation("Contraseña cambiada exitosamente para usuario {UserId}", usuario.Id);

                return new CambiarPasswordResponse
                {
                    Success = true,
                    Message = "Contraseña cambiada exitosamente"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al cambiar contraseña");
                return new CambiarPasswordResponse
                {
                    Success = false,
                    Message = "Error al cambiar la contraseña"
                };
            }
        }

        public async Task LimpiarCodigosExpiradosAsync()
        {
            try
            {
                var codigosExpirados = await _db.CodigosVerificacion
                    .Where(c => c.FechaExpiracion < DateTime.Now.AddDays(-1))
                    .ToListAsync();

                if (codigosExpirados.Any())
                {
                    _db.CodigosVerificacion.RemoveRange(codigosExpirados);
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("Se eliminaron {Count} códigos expirados", codigosExpirados.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar códigos expirados");
            }
        }

        private string GenerarCodigo5Digitos()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var randomNumber = BitConverter.ToUInt32(bytes, 0);
            var codigo = (randomNumber % 90000) + 10000; // Genera número entre 10000 y 99999
            return codigo.ToString();
        }

    }
}