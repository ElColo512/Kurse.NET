using Kurse.Data;
using Kurse.Models.Entities;
using Kurse.Models.Enums;
using Kurse.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Kurse.Repositories.Implementations
{
    public class CursoRepository(ApplicationDbContext context) : ICursoRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task AddAsync(Curso curso)
        {
            _context.Cursos.Add(curso);
            await SaveChangesAsync();
        }

        public async Task UpdateAsync(Curso curso)
        {
            _context.Cursos.Update(curso);
            await SaveChangesAsync();
        }

        public async Task<List<Curso>> GetAllAsync(bool mostrarInactivos, string profesorId)
        {
            IQueryable<Curso> query = _context.Cursos.Include(c => c.Profesor).Include(c => c.CursoAlumnos);

            if (!mostrarInactivos)
                query = query.Where(c => c.EstadoCurso != EstadoCurso.Cancelado);

            if (!string.IsNullOrEmpty(profesorId))
                query = query.Where(c => c.IdProfesor == profesorId);

            return await query.ToListAsync();
        }

        public async Task<Curso?> GetByIdAsync(int id) => await _context.Cursos.Include(c => c.Profesor).Include(c => c.CursoAlumnos).Include(c => c.CursoDias).FirstOrDefaultAsync(c => c.CursoId == id);

        public async Task<Curso?> GetByIdWithAlumnosAsync(int id) => await _context.Cursos.Include(c => c.CursoAlumnos).ThenInclude(ca => ca.Alumno).FirstOrDefaultAsync(c => c.CursoId == id);

        public async Task<bool> TieneAlumnosActivosAsync(int cursoId) => await _context.CursoAlumnos.AnyAsync(ca => ca.CursoId == cursoId && ca.Estado != EstadoAcademico.Baja);

        public async Task<bool> CursoPerteneceProfesorAsync(int cursoId, string profesorId) => await _context.Cursos.AnyAsync(c => c.CursoId == cursoId && c.IdProfesor == profesorId);

        public async Task<List<Curso>> GetCursosDisponiblesAsync() => await _context.Cursos.Include(c => c.Profesor).Include(c => c.CursoAlumnos).Include(c => c.CursoDias).Where(c => c.EstadoCurso == EstadoCurso.Autorizado).OrderBy(c => c.FechaInicio).ToListAsync();

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
