using GestionesCIST.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GestionesCIST.WebMVC.Models.ViewModels
{
    public class AdminViewModels
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string EstadoAprobacion { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimoLogin { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class UsuarioPendienteViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? TipoDocumento { get; set; }
        public string? NumeroDocumento { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public class TecnicoAdminViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string CodigoTecnico { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;
        public string Sede { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int CargaActual { get; set; }
        public int CargaMaximaDiaria { get; set; }
        public double PromedioCalificacion { get; set; }
        public int TotalOrdenesCompletadas { get; set; }
        public bool Activo { get; set; }
    }

    public class CrearTecnicoViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Nombres { get; set; } = string.Empty;

        [Required]
        public string Apellidos { get; set; } = string.Empty;

        [Required]
        public string CodigoTecnico { get; set; } = string.Empty;

        [Required]
        public string Nivel { get; set; } = "JUNIOR";

        [Required]
        public string Sede { get; set; } = "Lima";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public List<int> EspecialidadesIds { get; set; } = new();
    }

    public class RoleViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Seleccionado { get; set; }
    }

    public class DashboardAdminViewModel
    {
        public int TotalUsuarios { get; set; }
        public int UsuariosPendientes { get; set; }
        public int TotalTecnicos { get; set; }
        public int TecnicosDisponibles { get; set; }
        public int TicketsAbiertos { get; set; }
        public int OrdenesActivas { get; set; }
        public double TiempoPromedioCierre { get; set; }
        public List<ChartDataViewModel> TicketsPorDia { get; set; } = new();
    }

    public class ChartDataViewModel
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}