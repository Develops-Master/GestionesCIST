using GestionesCIST.Application.DTOs.Auth;
using GestionesCIST.Application.DTOs.Common;
using GestionesCIST.Application.Interfaces;
using GestionesCIST.Domain.Entities;
using GestionesCIST.Domain.Enums;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GestionesCIST.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly INotificacionService _notificacionService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            INotificacionService notificacionService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _notificacionService = notificacionService;
        }

        public async Task<ApiResponse<TokenResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    return ApiResponse<TokenResponseDto>.Fail("Usuario o contraseña incorrectos");
                }

                // Validar estado de aprobación
                if (user.EstadoAprobacion != EstadoAprobacion.Aprobado)
                {
                    var mensaje = user.EstadoAprobacion switch
                    {
                        EstadoAprobacion.Pendiente => "Tu cuenta está pendiente de aprobación.",
                        EstadoAprobacion.Rechazado => "Tu cuenta ha sido rechazada.",
                        EstadoAprobacion.Bloqueado => "Tu cuenta ha sido bloqueada.",
                        _ => "Estado de cuenta no válido."
                    };
                    return ApiResponse<TokenResponseDto>.Fail(mensaje);
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
                if (!result.Succeeded)
                {
                    user.IntentosFallidos++;
                    await _userManager.UpdateAsync(user);
                    return ApiResponse<TokenResponseDto>.Fail("Usuario o contraseña incorrectos");
                }

                // Actualizar último login
                user.UltimoLogin = DateTime.UtcNow;
                user.IntentosFallidos = 0;
                await _userManager.UpdateAsync(user);

                var roles = await _userManager.GetRolesAsync(user);
                var token = await GenerateJwtTokenAsync(user, roles.ToList());

                _logger.LogInformation("Usuario {Email} ha iniciado sesión exitosamente", user.Email);

                return ApiResponse<TokenResponseDto>.Ok(token, "Inicio de sesión exitoso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login de {Email}", loginDto.Email);
                return ApiResponse<TokenResponseDto>.Fail("Error interno del servidor");
            }
        }

        public async Task<ApiResponse<TokenResponseDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return ApiResponse<TokenResponseDto>.Fail("El email ya está registrado");
                }

                var user = new ApplicationUser
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    Nombres = registerDto.Nombres,
                    Apellidos = registerDto.Apellidos,
                    TipoDocumento = registerDto.TipoDocumento,
                    NumeroDocumento = registerDto.NumeroDocumento,
                    PhoneNumber = registerDto.Telefono,
                    EstadoAprobacion = EstadoAprobacion.Pendiente,
                    FechaRegistro = DateTime.UtcNow,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse<TokenResponseDto>.Fail("Error al crear el usuario", errors);
                }

                await _userManager.AddToRoleAsync(user, "CLIENTE");

                // Notificar a administradores
                await _notificacionService.NotificarJefesSoporteAsync(
                    "Nuevo registro pendiente",
                    $"El usuario {user.NombreCompleto} ({user.Email}) está pendiente de aprobación.");

                _logger.LogInformation("Nuevo usuario registrado: {Email}", user.Email);

                return ApiResponse<TokenResponseDto>.Ok(new TokenResponseDto(),
                    "Registro exitoso. Tu cuenta está pendiente de aprobación.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el registro de {Email}", registerDto.Email);
                return ApiResponse<TokenResponseDto>.Fail("Error interno del servidor");
            }
        }

        private async Task<TokenResponseDto> GenerateJwtTokenAsync(ApplicationUser user, List<string> roles)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException());

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("nombreCompleto", user.NombreCompleto)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"] ?? "60")),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new TokenResponseDto
            {
                Token = tokenHandler.WriteToken(token),
                RefreshToken = GenerateRefreshToken(),
                Expiration = tokenDescriptor.Expires.Value,
                Username = user.UserName ?? string.Empty,
                NombreCompleto = user.NombreCompleto,
                Roles = roles
            };
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        // Otros métodos (RefreshToken, Logout, ChangePassword, etc.)...
        public Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<bool>> LogoutAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<bool>> ForgotPasswordAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<bool>> ResetPasswordAsync(string email, string token, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<bool>> ConfirmEmailAsync(string userId, string token)
        {
            throw new NotImplementedException();
        }
    }
}