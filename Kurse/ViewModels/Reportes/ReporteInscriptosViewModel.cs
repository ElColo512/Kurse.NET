namespace Kurse.ViewModels.Reportes
{
    public class ReporteInscriptosViewModel
    {
        public string Curso { get; set; } = string.Empty;
        public string Alumno { get; set; } = string.Empty;
        public string Legajo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateOnly FechaInscripcion { get; set; }
        public string EstadoAcademico { get; set; } = string.Empty;
    }
}
