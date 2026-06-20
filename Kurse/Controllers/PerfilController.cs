using Kurse.Models.Entities;
using Kurse.Models.Helpers;
using Kurse.ViewModels.Perfil;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Kurse.Controllers
{
    [Authorize(Roles = RoleConstants.Alumno)]
    public class PerfilController(UserManager<ApplicationUser> userManager) : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public async Task<IActionResult> MisDatos()
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);

            if (user == null) return NotFound();

            MiPerfilViewModel model = new()
            {
                Dni = user.Dni,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                Carrera = user.Carrera
            };

            return PartialView("_MisDatosModal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MisDatos(MiPerfilViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return PartialView("_MisDatosModal", model);

                ApplicationUser? user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Usuario no encontrado."
                    });
                }

                user.Nombre = model.Nombre;
                user.Apellido = model.Apellido;
                user.PhoneNumber = model.PhoneNumber;

                if (User.IsInRole(RoleConstants.Alumno))
                {
                    user.Carrera = model.Carrera ?? string.Empty;
                }

                IdentityResult result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return Json(new
                    {
                        success = false,
                        message = string.Join("<br>", result.Errors.Select(e => e.Description))
                    });
                }

                return Json(new
                {
                    success = true,
                    message = "Datos actualizados correctamente."
                });

            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    message = "Ocurrió un error inesperado."
                });
            }
        }

        public IActionResult CambiarPassword()
        {
            return PartialView("_CambiarPasswordModal", new CambiarPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(CambiarPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return PartialView(
                        "_CambiarPasswordModal",
                        model);
                }

                ApplicationUser? user =
                    await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Usuario no encontrado."
                    });
                }

                IdentityResult result =
                    await _userManager.ChangePasswordAsync(
                        user,
                        model.CurrentPassword,
                        model.NewPassword);

                if (!result.Succeeded)
                {
                    return Json(new
                    {
                        success = false,
                        message = string.Join(
                            "<br>",
                            result.Errors.Select(e => e.Description))
                    });
                }

                return Json(new
                {
                    success = true,
                    message = "Contraseña actualizada correctamente."
                });
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    message = "Ocurrió un error inesperado."
                });
            }
        }
    }
}
