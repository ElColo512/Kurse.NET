using Kurse.Models.Enums;

namespace Kurse.Models.Entities
{
    public class CursoDia
    {
        public int CursoDiaId { get; set; }
        public int CursoId { get; set; }
        public DiaSemana DiaSemana { get; set; }
        public Curso Curso { get; set; } = null!;
    }
}
