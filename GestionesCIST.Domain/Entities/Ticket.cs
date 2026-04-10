using GestionesCIST.Domain.Enums;

namespace GestionesCIST.Domain.Entities
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string CodigoTicket { get; set; } = string.Empty;

        [Required]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; } = null!;

        public int? EquipoId { get; set; }

        [ForeignKey("EquipoId")]
        public virtual EquipoCliente? Equipo { get; set; }

        [StringLength(100)]
        public string? ContactoAlternativo { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoEquipo { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Marca { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Modelo { get; set; } = string.Empty;

        [StringLength(100)]
        public string? NumeroSerie { get; set; }

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string DescripcionProblema { get; set; } = string.Empty;

        public int? CategoriaSugeridaId { get; set; }

        [ForeignKey("CategoriaSugeridaId")]
        public virtual CategoriaTicket? CategoriaSugerida { get; set; }

        public PrioridadTicket Prioridad { get; set; } = PrioridadTicket.Media;

        [Column(TypeName = "decimal(5,4)")]
        public decimal? ConfianzaPrediccion { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? TiempoEstimadoIA_Horas { get; set; }

        public EstadoTicket Estado { get; set; } = EstadoTicket.Abierto;

        [StringLength(30)]
        public string? SubEstado { get; set; }

        public int? OrdenServicioId { get; set; }

        [ForeignKey("OrdenServicioId")]
        public virtual OrdenServicio? OrdenServicio { get; set; }

        public string? RutaArchivos { get; set; }

        [Required]
        [StringLength(20)]
        public string Origen { get; set; } = "PORTAL_WEB";

        [Required]
        [StringLength(450)]
        public string CreadoPor { get; set; } = string.Empty;

        [ForeignKey("CreadoPor")]
        public virtual ApplicationUser Creador { get; set; } = null!;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaAsignacion { get; set; }

        public DateTime? FechaCierre { get; set; }

        public DateTime FechaUltimaActualizacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaObjetivoRespuesta { get; set; }

        public DateTime? FechaObjetivoSolucion { get; set; }

        public bool? CumpleSLA { get; set; }

        // Relaciones
        public virtual ICollection<TicketAsignacion> Asignaciones { get; set; } = new List<TicketAsignacion>();
        public virtual ICollection<ComentarioTicket> Comentarios { get; set; } = new List<ComentarioTicket>();
        public virtual ICollection<NotaTicket> Notas { get; set; } = new List<NotaTicket>();
    }
}