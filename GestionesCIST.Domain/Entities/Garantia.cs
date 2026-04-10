namespace GestionesCIST.Domain.Entities
{
    public class Garantia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrdenServicioId { get; set; }

        [ForeignKey("OrdenServicioId")]
        public virtual OrdenServicio OrdenServicio { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string TipoGarantia { get; set; } = "FABRICANTE";

        [Required]
        [StringLength(100)]
        public string ProveedorGarantia { get; set; } = string.Empty;

        [StringLength(50)]
        public string? NumeroCasoProveedor { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "PENDIENTE";

        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

        public DateTime? FechaRespuesta { get; set; }

        public DateTime? FechaCierre { get; set; }

        public int? CoordinadorId { get; set; }

        [ForeignKey("CoordinadorId")]
        public virtual Tecnico? Coordinador { get; set; }

        public string? Observaciones { get; set; }

        // Relaciones
        public virtual ICollection<RMADevolucion> Devoluciones { get; set; } = new List<RMADevolucion>();
    }
}