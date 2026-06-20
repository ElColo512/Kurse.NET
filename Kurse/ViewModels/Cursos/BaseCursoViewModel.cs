using Kurse.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Kurse.ViewModels.Cursos
{
    public class BaseCursoViewModel : IValidatableObject
    {
        [Display(Name = "Código")]
        [Required(ErrorMessage = "El Código del Curso es requerido.")]
        [StringLength(20)]
        public string Codigo { get; set; } = string.Empty;
        [Required(ErrorMessage = "El Nombre del Curso es requerido.")]
        [StringLength(100)]
        [Display(Name = "Nombre del Curso")]
        public string NombreCurso { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Profesor es requerido.")]
        public string IdProfesor { get; set; } = string.Empty;

        [Required(ErrorMessage = "El contenido es requerido.")]
        [StringLength(1000)]
        [Display(Name = "Contenido")]
        public string ContenidoCurso { get; set; } = string.Empty;

        [Required(ErrorMessage = "La Fecha de inicio es requerida.")]
        [Display(Name = "Fecha de inicio")]
        public DateOnly FechaInicio { get; set; }

        [Required(ErrorMessage = "La Fecha de Finalización es requerida.")]
        [Display(Name = "Fecha de Finalización")]
        public DateOnly FechaFin { get; set; }

        [Required(ErrorMessage = "La Hora de Inicio es requerida.")]
        [Display(Name = "Hora de Finalización")]
        public TimeOnly HoraInicio { get; set; }
        [Required(ErrorMessage = "La Hora de Finalización es requerida.")]
        [Display(Name = "Hora de Finalización")]
        public TimeOnly HoraFin { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public List<DiaSemana> DiasDictado { get; set; } = [];
        [Display(Name = "Carga horaria (hs)")]
        [Range(1, 1000)]
        [Required(ErrorMessage = "El campo {0} es requerido.")]
        public int DuracionHoras { get; set; }

        [Required(ErrorMessage = "El Tipo de Aprobación es requerido.")]
        [Display(Name = "Tipo de Aprobación")]
        public TipoAprobacion? TipoAprobacion { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido.")]
        [Range(1, 9999)]
        [Display(Name = "Cupo Máximo")]
        public int CupoMaximo { get; set; }

        public bool Arancelado { get; set; }

        [Range(0, 9999999)]
        public decimal? Arancel { get; set; }

        [Required(ErrorMessage = "El lugar de dictado es requerido.")]
        [Display(Name = "Lugar de Dictado")]
        public string LugarDictado { get; set; } = string.Empty;

        [Required(ErrorMessage = "La Cantidad de Clases es requerida.")]
        [Range(1, 999)]
        [Display(Name = "Cantidad de Clases")]
        public int CantidadClases { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FechaFin < FechaInicio)
                yield return new ValidationResult("La fecha de finalización no puede ser menor a la fecha de inicio.", [nameof(FechaFin)]);

            if (HoraInicio < new TimeOnly(8, 0) || HoraInicio > new TimeOnly(22, 0))
                yield return new ValidationResult("La hora de inicio debe estar entre las 08:00 y las 22:00.", [nameof(HoraInicio)]);

            if (HoraFin < new TimeOnly(8, 0) || HoraFin > new TimeOnly(22, 0))
                yield return new ValidationResult("La hora de finalización debe estar entre las 08:00 y las 22:00.", [nameof(HoraFin)]);

            if (Arancelado && !Arancel.HasValue)
                yield return new ValidationResult("Debe ingresar un arancel.", [nameof(Arancel)]);
        }
    }
}
