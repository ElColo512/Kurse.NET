namespace Kurse.ViewModels.Cursos
{
    public class CursoOfertaViewModel
    {
        public int CursoId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string NombreCurso { get; set; } = string.Empty;
        public string Profesor { get; set; } = string.Empty;
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        public TimeOnly HoraInicio { get; set; }
        public TimeOnly HoraFin { get; set; }
        public int DuracionHoras { get; set; }
        public string DiasDictado { get; set; } = string.Empty;
        public string LugarDictado { get; set; } = string.Empty;
        public int CupoMaximo { get; set; }
        public int CantidadInscriptos { get; set; }
        public bool Arancelado { get; set; }
        public decimal? Arancel { get; set; }
        public bool YaInscripto { get; set; }
    }
}
