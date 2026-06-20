using Kurse.Models.Entities;
using Kurse.Models.Helpers;
using Kurse.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Kurse.Services.Implementations
{
    public class UserRedirectService(
        UserManager<ApplicationUser> userManager) : IUserRedirectService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        private readonly Dictionary<string, string> _roleRedirects =
            new()
            {
                { RoleConstants.Administrador, "Cursos" },
                { RoleConstants.Administrativo, "Cursos" },
                { RoleConstants.Profesor, "Cursos" },
                { RoleConstants.Alumno, "Home" }
            };

        public async Task<string> GetControllerByRoleAsync(ApplicationUser user)
        {
            foreach (var roleRedirect in _roleRedirects)
            {
                bool isInRole = await _userManager.IsInRoleAsync(user, roleRedirect.Key);

                if (isInRole) return roleRedirect.Value;
            }

            return "Home";
        }
    }
}
