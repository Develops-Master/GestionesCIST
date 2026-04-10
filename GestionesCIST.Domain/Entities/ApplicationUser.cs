using GestionesCIST.Domain.Enums;
using Microsoft.AspNet.Identity.EntityFramework;

namespace GestionesCIST.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Los nombres son requeridos")]
        [StringLength(100)]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        [StringLength(100)]
        public string Apellidos { get; set; } = string.Empty;

        [StringLength(20)]
        public string? TipoDocumento { get; set; }

        [StringLength(20)]
        public string? NumeroDocumento { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        [StringLength(1)]
        public string? Genero { get; set; }

        [StringLength(500)]
        public string? FotoPerfil { get; set; }

        public EstadoAprobacion EstadoAprobacion { get; set; } = EstadoAprobacion.Pendiente;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public DateTime? FechaAprobacion { get; set; }

        [StringLength(450)]
        public string? AprobadoPor { get; set; }

        public DateTime? UltimoLogin { get; set; }

        [StringLength(45)]
        public string? UltimoIP { get; set; }

        public int IntentosFallidos { get; set; }

        // Propiedad calculada
        public string NombreCompleto => $"{Nombres} {Apellidos}";

        // Relaciones
        public virtual Cliente? Cliente { get; set; }
        public virtual Tecnico? Tecnico { get; set; }
        public virtual ICollection<Ticket> TicketsCreados { get; set; } = new List<Ticket>();
        public virtual ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
    }
}