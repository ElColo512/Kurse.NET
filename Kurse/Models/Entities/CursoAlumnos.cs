using Kurse.Models.Enums;

namespace Kurse.Models.Entities
{
    public class CursoAlumnos
    {
        public int CursoAlumnosId { get; set; }
        public string AlumnoId { get; set; } = string.Empty;
        public int CursoId { get; set; }
        public int? Asistencia { get; set; }
        public decimal? NotaFinal { get; set; }
        public DateTime FechaInscripcion { get; set; }
        public EstadoAcademico Estado { get; set; } = EstadoAcademico.Inscripto;
        public DateTime? FechaBaja { get; set; }
        public string? MotivoBaja { get; set; }
        public ApplicationUser Alumno { get; set; } = null!;
        public Curso Curso { get; set; } = null!;
    }
}
