namespace GestionesCIST.Domain.Entities
{
    public class RMADevolucion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GarantiaId { get; set; }

        [ForeignKey("GarantiaId")]
        public virtual Garantia Garantia { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string NumeroRMA { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "PENDIENTE_ENVIO";

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaEnvio { get; set; }

        public DateTime? FechaRecepcionProveedor { get; set; }

        public DateTime? FechaRecepcionRepuesto { get; set; }

        [StringLength(50)]
        public string? TrackingNumber { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? CostoEnvio { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }
    }
}