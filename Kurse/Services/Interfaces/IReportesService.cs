using Kurse.ViewModels.Reportes;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kurse.Services.Interfaces
{
    public interface IReportesService
    {
        Task<List<ReporteUsuarioViewModel>> GetUsuariosAsync(string? rol);
        Task<List<ReporteCursoViewModel>> GetCursosAsync(ReporteFiltroViewModel filtro);
        Task<List<ReporteInscriptosViewModel>> GetInscriptosAsync(int? cursoId);
        Task<List<ReporteNotasViewModel>> GetNotasAsync(int? cursoId);
        Task<ReportesDashboardViewModel> GetDashboardAsync();
        Task<List<SelectListItem>> GetSelectAsync(string? profesorId = null);
    }
}
