using GestionesCIST.Domain.Enums;

namespace GestionesCIST.Domain.Entities
{
    public class SolicitudRepuesto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string NumeroSolicitud { get; set; } = string.Empty;

        [Required]
        public int OrdenServicioId { get; set; }

        [ForeignKey("OrdenServicioId")]
        public virtual OrdenServicio OrdenServicio { get; set; } = null!;

        [Required]
        public int TecnicoSolicitanteId { get; set; }

        [ForeignKey("TecnicoSolicitanteId")]
        public virtual Tecnico TecnicoSolicitante { get; set; } = null!;

        public EstadoSolicitudRepuesto Estado { get; set; } = EstadoSolicitudRepuesto.Pendiente;

        public bool Urgente { get; set; }

        [StringLength(500)]
        public string? Justificacion { get; set; }

        [StringLength(500)]
        public string? NotasAdicionales { get; set; }

        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

        public int? AprobadoPor { get; set; }

        [ForeignKey("AprobadoPor")]
        public virtual Tecnico? Aprobador { get; set; }

        public DateTime? FechaAprobacion { get; set; }

        public DateTime? FechaEntrega { get; set; }

        // Relaciones
        public virtual ICollection<SolicitudRepuestoDetalle> Detalles { get; set; } = new List<SolicitudRepuestoDetalle>();
    }
}