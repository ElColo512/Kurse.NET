using Kurse.ViewModels;
using Kurse.ViewModels.Cursos;

namespace Kurse.Services.Interfaces
{
    public interface ICursoService
    {
        Task<List<CursoViewModel>> GetAllAsync(bool mostrarInactivos, string profesorId);
        Task<EditCursoViewModel?> GetEditByIdAsync(int id);
        Task<CursoDetailsViewModel?> GetDetailsByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(CreateCursoViewModel model, bool esProfesor);
        Task<bool> UpdateAsync(EditCursoViewModel model);
        Task<(bool Success, string Message)> DarBajaAsync(int id);
        Task<bool> CursoPerteneceProfesorAsync(int cursoId, string profesorId);
        Task<(bool Success, string Message)> AutorizarAsync(int cursoId);
        Task<(bool Success, string Message)> RechazarAsync(int cursoId, string motivo);
        Task<(bool Success, string Message)> FinalizarAsync(int cursoId);
        Task<List<CursoOfertaViewModel>> GetOfertaAcademicaAsync(string alumnoId);
    }
}
