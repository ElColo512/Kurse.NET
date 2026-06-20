using Kurse.Data;
using Kurse.Models.Entities;
using Kurse.Models.Enums;
using Kurse.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Kurse.Repositories.Implementations
{
    public class CursoAlumnosRepository(ApplicationDbContext context) : ICursoAlumnosRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task InscribirAlumnoAsync(CursoAlumnos inscripcion) => await _context.CursoAlumnos.AddAsync(inscripcion);

        public async Task AddAsync(CursoAlumnos cursoAlumno) => await _context.CursoAlumnos.AddAsync(cursoAlumno);

        public async Task<List<CursoAlumnos>> GetCursoAlumnosByIdsAsync(List<int> ids) => await _context.CursoAlumnos.Where(ca => ids.Contains(ca.CursoAlumnosId)).ToListAsync();

        public async Task<CursoAlumnos?> GetCursoAlumnoByIdAsync(int id) => await _context.CursoAlumnos.Include(ca => ca.Curso).FirstOrDefaultAsync(ca => ca.CursoAlumnosId == id);

        public async Task<bool> AlumnoYaInscriptoAsync(int cursoId, string alumnoId) => await _context.CursoAlumnos.AnyAsync(x => x.CursoId == cursoId && x.AlumnoId == alumnoId && x.Estado != EstadoAcademico.Baja);

        public async Task<List<CursoAlumnos>> GetHistoriaAcademicaAsync(string alumnoId) => await _context.CursoAlumnos.Include(ca => ca.Curso).ThenInclude(c => c.Profesor).Where(ca => ca.AlumnoId == alumnoId).OrderByDescending(ca => ca.Curso.FechaInicio).ToListAsync();

        public async Task<CursoAlumnos?> GetCertificadoAsync(string alumnoId, int cursoId) => await _context.CursoAlumnos.Include(ca => ca.Alumno).Include(ca => ca.Curso).ThenInclude(c => c.Profesor).FirstOrDefaultAsync(ca => ca.AlumnoId == alumnoId && ca.CursoId == cursoId);

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
