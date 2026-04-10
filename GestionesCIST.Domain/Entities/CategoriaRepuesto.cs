namespace GestionesCIST.Domain.Entities
{
    public class CategoriaRepuesto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        // Relaciones
        public virtual ICollection<Repuesto> Repuestos { get; set; } = new List<Repuesto>();
    }
}