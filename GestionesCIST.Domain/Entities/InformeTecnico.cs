namespace GestionesCIST.Domain.Entities
{
    public class InformeTecnico
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrdenServicioId { get; set; }

        [ForeignKey("OrdenServicioId")]
        public virtual OrdenServicio OrdenServicio { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string TipoInforme { get; set; } = "DIAGNOSTICO";

        [StringLength(500)]
        public string? Resumen { get; set; }

        [Required]
        public string Detalle { get; set; } = string.Empty;

        public string? Recomendaciones { get; set; }

        [StringLength(30)]
        public string? EstadoFinal { get; set; }

        [Required]
        public int TecnicoId { get; set; }

        [ForeignKey("TecnicoId")]
        public virtual Tecnico Tecnico { get; set; } = null!;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaModificacion { get; set; }

        public int? AprobadoPor { get; set; }

        [ForeignKey("AprobadoPor")]
        public virtual Tecnico? Aprobador { get; set; }

        public DateTime? FechaAprobacion { get; set; }
    }
}