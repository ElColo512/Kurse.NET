using Kurse.Models.Entities;

namespace Kurse.Repositories.Interfaces
{
    public interface IReportesRepository
    {
        Task<List<Curso>> GetCursosAsync(DateOnly? desde, DateOnly? hasta);
        Task<List<CursoAlumnos>> GetNotasAsync(int? cursoId);
        Task<List<CursoAlumnos>> GetInscriptosAsync(int? cursoId);
        Task<int> GetTotalAlumnosAsync();
        Task<int> GetTotalProfesoresAsync();
        Task<int> GetTotalCursosAsync();
        Task<int> GetTotalInscripcionesAsync();
        Task<List<Curso>> GetAllCursosAsync(string? profesorId = null);
    }
}
