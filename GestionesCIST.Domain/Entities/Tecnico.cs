using GestionesCIST.Domain.Enums;

namespace GestionesCIST.Domain.Entities
{
    public class Tecnico
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
        public string CodigoTecnico { get; set; } = string.Empty;

        [Required]
        public DateTime FechaIngreso { get; set; }

        [Required]
        [StringLength(20)]
        public string Nivel { get; set; } = "JUNIOR";

        public int AniosExperiencia { get; set; }

        public string? Certificaciones { get; set; }

        [Required]
        [StringLength(50)]
        public string Sede { get; set; } = "Lima";

        public bool DisponibleViaje { get; set; }

        public int RadioCoberturaKM { get; set; } = 15;

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "DISPONIBLE";

        public int CargaMaximaDiaria { get; set; } = 8;

        public int CargaActual { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal PromedioCalificacion { get; set; }

        public int TotalOrdenesCompletadas { get; set; }

        public int TotalReintentosQA { get; set; }

        public bool Activo { get; set; } = true;

        // Relaciones
        public virtual ICollection<TecnicoEspecialidad> Especialidades { get; set; } = new List<TecnicoEspecialidad>();
        public virtual ICollection<OrdenServicio> DiagnosticosRealizados { get; set; } = new List<OrdenServicio>();
        public virtual ICollection<OrdenServicio> ReparacionesRealizadas { get; set; } = new List<OrdenServicio>();
        public virtual ICollection<OrdenServicio> QAsRealizados { get; set; } = new List<OrdenServicio>();
    }
}