namespace GestionesCIST.Domain.Entities
{
    public class AuditoriaLog
    {
        [Key]
        public long Id { get; set; }

        [StringLength(450)]
        public string? UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual ApplicationUser? Usuario { get; set; }

        [Required]
        [StringLength(50)]
        public string Accion { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Entidad { get; set; } = string.Empty;

        [StringLength(100)]
        public string? EntidadId { get; set; }

        public string? ValoresAnteriores { get; set; }

        public string? ValoresNuevos { get; set; }

        [StringLength(45)]
        public string? IP_Address { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        public DateTime FechaEvento { get; set; } = DateTime.UtcNow;
    }
}