using GestionesCIST.Domain.Enums;

namespace GestionesCIST.Domain.Entities
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string TipoCliente { get; set; } = "NATURAL";

        [StringLength(200)]
        public string? RazonSocial { get; set; }

        [StringLength(20)]
        public string? RUC { get; set; }

        [StringLength(150)]
        public string? ContactoPrincipal { get; set; }

        [StringLength(20)]
        public string? TelefonoFijo { get; set; }

        [StringLength(20)]
        public string? TelefonoMovil { get; set; }

        [StringLength(100)]
        public string? CorreoFacturacion { get; set; }

        [StringLength(50)]
        public string? Departamento { get; set; }

        [StringLength(50)]
        public string? Provincia { get; set; }

        [StringLength(50)]
        public string? Distrito { get; set; }

        [StringLength(200)]
        public string? DireccionFiscal { get; set; }

        [StringLength(200)]
        public string? Referencia { get; set; }

        [StringLength(20)]
        public string? Categoria { get; set; }

        public bool TieneContrato { get; set; }

        [StringLength(50)]
        public string? NumeroContrato { get; set; }

        public DateTime? FechaInicioContrato { get; set; }

        public DateTime? FechaFinContrato { get; set; }

        public int SLA_TiempoRespuestaHoras { get; set; } = 4;

        public int SLA_TiempoSolucionDias { get; set; } = 3;

        public bool Activo { get; set; } = true;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? RegistradoPor { get; set; }

        [ForeignKey("RegistradoPor")]
        public virtual ApplicationUser? Registrador { get; set; }

        // Relaciones
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public virtual ICollection<OrdenServicio> OrdenesServicio { get; set; } = new List<OrdenServicio>();
        public virtual ICollection<EquipoCliente> EquiposRegistrados { get; set; } = new List<EquipoCliente>();
    }
}