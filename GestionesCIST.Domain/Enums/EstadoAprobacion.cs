namespace GestionesCIST.Domain.Enums
{
    public enum EstadoAprobacion
    {
        [Display(Name = "Pendiente")]
        Pendiente = 0,

        [Display(Name = "Aprobado")]
        Aprobado = 1,

        [Display(Name = "Rechazado")]
        Rechazado = 2,

        [Display(Name = "Bloqueado")]
        Bloqueado = 3
    }
}