using Kurse.Models.Entities;
using Kurse.Models.Helpers;
using Kurse.Services.Interfaces;
using Kurse.ViewModels.Alumno;
using Kurse.ViewModels.Cursos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Kurse.Controllers
{
    [Authorize(Roles = RoleConstants.Alumno)]
    public class AlumnosController(ICursoService service, UserManager<ApplicationUser> userManager, ICursoAlumnosService cursoAlumnosService) : Controller
    {
        private readonly ICursoService _service = service;
        private readonly ICursoAlumnosService _cursoAlumnosService = cursoAlumnosService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public async Task<IActionResult> OfertaAcademica()
        {
            string? alumnoId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(alumnoId))
            {
                return RedirectToAction("Login", "Account");
            }

            List<CursoOfertaViewModel> model = await _service.GetOfertaAcademicaAsync(alumnoId);

            return View(model);
        }

        public async Task<IActionResult> HistoriaAcademica()
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            HistoriaAcademicaViewModel model = await _cursoAlumnosService.GetHistoriaAcademicaAsync(user.Id);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribirse(int cursoId)
        {
            try
            {
                string? alumnoId = _userManager.GetUserId(User);

                if (string.IsNullOrEmpty(alumnoId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Usuario no encontrado."
                    });
                }

                (bool Success, string? Message) = await _cursoAlumnosService.InscribirseAsync(cursoId, alumnoId);

                return Json(new
                {
                    success = Success,
                    message = Message
                });
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    message = "Ocurrió un error inesperado."
                });
            }
        }

        public async Task<IActionResult> Certificado(int cursoId)
        {
            ApplicationUser? user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            byte[]? pdf = await _cursoAlumnosService.GenerarCertificadoAsync(user.Id, cursoId);

            if (pdf == null)
                return Forbid();

            return File(pdf, "application/pdf", $"certificado-{cursoId}.pdf");
        }
    }
}
