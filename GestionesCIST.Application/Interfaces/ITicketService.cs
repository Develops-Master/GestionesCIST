using GestionesCIST.Application.DTOs.Common;
using GestionesCIST.Application.DTOs.Ordenes;
using GestionesCIST.Application.DTOs.Tickets;

namespace GestionesCIST.Application.Interfaces
{
    public interface ITicketService
    {
        Task<ApiResponse<TicketResponseDto>> CrearTicketAsync(CrearTicketDto dto, string clienteId);
        Task<ApiResponse<TicketResponseDto>> GetTicketPorIdAsync(int id);
        Task<ApiResponse<PagedResult<TicketResponseDto>>> GetTicketsPaginadosAsync(int pageNumber, int pageSize, string? estado = null);
        Task<ApiResponse<List<TicketClienteDto>>> GetTicketsPorClienteAsync(string clienteId);
        Task<ApiResponse<List<TicketResponseDto>>> GetTicketsPendientesAsignacionAsync();
        Task<ApiResponse<bool>> AsignarTicketAsync(int ticketId, int tecnicoId, string asignadoPor);
        Task<ApiResponse<bool>> CambiarEstadoTicketAsync(int ticketId, string nuevoEstado, string usuarioId);
        Task<ApiResponse<bool>> AgregarComentarioAsync(int ticketId, AgregarComentarioDto dto, string usuarioId);
        Task<ApiResponse<ClasificacionIADto>> ClasificarTicketConIAAsync(string descripcion);
        Task<ApiResponse<OrdenServicioDto>> ConvertirTicketAOrdenAsync(int ticketId, string usuarioId);
    }
}