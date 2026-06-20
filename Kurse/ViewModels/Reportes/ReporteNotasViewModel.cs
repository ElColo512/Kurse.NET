namespace Kurse.ViewModels.Reportes
{
    public class ReporteNotasViewModel
    {
        public string Curso { get; set; } = string.Empty;
        public string Legajo { get; set; } = string.Empty;
        public string Alumno { get; set; } = string.Empty;
        public int TotalClases { get; set; }
        public decimal? NotaFinal { get; set; }
        public int ClasesAsistidas { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
