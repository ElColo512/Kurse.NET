namespace Kurse.Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<bool> TieneCursosActivosAsync(string profesorId);
        Task<bool> TieneInscripcionesActivasAsync(string alumnoId);
        Task SaveChangesAsync();
    }
}
