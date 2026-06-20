namespace Kurse.ViewModels.Actas
{
    public class UpdateActaViewModel
    {
        public int CursoId { get; set; }
        public List<UpdateActaAlumnoViewModel> Alumnos { get; set; } = [];
    }
}
