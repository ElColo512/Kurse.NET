namespace Kurse.ViewModels
{
    public class AlumnoCursoViewModel
    {
        public string AlumnoId { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime FechaInscripcion { get; set; }
    }
}
