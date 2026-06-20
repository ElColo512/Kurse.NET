using Kurse.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kurse.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Curso> Cursos => Set<Curso>();

        public DbSet<CursoAlumnos> CursoAlumnos => Set<CursoAlumnos>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.Legajo)
                .IsUnique();

            builder.Entity<CursoAlumnos>()
                .HasOne(ca => ca.Curso)
                .WithMany(c => c.CursoAlumnos)
                .HasForeignKey(ca => ca.CursoId);

            builder.Entity<CursoAlumnos>()
                .HasOne(ca => ca.Alumno)
                .WithMany()
                .HasForeignKey(ca => ca.AlumnoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Curso>()
                .HasOne(c => c.Profesor)
                .WithMany()
                .HasForeignKey(c => c.IdProfesor)
                .OnDelete(DeleteBehavior.Restrict);

            foreach (var property in builder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }
        }
    }
}
