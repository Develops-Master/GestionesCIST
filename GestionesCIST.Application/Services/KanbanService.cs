using GestionesCIST.Application.DTOs.Analytics;
using GestionesCIST.Application.DTOs.Common;
using GestionesCIST.Application.DTOs.Kanban;
using GestionesCIST.Application.DTOs.Ordenes;
using GestionesCIST.Application.DTOs.Tecnicos;
using GestionesCIST.Application.Interfaces;
using GestionesCIST.Domain.Entities;
using GestionesCIST.Domain.Enums;
using GestionesCIST.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace GestionesCIST.Application.Services
{
    public class KanbanService : IKanbanService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<KanbanService> _logger;
        private readonly INotificacionService _notificacionService;

        public KanbanService(
            AppDbContext context,
            ILogger<KanbanService> logger,
            INotificacionService notificacionService)
        {
            _context = context;
            _logger = logger;
            _notificacionService = notificacionService;
        }

        // RF001 - Obtener tablero Kanban agrupado por estado
        public async Task<ApiResponse<KanbanResponseDto>> GetTableroKanbanAsync()
        {
            try
            {
                var ordenes = await _context.OrdenesServicio
                    .Include(o => o.TecnicoDiagnostico)
                        .ThenInclude(t => t!.User)
                    .Include(o => o.TecnicoReparacion)
                        .ThenInclude(t => t!.User)
                    .Include(o => o.Cliente)
                        .ThenInclude(c => c.User)
                    .Where(o => o.Estado != EstadoOrden.Almacen)
                    .OrderByDescending(o => o.Prioridad)
                    .ThenBy(o => o.FechaRecepcion)
                    .ToListAsync();

                var columnas = Enum.GetValues<EstadoOrden>()
                    .Where(e => e != EstadoOrden.Almacen)
                    .Select(estado => new ColumnaKanbanDto
                    {
                        Estado = estado,
                        Nombre = GetNombreEstado(estado),
                        ColorClase = GetColorClase(estado),
                        LimiteWIP = GetLimiteWIP(estado),
                        Tarjetas = ordenes
                            .Where(o => o.Estado == estado)
                            .Select(o => MapearATarjeta(o))
                            .ToList()
                    }).ToList();

                var conteoPorEstado = ordenes
                    .GroupBy(o => o.Estado)
                    .ToDictionary(g => g.Key, g => g.Count());

                var resumen = new ResumenKanbanDto
                {
                    TotalOrdenesActivas = ordenes.Count,
                    OrdenesRetrasadas = ordenes.Count(o => EstaRetrasada(o)),
                    OrdenesCriticas = ordenes.Count(o => o.Prioridad == PrioridadTicket.Critica),
                    TiempoPromedioCierreHoras = ordenes
                        .Where(o => o.FechaEntregaAlmacen.HasValue)
                        .Average(o => (o.FechaEntregaAlmacen!.Value - o.FechaRecepcion).TotalHours),
                    TecnicosDisponibles = await _context.Tecnicos
                        .CountAsync(t => t.Activo && t.Estado == "DISPONIBLE")
                };

                var response = new KanbanResponseDto
                {
                    Columnas = columnas,
                    ConteoPorEstado = conteoPorEstado,
                    Resumen = resumen
                };

                return ApiResponse<KanbanResponseDto>.Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tablero Kanban");
                return ApiResponse<KanbanResponseDto>.Fail("Error al obtener el tablero");
            }
        }

        // RF007 - Asignar técnico manualmente
        public async Task<ApiResponse<bool>> AsignarTecnicoManualAsync(int ordenId, int tecnicoId, string asignadoPor)
        {
            try
            {
                var orden = await _context.OrdenesServicio
                    .Include(o => o.Cliente)
                    .FirstOrDefaultAsync(o => o.Id == ordenId);

                var tecnico = await _context.Tecnicos
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == tecnicoId);

                if (orden == null)
                    return ApiResponse<bool>.Fail("Orden no encontrada");

                if (tecnico == null)
                    return ApiResponse<bool>.Fail("Técnico no encontrado");

                if (orden.Estado != EstadoOrden.Asignacion)
                    return ApiResponse<bool>.Fail("La orden ya fue asignada");

                if (!tecnico.Activo || tecnico.Estado != "DISPONIBLE")
                    return ApiResponse<bool>.Fail("El técnico no está disponible");

                // Validar carga del técnico (RF011)
                var cargaActual = await _context.OrdenesServicio
                    .CountAsync(o => (o.TecnicoDiagnosticoId == tecnicoId || o.TecnicoReparacionId == tecnicoId)
                        && o.Estado != EstadoOrden.Almacen);

                if (cargaActual >= tecnico.CargaMaximaDiaria)
                    return ApiResponse<bool>.Fail("El técnico ha alcanzado su carga máxima diaria");

                orden.TecnicoDiagnosticoId = tecnicoId;
                orden.Estado = EstadoOrden.Diagnostico;
                orden.FechaAsignacion = DateTime.UtcNow;
                orden.FechaInicioDiagnostico = DateTime.UtcNow;
                orden.ModificadoPor = asignadoPor;
                orden.UltimaModificacion = DateTime.UtcNow;

                tecnico.CargaActual = cargaActual + 1;

                await _context.SaveChangesAsync();

                // Notificar al técnico (RF005)
                await _notificacionService.EnviarNotificacionAsync(
                    tecnico.UserId,
                    "ORDEN_ASIGNADA",
                    "Nueva Orden Asignada",
                    $"Se te ha asignado la orden {orden.NumeroOrden} - {orden.Cliente?.User?.NombreCompleto}",
                    $"/ordenes/diagnostico/{orden.Id}",
                    2);

                _logger.LogInformation("Orden {NumeroOrden} asignada al técnico {TecnicoId} por {Usuario}",
                    orden.NumeroOrden, tecnicoId, asignadoPor);

                return ApiResponse<bool>.Ok(true, "Técnico asignado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar técnico manualmente");
                return ApiResponse<bool>.Fail("Error al asignar técnico");
            }
        }

        // RF018, RF019, RF021, RF022 - Mover orden entre estados
        public async Task<ApiResponse<bool>> MoverOrdenAsync(int ordenId, EstadoOrden nuevoEstado, string movidoPor, string? informe = null)
        {
            try
            {
                var orden = await _context.OrdenesServicio
                    .Include(o => o.Cliente)
                        .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(o => o.Id == ordenId);

                if (orden == null)
                    return ApiResponse<bool>.Fail("Orden no encontrada");

                var estadoAnterior = orden.Estado;

                // Validaciones de Reglas de Negocio (RN001 - RN006)
                var validacion = ValidarMovimiento(orden, nuevoEstado);
                if (!validacion.Success)
                    return validacion;

                orden.Estado = nuevoEstado;
                orden.ModificadoPor = movidoPor;
                orden.UltimaModificacion = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(informe))
                {
                    orden.DiagnosticoFinal = informe;
                }

                ActualizarFechasPorEstado(orden, estadoAnterior, nuevoEstado);

                // Si se mueve a pruebas, aplicar predicción IA
                if (nuevoEstado == EstadoOrden.Pruebas)
                {
                    await AplicarPrediccionIAAsync(orden);
                }

                // Si se mueve a almacén, liberar carga del técnico
                if (nuevoEstado == EstadoOrden.Almacen)
                {
                    await LiberarCargaTecnicoAsync(orden);

                    // Notificar al cliente (Equipo listo)
                    await _notificacionService.EnviarNotificacionAsync(
                        orden.Cliente!.UserId,
                        "EQUIPO_LISTO",
                        "¡Tu equipo está listo!",
                        $"El equipo de la orden {orden.NumeroOrden} está listo para ser recogido en nuestra sede.",
                        $"/cliente/mis-tickets",
                        3);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Orden {NumeroOrden} movida de {EstadoAnterior} a {NuevoEstado} por {Usuario}",
                    orden.NumeroOrden, estadoAnterior, nuevoEstado, movidoPor);

                return ApiResponse<bool>.Ok(true, $"Orden movida a {GetNombreEstado(nuevoEstado)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al mover orden {OrdenId}", ordenId);
                return ApiResponse<bool>.Fail("Error al mover la orden");
            }
        }

        // RF010, RF011, RF012 - Asignación automática
        public async Task<ApiResponse<int>> EjecutarAsignacionAutomaticaAsync()
        {
            try
            {
                var tiempoLimite = DateTime.UtcNow.AddHours(-2); // RF010: > 2 horas

                var ordenesPendientes = await _context.OrdenesServicio
                    .Include(o => o.Cliente)
                    .Where(o => o.Estado == EstadoOrden.Asignacion && o.FechaRecepcion <= tiempoLimite)
                    .OrderByDescending(o => o.Prioridad)
                    .ToListAsync();

                if (!ordenesPendientes.Any())
                    return ApiResponse<int>.Ok(0, "No hay órdenes pendientes para asignar");

                var tecnicos = await _context.Tecnicos
                    .Include(t => t.User)
                    .Where(t => t.Activo && t.Estado == "DISPONIBLE")
                    .ToListAsync();

                if (!tecnicos.Any())
                    return ApiResponse<int>.Fail("No hay técnicos disponibles");

                int asignadas = 0;
                var random = new Random();

                foreach (var orden in ordenesPendientes)
                {
                    // RF011: Calcular carga actual de cada técnico
                    var tecnicosConCarga = new List<(Tecnico Tecnico, int Carga)>();

                    foreach (var t in tecnicos)
                    {
                        var carga = await _context.OrdenesServicio
                            .CountAsync(o => (o.TecnicoDiagnosticoId == t.Id || o.TecnicoReparacionId == t.Id)
                                && o.Estado != EstadoOrden.Almacen);
                        tecnicosConCarga.Add((t, carga));
                    }

                    // Ordenar por carga (menor primero) y luego aleatorio (RF012)
                    var seleccionado = tecnicosConCarga
                        .Where(t => t.Carga < t.Tecnico.CargaMaximaDiaria)
                        .OrderBy(t => t.Carga)
                        .ThenBy(t => random.Next())
                        .FirstOrDefault();

                    if (seleccionado.Tecnico == null) continue;

                    orden.TecnicoDiagnosticoId = seleccionado.Tecnico.Id;
                    orden.Estado = EstadoOrden.Diagnostico;
                    orden.FechaAsignacion = DateTime.UtcNow;
                    orden.FechaInicioDiagnostico = DateTime.UtcNow;
                    orden.ModificadoPor = "SISTEMA_AUTOMATICO";
                    orden.UltimaModificacion = DateTime.UtcNow;

                    // Notificar al técnico
                    await _notificacionService.EnviarNotificacionAsync(
                        seleccionado.Tecnico.UserId,
                        "ORDEN_ASIGNADA",
                        "Nueva Orden Asignada (Automático)",
                        $"Se te ha asignado automáticamente la orden {orden.NumeroOrden}",
                        $"/ordenes/diagnostico/{orden.Id}",
                        2);

                    asignadas++;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Asignación automática completada. {Count} órdenes asignadas.", asignadas);

                return ApiResponse<int>.Ok(asignadas, $"{asignadas} órdenes asignadas automáticamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en asignación automática");
                return ApiResponse<int>.Fail("Error en asignación automática");
            }
        }

        // RF008 - Obtener técnicos disponibles
        public async Task<ApiResponse<List<TecnicoDisponibleDto>>> GetTecnicosDisponiblesAsync()
        {
            try
            {
                var tecnicos = await _context.Tecnicos
                    .Include(t => t.User)
                    .Include(t => t.Especialidades)
                        .ThenInclude(e => e.Especialidad)
                    .Where(t => t.Activo && t.Estado == "DISPONIBLE")
                    .Select(t => new TecnicoDisponibleDto
                    {
                        Id = t.Id,
                        CodigoTecnico = t.CodigoTecnico,
                        NombreCompleto = t.User.Nombres + " " + t.User.Apellidos,
                        Nivel = t.Nivel,
                        Especialidades = t.Especialidades.Select(e => e.Especialidad.Nombre).ToList(),
                        CargaActual = t.CargaActual,
                        CargaMaxima = t.CargaMaximaDiaria,
                        PorcentajeCarga = t.CargaMaximaDiaria > 0
                            ? (double)t.CargaActual / t.CargaMaximaDiaria * 100
                            : 0
                    })
                    .ToListAsync();

                return ApiResponse<List<TecnicoDisponibleDto>>.Ok(tecnicos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener técnicos disponibles");
                return ApiResponse<List<TecnicoDisponibleDto>>.Fail("Error al obtener técnicos");
            }
        }

        // RF009 - Obtener técnicos con órdenes asignadas
        public async Task<ApiResponse<List<TecnicoCargaDto>>> GetTecnicosConCargaAsync()
        {
            try
            {
                var tecnicos = await _context.Tecnicos
                    .Include(t => t.User)
                    .Where(t => t.Activo)
                    .Select(t => new TecnicoCargaDto
                    {
                        TecnicoId = t.Id,
                        Nombre = t.User.Nombres + " " + t.User.Apellidos,
                        Nivel = t.Nivel,
                        CargaActual = t.CargaActual,
                        CargaMaxima = t.CargaMaximaDiaria,
                        OrdenesActivas = _context.OrdenesServicio
                            .Count(o => (o.TecnicoDiagnosticoId == t.Id || o.TecnicoReparacionId == t.Id)
                                && o.Estado != EstadoOrden.Almacen)
                    })
                    .ToListAsync();

                foreach (var t in tecnicos)
                {
                    t.PorcentajeCarga = t.CargaMaxima > 0
                        ? (double)t.CargaActual / t.CargaMaxima * 100
                        : 0;
                }

                return ApiResponse<List<TecnicoCargaDto>>.Ok(tecnicos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener técnicos con carga");
                return ApiResponse<List<TecnicoCargaDto>>.Fail("Error al obtener técnicos");
            }
        }

        // Obtener detalle de orden
        public async Task<ApiResponse<OrdenServicioDto>> GetOrdenDetalleAsync(int ordenId)
        {
            try
            {
                var orden = await _context.OrdenesServicio
                    .Include(o => o.Cliente).ThenInclude(c => c!.User)
                    .Include(o => o.TecnicoDiagnostico).ThenInclude(t => t!.User)
                    .Include(o => o.TecnicoReparacion).ThenInclude(t => t!.User)
                    .Include(o => o.TesterQA).ThenInclude(t => t!.User)
                    .Include(o => o.Informes)
                    .Include(o => o.SolicitudesRepuesto).ThenInclude(s => s.Detalles)
                    .FirstOrDefaultAsync(o => o.Id == ordenId);

                if (orden == null)
                    return ApiResponse<OrdenServicioDto>.Fail("Orden no encontrada");

                var dto = new OrdenServicioDto
                {
                    Id = orden.Id,
                    NumeroOrden = orden.NumeroOrden,
                    Cliente = orden.Cliente?.User?.NombreCompleto ?? orden.Cliente?.RazonSocial ?? "N/A",
                    TipoEquipo = orden.TipoEquipo,
                    Marca = orden.Marca,
                    Modelo = orden.Modelo,
                    NumeroSerie = orden.NumeroSerie,
                    Estado = orden.Estado,
                    Prioridad = orden.Prioridad,
                    EsGarantia = orden.EsGarantia,
                    TecnicoDiagnostico = orden.TecnicoDiagnostico?.User?.NombreCompleto,
                    TecnicoReparacion = orden.TecnicoReparacion?.User?.NombreCompleto,
                    TesterQA = orden.TesterQA?.User?.NombreCompleto,
                    DiagnosticoFinal = orden.DiagnosticoFinal,
                    ProbabilidadExitoQA = orden.ProbabilidadExitoQA,
                    RepuestosSugeridosIA = orden.RepuestosSugeridosIA,
                    FechaRecepcion = orden.FechaRecepcion,
                    FechaEntregaAlmacen = orden.FechaEntregaAlmacen,
                    TiempoTotalHoras = orden.TiempoTotalHoras
                };

                return ApiResponse<OrdenServicioDto>.Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalle de orden {OrdenId}", ordenId);
                return ApiResponse<OrdenServicioDto>.Fail("Error al obtener detalle de orden");
            }
        }

        // RF014 - Registrar diagnóstico
        public async Task<ApiResponse<bool>> RegistrarDiagnosticoAsync(int ordenId, string diagnostico, int tecnicoId)
        {
            try
            {
                var orden = await _context.OrdenesServicio.FindAsync(ordenId);
                if (orden == null)
                    return ApiResponse<bool>.Fail("Orden no encontrada");

                if (orden.Estado != EstadoOrden.Diagnostico)
                    return ApiResponse<bool>.Fail("La orden no está en estado de diagnóstico");

                orden.DiagnosticoInicial = diagnostico;
                orden.UltimaModificacion = DateTime.UtcNow;

                // Crear informe técnico
                var informe = new InformeTecnico
                {
                    OrdenServicioId = ordenId,
                    TipoInforme = "DIAGNOSTICO",
                    Detalle = diagnostico,
                    TecnicoId = tecnicoId,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.InformesTecnicos.Add(informe);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Ok(true, "Diagnóstico registrado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar diagnóstico para orden {OrdenId}", ordenId);
                return ApiResponse<bool>.Fail("Error al registrar diagnóstico");
            }
        }

        // RF017 - Registrar informe técnico
        public async Task<ApiResponse<bool>> RegistrarInformeTecnicoAsync(int ordenId, string informe, int tecnicoId)
        {
            try
            {
                var orden = await _context.OrdenesServicio.FindAsync(ordenId);
                if (orden == null)
                    return ApiResponse<bool>.Fail("Orden no encontrada");

                orden.DiagnosticoFinal = informe;
                orden.UltimaModificacion = DateTime.UtcNow;

                var informeTecnico = new InformeTecnico
                {
                    OrdenServicioId = ordenId,
                    TipoInforme = orden.Estado == EstadoOrden.Diagnostico ? "DIAGNOSTICO" : "REPARACION",
                    Detalle = informe,
                    TecnicoId = tecnicoId,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.InformesTecnicos.Add(informeTecnico);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Ok(true, "Informe registrado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar informe para orden {OrdenId}", ordenId);
                return ApiResponse<bool>.Fail("Error al registrar informe");
            }
        }

        #region Métodos Privados

        private ApiResponse<bool> ValidarMovimiento(OrdenServicio orden, EstadoOrden nuevoEstado)
        {
            // RN003: Control de Calidad
            if (nuevoEstado == EstadoOrden.Pruebas && orden.Estado != EstadoOrden.Reparacion)
                return ApiResponse<bool>.Fail("El equipo debe estar reparado antes de enviar a pruebas.");

            // RN005: Toda revisión debe pasar por QA
            if (nuevoEstado == EstadoOrden.Almacen && orden.Estado != EstadoOrden.Pruebas)
                return ApiResponse<bool>.Fail("El equipo debe pasar control de calidad antes de ir a almacén.");

            // RN006: Devolución requiere informe
            if (nuevoEstado == EstadoOrden.Almacen && string.IsNullOrEmpty(orden.DiagnosticoFinal))
                return ApiResponse<bool>.Fail("Se requiere un informe técnico para cerrar la orden.");

            return ApiResponse<bool>.Ok(true);
        }

        private void ActualizarFechasPorEstado(OrdenServicio orden, EstadoOrden estadoAnterior, EstadoOrden nuevoEstado)
        {
            switch (nuevoEstado)
            {
                case EstadoOrden.Diagnostico:
                    orden.FechaInicioDiagnostico ??= DateTime.UtcNow;
                    break;
                case EstadoOrden.Reparacion:
                    orden.FechaFinDiagnostico = DateTime.UtcNow;
                    orden.FechaInicioReparacion = DateTime.UtcNow;
                    break;
                case EstadoOrden.Pruebas:
                    orden.FechaFinReparacion = DateTime.UtcNow;
                    orden.FechaEnvioPruebas = DateTime.UtcNow;
                    break;
                case EstadoOrden.Almacen:
                    orden.FechaFinPruebas = DateTime.UtcNow;
                    orden.FechaEntregaAlmacen = DateTime.UtcNow;
                    break;
            }
        }

        private async Task AplicarPrediccionIAAsync(OrdenServicio orden)
        {
            // Simulación de predicción IA
            var random = new Random();
            orden.ProbabilidadExitoQA = (decimal)(0.5 + random.NextDouble() * 0.4);

            if (orden.ProbabilidadExitoQA < 0.6m)
            {
                await _notificacionService.NotificarJefesSoporteAsync(
                    "⚠️ ALERTA IA: Riesgo de fallo QA",
                    $"Orden {orden.NumeroOrden} tiene baja probabilidad de éxito en QA ({orden.ProbabilidadExitoQA:P0})");
            }
        }

        private async Task LiberarCargaTecnicoAsync(OrdenServicio orden)
        {
            if (orden.TecnicoReparacionId.HasValue)
            {
                var tecnico = await _context.Tecnicos.FindAsync(orden.TecnicoReparacionId.Value);
                if (tecnico != null && tecnico.CargaActual > 0)
                {
                    tecnico.CargaActual--;
                    tecnico.TotalOrdenesCompletadas++;
                }
            }
        }

        private TarjetaKanbanDto MapearATarjeta(OrdenServicio orden)
        {
            return new TarjetaKanbanDto
            {
                Id = orden.Id,
                NumeroOrden = orden.NumeroOrden,
                Cliente = orden.Cliente?.User?.NombreCompleto ?? orden.Cliente?.RazonSocial ?? "N/A",
                Marca = orden.Marca,
                Modelo = orden.Modelo,
                TecnicoAsignado = orden.TecnicoDiagnostico?.User?.NombreCompleto
                    ?? orden.TecnicoReparacion?.User?.NombreCompleto,
                Prioridad = orden.Prioridad,
                EsGarantia = orden.EsGarantia,
                ColorEtiqueta = orden.ColorEtiqueta,
                ProbabilidadExitoQA = orden.ProbabilidadExitoQA,
                TiempoEnEstado = CalcularTiempoEnEstado(orden),
                EstaRetrasada = EstaRetrasada(orden),
                AlertaIA = orden.ProbabilidadExitoQA < 0.6m ? "Riesgo de fallo QA" : null
            };
        }

        private string CalcularTiempoEnEstado(OrdenServicio orden)
        {
            DateTime? fechaInicio = orden.Estado switch
            {
                EstadoOrden.Asignacion => orden.FechaRecepcion,
                EstadoOrden.Diagnostico => orden.FechaInicioDiagnostico,
                EstadoOrden.Reparacion => orden.FechaInicioReparacion,
                EstadoOrden.Pruebas => orden.FechaEnvioPruebas,
                _ => null
            };

            if (!fechaInicio.HasValue) return "N/A";

            var tiempo = DateTime.UtcNow - fechaInicio.Value;

            if (tiempo.TotalHours < 1) return $"{(int)tiempo.TotalMinutes} min";
            if (tiempo.TotalHours < 24) return $"{(int)tiempo.TotalHours}h {tiempo.Minutes}m";
            return $"{(int)tiempo.TotalDays}d {tiempo.Hours}h";
        }

        private bool EstaRetrasada(OrdenServicio orden)
        {
            var horasTranscurridas = (DateTime.UtcNow - orden.FechaRecepcion).TotalHours;

            return orden.Estado switch
            {
                EstadoOrden.Asignacion => horasTranscurridas > 2, // RF010: > 2 horas
                EstadoOrden.Diagnostico => horasTranscurridas > 24,
                EstadoOrden.Reparacion => horasTranscurridas > 72,
                EstadoOrden.Pruebas => horasTranscurridas > 96,
                _ => false
            };
        }

        private string GetNombreEstado(EstadoOrden estado) => estado switch
        {
            EstadoOrden.Asignacion => "Asignación",
            EstadoOrden.Diagnostico => "Diagnóstico",
            EstadoOrden.Reparacion => "Reparación",
            EstadoOrden.Pruebas => "Pruebas QA",
            EstadoOrden.Almacen => "Almacén",
            _ => estado.ToString()
        };

        private string GetColorClase(EstadoOrden estado) => estado switch
        {
            EstadoOrden.Asignacion => "bg-primary",
            EstadoOrden.Diagnostico => "bg-info",
            EstadoOrden.Reparacion => "bg-warning",
            EstadoOrden.Pruebas => "bg-success",
            EstadoOrden.Almacen => "bg-secondary",
            _ => "bg-light"
        };

        private int GetLimiteWIP(EstadoOrden estado) => estado switch
        {
            EstadoOrden.Asignacion => 20,
            EstadoOrden.Diagnostico => 10,
            EstadoOrden.Reparacion => 8,
            EstadoOrden.Pruebas => 6,
            EstadoOrden.Almacen => 30,
            _ => 10
        };

        #endregion
    }
}