namespace GestionesCIST.Domain.Entities
{
    public class ModeloEquipo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MarcaId { get; set; }

        [ForeignKey("MarcaId")]
        public virtual Marca Marca { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Categoria { get; set; }

        public int? AnioLanzamiento { get; set; }

        public bool Activo { get; set; } = true;

        // Relaciones
        public virtual ICollection<EquipoCliente> Equipos { get; set; } = new List<EquipoCliente>();
    }
}