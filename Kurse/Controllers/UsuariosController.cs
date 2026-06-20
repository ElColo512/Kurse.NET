using Kurse.Models.Entities;
using Kurse.Models.Helpers;
using Kurse.Services.Interfaces;
using Kurse.ViewModels.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Kurse.Controllers
{
    [Authorize(Roles = RoleConstants.Administrador + "," + RoleConstants.Administrativo)]
    public class UsuariosController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<UsuariosController> logger, IUsuarioService usuarioService) : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly ILogger<UsuariosController> _logger = logger;
        private readonly IUsuarioService _usuarioService = usuarioService;

        public async Task<IActionResult> Index()
        {
            CargarRoles();
            return View();
        }

        public async Task<IActionResult> GetAll(string rol, bool mostrarInactivos = false)
        {
            List<ApplicationUser> usuarios = [.. _userManager.Users];
            if (!mostrarInactivos)
            {
                usuarios = [.. usuarios.Where(u => u.Activo)];
            }

            List<object> resultado = [];

            foreach (var user in usuarios)
            {
                IList<string> roles = await _userManager.GetRolesAsync(user);

                string? rolUsuario = roles.FirstOrDefault();

                if (User.IsInRole(RoleConstants.Administrativo) && rolUsuario is not (RoleConstants.Alumno or RoleConstants.Profesor)) continue;

                if (!string.IsNullOrEmpty(rol) && rolUsuario != rol) continue;

                resultado.Add(new
                {
                    id = user.Id,
                    legajo = user.Legajo,
                    nombreCompleto = $"{user.Nombre} {user.Apellido}",
                    dni = user.Dni,
                    email = user.Email,
                    telefono = user.PhoneNumber,
                    campoExtra = rolUsuario == "Alumno" ? user.Carrera : user.Titulo,
                    activo = user.Activo
                });
            }

            return Json(new { data = resultado });
        }

        public IActionResult Create()
        {
            CargarRoles();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            try
            {
                ValidarRol(model.Rol!);
                if (!ModelState.IsValid)
                {
                    CargarRoles();
                    return View(model);
                }

                ApplicationUser user = new()
                {
                    Legajo = await GenerarLegajoAsync(),
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Dni = model.Dni,
                    Email = model.Email,
                    UserName = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Titulo = model.Titulo ?? string.Empty,
                    Carrera = model.Carrera ?? string.Empty,
                    Activo = true
                };

                IdentityResult result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    SetToast("No se pudo crear el usuario.", "bg-danger");

                    CargarRoles();
                    return View(model);
                }

                await _userManager.AddToRoleAsync(user, model.Rol!);
                SetToast("Usuario creado correctamente.", "bg-success");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario.");
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado.");
                SetToast("Ocurrió un error inesperado.", "bg-danger");
                CargarRoles();
                return View(model);
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(id);

            if (user == null) return NotFound();

            IList<string> roles = await _userManager.GetRolesAsync(user);

            UserDetailsViewModel model = new()
            {
                Id = user.Id,
                Legajo = user.Legajo,
                NombreApellido = $"{user.Nombre} {user.Apellido}",
                Dni = user.Dni,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                Rol = roles.FirstOrDefault(),
                Titulo = user.Titulo,
                Carrera = user.Carrera
            };

            return PartialView("_DetailsModal", model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(id);

            if (user == null) return NotFound();

            IList<string> roles = await _userManager.GetRolesAsync(user);

            EditUserViewModel model = new()
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Dni = user.Dni,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                Rol = roles.FirstOrDefault(),
                Titulo = user.Titulo,
                Carrera = user.Carrera,
                Activo = user.Activo
            };

            CargarRoles();

            return PartialView("_EditModal", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            try
            {
                ValidarRol(model.Rol!);
                if (!ModelState.IsValid)
                {
                    CargarRoles();
                    return PartialView("_EditModal", model);
                }

                ApplicationUser? user = await _userManager.FindByIdAsync(model.Id);

                if (user == null) return NotFound();

                if (!user.Activo)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No es posible modificar un usuario inactivo."
                    });
                }

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    string token = await _userManager.GeneratePasswordResetTokenAsync(user);

                    IdentityResult passwordResult = await _userManager.ResetPasswordAsync(user, token, model.Password);

                    if (!passwordResult.Succeeded)
                    {
                        foreach (var error in passwordResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }

                        CargarRoles();
                        return PartialView("_EditModal", model);
                    }
                }

                user.Nombre = model.Nombre;
                user.Apellido = model.Apellido;
                user.Dni = model.Dni;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.Titulo = model.Titulo ?? string.Empty;
                user.Carrera = model.Carrera ?? string.Empty;

                IdentityResult result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    CargarRoles();
                    return PartialView("_EditModal", model);
                }

                IList<string> currentRoles = await _userManager.GetRolesAsync(user);

                if (!currentRoles.Contains(model.Rol!))
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, model.Rol!);
                }

                return Json(new
                {
                    success = true,
                    message = "Usuario actualizado correctamente."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar usuario.");

                return Json(new
                {
                    success = false,
                    message = "Ocurrió un error inesperado."
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            string usuarioActualId = _userManager.GetUserId(User)!;

            (bool Success, string Message) = await _usuarioService.DarBajaAsync(id, usuarioActualId);

            return Json(new
            {
                success = Success,
                message = Message
            });
        }

        private async Task<string> GenerarLegajoAsync()
        {
            List<string> legajos = await _userManager.Users.Select(u => u.Legajo).ToListAsync();
            int ultimoLegajo = legajos.Where(l => int.TryParse(l, out _)).Select(int.Parse).DefaultIfEmpty(0).Max();

            return (ultimoLegajo + 1).ToString("D6");
        }

        private void CargarRoles()
        {
            IQueryable<IdentityRole> roles = _roleManager.Roles;

            if (User.IsInRole("Administrativo"))
            {
                roles = roles.Where(r => r.Name != "Administrador" && r.Name != "Administrativo");
            }

            ViewBag.Roles = roles
                .Select(r => new SelectListItem
                {
                    Value = r.Name!,
                    Text = r.Name!
                })
                .ToList();
        }

        private bool ValidarRol(string rol)
        {
            if (PuedeAsignarRol(rol)) return true;

            ModelState.AddModelError(nameof(BaseUserViewModel.Rol), "No posee permisos para asignar ese rol.");
            return false;
        }

        private bool PuedeAsignarRol(string rol)
        {
            if (User.IsInRole("Administrador")) return true;

            if (User.IsInRole("Administrativo"))
                return rol != "Administrador" && rol != "Administrativo";

            return false;
        }

        private void SetToast(string message, string type)
        {
            TempData["ToastMessage"] = message;
            TempData["ToastClass"] = type;
        }
    }
}
