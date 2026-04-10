using Microsoft.AspNetCore.SignalR;
using GestionesCIST.Application.Interfaces;
using GestionesCIST.WebAPI.Hubs;

namespace GestionesCIST.WebAPI.BackgroundServices
{
    public class AsignacionAutomaticaService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<KanbanHub> _hubContext;
        private readonly ILogger<AsignacionAutomaticaService> _logger;
        private readonly TimeSpan _intervalo = TimeSpan.FromHours(2); // RF010: 2 horas

        public AsignacionAutomaticaService(
            IServiceProvider serviceProvider,
            IHubContext<KanbanHub> hubContext,
            ILogger<AsignacionAutomaticaService> logger)
        {
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de Asignación Automática iniciado. Intervalo: {Intervalo}", _intervalo);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var kanbanService = scope.ServiceProvider.GetRequiredService<IKanbanService>();

                    var result = await kanbanService.EjecutarAsignacionAutomaticaAsync();

                    if (result.Success && result.Data > 0)
                    {
                        await _hubContext.Clients.Group("JEFE_SOPORTE")
                            .SendAsync("AsignacionAutomaticaCompletada", new { total = result.Data }, stoppingToken);

                        _logger.LogInformation("Asignación automática: {Count} órdenes asignadas.", result.Data);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en servicio de asignación automática");
                }

                await Task.Delay(_intervalo, stoppingToken);
            }

            _logger.LogInformation("Servicio de Asignación Automática detenido.");
        }
    }
}