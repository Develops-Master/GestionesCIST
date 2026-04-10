using GestionesCIST.Application.DTOs.Common;
using GestionesCIST.Application.DTOs.Ordenes;
using GestionesCIST.Application.DTOs.Tickets;
using GestionesCIST.Application.Interfaces;
using GestionesCIST.Domain.Entities;
using GestionesCIST.Domain.Enums;
using GestionesCIST.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace GestionesCIST.Application.Services
{
    public class TicketService : ITicketService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TicketService> _logger;
        private readonly INotificacionService _notificacionService;
        private readonly IFileStorageService? _fileStorageService;

        public TicketService(
            AppDbContext context,
            ILogger<TicketService> logger,
            INotificacionService notificacionService,
            IFileStorageService? fileStorageService = null)
        {
            _context = context;
            _logger = logger;
            _notificacionService = notificacionService;
            _fileStorageService = fileStorageService;
        }

        public async Task<ApiResponse<TicketResponseDto>> CrearTicketAsync(CrearTicketDto dto, string clienteId)
        {
            try
            {
                var cliente = await _context.Clientes
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.UserId == clienteId);

                if (cliente == null)
                    return ApiResponse<TicketResponseDto>.Fail("Cliente no encontrado");

                // Generar código de ticket
                var codigo = $"TKT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

                // Clasificar con IA (simulado)
                var clasificacion = await ClasificarTicketConIAAsync(dto.DescripcionProblema);

                var ticket = new Ticket
                {
                    CodigoTicket = codigo,
                    ClienteId = cliente.Id,
                    TipoEquipo = dto.TipoEquipo,
                    Marca = dto.Marca,
                    Modelo = dto.Modelo,
                    NumeroSerie = dto.NumeroSerie,
                    Titulo = dto.Titulo,
                    DescripcionProblema = dto.DescripcionProblema,
                    ContactoAlternativo = dto.ContactoAlternativo,
                    Prioridad = clasificacion.Data?.Prioridad ?? PrioridadTicket.Media,
                    CategoriaSugeridaId = await ObtenerCategoriaIdPorNombre(clasificacion.Data?.Categoria),
                    ConfianzaPrediccion = clasificacion.Data?.Confianza,
                    TiempoEstimadoIA_Horas = clasificacion.Data?.TiempoEstimadoHoras,
                    Estado = EstadoTicket.Abierto,
                    Origen = "PORTAL_WEB",
                    CreadoPor = clienteId,
                    FechaCreacion = DateTime.UtcNow,
                    FechaUltimaActualizacion = DateTime.UtcNow
                };

                // Calcular SLA basado en configuración del cliente
                ticket.FechaObjetivoRespuesta = DateTime.UtcNow.AddHours(cliente.SLA_TiempoRespuestaHoras);
                ticket.FechaObjetivoSolucion = DateTime.UtcNow.AddDays(cliente.SLA_TiempoSolucionDias);

                // Guardar archivo adjunto si existe
                if (dto.ArchivoAdjunto != null && _fileStorageService != null)
                {
                    var ruta = await _fileStorageService.SaveFileAsync(dto.ArchivoAdjunto, "tickets");
                    ticket.RutaArchivos = ruta;
                }

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                // Notificar a jefes de soporte
                await _notificacionService.NotificarJefesSoporteAsync(
                    "🎫 Nuevo Ticket Creado",
                    $"Ticket {codigo} - {dto.Titulo} - Cliente: {cliente.User?.NombreCompleto}");

                _logger.LogInformation("Ticket {Codigo} creado por cliente {ClienteId}", codigo, clienteId);

                var response = await MapearATicketResponseDto(ticket);
                return ApiResponse<TicketResponseDto>.Ok(response, "Ticket creado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ticket");
                return ApiResponse<TicketResponseDto>.Fail("Error al crear ticket");
            }
        }

        public async Task<ApiResponse<TicketResponseDto>> GetTicketPorIdAsync(int id)
        {
            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.Cliente).ThenInclude(c => c!.User)
                    .Include(t => t.CategoriaSugerida)
                    .Include(t => t.Comentarios).ThenInclude(c => c.Usuario)
                    .Include(t => t.Asignaciones).ThenInclude(a => a.Tecnico).ThenInclude(t => t!.User)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (ticket == null)
                    return ApiResponse<TicketResponseDto>.Fail("Ticket no encontrado");

                var response = await MapearATicketResponseDto(ticket);
                return ApiResponse<TicketResponseDto>.Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ticket {TicketId}", id);
                return ApiResponse<TicketResponseDto>.Fail("Error al obtener ticket");
            }
        }

        public async Task<ApiResponse<PagedResult<TicketResponseDto>>> GetTicketsPaginadosAsync(
            int pageNumber, int pageSize, string? estado = null)
        {
            try
            {
                var query = _context.Tickets
                    .Include(t => t.Cliente).ThenInclude(c => c!.User)
                    .Include(t => t.CategoriaSugerida)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoTicket>(estado, true, out var estadoEnum))
                {
                    query = query.Where(t => t.Estado == estadoEnum);
                }

                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                var tickets = await query
                    .OrderByDescending(t => t.Prioridad)
                    .ThenBy(t => t.FechaCreacion)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var data = new List<TicketResponseDto>();
                foreach (var ticket in tickets)
                {
                    data.Add(await MapearATicketResponseDto(ticket));
                }

                var result = new PagedResult<TicketResponseDto>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords,
                    Data = data
                };

                return ApiResponse<PagedResult<TicketResponseDto>>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tickets paginados");
                return ApiResponse<PagedResult<TicketResponseDto>>.Fail("Error al obtener tickets");
            }
        }

        public async Task<ApiResponse<List<TicketClienteDto>>> GetTicketsPorClienteAsync(string clienteId)
        {
            try
            {
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == clienteId);
                if (cliente == null)
                    return ApiResponse<List<TicketClienteDto>>.Fail("Cliente no encontrado");

                var tickets = await _context.Tickets
                    .Include(t => t.OrdenServicio)
                    .Include(t => t.Comentarios)
                    .Where(t => t.ClienteId == cliente.Id)
                    .OrderByDescending(t => t.FechaCreacion)
                    .Select(t => new TicketClienteDto
                    {
                        CodigoTicket = t.CodigoTicket,
                        Estado = t.Estado.ToString(),
                        EstadoKanban = t.OrdenServicio != null ? t.OrdenServicio.Estado.ToString() : "Pendiente Asignación",
                        FechaCreacion = t.FechaCreacion,
                        UltimoComentario = t.Comentarios.OrderByDescending(c => c.FechaCreacion)
                            .FirstOrDefault(c => !c.EsInterno)!.Comentario,
                        Progreso = CalcularProgreso(t)
                    })
                    .ToListAsync();

                return ApiResponse<List<TicketClienteDto>>.Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tickets del cliente {ClienteId}", clienteId);
                return ApiResponse<List<TicketClienteDto>>.Fail("Error al obtener tickets");
            }
        }

        public async Task<ApiResponse<List<TicketResponseDto>>> GetTicketsPendientesAsignacionAsync()
        {
            try
            {
                var tickets = await _context.Tickets
                    .Include(t => t.Cliente).ThenInclude(c => c!.User)
                    .Where(t => t.Estado == EstadoTicket.Abierto)
                    .OrderByDescending(t => t.Prioridad)
                    .ThenBy(t => t.FechaCreacion)
                    .ToListAsync();

                var data = new List<TicketResponseDto>();
                foreach (var ticket in tickets)
                {
                    data.add(await MapearATicketResponseDto(ticket));
                }

                return ApiResponse<List<TicketResponseDto>>.Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tickets pendientes");
                return ApiResponse<List<TicketResponseDto>>.Fail("Error al obtener tickets pendientes");
            }
        }

        public async Task<ApiResponse<bool>> AsignarTicketAsync(int ticketId, int tecnicoId, string asignadoPor)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                var tecnico = await _context.Tecnicos.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == tecnicoId);

                if (ticket == null) return ApiResponse<bool>.Fail("Ticket no encontrado");
                if (tecnico == null) return ApiResponse<bool>.Fail("Técnico no encontrado");

                var asignacion = new TicketAsignacion
                {
                    TicketId = ticketId,
                    TecnicoId = tecnicoId,
                    AsignadoPor = asignadoPor,
                    FechaAsignacion = DateTime.UtcNow,
                    Activo = true
                };

                ticket.Estado = EstadoTicket.Asignado;
                ticket.FechaAsignacion = DateTime.UtcNow;
                ticket.FechaUltimaActualizacion = DateTime.UtcNow;

                _context.TicketAsignaciones.Add(asignacion);

                // Agregar comentario automático
                _context.ComentariosTicket.Add(new ComentarioTicket
                {
                    TicketId = ticketId,
                    UsuarioId = asignadoPor,
                    Comentario = $"Ticket asignado al técnico {tecnico.User?.NombreCompleto}",
                    EsInterno = true,
                    FechaCreacion = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                // Notificar al técnico
                await _notificacionService.EnviarNotificacionAsync(
                    tecnico.UserId,
                    "TICKET_ASIGNADO",
                    "Nuevo Ticket Asignado",
                    $"Se te ha asignado el ticket {ticket.CodigoTicket}",
                    $"/tickets/detalle/{ticketId}",
                    2);

                return ApiResponse<bool>.Ok(true, "Ticket asignado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar ticket {TicketId}", ticketId);
                return ApiResponse<bool>.Fail("Error al asignar ticket");
            }
        }

        public async Task<ApiResponse<bool>> CambiarEstadoTicketAsync(int ticketId, string nuevoEstado, string usuarioId)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket == null) return ApiResponse<bool>.Fail("Ticket no encontrado");

                if (!Enum.TryParse<EstadoTicket>(nuevoEstado, true, out var estadoEnum))
                    return ApiResponse<bool>.Fail("Estado no válido");

                ticket.Estado = estadoEnum;
                ticket.FechaUltimaActualizacion = DateTime.UtcNow;

                if (estadoEnum == EstadoTicket.Cerrado || estadoEnum == EstadoTicket.Resuelto)
                {
                    ticket.FechaCierre = DateTime.UtcNow;
                    ticket.CumpleSLA = DateTime.UtcNow <= ticket.FechaObjetivoSolucion;
                }

                // Agregar comentario de cambio de estado
                _context.ComentariosTicket.Add(new ComentarioTicket
                {
                    TicketId = ticketId,
                    UsuarioId = usuarioId,
                    Comentario = $"Estado cambiado a: {estadoEnum}",
                    EsInterno = true,
                    FechaCreacion = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Ok(true, "Estado actualizado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado del ticket {TicketId}", ticketId);
                return ApiResponse<bool>.Fail("Error al cambiar estado");
            }
        }

        public async Task<ApiResponse<bool>> AgregarComentarioAsync(int ticketId, AgregarComentarioDto dto, string usuarioId)
        {
            try
            {
                var comentario = new ComentarioTicket
                {
                    TicketId = ticketId,
                    UsuarioId = usuarioId,
                    Comentario = dto.Comentario,
                    EsInterno = dto.EsInterno,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.ComentariosTicket.Add(comentario);

                var ticket = await _context.Tickets.FindAsync(ticketId);
                if (ticket != null)
                {
                    ticket.FechaUltimaActualizacion = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Ok(true, "Comentario agregado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar comentario al ticket {TicketId}", ticketId);
                return ApiResponse<bool>.Fail("Error al agregar comentario");
            }
        }

        public async Task<ApiResponse<ClasificacionIADto>> ClasificarTicketConIAAsync(string descripcion)
        {
            try
            {
                // Simulación de clasificación IA
                // En producción, esto llamaría al servicio ML.NET
                var random = new Random();
                var categorias = new[] { "Hardware", "Software", "Redes", "Impresora", "Servidor" };
                var categoria = categorias[random.Next(categorias.Length)];

                var tiemposEstimados = new Dictionary<string, decimal>
                {
                    ["Hardware"] = 6,
                    ["Software"] = 4,
                    ["Redes"] = 3,
                    ["Impresora"] = 2,
                    ["Servidor"] = 8
                };

                var resultado = new ClasificacionIADto
                {
                    Categoria = categoria,
                    Prioridad = DeterminarPrioridad(descripcion),
                    Confianza = (decimal)(0.7 + random.NextDouble() * 0.25),
                    TiempoEstimadoHoras = tiemposEstimados.GetValueOrDefault(categoria, 4)
                };

                return ApiResponse<ClasificacionIADto>.Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en clasificación IA");
                return ApiResponse<ClasificacionIADto>.Fail("Error en clasificación IA");
            }
        }

        public async Task<ApiResponse<OrdenServicioDto>> ConvertirTicketAOrdenAsync(int ticketId, string usuarioId)
        {
            try
            {
                var ticket = await _context.Tickets
                    .Include(t => t.Cliente)
                    .FirstOrDefaultAsync(t => t.Id == ticketId);

                if (ticket == null)
                    return ApiResponse<OrdenServicioDto>.Fail("Ticket no encontrado");

                if (ticket.OrdenServicioId.HasValue)
                    return ApiResponse<OrdenServicioDto>.Fail("Este ticket ya fue convertido a orden");

                // Generar número de orden
                var numeroOrden = $"OS-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

                var orden = new OrdenServicio
                {
                    NumeroOrden = numeroOrden,
                    TicketId = ticketId,
                    ClienteId = ticket.ClienteId,
                    TipoEquipo = ticket.TipoEquipo,
                    Marca = ticket.Marca,
                    Modelo = ticket.Modelo,
                    NumeroSerie = ticket.NumeroSerie ?? "N/A",
                    ProblemaReportado = ticket.DescripcionProblema,
                    Prioridad = ticket.Prioridad,
                    Estado = EstadoOrden.Asignacion,
                    FechaRecepcion = DateTime.UtcNow,
                    CreadoPor = usuarioId,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.OrdenesServicio.Add(orden);
                await _context.SaveChangesAsync();

                // Actualizar ticket
                ticket.OrdenServicioId = orden.Id;
                ticket.Estado = EstadoTicket.EnProgreso;
                ticket.FechaUltimaActualizacion = DateTime.UtcNow;

                // Agregar comentario
                _context.ComentariosTicket.Add(new ComentarioTicket
                {
                    TicketId = ticketId,
                    UsuarioId = usuarioId,
                    Comentario = $"Ticket convertido a orden de servicio {numeroOrden}",
                    EsInterno = false,
                    FechaCreacion = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                // Notificar al cliente
                await _notificacionService.EnviarNotificacionAsync(
                    ticket.Cliente!.UserId,
                    "TICKET_CONVERTIDO",
                    "Ticket en Proceso",
                    $"Tu ticket {ticket.CodigoTicket} ha sido convertido a orden de servicio {numeroOrden}",
                    $"/cliente/seguimiento/{ticket.CodigoTicket}",
                    2);

                var response = new OrdenServicioDto
                {
                    Id = orden.Id,
                    NumeroOrden = orden.NumeroOrden,
                    Cliente = ticket.Cliente?.User?.NombreCompleto ?? "N/A",
                    Estado = orden.Estado,
                    Prioridad = orden.Prioridad
                };

                return ApiResponse<OrdenServicioDto>.Ok(response, "Ticket convertido exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir ticket {TicketId} a orden", ticketId);
                return ApiResponse<OrdenServicioDto>.Fail("Error al convertir ticket");
            }
        }

        #region Métodos Privados

        private async Task<TicketResponseDto> MapearATicketResponseDto(Ticket ticket)
        {
            var tecnicoAsignado = ticket.Asignaciones
                .FirstOrDefault(a => a.Activo)?.Tecnico?.User?.NombreCompleto;

            return new TicketResponseDto
            {
                Id = ticket.Id,
                CodigoTicket = ticket.CodigoTicket,
                Cliente = ticket.Cliente?.User?.NombreCompleto ?? ticket.Cliente?.RazonSocial ?? "N/A",
                TipoEquipo = ticket.TipoEquipo,
                Marca = ticket.Marca,
                Modelo = ticket.Modelo,
                Titulo = ticket.Titulo,
                DescripcionProblema = ticket.DescripcionProblema,
                Estado = ticket.Estado,
                Prioridad = ticket.Prioridad,
                CategoriaSugerida = ticket.CategoriaSugerida?.Nombre,
                ConfianzaPrediccion = ticket.ConfianzaPrediccion,
                TiempoEstimadoIA_Horas = ticket.TiempoEstimadoIA_Horas,
                FechaCreacion = ticket.FechaCreacion,
                FechaCierre = ticket.FechaCierre,
                TecnicoAsignado = tecnicoAsignado,
                EstadoSLA = CalcularEstadoSLA(ticket)
            };
        }

        private string CalcularEstadoSLA(Ticket ticket)
        {
            if (ticket.Estado == EstadoTicket.Cerrado || ticket.Estado == EstadoTicket.Resuelto)
                return ticket.CumpleSLA == true ? "Cumplido" : "Incumplido";

            if (!ticket.FechaObjetivoSolucion.HasValue)
                return "Sin SLA";

            return DateTime.UtcNow > ticket.FechaObjetivoSolucion ? "Vencido" : "En plazo";
        }

        private int CalcularProgreso(Ticket ticket)
        {
            return ticket.Estado switch
            {
                EstadoTicket.Abierto => 10,
                EstadoTicket.Asignado => 25,
                EstadoTicket.EnProgreso => 50,
                EstadoTicket.PendienteCliente => 70,
                EstadoTicket.Resuelto => 90,
                EstadoTicket.Cerrado => 100,
                _ => 0
            };
        }

        private PrioridadTicket DeterminarPrioridad(string descripcion)
        {
            var texto = descripcion.ToLower();

            if (texto.Contains("urgente") || texto.Contains("crítico") || texto.Contains("no enciende") || texto.Contains("caído"))
                return PrioridadTicket.Critica;

            if (texto.Contains("lento") || texto.Contains("error") || texto.Contains("falla"))
                return PrioridadTicket.Alta;

            if (texto.Contains("consulta") || texto.Contains("pregunta"))
                return PrioridadTicket.Baja;

            return PrioridadTicket.Media;
        }

        private async Task<int?> ObtenerCategoriaIdPorNombre(string? categoria)
        {
            if (string.IsNullOrEmpty(categoria)) return null;

            var cat = await _context.CategoriasTicket
                .FirstOrDefaultAsync(c => c.Nombre.Contains(categoria));

            return cat?.Id;
        }

        #endregion
    }
}