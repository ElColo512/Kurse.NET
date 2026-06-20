using Kurse.Models.Enums;

namespace Kurse.ViewModels.Actas
{
    public class UpdateActaAlumnoViewModel
    {
        public int CursoAlumnoId { get; set; }
        public int? Asistencias { get; set; }
        public decimal? NotaFinal { get; set; }
        public EstadoAcademico Estado { get; set; }
    }
}
