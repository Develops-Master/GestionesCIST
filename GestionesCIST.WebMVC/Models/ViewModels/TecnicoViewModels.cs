using System.ComponentModel.DataAnnotations;

nnamespace GestionesCIST.WebMVC.Models.ViewModels
{
    public class MisOrdenesViewModel
{
    public List<OrdenAsignadaViewModel> OrdenesDiagnostico { get; set; } = new();
    public List<OrdenAsignadaViewModel> OrdenesReparacion { get; set; } = new();
    public int CargaActual { get; set; }
    public int CargaMaxima { get; set; }
    public double PorcentajeCarga { get; set; }
}

public class OrdenAsignadaViewModel
{
    public int Id { get; set; }
    public string NumeroOrden { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Equipo { get; set; } = string.Empty;
    public string Prioridad { get; set; } = string.Empty;
    public DateTime FechaAsignacion { get; set; }
    public string TiempoTranscurrido { get; set; } = string.Empty;
    public bool EstaRetrasada { get; set; }
}

public class DiagnosticoViewModel
{
    public int OrdenId { get; set; }
    public string NumeroOrden { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Equipo { get; set; } = string.Empty;
    public string ProblemaReportado { get; set; } = string.Empty;
    public bool EsGarantia { get; set; }
}

public class RegistrarDiagnosticoViewModel
{
    [Required]
    public string Diagnostico { get; set; } = string.Empty;

    public bool RequiereRepuestos { get; set; }

    public List<SolicitarRepuestoViewModel>? Repuestos { get; set; }
}

public class SolicitarRepuestoViewModel
{
    public int RepuestoId { get; set; }
    public string RepuestoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; } = 1;
}

public class RepuestoDisponibleViewModel
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int StockActual { get; set; }
}
}