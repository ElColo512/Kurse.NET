namespace Kurse.ViewModels.Usuarios
{
    public class UserDetailsViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Legajo { get; set; } = string.Empty;
        public string NombreApellido { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Rol { get; set; }
        public string? Titulo { get; set; }
        public string? Carrera { get; set; }
    }
}
