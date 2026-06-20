using System.ComponentModel.DataAnnotations;

namespace Kurse.ViewModels.Perfil
{
    public class MiPerfilViewModel
    {
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(50)]
        [RegularExpression(@"^[A-Za-zÀ-ÿ\s]+$", ErrorMessage = "Solo se permiten letras")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(50)]
        [RegularExpression(@"^[A-Za-zÀ-ÿ\s]+$", ErrorMessage = "Solo se permiten letras")]
        public string Apellido { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Teléfono inválido")]
        [Display(Name = "Teléfono")]
        public string? PhoneNumber { get; set; }

        public string? Carrera { get; set; }
    }
}
