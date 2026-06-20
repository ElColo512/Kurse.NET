using Microsoft.AspNetCore.Identity;

namespace Kurse.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Legajo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Carrera { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }
}
