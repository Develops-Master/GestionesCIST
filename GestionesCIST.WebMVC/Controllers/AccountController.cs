using GestionesCIST.Application.DTOs.Auth;
using GestionesCIST.Application.DTOs.Common;
using GestionesCIST.WebMVC.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace GestionesCIST.WebMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AccountController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var loginDto = new LoginDto
                {
                    Email = model.Email,
                    Password = model.Password,
                    RememberMe = model.RememberMe
                };

                var content = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponseDto>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (tokenResponse != null)
                    {
                        // Guardar token en cookie
                        var claims = new List<Claim>
                        {
                            new(ClaimTypes.NameIdentifier, tokenResponse.Username),
                            new(ClaimTypes.Name, tokenResponse.NombreCompleto),
                            new("Token", tokenResponse.Token)
                        };

                        foreach (var role in tokenResponse.Roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = tokenResponse.Expiration
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity), authProperties);

                        // Guardar token en localStorage (vía JavaScript)
                        HttpContext.Session.SetString("JwtToken", tokenResponse.Token);

                        _logger.LogInformation("Usuario {Email} ha iniciado sesión", model.Email);

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            return Redirect(returnUrl);

                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(error, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (errorResponse?.Message.Contains("pendiente") == true)
                        ModelState.AddModelError(string.Empty, "Tu cuenta está pendiente de aprobación. Espera la confirmación por correo.");
                    else if (errorResponse?.Message.Contains("rechazada") == true)
                        ModelState.AddModelError(string.Empty, "Tu cuenta ha sido rechazada. Contacta a soporte.");
                    else
                        ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login");
                ModelState.AddModelError(string.Empty, "Error al intentar iniciar sesión. Intente nuevamente.");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var registerDto = new RegisterDto
                {
                    Nombres = model.Nombres,
                    Apellidos = model.Apellidos,
                    Email = model.Email,
                    TipoDocumento = model.TipoDocumento,
                    NumeroDocumento = model.NumeroDocumento,
                    Telefono = model.Telefono,
                    Password = model.Password,
                    ConfirmPassword = model.ConfirmPassword
                };

                var content = new StringContent(JsonSerializer.Serialize(registerDto), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/auth/register", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Registro exitoso. Tu cuenta está pendiente de aprobación. Recibirás un correo cuando sea activada.";
                    return RedirectToAction("Login");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, "Error al registrar usuario. Verifica los datos ingresados.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el registro");
                ModelState.AddModelError(string.Empty, "Error al intentar registrarse. Intente nuevamente.");
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(new { model.Email }), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/auth/forgot-password", content);

                TempData["SuccessMessage"] = "Si el correo existe en nuestro sistema, recibirás instrucciones para restablecer tu contraseña.";
                return RedirectToAction("Login");
            }
            catch
            {
                TempData["SuccessMessage"] = "Si el correo existe en nuestro sistema, recibirás instrucciones para restablecer tu contraseña.";
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("/api/auth/perfil");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var perfil = JsonSerializer.Deserialize<PerfilViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(perfil);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var token = HttpContext.Session.GetString("JwtToken");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/auth/change-password", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Contraseña actualizada exitosamente.";
                return RedirectToAction("Perfil");
            }

            ModelState.AddModelError(string.Empty, "Error al cambiar la contraseña.");
            return View(model);
        }
    }
}