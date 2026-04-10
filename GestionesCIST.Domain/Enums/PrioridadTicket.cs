namespace GestionesCIST.Domain.Enums
{
    public enum PrioridadTicket
    {
        [Display(Name = "Baja")]
        Baja = 1,

        [Display(Name = "Media")]
        Media = 2,

        [Display(Name = "Alta")]
        Alta = 3,

        [Display(Name = "Crítica")]
        Critica = 4
    }
}