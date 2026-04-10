namespace GestionesCIST.Domain.Enums
{
    public enum EstadoTicket
    {
        [Display(Name = "Abierto")]
        Abierto = 1,

        [Display(Name = "Asignado")]
        Asignado = 2,

        [Display(Name = "En Progreso")]
        EnProgreso = 3,

        [Display(Name = "Pendiente Cliente")]
        PendienteCliente = 4,

        [Display(Name = "Resuelto")]
        Resuelto = 5,

        [Display(Name = "Cerrado")]
        Cerrado = 6,

        [Display(Name = "Cancelado")]
        Cancelado = 7
    }
}