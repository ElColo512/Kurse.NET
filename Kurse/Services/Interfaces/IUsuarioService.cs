namespace Kurse.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<(bool Success, string Message)> DarBajaAsync(string id, string usuarioActualId);
    }
}
