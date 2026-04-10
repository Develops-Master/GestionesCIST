using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace GestionesCIST.WebAPI.Hubs
{
    [Authorize]
    public class KanbanHub : Hub
    {
        private readonly ILogger<KanbanHub> _logger;

        public KanbanHub(ILogger<KanbanHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rol = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(rol))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, rol);
            }

            _logger.LogInformation("Cliente conectado: {ConnectionId}, Usuario: {UserId}, Rol: {Rol}",
                Context.ConnectionId, userId, rol);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Cliente desconectado: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Unirse a grupo específico
        /// </summary>
        public async Task UnirseAGrupo(string grupo)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, grupo);
        }

        /// <summary>
        /// Salir de grupo
        /// </summary>
        public async Task SalirDeGrupo(string grupo)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, grupo);
        }

        /// <summary>
        /// Seguir una orden específica
        /// </summary>
        public async Task SeguirOrden(int ordenId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"orden-{ordenId}");
        }

        /// <summary>
        /// Dejar de seguir una orden
        /// </summary>
        public async Task DejarSeguirOrden(int ordenId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"orden-{ordenId}");
        }

        /// <summary>
        /// Enviar mensaje de chat sobre una orden
        /// </summary>
        public async Task EnviarMensajeOrden(int ordenId, string mensaje)
        {
            var usuario = Context.User?.FindFirst("nombreCompleto")?.Value ?? "Sistema";
            await Clients.Group($"orden-{ordenId}").SendAsync("MensajeRecibido", new
            {
                OrdenId = ordenId,
                Usuario = usuario,
                Mensaje = mensaje,
                Fecha = DateTime.UtcNow
            });
        }
    }
}