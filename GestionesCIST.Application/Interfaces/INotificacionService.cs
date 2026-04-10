using GestionesCIST.Application.DTOs.Common;

namespace GestionesCIST.Application.Interfaces
{
    public interface INotificacionService
    {
        Task<ApiResponse<bool>> EnviarNotificacionAsync(string usuarioId, string tipo, string titulo, string mensaje, string? urlAccion = null, int prioridad = 2);
        Task<ApiResponse<bool>> NotificarJefesSoporteAsync(string titulo, string mensaje);
        Task<ApiResponse<bool>> NotificarTecnicosAsync(string titulo, string mensaje);
        Task<ApiResponse<bool>> MarcarComoLeidaAsync(long notificacionId, string usuarioId);
        Task<ApiResponse<bool>> MarcarTodasComoLeidasAsync(string usuarioId);
        Task<ApiResponse<List<NotificacionDto>>> GetNotificacionesNoLeidasAsync(string usuarioId);
        Task<ApiResponse<int>> GetConteoNoLeidasAsync(string usuarioId);
    }

    public class NotificacionDto
    {
        public long Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string? URL_Accion { get; set; }
        public bool Leida { get; set; }
        public int Prioridad { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string TiempoTranscurrido { get; set; } = string.Empty;
    }
}