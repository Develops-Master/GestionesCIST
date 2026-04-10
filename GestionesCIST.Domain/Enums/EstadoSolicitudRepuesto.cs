namespace GestionesCIST.Domain.Enums
{
    public enum EstadoSolicitudRepuesto
    {
        [Display(Name = "Pendiente")]
        Pendiente = 1,

        [Display(Name = "Aprobada")]
        Aprobada = 2,

        [Display(Name = "Rechazada")]
        Rechazada = 3,

        [Display(Name = "En Proceso")]
        EnProceso = 4,

        [Display(Name = "Entregada")]
        Entregada = 5,

        [Display(Name = "Cancelada")]
        Cancelada = 6
    }
}