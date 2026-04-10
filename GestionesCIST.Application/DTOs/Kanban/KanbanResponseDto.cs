using GestionesCIST.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GestionesCIST.Application.DTOs.Kanban
{
    public class KanbanResponseDto
    {
        public List<ColumnaKanbanDto> Columnas { get; set; } = new();
        public Dictionary<EstadoOrden, int> ConteoPorEstado { get; set; } = new();
        public ResumenKanbanDto Resumen { get; set; } = new();
    }

    public class ColumnaKanbanDto
    {
        public EstadoOrden Estado { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string ColorClase { get; set; } = string.Empty;
        public List<TarjetaKanbanDto> Tarjetas { get; set; } = new();
        public int LimiteWIP { get; set; } // Work In Progress Limit
    }

    public class TarjetaKanbanDto
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string? TecnicoAsignado { get; set; }
        public PrioridadTicket Prioridad { get; set; }
        public bool EsGarantia { get; set; }
        public string? ColorEtiqueta { get; set; }
        public decimal? ProbabilidadExitoQA { get; set; }
        public string TiempoEnEstado { get; set; } = string.Empty;
        public bool EstaRetrasada { get; set; }
        public string? AlertaIA { get; set; }
    }

    public class ResumenKanbanDto
    {
        public int TotalOrdenesActivas { get; set; }
        public int OrdenesRetrasadas { get; set; }
        public int OrdenesCriticas { get; set; }
        public double TiempoPromedioCierreHoras { get; set; }
        public int TecnicosDisponibles { get; set; }
    }

    public class MoverOrdenDto
    {
        [Required]
        public EstadoOrden NuevoEstado { get; set; }

        public string? Informe { get; set; }
        public string? Diagnostico { get; set; }
    }

    public class AsignarTecnicoDto
    {
        [Required]
        public int TecnicoId { get; set; }

        public string? Observacion { get; set; }
    }
}