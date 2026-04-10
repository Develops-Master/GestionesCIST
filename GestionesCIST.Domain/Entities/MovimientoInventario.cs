using GestionesCIST.Domain.Entities;

namespace GestionesCIST.Domain.Entities
{
    public class MovimientoInventario
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int RepuestoId { get; set; }

    [ForeignKey("RepuestoId")]
    public virtual Repuesto Repuesto { get; set; } = null!;

    [Required]
    [StringLength(20)]
    public string TipoMovimiento { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    public int StockAnterior { get; set; }

    public int StockNuevo { get; set; }

    [StringLength(100)]
    public string? Referencia { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }

    [Required]
    [StringLength(450)]
    public string UsuarioId { get; set; } = string.Empty;

    [ForeignKey("UsuarioId")]
    public virtual ApplicationUser Usuario { get; set; } = null!;

    public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;
}
}