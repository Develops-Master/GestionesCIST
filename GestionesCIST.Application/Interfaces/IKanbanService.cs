using GestionesCIST.Application.DTOs.Analytics;
using GestionesCIST.Application.DTOs.Common;
using GestionesCIST.Application.DTOs.Kanban;
using GestionesCIST.Application.DTOs.Ordenes;
using GestionesCIST.Application.DTOs.Tecnicos;
using GestionesCIST.Domain.Enums;

namespace GestionesCIST.Application.Interfaces
{
    public interface IKanbanService
    {
        // RF001 - Obtener tablero Kanban
        Task<ApiResponse<KanbanResponseDto>> GetTableroKanbanAsync();

        // RF007 - Asignar técnico manualmente
        Task<ApiResponse<bool>> AsignarTecnicoManualAsync(int ordenId, int tecnicoId, string asignadoPor);

        // RF018, RF019, RF021, RF022 - Mover orden entre estados
        Task<ApiResponse<bool>> MoverOrdenAsync(int ordenId, EstadoOrden nuevoEstado, string movidoPor, string? informe = null);

        // RF010, RF011, RF012 - Asignación automática
        Task<ApiResponse<int>> EjecutarAsignacionAutomaticaAsync();

        // RF008 - Obtener técnicos disponibles
        Task<ApiResponse<List<TecnicoDisponibleDto>>> GetTecnicosDisponiblesAsync();

        // RF009 - Obtener técnicos con órdenes asignadas
        Task<ApiResponse<List<TecnicoCargaDto>>> GetTecnicosConCargaAsync();

        // Obtener detalle de orden
        Task<ApiResponse<OrdenServicioDto>> GetOrdenDetalleAsync(int ordenId);

        // RF014 - Registrar diagnóstico
        Task<ApiResponse<bool>> RegistrarDiagnosticoAsync(int ordenId, string diagnostico, int tecnicoId);

        // RF017 - Registrar informe técnico
        Task<ApiResponse<bool>> RegistrarInformeTecnicoAsync(int ordenId, string informe, int tecnicoId);
    }
}