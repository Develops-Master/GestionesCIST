namespace GestionesCIST.Domain.Entities
{
    public class NotaTicket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }

        [ForeignKey("TicketId")]
        public virtual Ticket Ticket { get; set; } = null!;

        [StringLength(20)]
        public string? Tipo { get; set; }

        [Required]
        public string Contenido { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string CreadoPor { get; set; } = string.Empty;

        [ForeignKey("CreadoPor")]
        public virtual ApplicationUser Creador { get; set; } = null!;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public bool VisibleCliente { get; set; }
    }
}