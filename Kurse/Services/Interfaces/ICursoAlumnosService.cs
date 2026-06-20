using Kurse.Models.Enums;
using Kurse.ViewModels;
using Kurse.ViewModels.Actas;
using Kurse.ViewModels.Alumno;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kurse.Services.Interfaces
{
    public interface ICursoAlumnosService
    {
        Task<List<AlumnoCursoViewModel>> GetInscriptosRowsAsync(int cursoId);
        Task<CursoInscriptosViewModel?> GetInscriptosModalAsync(int cursoId);
        Task<List<SelectListItem>> GetAlumnosDisponiblesAsync(int cursoId);
        Task<(bool Success, string Message)> InscribirAlumnoAsync(int cursoId, string alumnoId);
        Task<List<ActaAlumnoViewModel>> GetActaRowsAsync(int cursoId);
        Task<CursoActaViewModel?> GetActaModalAsync(int cursoId);
        Task<(bool Success, string Message)> UpdateActaAsync(UpdateActaViewModel model);
        Task<(bool Success, string Message)> CambiarEstadoAlumnoAsync(int cursoAlumnoId, EstadoAcademico estado);
        Task<(bool Success, string Message)> InscribirseAsync(int cursoId, string alumnoId);
        Task<HistoriaAcademicaViewModel> GetHistoriaAcademicaAsync(string alumnoId);
        Task<byte[]?> GenerarCertificadoAsync(string alumnoId, int cursoId);
    }
}
