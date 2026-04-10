namespace GestionesCIST.Domain.Entities
{
    public class CategoriaTicket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Descripcion { get; set; }

        public int TiempoEstimadoHoras { get; set; } = 4;

        public bool Activo { get; set; } = true;

        // Relaciones
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}