namespace Kurse.ViewModels.Actas
{
    public class ActaAlumnoViewModel
    {
        public int CursoAlumnoId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public int? Asistencias { get; set; }
        public decimal? NotaFinal { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
