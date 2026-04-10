using GestionesCIST.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GestionesCIST.Application.DTOs.Tickets
{
    public class CrearTicketDto
    {
        [Required(ErrorMessage = "El tipo de equipo es requerido")]
        public string TipoEquipo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La marca es requerida")]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "El modelo es requerido")]
        public string Modelo { get; set; } = string.Empty;

        public string? NumeroSerie { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción del problema es requerida")]
        public string DescripcionProblema { get; set; } = string.Empty;

        public string? ContactoAlternativo { get; set; }
        public IFormFile? ArchivoAdjunto { get; set; }
    }

    public class TicketResponseDto
    {
        public int Id { get; set; }
        public string CodigoTicket { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string TipoEquipo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string DescripcionProblema { get; set; } = string.Empty;
        public EstadoTicket Estado { get; set; }
        public PrioridadTicket Prioridad { get; set; }
        public string? CategoriaSugerida { get; set; }
        public decimal? ConfianzaPrediccion { get; set; }
        public decimal? TiempoEstimadoIA_Horas { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaCierre { get; set; }
        public string? TecnicoAsignado { get; set; }
        public List<ComentarioTicketDto> Comentarios { get; set; } = new();
        public string? EstadoSLA { get; set; }
    }

    public class ComentarioTicketDto
    {
        public int Id { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Comentario { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public bool EsInterno { get; set; }
    }

    public class AgregarComentarioDto
    {
        [Required(ErrorMessage = "El comentario es requerido")]
        public string Comentario { get; set; } = string.Empty;

        public bool EsInterno { get; set; }
    }

    public class TicketClienteDto
    {
        public string CodigoTicket { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string EstadoKanban { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public string? UltimoComentario { get; set; }
        public int Progreso { get; set; }
    }

    public class ClasificacionIADto
    {
        public string Categoria { get; set; } = string.Empty;
        public PrioridadTicket Prioridad { get; set; }
        public decimal Confianza { get; set; }
        public decimal TiempoEstimadoHoras { get; set; }
    }
}