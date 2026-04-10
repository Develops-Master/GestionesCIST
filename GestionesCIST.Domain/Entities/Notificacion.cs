namespace GestionesCIST.Domain.Entities
{
    public class Notificacion
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UsuarioId { get; set; } = string.Empty;

        [ForeignKey("UsuarioId")]
        public virtual ApplicationUser Usuario { get; set; } = null!;

        [Required]
        [StringLength(30)]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Mensaje { get; set; } = string.Empty;

        [StringLength(500)]
        public string? URL_Accion { get; set; }

        public bool Leida { get; set; }

        public DateTime? FechaLeida { get; set; }

        public int Prioridad { get; set; } = 2;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaExpiracion { get; set; }
    }
}