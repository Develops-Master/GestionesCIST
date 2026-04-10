namespace GestionesCIST.Domain.Entities
{
    public class Marca
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        public bool Activo { get; set; } = true;

        // Relaciones
        public virtual ICollection<ModeloEquipo> Modelos { get; set; } = new List<ModeloEquipo>();
        public virtual ICollection<EquipoCliente> Equipos { get; set; } = new List<EquipoCliente>();
    }
}