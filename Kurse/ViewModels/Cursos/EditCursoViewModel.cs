using Kurse.Models.Enums;

namespace Kurse.ViewModels.Cursos
{
    public class EditCursoViewModel : BaseCursoViewModel
    {
        public int CursoId { get; set; }
        public EstadoCurso EstadoCurso { get; set; }
    }
}
