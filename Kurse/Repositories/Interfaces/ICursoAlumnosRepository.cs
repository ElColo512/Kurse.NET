using Kurse.Models.Entities;

namespace Kurse.Repositories.Interfaces
{
    public interface ICursoAlumnosRepository
    {
        Task AddAsync(CursoAlumnos cursoAlumno);
        Task<List<CursoAlumnos>> GetCursoAlumnosByIdsAsync(List<int> ids);
        Task<CursoAlumnos?> GetCursoAlumnoByIdAsync(int id);
        Task<List<CursoAlumnos>> GetHistoriaAcademicaAsync(string alumnoId);
        Task<bool> AlumnoYaInscriptoAsync(int cursoId, string alumnoId);
        Task InscribirAlumnoAsync(CursoAlumnos inscripcion);
        Task<CursoAlumnos?> GetCertificadoAsync(string alumnoId, int cursoId);
        Task SaveChangesAsync();
    }
}
