namespace GestionesCIST.Domain.Entities
{
    public class TecnicoEspecialidad
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TecnicoId { get; set; }

        [ForeignKey("TecnicoId")]
        public virtual Tecnico Tecnico { get; set; } = null!;

        [Required]
        public int EspecialidadId { get; set; }

        [ForeignKey("EspecialidadId")]
        public virtual Especialidad Especialidad { get; set; } = null!;

        public int NivelDominio { get; set; } = 1; // 1:Básico, 2:Intermedio, 3:Avanzado

        public DateTime FechaObtencion { get; set; } = DateTime.UtcNow;
    }
}