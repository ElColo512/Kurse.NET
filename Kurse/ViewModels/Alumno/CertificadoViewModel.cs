namespace Kurse.ViewModels.Alumno
{
    public class CertificadoViewModel
    {
        public string Alumno { get; set; } = string.Empty;
        public string Legajo { get; set; } = string.Empty;
        public string Curso { get; set; } = string.Empty;
        public string Profesor { get; set; } = string.Empty;
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        public int DuracionHoras { get; set; }
        public DateOnly FechaEmision { get; set; }
    }
}
