using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionesCIST.Application.DTOs.Analytics;
using GestionesCIST.Application.Interfaces;

namespace GestionesCIST.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN,GERENTE_GENERAL,JEFE_SOPORTE")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener dashboard ejecutivo con KPIs
        /// </summary>
        [HttpGet("dashboard-ejecutivo")]
        [ProducesResponseType(typeof(DashboardEjecutivoDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboardEjecutivo([FromQuery] DateRangeDto range)
        {
            var result = await _analyticsService.GetDashboardEjecutivoAsync(range);
            return Ok(result);
        }

        /// <summary>
        /// Obtener KPIs principales
        /// </summary>
        [HttpGet("kpis")]
        [ProducesResponseType(typeof(KPIsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetKPIs([FromQuery] DateRangeDto range)
        {
            var result = await _analyticsService.GetKPIsAsync(range);
            return Ok(result);
        }

        /// <summary>
        /// Obtener tendencia de tickets
        /// </summary>
        [HttpGet("tendencia-tickets")]
        [ProducesResponseType(typeof(List<ChartDataDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTendenciaTickets([FromQuery] int dias = 30)
        {
            var result = await _analyticsService.GetTendenciaTicketsAsync(dias);
            return Ok(result);
        }

        /// <summary>
        /// Obtener distribución por estados
        /// </summary>
        [HttpGet("distribucion-estados")]
        [ProducesResponseType(typeof(List<ChartDataDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDistribucionEstados()
        {
            var result = await _analyticsService.GetDistribucionEstadosAsync();
            return Ok(result);
        }

        /// <summary>
        /// Obtener carga de técnicos
        /// </summary>
        [HttpGet("carga-tecnicos")]
        [ProducesResponseType(typeof(List<TecnicoCargaDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCargaTecnicos()
        {
            var result = await _analyticsService.GetCargaTecnicosAsync();
            return Ok(result);
        }

        /// <summary>
        /// Obtener ranking de técnicos
        /// </summary>
        [HttpGet("ranking-tecnicos")]
        [ProducesResponseType(typeof(List<RankingTecnicoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRankingTecnicos([FromQuery] int top = 10)
        {
            var result = await _analyticsService.GetRankingTecnicosAsync(top);
            return Ok(result);
        }

        /// <summary>
        /// Obtener predicción de demanda (IA)
        /// </summary>
        [HttpGet("forecast-demanda")]
        [ProducesResponseType(typeof(ForecastDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetForecastDemanda([FromQuery] int meses = 3)
        {
            var result = await _analyticsService.GetForecastDemandaAsync(meses);
            return Ok(result);
        }

        /// <summary>
        /// Obtener categorías más frecuentes
        /// </summary>
        [HttpGet("categorias-frecuentes")]
        [ProducesResponseType(typeof(List<CategoriaTendenciaDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategoriasFrecuentes([FromQuery] int dias = 30)
        {
            var result = await _analyticsService.GetCategoriasFrecuentesAsync(dias);
            return Ok(result);
        }

        /// <summary>
        /// Obtener métricas de SLA
        /// </summary>
        [HttpGet("metricas-sla")]
        [ProducesResponseType(typeof(SLAMetricasDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMetricasSLA([FromQuery] DateRangeDto range)
        {
            var result = await _analyticsService.GetMetricasSLAAsync(range);
            return Ok(result);
        }
    }

    public class SLAMetricasDto
    {
        public double PorcentajeCumplimiento { get; set; }
        public int TotalEnPlazo { get; set; }
        public int TotalVencidos { get; set; }
        public double TiempoPromedioRespuesta { get; set; }
        public double TiempoPromedioSolucion { get; set; }
    }
}