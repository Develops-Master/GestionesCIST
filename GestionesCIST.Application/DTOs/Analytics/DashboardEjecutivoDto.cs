namespace GestionesCIST.Application.DTOs.Analytics
{
    public class DashboardEjecutivoDto
    {
        public KPIsDto KPIs { get; set; } = new();
        public List<ChartDataDto> TendenciaTickets { get; set; } = new();
        public List<ChartDataDto> DistribucionEstados { get; set; } = new();
        public List<TecnicoCargaDto> CargaTecnicos { get; set; } = new();
        public List<CategoriaTendenciaDto> CategoriasFrecuentes { get; set; } = new();
        public ForecastDto PrediccionDemanda { get; set; } = new();
    }

    public class KPIsDto
    {
        public int TotalTicketsMes { get; set; }
        public int TotalOrdenesMes { get; set; }
        public int TicketsActivos { get; set; }
        public int OrdenesEnProceso { get; set; }
        public double TiempoPromedioCierreHoras { get; set; }
        public double PorcentajeReintentosQA { get; set; }
        public double PromedioSatisfaccion { get; set; }
        public double NPS { get; set; }
        public decimal CostoTotalRepuestos { get; set; }
        public double TendenciaMTTR { get; set; } // % mejora vs periodo anterior
        public double ComparativaReintentoIA { get; set; } // % mejora con IA
    }

    public class ChartDataDto
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
        public string? Color { get; set; }
    }

    public class TecnicoCargaDto
    {
        public int TecnicoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;
        public int CargaActual { get; set; }
        public int CargaMaxima { get; set; }
        public double PorcentajeCarga { get; set; }
        public int OrdenesActivas { get; set; }
    }

    public class CategoriaTendenciaDto
    {
        public string Categoria { get; set; } = string.Empty;
        public int TotalTickets { get; set; }
        public double TiempoPromedioResolucion { get; set; }
        public double PorcentajeCumplimientoSLA { get; set; }
    }

    public class ForecastDto
    {
        public List<ForecastDataPointDto> Proyeccion { get; set; } = new();
        public int CapacidadRecomendada { get; set; }
        public string Recomendacion { get; set; } = string.Empty;
    }

    public class ForecastDataPointDto
    {
        public string Periodo { get; set; } = string.Empty;
        public int TicketsEstimados { get; set; }
        public int IntervaloInferior { get; set; }
        public int IntervaloSuperior { get; set; }
    }

    public class DateRangeDto
    {
        public DateTime FechaInicio { get; set; } = DateTime.UtcNow.AddDays(-30);
        public DateTime FechaFin { get; set; } = DateTime.UtcNow;
    }

    public class RankingTecnicoDto
    {
        public int TecnicoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;
        public int Posicion { get; set; }
        public int OrdenesCompletadas { get; set; }
        public double TiempoPromedioHoras { get; set; }
        public int PuntosGamificacion { get; set; }
        public List<string> Insignias { get; set; } = new();
    }
}