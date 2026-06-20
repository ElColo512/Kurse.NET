namespace Kurse.ViewModels.Actas
{
    public class CursoActaViewModel
    {
        public int CursoId { get; set; }
        public string NombreCurso { get; set; } = string.Empty;
        public List<ActaAlumnoViewModel> Alumnos { get; set; } = [];
        public bool EstaFinalizado { get; set; }
    }
}
