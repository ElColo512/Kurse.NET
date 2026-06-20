using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kurse.ViewModels.Actas
{
    public class CursoInscriptosViewModel
    {
        public int CursoId { get; set; }
        public string? AlumnoId { get; set; }
        public List<SelectListItem> AlumnosDisponibles { get; set; } = [];
        public string NombreCurso { get; set; } = string.Empty;
        public List<AlumnoCursoViewModel> Alumnos { get; set; } = [];
        public bool EstaFinalizado { get; set; }
    }
}
