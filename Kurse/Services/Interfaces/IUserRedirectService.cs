using Kurse.Models.Entities;

namespace Kurse.Services.Interfaces
{
    public interface IUserRedirectService
    {
        Task<string> GetControllerByRoleAsync(ApplicationUser user);
    }
}
