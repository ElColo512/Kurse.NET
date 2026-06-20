using Kurse.Models.Entities;
using Kurse.Models.Enums;
using Kurse.Models.Helpers;
using Kurse.Models.Requests;
using Kurse.Services.Interfaces;
using Kurse.ViewModels.Reportes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kurse.Controllers
{
    [Authorize]
    public class ReportesController(UserManager<ApplicationUser> userManager, ICursoService cursoService, IReportesService service, IPdfService pdfService) : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ICursoService _cursoService = cursoService;
        private readonly IReportesService _reportesService = service;
        private readonly IPdfService _pdfService = pdfService;
        List<string> headers = [];
        List<float> widths = [];
        List<PdfColumnAlignment> alignments = [];
        List<List<string>> rows = [];

        public async Task<IActionResult> Index()
        {
            ReportesDashboardViewModel dashboard = await _reportesService.GetDashboardAsync();

            string? profesorId = null;

            if (User.IsInRole(RoleConstants.Profesor))
            {
                profesorId = _userManager.GetUserId(User);
            }

            List<SelectListItem> cursos = await _reportesService.GetSelectAsync(profesorId);

            ReportesIndexViewModel model = new()
            {
                Dashboard = dashboard,
                Cursos = cursos
            };

            return View(model);
        }

        [Authorize(Roles = RoleConstants.Administrador + "," + RoleConstants.Administrativo)]
        public async Task<IActionResult> GetUsuarios(string? rol)
        {
            List<ReporteUsuarioViewModel> data = await _reportesService.GetUsuariosAsync(rol);
            return Json(new { data });
        }

        [Authorize(Roles = RoleConstants.Administrador + "," + RoleConstants.Administrativo)]
        public async Task<IActionResult> GetCursos(ReporteFiltroViewModel filtro)
        {
            List<ReporteCursoViewModel> data = await _reportesService.GetCursosAsync(filtro);
            return Json(new { data });
        }

        public async Task<IActionResult> GetInscriptos(int? cursoId)
        {
            if (User.IsInRole(RoleConstants.Profesor) && cursoId.HasValue)
            {
                bool pertenece = await _cursoService.CursoPerteneceProfesorAsync(cursoId.Value, _userManager.GetUserId(User)!);

                if (!pertenece)
                    return Forbid();
            }

            List<ReporteInscriptosViewModel> data = await _reportesService.GetInscriptosAsync(cursoId);
            return Json(new { data });
        }

        public async Task<IActionResult> GetNotas(int? cursoId)
        {
            if (User.IsInRole(RoleConstants.Profesor) && cursoId.HasValue)
            {
                bool pertenece = await _cursoService.CursoPerteneceProfesorAsync(cursoId.Value, _userManager.GetUserId(User)!);

                if (!pertenece)
                    return Forbid();
            }

            List<ReporteNotasViewModel> data = await _reportesService.GetNotasAsync(cursoId);
            return Json(new { data });
        }

        public async Task<IActionResult> ExportUsuariosPdf(string? rol)
        {
            List<ReporteUsuarioViewModel> usuarios = await _reportesService.GetUsuariosAsync(rol);
            bool mostrarRol = string.IsNullOrWhiteSpace(rol);
            string title = mostrarRol ? "Reporte General de Usuarios" : $"Reporte de {rol}";

            if (mostrarRol)
            {
                headers = ["Legajo", "Nombre", "DNI", "Email", "Telefono", "Rol", "Estado"];
                widths = [1 /* Legajo*/, 2 /*Nombre*/, 1 /*DNI*/, 2.5f /*Email*/, 2 /*Teléfono*/, 2 /*Rol*/, 1 /*Estado*/];
                alignments = [PdfColumnAlignment.Center /*Legajo*/, PdfColumnAlignment.Left /*Nombre*/, PdfColumnAlignment.Center /*DNI*/, PdfColumnAlignment.Left /*Email*/, PdfColumnAlignment.Left /*Teléfono*/, PdfColumnAlignment.Center /*Rol*/, PdfColumnAlignment.Center /*Estado*/];

                rows =
                [.. usuarios.Select(u =>
                new List<string>
                {
                    u.Legajo,
                    u.NombreCompleto,
                    u.Dni,
                    u.Email,
                    u.Telefono,
                    u.Rol,
                    u.Activo ? "Activo" : "Inactivo"
                })];
            }
            else
            {
                headers = ["Legajo", "Nombre", "DNI", "Email", "Telefono", "Estado"];
                widths = [1 /* Legajo*/, 2 /*Nombre*/, 1 /*DNI*/, 2.5f /*Email*/, 2 /*Teléfono*/, 1 /*Estado*/];
                alignments = [PdfColumnAlignment.Center /*Legajo*/, PdfColumnAlignment.Left /*Nombre*/, PdfColumnAlignment.Center /*DNI*/, PdfColumnAlignment.Left /*Email*/, PdfColumnAlignment.Left /*Teléfono*/, PdfColumnAlignment.Center /*Estado*/];

                rows =
                [.. usuarios.Select(u =>
                new List<string>
                {
                    u.Legajo,
                    u.NombreCompleto,
                    u.Dni,
                    u.Email,
                    u.Telefono,
                    u.Activo ? "Activo" : "Inactivo"
                })];
            }

            PdfReportRequest request = new()
            {
                Title = title,
                Landscape = true,
                Headers = headers,
                ColumnWidths = widths,
                Alignments = alignments,
                Rows = rows
            };

            byte[] pdf = _pdfService.Generate(request);

            return File(pdf, "application/pdf", "reporte-usuarios.pdf");
        }

        public async Task<IActionResult> ExportCursosPdf(ReporteFiltroViewModel filtro)
        {
            List<ReporteCursoViewModel> cursos = await _reportesService.GetCursosAsync(filtro);

            PdfReportRequest request = new()
            {
                Title = "Reporte de Cursos",
                Landscape = true,
                ColumnWidths = [1.2f /*Código*/, 3f /*Curso*/, 2.5f /*Profesor*/, 1.5f /*Inicio*/, 1.5f /*Fin*/, 1.5f /*Dias*/, 1.5f /*Horario*/, 1f /*Duración*/, 1f /*Inscriptos*/, 1.3f /*Estado*/],
                Alignments = [PdfColumnAlignment.Center /*Código*/, PdfColumnAlignment.Left /*Curso*/, PdfColumnAlignment.Left /*Profesor*/, PdfColumnAlignment.Center /*Inicio*/, PdfColumnAlignment.Center /*Fin*/, PdfColumnAlignment.Center /*Dias*/, PdfColumnAlignment.Center /*Horario*/, PdfColumnAlignment.Center /*Duración*/, PdfColumnAlignment.Center /*Inscriptos*/, PdfColumnAlignment.Center /*Estado*/],
                Headers = ["Código", "Curso", "Profesor", "Inicio", "Fin", "Dias", "Horario", "Horas", "Inscriptos", "Estado"],
                Rows = [.. cursos.Select(c =>
                new List<string>
                {
                    c.Codigo,
                    c.Curso,
                    c.Profesor,
                    c.FechaInicio.ToString("dd/MM/yyyy"),
                    c.FechaFin.ToString("dd/MM/yyyy"),
                    c.DiasDictado,
                    c.Horario,
                    $"{c.DuracionHoras} hs",
                    c.CantidadInscriptos.ToString(),
                    c.Estado
                    })]
            };

            byte[] pdf = _pdfService.Generate(request);

            return File(pdf, "application/pdf", "reporte-cursos.pdf");
        }

        public async Task<IActionResult> ExportInscriptosPdf(int? cursoId)
        {
            List<ReporteInscriptosViewModel> inscriptos = await _reportesService.GetInscriptosAsync(cursoId);
            bool esCursoEspecifico = cursoId.HasValue;
            string title = cursoId.HasValue && inscriptos.Count != 0 ? $"Inscriptos - {inscriptos.First().Curso}" : "Reporte General de Inscriptos";


            if (esCursoEspecifico)
            {
                headers = ["Legajo", "Alumno", "Fecha", "Estado"];
                widths = [1f, 3f, 2f, 2f];
                alignments = [PdfColumnAlignment.Center, PdfColumnAlignment.Left, PdfColumnAlignment.Center, PdfColumnAlignment.Center];

                rows = [.. inscriptos.Select(i => new List<string>
                {
                    i.Legajo,
                    i.Alumno,
                    i.FechaInscripcion.ToString("dd/MM/yyyy"),
                    i.EstadoAcademico
                })];
            }
            else
            {
                headers = ["Curso", "Legajo", "Alumno", "Fecha", "Estado"];
                widths = [3f, 1f, 3f, 2f, 2f];
                alignments = [PdfColumnAlignment.Left, PdfColumnAlignment.Center, PdfColumnAlignment.Left, PdfColumnAlignment.Center, PdfColumnAlignment.Center];

                rows = [.. inscriptos.Select(i => new List<string>
                {
                    i.Curso,
                    i.Legajo,
                    i.Alumno,
                    i.FechaInscripcion.ToString("dd/MM/yyyy"),
                    i.EstadoAcademico
                })];
            }

            PdfReportRequest request = new()
            {
                Title = title,
                Landscape = true,
                Headers = headers,
                ColumnWidths = widths,
                Alignments = alignments,
                Rows = rows
            };

            byte[] pdf = _pdfService.Generate(request);

            return File(pdf, "application/pdf", "reporte-inscriptos.pdf");
        }

        public async Task<IActionResult> ExportNotasPdf(int? cursoId)
        {
            List<ReporteNotasViewModel> notas = await _reportesService.GetNotasAsync(cursoId);
            bool esCursoEspecifico = cursoId.HasValue;
            string title = esCursoEspecifico && notas.Count != 0 ? $"Notas - {notas.First().Curso}" : "Reporte General de Notas";

            if (esCursoEspecifico)
            {
                headers = ["Legajo", "Alumno", "Asistencia", "Nota", "Estado"];
                widths = [1.2f, 3f, 1.5f, 1f, 1.5f];
                alignments = [PdfColumnAlignment.Center, PdfColumnAlignment.Left, PdfColumnAlignment.Center, PdfColumnAlignment.Center, PdfColumnAlignment.Center];

                rows = [.. notas.Select(n => new List<string>
                {
                    n.Legajo,
                    n.Alumno,
                    $"{n.ClasesAsistidas}/{n.TotalClases} ({(n.TotalClases > 0 ? n.ClasesAsistidas * 100 / n.TotalClases : 0)}%)",
                    n.NotaFinal.ToString() ?? string.Empty,
                    n.Estado
                })];
            }
            else
            {
                headers = ["Curso", "Legajo", "Alumno", "Asistencia", "Nota", "Estado"];
                widths = [2.5f, 1.2f, 2.5f, 1.5f, 1f, 1.5f];
                alignments = [PdfColumnAlignment.Left, PdfColumnAlignment.Center, PdfColumnAlignment.Left, PdfColumnAlignment.Center, PdfColumnAlignment.Center, PdfColumnAlignment.Center];

                rows = [.. notas.Select(n => new List<string>
                {
                    n.Curso,
                    n.Legajo,
                    n.Alumno,
                    $"{n.ClasesAsistidas}/{n.TotalClases} ({(n.TotalClases > 0 ? n.ClasesAsistidas * 100 / n.TotalClases : 0)}%)",
                    n.NotaFinal.ToString() ?? string.Empty,
                    n.Estado
                })];
            }

            PdfReportRequest request = new()
            {
                Title = title,
                Landscape = true,
                Headers = headers,
                ColumnWidths = widths,
                Alignments = alignments,
                Rows = rows
            };

            byte[] pdf = _pdfService.Generate(request);

            return File(pdf, "application/pdf", "reporte-notas.pdf");
        }
    }
}
