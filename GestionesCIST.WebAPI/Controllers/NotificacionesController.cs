using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionesCIST.Application.Interfaces;

namespace GestionesCIST.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificacionesController : ControllerBase
    {
        private readonly INotificacionService _notificacionService;

        public NotificacionesController(INotificacionService notificacionService)
        {
            _notificacionService = notificacionService;
        }

        /// <summary>
        /// Obtener notificaciones no leídas
        /// </summary>
        [HttpGet("no-leidas")]
        [ProducesResponseType(typeof(List<NotificacionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNoLeidas()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _notificacionService.GetNotificacionesNoLeidasAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Obtener conteo de notificaciones no leídas
        /// </summary>
        [HttpGet("conteo")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetConteo()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _notificacionService.GetConteoNoLeidasAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Marcar notificación como leída
        /// </summary>
        [HttpPut("{id}/leida")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> MarcarLeida(long id)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _notificacionService.MarcarComoLeidaAsync(id, userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Marcar todas como leídas
        /// </summary>
        [HttpPut("marcar-todas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> MarcarTodasLeidas()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _notificacionService.MarcarTodasComoLeidasAsync(userId);
            return Ok(result);
        }
    }
}