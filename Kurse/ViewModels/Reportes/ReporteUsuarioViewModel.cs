namespace Kurse.ViewModels.Reportes
{
    public class ReporteUsuarioViewModel
    {
        public string Legajo { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
