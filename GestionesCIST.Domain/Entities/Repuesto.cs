namespace GestionesCIST.Domain.Entities
{
    public class Repuesto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [StringLength(100)]
        public string? CodigoBarras { get; set; }

        [Required]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public virtual CategoriaRepuesto Categoria { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [StringLength(50)]
        public string? Marca { get; set; }

        public string? ModeloCompatible { get; set; }

        public int StockActual { get; set; }

        public int StockMinimo { get; set; } = 5;

        public int StockMaximo { get; set; } = 50;

        [StringLength(50)]
        public string? UbicacionAlmacen { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioCosto { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioVenta { get; set; }

        [StringLength(3)]
        public string Moneda { get; set; } = "PEN";

        [Column(TypeName = "decimal(5,2)")]
        public decimal IGV { get; set; } = 18.00m;

        public int? ProveedorPrincipalId { get; set; }

        [ForeignKey("ProveedorPrincipalId")]
        public virtual Proveedor? ProveedorPrincipal { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime? FechaUltimaCompra { get; set; }

        // Relaciones
        public virtual ICollection<SolicitudRepuestoDetalle> SolicitudesDetalle { get; set; } = new List<SolicitudRepuestoDetalle>();
        public virtual ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
    }
}