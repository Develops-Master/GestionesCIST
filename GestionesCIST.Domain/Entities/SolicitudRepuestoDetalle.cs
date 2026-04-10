namespace GestionesCIST.Domain.Entities
{
    public class SolicitudRepuestoDetalle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SolicitudId { get; set; }

        [ForeignKey("SolicitudId")]
        public virtual SolicitudRepuesto Solicitud { get; set; } = null!;

        [Required]
        public int RepuestoId { get; set; }

        [ForeignKey("RepuestoId")]
        public virtual Repuesto Repuesto { get; set; } = null!;

        public int CantidadSolicitada { get; set; }

        public int CantidadEntregada { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Descuento { get; set; }

        [StringLength(200)]
        public string? Notas { get; set; }
    }
}