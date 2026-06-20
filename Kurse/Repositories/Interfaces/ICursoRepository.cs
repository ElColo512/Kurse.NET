using Kurse.Models.Entities;

namespace Kurse.Repositories.Interfaces
{
    public interface ICursoRepository
    {
        Task<List<Curso>> GetAllAsync(bool mostrarInactivos, string profesorId);
        Task<List<Curso>> GetCursosDisponiblesAsync();
        Task<Curso?> GetByIdAsync(int id);
        Task<Curso?> GetByIdWithAlumnosAsync(int id);
        Task AddAsync(Curso curso);
        Task UpdateAsync(Curso curso);
        Task<bool> TieneAlumnosActivosAsync(int cursoId);
        Task<bool> CursoPerteneceProfesorAsync(int cursoId, string profesorId);
        Task SaveChangesAsync();
    }
}