namespace GestionesCIST.Domain.Entities
{
    public class ComentarioTicket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }

        [ForeignKey("TicketId")]
        public virtual Ticket Ticket { get; set; } = null!;

        [Required]
        [StringLength(450)]
        public string UsuarioId { get; set; } = string.Empty;

        [ForeignKey("UsuarioId")]
        public virtual ApplicationUser Usuario { get; set; } = null!;

        [Required]
        public string Comentario { get; set; } = string.Empty;

        public bool EsInterno { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}