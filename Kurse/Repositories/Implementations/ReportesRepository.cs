using Kurse.Data;
using Kurse.Models.Entities;
using Kurse.Models.Helpers;
using Kurse.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Kurse.Repositories.Implementations
{
    public class ReportesRepository(ApplicationDbContext context) : IReportesRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<List<Curso>> GetCursosAsync(DateOnly? desde, DateOnly? hasta)
        {
            IQueryable<Curso> query = _context.Cursos.Include(c => c.Profesor).Include(c => c.CursoAlumnos).Include(c => c.CursoDias);

            if (desde.HasValue)
                query = query.Where(c => c.FechaInicio >= desde.Value);


            if (hasta.HasValue)
                query = query.Where(c => c.FechaFin <= hasta.Value);

            return await query.ToListAsync();
        }

        public async Task<List<CursoAlumnos>> GetInscriptosAsync(int? cursoId)
        {
            IQueryable<CursoAlumnos> query = _context.CursoAlumnos.Include(ca => ca.Alumno).Include(ca => ca.Curso);

            if (cursoId.HasValue)
                query = query.Where(ca => ca.CursoId == cursoId.Value);

            return await query.ToListAsync();
        }

        public async Task<List<CursoAlumnos>> GetNotasAsync(int? cursoId)
        {
            IQueryable<CursoAlumnos> query = _context.CursoAlumnos.Include(ca => ca.Alumno).Include(ca => ca.Curso);

            if (cursoId.HasValue)
                query = query.Where(ca => ca.CursoId == cursoId.Value);

            return await query.ToListAsync();
        }

        public async Task<List<Curso>> GetAllCursosAsync(string? profesorId = null)
        {
            IQueryable<Curso> query = _context.Cursos;

            if (!string.IsNullOrEmpty(profesorId))
                query = query.Where(c => c.IdProfesor == profesorId);

            return await query.OrderBy(c => c.NombreCurso).ToListAsync();
        }

        public async Task<int> GetTotalCursosAsync() => await _context.Cursos.CountAsync();

        public async Task<int> GetTotalInscripcionesAsync() => await _context.CursoAlumnos.CountAsync();

        public async Task<int> GetTotalAlumnosAsync()
        {
            string? rolId = await _context.Roles
                .Where(r => r.Name == RoleConstants.Alumno)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            return await _context.UserRoles.CountAsync(r => r.RoleId == rolId);
        }

        public async Task<int> GetTotalProfesoresAsync()
        {
            string? rolId = await _context.Roles
                .Where(r => r.Name == RoleConstants.Profesor)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            return await _context.UserRoles.CountAsync(r => r.RoleId == rolId);
        }
    }
}
