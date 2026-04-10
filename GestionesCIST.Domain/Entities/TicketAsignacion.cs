namespace GestionesCIST.Domain.Entities
{
    public class TicketAsignacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }

        [ForeignKey("TicketId")]
        public virtual Ticket Ticket { get; set; } = null!;

        [Required]
        public int TecnicoId { get; set; }

        [ForeignKey("TecnicoId")]
        public virtual Tecnico Tecnico { get; set; } = null!;

        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(450)]
        public string AsignadoPor { get; set; } = string.Empty;

        [ForeignKey("AsignadoPor")]
        public virtual ApplicationUser Asignador { get; set; } = null!;

        [StringLength(200)]
        public string? Motivo { get; set; }

        public bool Activo { get; set; } = true;
    }
}