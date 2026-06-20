using Kurse.Models.Entities;
using Kurse.Repositories.Interfaces;
using Kurse.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Kurse.Services.Implementations
{
    public class UsuarioService(UserManager<ApplicationUser> userManager, IUsuarioRepository usuarioRepository) : IUsuarioService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;

        public async Task<(bool Success, string Message)> DarBajaAsync(string id, string usuarioActualId)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return (false, "Usuario no encontrado.");

            if (!user.Activo)
                return (false, "El usuario ya está dado de baja.");

            if (user.Id == usuarioActualId)
                return (false, "No puede darse de baja a sí mismo.");

            IList<string> roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Profesor"))
            {
                bool tieneCursos = await _usuarioRepository.TieneCursosActivosAsync(user.Id);

                if (tieneCursos)
                    return (false, "El profesor posee cursos activos.");
            }

            if (roles.Contains("Alumno"))
            {
                bool tieneCursosActivos = await _usuarioRepository.TieneInscripcionesActivasAsync(user.Id);

                if (tieneCursosActivos)
                    return (false, "El alumno posee cursos en progreso.");
            }

            if (roles.Contains("Administrador"))
            {
                IList<ApplicationUser> admins = [.. await _userManager.GetUsersInRoleAsync("Administrador")];

                int activos = admins.Count(a => a.Activo);

                if (activos <= 1)
                    return (false, "Debe existir al menos un administrador.");
            }

            user.Activo = false;

            await _usuarioRepository.SaveChangesAsync();
            return (true, "Usuario dado de baja correctamente.");
        }
    }
}
