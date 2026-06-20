namespace Kurse.ViewModels.Reportes
{
    public class ReporteCursoViewModel
    {
        public string Codigo { get; set; } = string.Empty;
        public string Curso { get; set; } = string.Empty;
        public string Profesor { get; set; } = string.Empty;
        public DateOnly FechaInicio { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public DateOnly FechaFin { get; set; }
        public string Horario => $"{HoraInicio:HH\\:mm} - {HoraFin:HH\\:mm}";
        public string DiasDictado { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int CantidadInscriptos { get; set; }
        public int DuracionHoras { get; set; }
    }
}
