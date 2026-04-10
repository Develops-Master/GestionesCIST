using GestionesCIST.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GestionesCIST.Application.DTOs.Ordenes
{
    public class OrdenServicioDto
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string TipoEquipo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public EstadoOrden Estado { get; set; }
        public PrioridadTicket Prioridad { get; set; }
        public bool EsGarantia { get; set; }
        public string? TecnicoDiagnostico { get; set; }
        public string? TecnicoReparacion { get; set; }
        public string? TesterQA { get; set; }
        public string? DiagnosticoFinal { get; set; }
        public decimal? ProbabilidadExitoQA { get; set; }
        public string? RepuestosSugeridosIA { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public DateTime? FechaEntregaAlmacen { get; set; }
        public int? TiempoTotalHoras { get; set; }
        public List<InformeTecnicoDto> Informes { get; set; } = new();
        public List<SolicitudRepuestoDto> SolicitudesRepuesto { get; set; } = new();
    }

    public class InformeTecnicoDto
    {
        public int Id { get; set; }
        public string TipoInforme { get; set; } = string.Empty;
        public string Resumen { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public string Tecnico { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }

    public class SolicitudRepuestoDto
    {
        public int Id { get; set; }
        public string NumeroSolicitud { get; set; } = string.Empty;
        public EstadoSolicitudRepuesto Estado { get; set; }
        public bool Urgente { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public List<SolicitudRepuestoDetalleDto> Detalles { get; set; } = new();
    }

    public class SolicitudRepuestoDetalleDto
    {
        public int RepuestoId { get; set; }
        public string Repuesto { get; set; } = string.Empty;
        public int CantidadSolicitada { get; set; }
        public int CantidadEntregada { get; set; }
        public decimal? PrecioUnitario { get; set; }
    }

    public class CrearOrdenDesdeTicketDto
    {
        [Required]
        public int TicketId { get; set; }

        [Required]
        public PrioridadTicket Prioridad { get; set; } = PrioridadTicket.Media;

        public string? Observaciones { get; set; }
    }

    public class RegistrarDiagnosticoDto
    {
        [Required]
        public string Diagnostico { get; set; } = string.Empty;

        public bool RequiereRepuestos { get; set; }

        public List<SolicitarRepuestoDto>? Repuestos { get; set; }
    }

    public class SolicitarRepuestoDto
    {
        public int RepuestoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class RegistrarInformeTecnicoDto
    {
        [Required]
        public string Detalle { get; set; } = string.Empty;

        public string? Recomendaciones { get; set; }

        [Required]
        public string EstadoFinal { get; set; } = string.Empty;
    }
}