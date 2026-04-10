using GestionesCIST.Application.DTOs.Common;
using GestionesCIST.Application.DTOs.Ordenes;
using GestionesCIST.Application.DTOs.Tickets;
using GestionesCIST.Application.Interfaces;
using GestionesCIST.WebAPI.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel.DataAnnotations;

namespace GestionesCIST.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly IHubContext<KanbanHub> _hubContext;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(
            ITicketService ticketService,
            IHubContext<KanbanHub> hubContext,
            ILogger<TicketsController> logger)
        {
            _ticketService = ticketService;
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Crear nuevo ticket (Portal Cliente)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "CLIENTE,ADMIN")]
        [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CrearTicket([FromForm] CrearTicketDto dto)
        {
            var clienteId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(clienteId))
                return Unauthorized();

            var result = await _ticketService.CrearTicketAsync(dto, clienteId);

            if (!result.Success)
                return BadRequest(result);

            // Notificar a jefes de soporte vía SignalR
            await _hubContext.Clients.Group("JEFE_SOPORTE")
                .SendAsync("NuevoTicketCreado", new { result.Data.CodigoTicket, result.Data.Titulo });

            return Ok(result);
        }

        /// <summary>
        /// Obtener ticket por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TicketResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTicket(int id)
        {
            var result = await _ticketService.GetTicketPorIdAsync(id);

            if (!result.Success)
                return NotFound(result);

            // Verificar permisos (cliente solo ve sus tickets)
            if (User.IsInRole("CLIENTE"))
            {
                var clienteId = User.FindFirst("sub")?.Value;
                // Validar que el ticket pertenece al cliente
            }

            return Ok(result);
        }

        /// <summary>
        /// Obtener tickets paginados (Admin/Jefe Soporte)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "ADMIN,JEFE_SOPORTE,TECNICO")]
        [ProducesResponseType(typeof(PagedResult<TicketResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTickets(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? estado = null)
        {
            var result = await _ticketService.GetTicketsPaginadosAsync(pageNumber, pageSize, estado);
            return Ok(result);
        }

        /// <summary>
        /// Obtener tickets del cliente autenticado
        /// </summary>
        [HttpGet("mis-tickets")]
        [Authorize(Roles = "CLIENTE")]
        [ProducesResponseType(typeof(List<TicketClienteDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMisTickets()
        {
            var clienteId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(clienteId))
                return Unauthorized();

            var result = await _ticketService.GetTicketsPorClienteAsync(clienteId);
            return Ok(result);
        }

        /// <summary>
        /// Obtener tickets pendientes de asignación
        /// </summary>
        [HttpGet("pendientes")]
        [Authorize(Roles = "ADMIN,JEFE_SOPORTE")]
        [ProducesResponseType(typeof(List<TicketResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTicketsPendientes()
        {
            var result = await _ticketService.GetTicketsPendientesAsignacionAsync();
            return Ok(result);
        }

        /// <summary>
        /// Asignar ticket a técnico
        /// </summary>
        [HttpPost("{id}/asignar")]
        [Authorize(Roles = "ADMIN,JEFE_SOPORTE")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AsignarTicket(int id, [FromBody] AsignarTicketDto dto)
        {
            var asignadoPor = User.FindFirst("sub")?.Value ?? "SISTEMA";
            var result = await _ticketService.AsignarTicketAsync(id, dto.TecnicoId, asignadoPor);

            if (!result.Success)
                return BadRequest(result);

            await _hubContext.Clients.All.SendAsync("TicketAsignado", new { ticketId = id, tecnicoId = dto.TecnicoId });

            return Ok(result);
        }

        /// <summary>
        /// Cambiar estado del ticket
        /// </summary>
        [HttpPut("{id}/estado")]
        [Authorize(Roles = "ADMIN,JEFE_SOPORTE,TECNICO")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoDto dto)
        {
            var usuarioId = User.FindFirst("sub")?.Value ?? "SISTEMA";
            var result = await _ticketService.CambiarEstadoTicketAsync(id, dto.Estado, usuarioId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Agregar comentario al ticket
        /// </summary>
        [HttpPost("{id}/comentarios")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AgregarComentario(int id, [FromBody] AgregarComentarioDto dto)
        {
            var usuarioId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized();

            var result = await _ticketService.AgregarComentarioAsync(id, dto, usuarioId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Clasificar ticket con IA
        /// </summary>
        [HttpPost("clasificar")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ClasificacionIADto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ClasificarConIA([FromBody] ClasificarRequestDto dto)
        {
            var result = await _ticketService.ClasificarTicketConIAAsync(dto.Descripcion);
            return Ok(result);
        }

        /// <summary>
        /// Convertir ticket a orden de servicio
        /// </summary>
        [HttpPost("{id}/convertir")]
        [Authorize(Roles = "ADMIN,JEFE_SOPORTE")]
        [ProducesResponseType(typeof(OrdenServicioDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ConvertirAOrden(int id)
        {
            var usuarioId = User.FindFirst("sub")?.Value ?? "SISTEMA";
            var result = await _ticketService.ConvertirTicketAOrdenAsync(id, usuarioId);

            if (!result.Success)
                return BadRequest(result);

            await _hubContext.Clients.All.SendAsync("TicketConvertido", new { ticketId = id, ordenId = result.Data.Id });

            return Ok(result);
        }
    }

    public class AsignarTicketDto
    {
        [Required]
        public int TecnicoId { get; set; }
    }

    public class CambiarEstadoDto
    {
        [Required]
        public string Estado { get; set; } = string.Empty;
    }

    public class ClasificarRequestDto
    {
        [Required]
        public string Descripcion { get; set; } = string.Empty;
    }
}