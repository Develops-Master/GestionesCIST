using GestionesCIST.Domain.Enums;

namespace GestionesCIST.Domain.Entities
{
    public class OrdenServicio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string NumeroOrden { get; set; } = string.Empty;

        public int? TicketId { get; set; }

        [ForeignKey("TicketId")]
        public virtual Ticket? Ticket { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; } = null!;

        public int? EquipoId { get; set; }

        [ForeignKey("EquipoId")]
        public virtual EquipoCliente? Equipo { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoEquipo { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Marca { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Modelo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string NumeroSerie { get; set; } = string.Empty;

        public bool EsGarantia { get; set; }

        [StringLength(30)]
        public string? TipoGarantia { get; set; }

        public bool TieneSeguro { get; set; }

        public string ProblemaReportado { get; set; } = string.Empty;

        public string? DiagnosticoInicial { get; set; }

        public string? DiagnosticoFinal { get; set; }

        public EstadoOrden Estado { get; set; } = EstadoOrden.Asignacion;

        public PrioridadTicket Prioridad { get; set; } = PrioridadTicket.Media;

        [StringLength(20)]
        public string? ColorEtiqueta { get; set; }

        public int? TecnicoDiagnosticoId { get; set; }

        [ForeignKey("TecnicoDiagnosticoId")]
        public virtual Tecnico? TecnicoDiagnostico { get; set; }

        public int? TecnicoReparacionId { get; set; }

        [ForeignKey("TecnicoReparacionId")]
        public virtual Tecnico? TecnicoReparacion { get; set; }

        public int? TesterQAId { get; set; }

        [ForeignKey("TesterQAId")]
        public virtual Tecnico? TesterQA { get; set; }

        [Column(TypeName = "decimal(5,4)")]
        public decimal? ProbabilidadExitoQA { get; set; }

        public string? RepuestosSugeridosIA { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? TiempoEstimadoTotal_Horas { get; set; }

        public DateTime FechaRecepcion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaAsignacion { get; set; }

        public DateTime? FechaInicioDiagnostico { get; set; }

        public DateTime? FechaFinDiagnostico { get; set; }

        public DateTime? FechaInicioReparacion { get; set; }

        public DateTime? FechaFinReparacion { get; set; }

        public DateTime? FechaEnvioPruebas { get; set; }

        public DateTime? FechaFinPruebas { get; set; }

        public DateTime? FechaEntregaAlmacen { get; set; }

        public DateTime? FechaEntregaCliente { get; set; }

        [Required]
        [StringLength(450)]
        public string CreadoPor { get; set; } = string.Empty;

        [ForeignKey("CreadoPor")]
        public virtual ApplicationUser Creador { get; set; } = null!;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? UltimaModificacion { get; set; }

        [StringLength(450)]
        public string? ModificadoPor { get; set; }

        [ForeignKey("ModificadoPor")]
        public virtual ApplicationUser? Modificador { get; set; }

        // Propiedades calculadas para métricas
        [NotMapped]
        public int? TiempoDiagnosticoMinutos =>
            FechaInicioDiagnostico.HasValue && FechaFinDiagnostico.HasValue
                ? (int?)(FechaFinDiagnostico.Value - FechaInicioDiagnostico.Value).TotalMinutes
                : null;

        [NotMapped]
        public int? TiempoReparacionMinutos =>
            FechaInicioReparacion.HasValue && FechaFinReparacion.HasValue
                ? (int?)(FechaFinReparacion.Value - FechaInicioReparacion.Value).TotalMinutes
                : null;

        [NotMapped]
        public int? TiempoTotalHoras =>
            FechaRecepcion != default && FechaEntregaAlmacen.HasValue
                ? (int?)(FechaEntregaAlmacen.Value - FechaRecepcion).TotalHours
                : null;

        // Relaciones
        public virtual ICollection<InformeTecnico> Informes { get; set; } = new List<InformeTecnico>();
        public virtual ICollection<SolicitudRepuesto> SolicitudesRepuesto { get; set; } = new List<SolicitudRepuesto>();
        public virtual Garantia? Garantia { get; set; }
    }
}