namespace GestionesCIST.Domain.Entities
{
    public class Especialidad
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        // Relaciones
        public virtual ICollection<TecnicoEspecialidad> Tecnicos { get; set; } = new List<TecnicoEspecialidad>();
    }
}