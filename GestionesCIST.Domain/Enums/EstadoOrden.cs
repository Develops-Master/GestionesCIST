namespace GestionesCIST.Domain.Enums
{
    public enum EstadoOrden
    {
        [Display(Name = "Asignación")]
        Asignacion = 1,

        [Display(Name = "Diagnóstico")]
        Diagnostico = 2,

        [Display(Name = "Reparación")]
        Reparacion = 3,

        [Display(Name = "Pruebas QA")]
        Pruebas = 4,

        [Display(Name = "Almacén")]
        Almacen = 5
    }
}