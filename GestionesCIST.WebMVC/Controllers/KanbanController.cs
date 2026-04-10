using GestionesCIST.Application.DTOs.Common;
using GestionesCIST.Application.DTOs.Kanban;
using GestionesCIST.Application.DTOs.Ordenes;
using GestionesCIST.WebMVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GestionesCIST.WebMVC.Controllers
{
    [Authorize(Roles = "ADMIN,JEFE_SOPORTE,TECNICO,QA")]
    public class KanbanController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KanbanController> _logger;

        public KanbanController(IHttpClientFactory httpClientFactory, ILogger<KanbanController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
        }

        private async Task ConfigurarTokenAsync()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                await ConfigurarTokenAsync();
                var response = await _httpClient.GetAsync("/api/kanban/tablero");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<KanbanResponseDto>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result?.Success == true)
                    {
                        var viewModel = new KanbanViewModel
                        {
                            Columnas = result.Data.Columnas.Select(c => new ColumnaKanbanViewModel
                            {
                                Estado = c.Estado,
                                Nombre = c.Nombre,
                                ColorClase = c.ColorClase,
                                LimiteWIP = c.LimiteWIP,
                                Tarjetas = c.Tarjetas.Select(t => new TarjetaKanbanViewModel
                                {
                                    Id = t.Id,
                                    NumeroOrden = t.NumeroOrden,
                                    Cliente = t.Cliente,
                                    Marca = t.Marca,
                                    Modelo = t.Modelo,
                                    TecnicoAsignado = t.TecnicoAsignado,
                                    Prioridad = t.Prioridad.ToString(),
                                    EsGarantia = t.EsGarantia,
                                    ProbabilidadExitoQA = t.ProbabilidadExitoQA,
                                    TiempoEnEstado = t.TiempoEnEstado,
                                    EstaRetrasada = t.EstaRetrasada,
                                    AlertaIA = t.AlertaIA
                                }).ToList()
                            }).ToList(),
                            Resumen = new ResumenKanbanViewModel
                            {
                                TotalOrdenesActivas = result.Data.Resumen.TotalOrdenesActivas,
                                OrdenesRetrasadas = result.Data.Resumen.OrdenesRetrasadas,
                                OrdenesCriticas = result.Data.Resumen.OrdenesCriticas,
                                TiempoPromedioCierreHoras = result.Data.Resumen.TiempoPromedioCierreHoras,
                                TecnicosDisponibles = result.Data.Resumen.TecnicosDisponibles
                            }
                        };

                        return View(viewModel);
                    }
                }

                TempData["ErrorMessage"] = "No se pudo cargar el tablero Kanban.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar tablero Kanban");
                TempData["ErrorMessage"] = "Error al cargar el tablero.";
            }

            return View(new KanbanViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            try
            {
                await ConfigurarTokenAsync();
                var response = await _httpClient.GetAsync($"/api/kanban/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<OrdenServicioDto>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result?.Success == true)
                    {
                        var viewModel = new OrdenDetalleViewModel
                        {
                            Id = result.Data.Id,
                            NumeroOrden = result.Data.NumeroOrden,
                            Cliente = result.Data.Cliente,
                            TipoEquipo = result.Data.TipoEquipo,
                            Marca = result.Data.Marca,
                            Modelo = result.Data.Modelo,
                            NumeroSerie = result.Data.NumeroSerie,
                            Estado = result.Data.Estado,
                            Prioridad = result.Data.Prioridad.ToString(),
                            EsGarantia = result.Data.EsGarantia,
                            TecnicoDiagnostico = result.Data.TecnicoDiagnostico,
                            TecnicoReparacion = result.Data.TecnicoReparacion,
                            TesterQA = result.Data.TesterQA,
                            DiagnosticoFinal = result.Data.DiagnosticoFinal,
                            ProbabilidadExitoQA = result.Data.ProbabilidadExitoQA,
                            RepuestosSugeridosIA = result.Data.RepuestosSugeridosIA,
                            FechaRecepcion = result.Data.FechaRecepcion,
                            FechaEntregaAlmacen = result.Data.FechaEntregaAlmacen,
                            TiempoTotalHoras = result.Data.TiempoTotalHoras
                        };

                        return View(viewModel);
                    }
                }

                TempData["ErrorMessage"] = "No se pudo cargar el detalle de la orden.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalle de orden {OrdenId}", id);
                TempData["ErrorMessage"] = "Error al cargar el detalle.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}