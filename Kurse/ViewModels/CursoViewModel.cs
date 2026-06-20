namespace Kurse.ViewModels
{
    public class CursoViewModel
    {
        public int CursoId { get; set; }
        public string NombreCurso { get; set; } = string.Empty;
        public string Profesor { get; set; } = string.Empty;
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        public int CupoMaximo { get; set; }
        public int Anotados { get; set; }
        public string EstadoCurso { get; set; } = string.Empty;
    }
}
