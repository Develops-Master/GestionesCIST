using GestionesCIST.Domain.Enums;

namespace GestionesCIST.WebMVC.Models.ViewModels
{
    public class KanbanViewModel
    {
        public List<ColumnaKanbanViewModel> Columnas { get; set; } = new();
        public ResumenKanbanViewModel Resumen { get; set; } = new();
    }

    public class ColumnaKanbanViewModel
    {
        public EstadoOrden Estado { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string ColorClase { get; set; } = string.Empty;
        public int LimiteWIP { get; set; }
        public List<TarjetaKanbanViewModel> Tarjetas { get; set; } = new();
    }

    public class TarjetaKanbanViewModel
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string? TecnicoAsignado { get; set; }
        public string Prioridad { get; set; } = string.Empty;
        public bool EsGarantia { get; set; }
        public string? ColorEtiqueta { get; set; }
        public decimal? ProbabilidadExitoQA { get; set; }
        public string TiempoEnEstado { get; set; } = string.Empty;
        public bool EstaRetrasada { get; set; }
        public string? AlertaIA { get; set; }
    }

    public class ResumenKanbanViewModel
    {
        public int TotalOrdenesActivas { get; set; }
        public int OrdenesRetrasadas { get; set; }
        public int OrdenesCriticas { get; set; }
        public double TiempoPromedioCierreHoras { get; set; }
        public int TecnicosDisponibles { get; set; }
    }

    public class OrdenDetalleViewModel
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string TipoEquipo { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string NumeroSerie { get; set; } = string.Empty;
        public EstadoOrden Estado { get; set; }
        public string Prioridad { get; set; } = string.Empty;
        public bool EsGarantia { get; set; }
        public string? TecnicoDiagnostico { get; set; }
        public string? TecnicoReparacion { get; set; }
        public string? TesterQA { get; set; }
        public string? DiagnosticoInicial { get; set; }
        public string? DiagnosticoFinal { get; set; }
        public decimal? ProbabilidadExitoQA { get; set; }
        public string? RepuestosSugeridosIA { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public DateTime? FechaEntregaAlmacen { get; set; }
        public int? TiempoTotalHoras { get; set; }
        public List<InformeTecnicoViewModel> Informes { get; set; } = new();
    }

    public class InformeTecnicoViewModel
    {
        public int Id { get; set; }
        public string TipoInforme { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public string Tecnico { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }
}