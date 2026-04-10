using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using System.Text.Json;
using GestionesCIST.WebMVC.Models.ViewModels;
using GestionesCIST.Application.DTOs.Common;

namespace GestionesCIST.WebMVC.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AdminController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IHttpClientFactory httpClientFactory, ILogger<AdminController> logger)
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
        public async Task<IActionResult> Usuarios()
        {
            try
            {
                await ConfigurarTokenAsync();
                var response = await _httpClient.GetAsync("/api/admin/usuarios");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<UsuarioAdminViewModel>>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result?.Success == true)
                    {
                        return View(result.Data);
                    }
                }

                TempData["ErrorMessage"] = "No se pudieron cargar los usuarios.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar usuarios");
                TempData["ErrorMessage"] = "Error al cargar usuarios.";
            }

            return View(new List<UsuarioAdminViewModel>());
        }

        [HttpGet]
        public async Task<IActionResult> Aprobaciones()
        {
            try
            {
                await ConfigurarTokenAsync();
                var response = await _httpClient.GetAsync("/api/admin/usuarios/pendientes");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<UsuarioPendienteViewModel>>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result?.Success == true)
                    {
                        return View(result.Data);
                    }
                }

                TempData["ErrorMessage"] = "No se pudieron cargar las aprobaciones pendientes.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar aprobaciones");
                TempData["ErrorMessage"] = "Error al cargar aprobaciones.";
            }

            return View(new List<UsuarioPendienteViewModel>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprobarUsuario(string id, bool aprobar)
        {
            try
            {
                await ConfigurarTokenAsync();

                var dto = new { aprobar };
                var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/api/admin/usuarios/{id}/aprobar", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = aprobar ? "Usuario aprobado exitosamente." : "Usuario rechazado.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al procesar la solicitud.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aprobar/rechazar usuario {UserId}", id);
                TempData["ErrorMessage"] = "Error al procesar la solicitud.";
            }

            return RedirectToAction(nameof(Aprobaciones));
        }

        [HttpGet]
        public async Task<IActionResult> Tecnicos()
        {
            try
            {
                await ConfigurarTokenAsync();
                // Asumiendo que existe un endpoint para técnicos
                var response = await _httpClient.GetAsync("/api/admin/tecnicos");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<List<TecnicoAdminViewModel>>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result?.Success == true)
                    {
                        return View(result.Data);
                    }
                }

                TempData["ErrorMessage"] = "No se pudieron cargar los técnicos.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar técnicos");
                TempData["ErrorMessage"] = "Error al cargar técnicos.";
            }

            return View(new List<TecnicoAdminViewModel>());
        }

        [HttpGet]
        public IActionResult CrearTecnico()
        {
            return View(new CrearTecnicoViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearTecnico(CrearTecnicoViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await ConfigurarTokenAsync();

                var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/admin/tecnicos", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Técnico creado exitosamente.";
                    return RedirectToAction(nameof(Tecnicos));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Error al crear el técnico.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear técnico");
                ModelState.AddModelError(string.Empty, "Error al crear el técnico.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBloqueoUsuario(string id)
        {
            try
            {
                await ConfigurarTokenAsync();
                var response = await _httpClient.PostAsync($"/api/admin/usuarios/{id}/toggle-bloqueo", null);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Estado del usuario actualizado.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al actualizar el usuario.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al bloquear/desbloquear usuario {UserId}", id);
                TempData["ErrorMessage"] = "Error al procesar la solicitud.";
            }

            return RedirectToAction(nameof(Usuarios));
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                await ConfigurarTokenAsync();
                var response = await _httpClient.GetAsync("/api/analytics/dashboard-ejecutivo");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<DashboardAdminViewModel>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result?.Success == true)
                    {
                        return View(result.Data);
                    }
                }

                TempData["ErrorMessage"] = "No se pudo cargar el dashboard.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar dashboard");
                TempData["ErrorMessage"] = "Error al cargar el dashboard.";
            }

            return View(new DashboardAdminViewModel());
        }
    }
}