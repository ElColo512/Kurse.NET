namespace Kurse.ViewModels.Alumno
{
    public class HistoriaAcademicaViewModel
    {
        public int TotalCursos { get; set; }
        public int CursosAprobados { get; set; }
        public int CursosEnCurso { get; set; }
        public decimal PromedioGeneral { get; set; }
        public List<HistoriaCursoViewModel> CursosActivos { get; set; } = [];
        public List<HistoriaCursoViewModel> CursosFinalizados { get; set; } = [];
    }
}
