using System.ComponentModel.DataAnnotations;

namespace Kurse.ViewModels.Perfil
{
    public class CambiarPasswordViewModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña Actual")]
        public string CurrentPassword { get; set; } = string.Empty;
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y un máximo de {1} caracteres de largo.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("NewPassword", ErrorMessage = "La contraseña y la contraseña de confirmación no concuerdan.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
