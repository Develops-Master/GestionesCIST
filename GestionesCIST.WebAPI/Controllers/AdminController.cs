using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionesCIST.Application.DTOs.Common;
using GestionesCIST.Domain.Entities;
using GestionesCIST.Domain.Enums;

namespace GestionesCIST.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        /// <summary>
        /// RF004 - Listar usuarios pendientes de aprobación
        /// </summary>
        [HttpGet("usuarios/pendientes")]
        [ProducesResponseType(typeof(List<UsuarioPendienteDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsuariosPendientes()
        {
            var usuarios = await _userManager.Users
                .Where(u => u.EstadoAprobacion == EstadoAprobacion.Pendiente)
                .Select(u => new UsuarioPendienteDto
                {
                    Id = u.Id,
                    Email = u.Email!,
                    NombreCompleto = u.NombreCompleto,
                    TipoDocumento = u.TipoDocumento,
                    NumeroDocumento = u.NumeroDocumento,
                    FechaRegistro = u.FechaRegistro
                })
                .ToListAsync();

            return Ok(ApiResponse<List<UsuarioPendienteDto>>.Ok(usuarios));
        }

        /// <summary>
        /// Aprobar o rechazar usuario
        /// </summary>
        [HttpPost("usuarios/{userId}/aprobar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AprobarUsuario(string userId, [FromBody] AprobacionDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<bool>.Fail("Usuario no encontrado"));

            var adminId = User.FindFirst("sub")?.Value;

            user.EstadoAprobacion = dto.Aprobar ? EstadoAprobacion.Aprobado : EstadoAprobacion.Rechazado;
            user.FechaAprobacion = DateTime.UtcNow;
            user.AprobadoPor = adminId;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<bool>.Fail("Error al actualizar usuario"));

            _logger.LogInformation("Usuario {UserId} {Accion} por admin {AdminId}",
                userId, dto.Aprobar ? "aprobado" : "rechazado", adminId);

            return Ok(ApiResponse<bool>.Ok(true, dto.Aprobar ? "Usuario aprobado" : "Usuario rechazado"));
        }

        /// <summary>
        /// Listar todos los usuarios
        /// </summary>
        [HttpGet("usuarios")]
        [ProducesResponseType(typeof(List<UsuarioAdminDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _userManager.Users
                .OrderByDescending(u => u.FechaRegistro)
                .Select(u => new UsuarioAdminDto
                {
                    Id = u.Id,
                    Email = u.Email!,
                    NombreCompleto = u.NombreCompleto,
                    EstadoAprobacion = u.EstadoAprobacion.ToString(),
                    FechaRegistro = u.FechaRegistro,
                    UltimoLogin = u.UltimoLogin
                })
                .ToListAsync();

            // Cargar roles para cada usuario
            foreach (var u in usuarios)
            {
                var user = await _userManager.FindByIdAsync(u.Id);
                u.Roles = (await _userManager.GetRolesAsync(user!)).ToList();
            }

            return Ok(ApiResponse<List<UsuarioAdminDto>>.Ok(usuarios));
        }

        /// <summary>
        /// RF004 - Actualizar roles de usuario
        /// </summary>
        [HttpPut("usuarios/{userId}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ActualizarRoles(string userId, [FromBody] ActualizarRolesDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<bool>.Fail("Usuario no encontrado"));

            var rolesActuales = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, rolesActuales);
            await _userManager.AddToRolesAsync(user, dto.Roles);

            _logger.LogInformation("Roles actualizados para usuario {UserId}: {Roles}", userId, string.Join(", ", dto.Roles));

            return Ok(ApiResponse<bool>.Ok(true, "Roles actualizados"));
        }

        /// <summary>
        /// Bloquear/Desbloquear usuario
        /// </summary>
        [HttpPost("usuarios/{userId}/toggle-bloqueo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ToggleBloqueo(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<bool>.Fail("Usuario no encontrado"));

            user.EstadoAprobacion = user.EstadoAprobacion == EstadoAprobacion.Bloqueado
                ? EstadoAprobacion.Aprobado
                : EstadoAprobacion.Bloqueado;

            await _userManager.UpdateAsync(user);

            return Ok(ApiResponse<bool>.Ok(true, user.EstadoAprobacion == EstadoAprobacion.Bloqueado
                ? "Usuario bloqueado"
                : "Usuario desbloqueado"));
        }

        /// <summary>
        /// Obtener todos los roles disponibles
        /// </summary>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name!,
                    Descripcion = r.Descripcion
                })
                .ToList();

            return Ok(ApiResponse<List<RoleDto>>.Ok(roles));
        }
    }

    public class UsuarioPendienteDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? TipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public class UsuarioAdminDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string EstadoAprobacion { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimoLogin { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class AprobacionDto
    {
        public bool Aprobar { get; set; }
        public string? MotivoRechazo { get; set; }
    }

    public class ActualizarRolesDto
    {
        public List<string> Roles { get; set; } = new();
    }

    public class RoleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}