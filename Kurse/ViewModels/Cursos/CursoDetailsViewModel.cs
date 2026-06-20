using Kurse.Models.Enums;

namespace Kurse.ViewModels.Cursos
{
    public class CursoDetailsViewModel
    {
        public int CursoId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string NombreCurso { get; set; } = string.Empty;
        public string Profesor { get; set; } = string.Empty;
        public string ContenidoCurso { get; set; } = string.Empty;
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public List<DiaSemana> DiasDictado { get; set; } = [];
        public TipoAprobacion TipoAprobacion { get; set; }
        public int CupoMaximo { get; set; }
        public bool Arancelado { get; set; }
        public decimal Arancel { get; set; }
        public string LugarDictado { get; set; } = string.Empty;
        public int CantidadClases { get; set; }
        public int DuracionHoras { get; set; }
        public string EstadoCurso { get; set; } = string.Empty;
        public bool PuedeRevisar { get; set; }
        public string? MotivoRechazo { get; set; }
    }
}
