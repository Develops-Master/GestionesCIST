namespace GestionesCIST.Domain.Entities
{
    public class Proveedor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string RazonSocial { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string RUC { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Contacto { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        [StringLength(200)]
        public string? SitioWeb { get; set; }

        public int? TiempoEntregaPromedioDias { get; set; }

        [Range(1, 5)]
        public int? Calificacion { get; set; }

        [StringLength(100)]
        public string? CondicionesPago { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Relaciones
        public virtual ICollection<Repuesto> Repuestos { get; set; } = new List<Repuesto>();
    }
}