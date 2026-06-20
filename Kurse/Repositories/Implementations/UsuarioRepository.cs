using Kurse.Data;
using Kurse.Models.Enums;
using Kurse.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Kurse.Repositories.Implementations
{
    public class UsuarioRepository(ApplicationDbContext context) : IUsuarioRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<bool> TieneCursosActivosAsync(string profesorId) => await _context.Cursos.AnyAsync(c => c.IdProfesor == profesorId && c.EstadoCurso == EstadoCurso.Autorizado);
        public async Task<bool> TieneInscripcionesActivasAsync(string alumnoId) => await _context.CursoAlumnos.Include(ca => ca.Curso).AnyAsync(ca => ca.AlumnoId == alumnoId && ca.Curso.EstadoCurso == EstadoCurso.Autorizado);
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
