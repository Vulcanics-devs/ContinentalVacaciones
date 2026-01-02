using Microsoft.EntityFrameworkCore;
using tiempo_libre.Models;

namespace tiempo_libre.Services
{
    public interface ISuplenciaService
    {
        Task<SuplenciaActiva?> ObtenerSuplenciaActiva(int usuarioId);
        Task<bool> EsSuplente(int usuarioId, string rol, int? areaId = null, int? grupoId = null);
    }

    public class SuplenciaActiva
    {
        public int UsuarioTitularId { get; set; }
        public string RolSuplente { get; set; } = null!;
        public int? AreaId { get; set; }
        public int? GrupoId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }

    public class SuplenciaService : ISuplenciaService
    {
        private readonly FreeTimeDbContext _context;

        public SuplenciaService(FreeTimeDbContext context)
        {
            _context = context;
        }

        public async Task<SuplenciaActiva?> ObtenerSuplenciaActiva(int usuarioId)
        {
            var ahora = DateTime.UtcNow;

            var suplencia = await _context.SuplentePeriodos
                .Where(sp => sp.SuplenteId == usuarioId
                          && sp.Activo
                          && sp.FechaInicio <= ahora
                          && sp.FechaFin >= ahora)
                .OrderByDescending(sp => sp.CreatedAt)
                .FirstOrDefaultAsync();

            if (suplencia == null)
                return null;

            return new SuplenciaActiva
            {
                UsuarioTitularId = suplencia.UsuarioId,
                RolSuplente = suplencia.Rol,
                AreaId = suplencia.AreaId,
                GrupoId = suplencia.GrupoId,
                FechaInicio = suplencia.FechaInicio,
                FechaFin = suplencia.FechaFin
            };
        }

        public async Task<bool> EsSuplente(int usuarioId, string rol, int? areaId = null, int? grupoId = null)
        {
            var suplencia = await ObtenerSuplenciaActiva(usuarioId);

            if (suplencia == null)
                return false;

            // Verificar que el rol coincida
            if (suplencia.RolSuplente != rol)
                return false;

            // Verificar área si aplica
            if (areaId.HasValue && suplencia.AreaId != areaId)
                return false;

            // Verificar grupo si aplica
            if (grupoId.HasValue && suplencia.GrupoId != grupoId)
                return false;

            return true;
        }
    }
}