using Kurse.Models.Enums;

namespace Kurse.ViewModels.Alumno
{
    public class HistoriaCursoViewModel
    {
        public int CursoId { get; set; }
        public string NombreCurso { get; set; } = string.Empty;
        public string Profesor { get; set; } = string.Empty;
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        public EstadoAcademico Estado { get; set; }
        public int Asistencia { get; set; }
        public decimal? NotaFinal { get; set; }
        public bool PuedeDescargarCertificado { get; set; }
        public int PorcentajeAsistencia { get; set; }
        public int CantidadClases { get; set; }
    }
}
