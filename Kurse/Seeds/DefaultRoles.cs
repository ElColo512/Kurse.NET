using Kurse.Models.Helpers;
using Microsoft.AspNetCore.Identity;

namespace Kurse.Seeds
{
    public class DefaultRoles
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { RoleConstants.Administrador, RoleConstants.Administrativo, RoleConstants.Profesor, RoleConstants.Alumno };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
