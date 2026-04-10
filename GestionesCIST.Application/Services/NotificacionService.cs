using GestionesCIST.Application.DTOs.Common;
using GestionesCIST.Application.Interfaces;
using GestionesCIST.Domain.Entities;
using GestionesCIST.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace GestionesCIST.Application.Services
{
    public class NotificacionService : INotificacionService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificacionService> _logger;

        public NotificacionService(AppDbContext context, ILogger<NotificacionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> EnviarNotificacionAsync(
            string usuarioId, string tipo, string titulo, string mensaje, string? urlAccion = null, int prioridad = 2)
        {
            try
            {
                var notificacion = new Notificacion
                {
                    UsuarioId = usuarioId,
                    Tipo = tipo,
                    Titulo = titulo,
                    Mensaje = mensaje,
                    URL_Accion = urlAccion,
                    Prioridad = prioridad,
                    FechaCreacion = DateTime.UtcNow,
                    Leida = false
                };

                _context.Notificaciones.Add(notificacion);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación a usuario {UsuarioId}", usuarioId);
                return ApiResponse<bool>.Fail("Error al enviar notificación");
            }
        }

        public async Task<ApiResponse<bool>> NotificarJefesSoporteAsync(string titulo, string mensaje)
        {
            try
            {
                var jefes = await _context.Users
                    .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                    .Join(_context.Roles, ur => ur.ur.RoleId, r => r.Id, (ur, r) => new { ur.u, Role = r.Name })
                    .Where(x => x.Role == "JEFE_SOPORTE" || x.Role == "ADMIN")
                    .Select(x => x.u.Id)
                    .ToListAsync();

                foreach (var jefeId in jefes)
                {
                    await EnviarNotificacionAsync(jefeId, "SISTEMA", titulo, mensaje, null, 3);
                }

                return ApiResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al notificar a jefes de soporte");
                return ApiResponse<bool>.Fail("Error al enviar notificaciones");
            }
        }

        public async Task<ApiResponse<bool>> NotificarTecnicosAsync(string titulo, string mensaje)
        {
            try
            {
                var tecnicos = await _context.Users
                    .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                    .Join(_context.Roles, ur => ur.ur.RoleId, r => r.Id, (ur, r) => new { ur.u, Role = r.Name })
                    .Where(x => x.Role == "TECNICO")
                    .Select(x => x.u.Id)
                    .ToListAsync();

                foreach (var tecnicoId in tecnicos)
                {
                    await EnviarNotificacionAsync(tecnicoId, "SISTEMA", titulo, mensaje, null, 2);
                }

                return ApiResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al notificar a técnicos");
                return ApiResponse<bool>.Fail("Error al enviar notificaciones");
            }
        }

        public async Task<ApiResponse<bool>> MarcarComoLeidaAsync(long notificacionId, string usuarioId)
        {
            try
            {
                var notificacion = await _context.Notificaciones
                    .FirstOrDefaultAsync(n => n.Id == notificacionId && n.UsuarioId == usuarioId);

                if (notificacion == null)
                    return ApiResponse<bool>.Fail("Notificación no encontrada");

                notificacion.Leida = true;
                notificacion.FechaLeida = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar notificación como leída");
                return ApiResponse<bool>.Fail("Error al actualizar notificación");
            }
        }

        public async Task<ApiResponse<bool>> MarcarTodasComoLeidasAsync(string usuarioId)
        {
            try
            {
                var notificaciones = await _context.Notificaciones
                    .Where(n => n.UsuarioId == usuarioId && !n.Leida)
                    .ToListAsync();

                foreach (var n in notificaciones)
                {
                    n.Leida = true;
                    n.FechaLeida = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar todas las notificaciones como leídas");
                return ApiResponse<bool>.Fail("Error al actualizar notificaciones");
            }
        }

        public async Task<ApiResponse<List<NotificacionDto>>> GetNotificacionesNoLeidasAsync(string usuarioId)
        {
            try
            {
                var notificaciones = await _context.Notificaciones
                    .Where(n => n.UsuarioId == usuarioId && !n.Leida)
                    .OrderByDescending(n => n.Prioridad)
                    .ThenByDescending(n => n.FechaCreacion)
                    .Take(50)
                    .Select(n => new NotificacionDto
                    {
                        Id = n.Id,
                        Tipo = n.Tipo,
                        Titulo = n.Titulo,
                        Mensaje = n.Mensaje,
                        URL_Accion = n.URL_Accion,
                        Leida = n.Leida,
                        Prioridad = n.Prioridad,
                        FechaCreacion = n.FechaCreacion,
                        TiempoTranscurrido = CalcularTiempoTranscurrido(n.FechaCreacion)
                    })
                    .ToListAsync();

                return ApiResponse<List<NotificacionDto>>.Ok(notificaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones");
                return ApiResponse<List<NotificacionDto>>.Fail("Error al obtener notificaciones");
            }
        }

        public async Task<ApiResponse<int>> GetConteoNoLeidasAsync(string usuarioId)
        {
            try
            {
                var conteo = await _context.Notificaciones
                    .CountAsync(n => n.UsuarioId == usuarioId && !n.Leida);

                return ApiResponse<int>.Ok(conteo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conteo de notificaciones");
                return ApiResponse<int>.Fail("Error al obtener conteo");
            }
        }

        private static string CalcularTiempoTranscurrido(DateTime fecha)
        {
            var tiempo = DateTime.UtcNow - fecha;

            if (tiempo.TotalMinutes < 1) return "Ahora mismo";
            if (tiempo.TotalMinutes < 60) return $"Hace {(int)tiempo.TotalMinutes} minutos";
            if (tiempo.TotalHours < 24) return $"Hace {(int)tiempo.TotalHours} horas";
            return $"Hace {(int)tiempo.TotalDays} días";
        }
    }
}