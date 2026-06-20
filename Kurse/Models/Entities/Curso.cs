using Kurse.Models.Enums;

namespace Kurse.Models.Entities
{
    public class Curso
    {
        public int CursoId { get; set; }
        public string IdProfesor { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string NombreCurso { get; set; } = string.Empty;
        public string ContenidoCurso { get; set; } = string.Empty;
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public ICollection<CursoDia> CursoDias { get; set; } = [];
        public TipoAprobacion TipoAprobacion { get; set; }
        public int CupoMaximo { get; set; }
        public bool Arancelado { get; set; }
        public decimal Arancel { get; set; }
        public string LugarDictado { get; set; } = string.Empty;
        public int CantidadClases { get; set; }
        public int DuracionHoras { get; set; }
        public EstadoCurso EstadoCurso { get; set; }
        public string? MotivoRechazo { get; set; }
        public ApplicationUser Profesor { get; set; } = null!;
        public ICollection<CursoAlumnos> CursoAlumnos { get; set; } = [];
    }
}
