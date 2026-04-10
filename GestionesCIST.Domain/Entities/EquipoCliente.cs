using System.Text.RegularExpressions;

namespace GestionesCIST.Domain.Entities
{
    public class EquipoCliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; } = null!;

        [Required]
        public int MarcaId { get; set; }

        [ForeignKey("MarcaId")]
        public virtual Marca Marca { get; set; } = null!;

        [Required]
        public int ModeloId { get; set; }

        [ForeignKey("ModeloId")]
        public virtual ModeloEquipo Modelo { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string NumeroSerie { get; set; } = string.Empty;

        [StringLength(50)]
        public string? CodigoActivo { get; set; }

        [StringLength(100)]
        public string? Procesador { get; set; }

        public int? RAM_GB { get; set; }

        [StringLength(100)]
        public string? Almacenamiento { get; set; }

        [StringLength(50)]
        public string? SistemaOperativo { get; set; }

        public bool EnGarantia { get; set; }

        public DateTime? FechaCompra { get; set; }

        public DateTime? FechaFinGarantia { get; set; }

        [StringLength(100)]
        public string? ProveedorGarantia { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public DateTime? UltimaRevision { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // Relaciones
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public virtual ICollection<OrdenServicio> OrdenesServicio { get; set; } = new List<OrdenServicio>();
    }
}