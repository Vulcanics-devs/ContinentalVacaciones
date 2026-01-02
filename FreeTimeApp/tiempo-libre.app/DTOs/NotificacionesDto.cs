using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using tiempo_libre.Models.Enums;

namespace tiempo_libre.DTOs
{
    public class ObtenerNotificacionesRequest
    {
        /// <summary>
        /// Filtro por tipo de notificación (opcional)
        /// </summary>
        public TiposDeNotificacionEnum? TipoNotificacion { get; set; }

        /// <summary>
        /// Filtro por estatus (opcional)
        /// </summary>
        public EstatusNotificacionEnum? Estatus { get; set; }

        /// <summary>
        /// Filtro por área (opcional, solo para SuperUsuarios)
        /// </summary>
        public int? AreaId { get; set; }

        /// <summary>
        /// Filtro por grupo (opcional, solo para SuperUsuarios)
        /// </summary>
        public int? GrupoId { get; set; }

        /// <summary>
        /// Fecha inicio para filtrar notificaciones (opcional)
        /// </summary>
        public DateTime? FechaInicio { get; set; }

        /// <summary>
        /// Fecha fin para filtrar notificaciones (opcional)
        /// </summary>
        public DateTime? FechaFin { get; set; }

        /// <summary>
        /// Número de página (default: 1)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor a 0")]
        public int Pagina { get; set; } = 1;

        /// <summary>
        /// Tamaño de página (default: 20, max: 100)
        /// </summary>
        [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100")]
        public int TamañoPagina { get; set; } = 20;

        /// <summary>
        /// Campo para ordenamiento (default: FechaAccion)
        /// </summary>
        public string OrdenarPor { get; set; } = "FechaAccion";

        /// <summary>
        /// Dirección de ordenamiento (default: DESC)
        /// </summary>
        public string DireccionOrden { get; set; } = "DESC";
    }

    public class NotificacionResponse
    {
        public int Id { get; set; }
        public TiposDeNotificacionEnum TipoDeNotificacion { get; set; }
        public string TipoNotificacionTexto { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string NombreEmisor { get; set; } = string.Empty;
        public DateTime FechaAccion { get; set; }
        public EstatusNotificacionEnum Estatus { get; set; }
        public string EstatusTexto { get; set; } = string.Empty;
        public string? TipoMovimiento { get; set; }
        public AreaNotificacionDto? Area { get; set; }
        public GrupoNotificacionDto? Grupo { get; set; }
        public int? IdSolicitud { get; set; }
        public bool PuedeMarcarLeida { get; set; }
        public bool PuedeArchivar { get; set; }
    }

    public class AreaNotificacionDto
    {
        public int AreaId { get; set; }
        public string NombreGeneral { get; set; } = string.Empty;
    }

    public class GrupoNotificacionDto
    {
        public int GrupoId { get; set; }
        public string Rol { get; set; } = string.Empty;
    }

    public class NotificacionesResponse
    {
        public List<NotificacionResponse> Notificaciones { get; set; } = new();
        public int TotalNotificaciones { get; set; }
        public int PaginaActual { get; set; }
        public int TamañoPagina { get; set; }
        public int TotalPaginas { get; set; }
        public bool TienePaginaAnterior { get; set; }
        public bool TienePaginaSiguiente { get; set; }
        public EstadisticasNotificacionesDto Estadisticas { get; set; } = new();
        public string RolUsuario { get; set; } = string.Empty;
        public List<int> AreasAccesibles { get; set; } = new();
        public List<int> GruposAccesibles { get; set; } = new();
    }

    public class EstadisticasNotificacionesDto
    {
        public int TotalNotificaciones { get; set; }
        public int NoLeidas { get; set; }
        public int Leidas { get; set; }
        public int Archivadas { get; set; }
        public Dictionary<string, int> PorTipo { get; set; } = new();
        public Dictionary<string, int> PorArea { get; set; } = new();
        public Dictionary<string, int> PorGrupo { get; set; } = new();
        public NotificacionResponse? UltimaNotificacion { get; set; }
    }

    public class MarcarNotificacionRequest
    {
        [Required]
        public int NotificacionId { get; set; }
    }
}