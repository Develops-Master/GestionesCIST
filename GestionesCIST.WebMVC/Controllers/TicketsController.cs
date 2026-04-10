using GestionesCIST.Application.DTOs.Common;
using GestionesCIST.Application.DTOs.Tickets;
using GestionesCIST.WebMVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace GestionesCIST.WebMVC.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(IHttpClientFactory httpClientFactory, ILogger<TicketsController> logger)
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
        [Authorize(Roles = "ADMIN,JEFE_SOPORTE,TECNICO")]
        public async Task<IActionResult> Index(string? estado = null, int page = 1)
        {
            try
            {
                await ConfigurarTokenAsync();
                var url = $"/api/tickets?pageNumber={page}&pageSize=10";
                if (!string.IsNullOrEmpty(estado))
                    url += $"&estado={estado}";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<PagedResult<TicketResponseDto>>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result?.Success == true)
                    {
                        ViewBag.EstadoActual = estado;
                        return View(result.Data);
                    }
                }

                TempData["ErrorMessage"] = "No se pudieron cargar los tickets.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar tickets");
                TempData["ErrorMessage"] = "Error al cargar los tickets.";
            }

            return View(new PagedResult<TicketResponseDto>());
        }

        [HttpGet]
        [Authorize(Roles = "CLIENTE,ADMIN")]
        public async Task<IActionResult> MisTickets()
        {
            try
            {
                await ConfigurarTokenAsync();
                var response = await _httpClient.GetAsync("/api/tickets/mis-tickets");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<TicketClienteDto>>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result?.Success == true)
                    {
                        var viewModel = result.Data.Select(t => new TicketClienteViewModel
                        {
                            Id = 0, // El DTO no tiene Id
                            CodigoTicket = t.CodigoTicket,
                            Titulo = "Ver detalle",
                            Equipo = "N/A",
                            Estado = t.Estado,
                            EstadoKanban = t.EstadoKanban,
                            FechaCreacion = t.FechaCreacion,
                            UltimoComentario = t.UltimoComentario,
                            Progreso = t.Progreso
                        }).ToList();

                        return View(viewModel);
                    }
                }

                TempData["ErrorMessage"] = "No se pudieron cargar tus tickets.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar tickets del cliente");
                TempData["ErrorMessage"] = "Error al cargar tus tickets.";
            }

            return View(new List<TicketClienteViewModel>());
        }

        [HttpGet]
        [Authorize(Roles = "CLIENTE,ADMIN")]
        public IActionResult Crear()
        {
            return View(new CrearTicketViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "CLIENTE,ADMIN")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearTicketViewModel model, IFormFile? archivoAdjunto)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await ConfigurarTokenAsync();

                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(model.TipoEquipo), nameof(model.TipoEquipo));
                content.Add(new StringContent(model.Marca), nameof(model.Marca));
                content.Add(new StringContent(model.Modelo), nameof(model.Modelo));
                if (!string.IsNullOrEmpty(model.NumeroSerie))
                    content.Add(new StringContent(model.NumeroSerie), nameof(model.NumeroSerie));
                content.Add(new StringContent(model.Titulo), nameof(model.Titulo));
                content.Add(new StringContent(model.DescripcionProblema), nameof(model.DescripcionProblema));
                if (!string.IsNullOrEmpty(model.ContactoAlternativo))
                    content.Add(new StringContent(model.ContactoAlternativo), nameof(model.ContactoAlternativo));

                if (archivoAdjunto != null)
                {
                    var fileContent = new StreamContent(archivoAdjunto.OpenReadStream());
                    content.Add(fileContent, "ArchivoAdjunto", archivoAdjunto.FileName);
                }

                var response = await _httpClient.PostAsync("/api/tickets", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Ticket creado exitosamente. Te notificaremos cuando sea atendido.";
                    return RedirectToAction(nameof(MisTickets));
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, "Error al crear el ticket. Verifica los datos.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ticket");
                ModelState.AddModelError(string.Empty, "Error al crear el ticket. Intente nuevamente.");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            try
            {
                await ConfigurarTokenAsync();
                var response = await _httpClient.GetAsync($"/api/tickets/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<TicketResponseDto>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result?.Success == true)
                    {
                        var viewModel = new TicketDetalleViewModel
                        {
                            Id = result.Data.Id,
                            CodigoTicket = result.Data.CodigoTicket,
                            Cliente = result.Data.Cliente,
                            TipoEquipo = result.Data.TipoEquipo,
                            Marca = result.Data.Marca,
                            Modelo = result.Data.Modelo,
                            NumeroSerie = result.Data.NumeroSerie,
                            Titulo = result.Data.Titulo,
                            DescripcionProblema = result.Data.DescripcionProblema,
                            Estado = result.Data.Estado,
                            EstadoNombre = result.Data.Estado.ToString(),
                            Prioridad = result.Data.Prioridad,
                            CategoriaSugerida = result.Data.CategoriaSugerida,
                            ConfianzaPrediccion = result.Data.ConfianzaPrediccion,
                            FechaCreacion = result.Data.FechaCreacion,
                            FechaCierre = result.Data.FechaCierre,
                            TecnicoAsignado = result.Data.TecnicoAsignado,
                            EstadoSLA = result.Data.EstadoSLA,
                            Comentarios = result.Data.Comentarios.Select(c => new ComentarioTicketViewModel
                            {
                                Id = c.Id,
                                Usuario = c.Usuario,
                                Comentario = c.Comentario,
                                FechaCreacion = c.FechaCreacion,
                                EsInterno = c.EsInterno,
                                AvatarInicial = c.Usuario.Length > 0 ? c.Usuario[0].ToString().ToUpper() : "U"
                            }).ToList()
                        };

                        return View(viewModel);
                    }
                }

                TempData["ErrorMessage"] = "No se pudo cargar el detalle del ticket.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalle de ticket {TicketId}", id);
                TempData["ErrorMessage"] = "Error al cargar el detalle.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarComentario(int id, AgregarComentarioViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Detalle), new { id });

            try
            {
                await ConfigurarTokenAsync();

                var dto = new AgregarComentarioDto
                {
                    Comentario = model.Comentario,
                    EsInterno = model.EsInterno
                };

                var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/api/tickets/{id}/comentarios", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Comentario agregado exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al agregar el comentario.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar comentario");
                TempData["ErrorMessage"] = "Error al agregar comentario.";
            }

            return RedirectToAction(nameof(Detalle), new { id });
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN,JEFE_SOPORTE")]
        public async Task<IActionResult> Pendientes()
        {
            try
            {
                await ConfigurarTokenAsync();
                var response = await _httpClient.GetAsync("/api/tickets/pendientes");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<TicketResponseDto>>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result?.Success == true)
                    {
                        return View(result.Data);
                    }
                }

                TempData["ErrorMessage"] = "No se pudieron cargar los tickets pendientes.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar tickets pendientes");
                TempData["ErrorMessage"] = "Error al cargar tickets pendientes.";
            }

            return View(new List<TicketResponseDto>());
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,JEFE_SOPORTE")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConvertirAOrden(int id)
        {
            try
            {
                await ConfigurarTokenAsync();
                var response = await _httpClient.PostAsync($"/api/tickets/{id}/convertir", null);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Ticket convertido a orden de servicio exitosamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al convertir el ticket a orden.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir ticket {TicketId}", id);
                TempData["ErrorMessage"] = "Error al convertir el ticket.";
            }

            return RedirectToAction(nameof(Detalle), new { id });
        }
    }
}