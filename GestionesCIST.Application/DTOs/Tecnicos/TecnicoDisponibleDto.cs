namespace GestionesCIST.Application.DTOs.Tecnicos
{
    public class TecnicoDisponibleDto
    {
        public int Id { get; set; }
        public string CodigoTecnico { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;
        public List<string> Especialidades { get; set; } = new();
        public int CargaActual { get; set; }
        public int CargaMaxima { get; set; }
        public double PorcentajeCarga { get; set; }
    }

    public class TecnicoCargaDto
    {
        public int TecnicoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;
        public int CargaActual { get; set; }
        public int CargaMaxima { get; set; }
        public double PorcentajeCarga { get; set; }
        public int OrdenesActivas { get; set; }
    }
}