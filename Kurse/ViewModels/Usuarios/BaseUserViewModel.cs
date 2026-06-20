using System.ComponentModel.DataAnnotations;

namespace Kurse.ViewModels.Usuarios
{
    public class BaseUserViewModel
    {
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(50)]
        [RegularExpression(@"^[A-Za-zÀ-ÿ\s]+$", ErrorMessage = "Solo se permiten letras")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(50)]
        [RegularExpression(@"^[A-Za-zÀ-ÿ\s]+$", ErrorMessage = "Solo se permiten letras")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [StringLength(8)]
        [RegularExpression(@"^\d+$", ErrorMessage = "El DNI solo puede contener números")]
        public string Dni { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [EmailAddress(ErrorMessage = "Por favor ingresa un correo electrónico válido.")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Teléfono inválido")]
        [Display(Name = "Teléfono")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "El {0} es requerido.")]
        public string? Rol { get; set; }
        public string? Titulo { get; set; }
        public string? Carrera { get; set; }
    }
}
