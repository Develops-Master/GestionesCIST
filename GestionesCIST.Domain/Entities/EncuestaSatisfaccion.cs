using GestionesCIST.Domain.Entities;

namespace GestionesCIST.Domain.Entities
{
    public class EncuestaSatisfaccion
{
    [Key]
    public int Id { get; set; }

    public int? TicketId { get; set; }

    [ForeignKey("TicketId")]
    public virtual Ticket? Ticket { get; set; }

    public int? OrdenServicioId { get; set; }

    [ForeignKey("OrdenServicioId")]
    public virtual OrdenServicio? OrdenServicio { get; set; }

    [Required]
    public int ClienteId { get; set; }

    [ForeignKey("ClienteId")]
    public virtual Cliente Cliente { get; set; } = null!;

    [Range(1, 5)]
    public int? P1_AtencionRecibida { get; set; }

    [Range(1, 5)]
    public int? P2_TiempoSolucion { get; set; }

    [Range(1, 5)]
    public int? P3_CalidadReparacion { get; set; }

    [Range(1, 5)]
    public int? P4_Comunicacion { get; set; }

    [Range(1, 5)]
    public int? P5_RelacionCalidadPrecio { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal? PromedioGeneral { get; set; }

    [Range(0, 10)]
    public int? NPS_Recomendaria { get; set; }

    [StringLength(1000)]
    public string? Comentarios { get; set; }

    [StringLength(1000)]
    public string? SugerenciasMejora { get; set; }

    public DateTime FechaRespuesta { get; set; } = DateTime.UtcNow;

    [StringLength(45)]
    public string? IP_Address { get; set; }
}
}