using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kurse.ViewModels.Reportes
{
    public class ReportesIndexViewModel
    {
        public ReportesDashboardViewModel Dashboard { get; set; } = new ReportesDashboardViewModel();
        public List<SelectListItem> Cursos { get; set; } = [];
    }
}
