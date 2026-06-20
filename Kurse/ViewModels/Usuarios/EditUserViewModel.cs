using System.ComponentModel.DataAnnotations;

namespace Kurse.ViewModels.Usuarios
{
    public class EditUserViewModel : BaseUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y un máximo de {1} caracteres de largo.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y la contraseña de confirmación no concuerdan.")]
        public string ConfirmPassword { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
