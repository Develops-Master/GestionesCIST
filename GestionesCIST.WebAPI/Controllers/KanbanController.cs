using GestionesCIST.Application.DTOs.Kanban;
using GestionesCIST.Application.DTOs.Ordenes;
using GestionesCIST.Application.DTOs.Tecnicos;
using GestionesCIST.Application.Interfaces;
using GestionesCIST.Domain.Enums;
using GestionesCIST.WebAPI.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace GestionesCIST.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class KanbanController : ControllerBase
    {
        private readonly IKanbanService _kanbanService;
        private readonly IHubContext<KanbanHub> _hubContext;
        private readonly ILogger<KanbanController> _logger;

        public KanbanController(
            IKanbanService kanbanService,
            IHubContext<KanbanHub> hubContext,
            ILogger<KanbanController> logger)
        {
            _kanbanService = kanbanService;
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// RF001 - Obtener tablero Kanban completo
        /// </summary>
        [HttpGet("tablero")]
        [Authorize(Roles = "ADMIN,JEFE_SOPORTE,TECNICO,QA")]
        [ProducesResponseType(typeof(KanbanResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTablero()
        {
            var result = await _kanbanService.GetTableroKanbanAsync();

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// RF007 - Asignar técnico manualmente
        /// </summary>
        [HttpPost("asignar/{ordenId}/tecnico/{tecnicoId}")]
        [Authorize(Roles = "ADMIN,JEFE_SOPORTE")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AsignarTecnico(int ordenId, int tecnicoId)
        {
            var userId = User.FindFirst("sub")?.Value ?? "SISTEMA";
            var result = await _kanbanService.AsignarTecnicoManualAsync(ordenId, tecnicoId, userId);

            if (!result.Success)
                return BadRequest(result);

            // Notificar a todos los clientes conectados
            await _hubContext.Clients.All.SendAsync("OrdenAsignada", new { ordenId, tecnicoId });

            return Ok(result);
        }

        /// <summary>
        /// RF018, RF019, RF021, RF022 - Mover orden entre estados
        /// </summary>
        [HttpPut("mover/{ordenId}")]
        [Authorize(Roles = "ADMIN,JEFE_SOPORTE,TECNICO,QA")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MoverOrden(int ordenId, [FromBody] MoverOrdenDto dto)
        {
            var userId = User.FindFirst("sub")?.Value ?? "SISTEMA";
            var result = await _kanbanService.MoverOrdenAsync(ordenId, dto.NuevoEstado, userId, dto.Informe);

            if (!result.Success)
                return BadRequest(result);

            // Notificar actualización del tablero
            await _hubContext.Clients.All.SendAsync("OrdenMovida", new
            {
                ordenId,
                estado = dto.NuevoEstado.ToString(),
                mensaje = result.Message
            });

            return Ok(result);
        }

        /// <summary>
        /// RF008 - Obtener técnicos disponibles
        /// </summary>
        [HttpGet("tecnicos/disponibles")]
        [Authorize(Roles = "ADMIN,JEFE_SOPORTE")]
        [ProducesResponseType(typeof(List<TecnicoDisponibleDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTecnicosDisponibles()
        {
            var result = await _kanbanService.GetTecnicosDisponiblesAsync();

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// RF009 - Obtener técnicos con carga
        /// </summary>
        [HttpGet("tecnicos/carga")]
        [Authorize(Roles = "ADMIN,JEFE_SOPORTE")]
        [ProducesResponseType(typeof(List<TecnicoCargaDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTecnicosConCarga()
        {
            var result = await _kanbanService.GetTecnicosConCargaAsync();

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// RF010 - Ejecutar asignación automática
        /// </summary>
        [HttpPost("asignacion-automatica")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> EjecutarAsignacionAutomatica()
        {
            var result = await _kanbanService.EjecutarAsignacionAutomaticaAsync();

            if (!result.Success)
                return BadRequest(result);

            await _hubContext.Clients.Group("JEFE_SOPORTE")
                .SendAsync("AsignacionAutomaticaCompletada", new { total = result.Data });

            return Ok(result);
        }

        /// <summary>
        /// Obtener detalle de orden
        /// </summary>
        [HttpGet("{ordenId}")]
        [Authorize(Roles = "ADMIN,JEFE_SOPORTE,TECNICO,QA")]
        [ProducesResponseType(typeof(OrdenServicioDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrdenDetalle(int ordenId)
        {
            var result = await _kanbanService.GetOrdenDetalleAsync(ordenId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// RF014 - Registrar diagnóstico
        /// </summary>
        [HttpPost("{ordenId}/diagnostico")]
        [Authorize(Roles = "TECNICO")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RegistrarDiagnostico(int ordenId, [FromBody] RegistrarDiagnosticoDto dto)
        {
            var tecnicoId = await GetTecnicoIdFromUser();
            if (!tecnicoId.HasValue)
                return Unauthorized("Usuario no es técnico");

            var result = await _kanbanService.RegistrarDiagnosticoAsync(ordenId, dto.Diagnostico, tecnicoId.Value);

            if (!result.Success)
                return BadRequest(result);

            await _hubContext.Clients.Group($"orden-{ordenId}")
                .SendAsync("DiagnosticoRegistrado", new { ordenId, dto.Diagnostico });

            return Ok(result);
        }

        /// <summary>
        /// RF017 - Registrar informe técnico
        /// </summary>
        [HttpPost("{ordenId}/informe")]
        [Authorize(Roles = "TECNICO,QA")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RegistrarInforme(int ordenId, [FromBody] RegistrarInformeTecnicoDto dto)
        {
            var tecnicoId = await GetTecnicoIdFromUser();
            if (!tecnicoId.HasValue)
                return Unauthorized("Usuario no es técnico");

            var result = await _kanbanService.RegistrarInformeTecnicoAsync(ordenId, dto.Detalle, tecnicoId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        private async Task<int?> GetTecnicoIdFromUser()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return null;

            // Buscar técnico por UserId (implementar en servicio)
            // Por ahora retornamos un valor simulado
            return 1;
        }
    }
}