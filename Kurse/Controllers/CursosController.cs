using Kurse.Models.Entities;
using Kurse.Models.Enums;
using Kurse.Models.Helpers;
using Kurse.Services.Interfaces;
using Kurse.ViewModels;
using Kurse.ViewModels.Actas;
using Kurse.ViewModels.Cursos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kurse.Controllers
{
    [Authorize]
    public class CursosController(ICursoService service, UserManager<ApplicationUser> userManager, ILogger<CursosController> logger, ICursoAlumnosService cursoAlumnosService) : Controller
    {
        private readonly ICursoService _service = service;
        private readonly ICursoAlumnosService _cursoAlumnosService = cursoAlumnosService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ILogger<CursosController> _logger = logger;

        public ActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetAll(bool mostrarInactivos = false)
        {
            string? profesorId = null;

            if (User.IsInRole(RoleConstants.Profesor))
            {
                profesorId = _userManager.GetUserId(User);
            }

            List<CursoViewModel> cursos = await _service.GetAllAsync(mostrarInactivos, profesorId!);
            return Json(new { data = cursos });
        }

        public async Task<IActionResult> Details(int id)
        {
            CursoDetailsViewModel? curso = await _service.GetDetailsByIdAsync(id);

            if (curso == null) return NotFound();

            return PartialView("_DetailsCurso", curso);
        }

        public async Task<IActionResult> Create()
        {
            await CargarCombos();
            CreateCursoViewModel model = new()
            {
                FechaInicio = DateOnly.FromDateTime(DateTime.Today),
                FechaFin = DateOnly.FromDateTime(DateTime.Today.AddMonths(1))
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCursoViewModel model)
        {
            try
            {
                if (User.IsInRole(RoleConstants.Profesor))
                {
                    model.IdProfesor = _userManager.GetUserId(User)!;
                    ModelState.Remove(nameof(model.IdProfesor));
                }

                if (!ModelState.IsValid)
                {
                    await CargarCombos();
                    return View(model);
                }

                if (User.IsInRole(RoleConstants.Profesor))
                {
                    model.IdProfesor = _userManager.GetUserId(User)!;
                }

                (bool Success, string? Message) = await _service.CreateAsync(model, User.IsInRole(RoleConstants.Profesor));

                if (!Success)
                {
                    ModelState.AddModelError(string.Empty, Message);

                    SetToast(Message, "bg-danger");

                    await CargarCombos();

                    return View(model);
                }

                SetToast("Curso creado correctamente.", "bg-success");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el curso.");
                ModelState.AddModelError(string.Empty, "Ocurrió un error inesperado.");
                SetToast("Ocurrió un error inesperado.", "bg-danger");
                await CargarCombos();
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            EditCursoViewModel? model = await _service.GetEditByIdAsync(id);

            if (model == null) return NotFound();

            await CargarCombos();
            return PartialView("_EditCurso", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditCursoViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await CargarCombos();

                    return PartialView("_EditCurso", model);
                }

                if (model.EstadoCurso == EstadoCurso.Cancelado)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No es posible modificar un curso cancelado."
                    });
                }

                bool updated = await _service.UpdateAsync(model);

                if (!updated)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se encontró el curso."
                    });
                }

                return Json(new
                {
                    success = true,
                    message = "Curso actualizado correctamente."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar el curso.");

                return Json(new
                {
                    success = false,
                    message = "Ocurrió un error inesperado."
                });
            }
        }

        public async Task<IActionResult> Inscriptos(int id)
        {
            CursoInscriptosViewModel? model = await _cursoAlumnosService.GetInscriptosModalAsync(id);

            if (model == null) return NotFound();

            return PartialView("_InscriptosCurso", model);
        }

        public async Task<IActionResult> GetInscriptos(int cursoId)
        {
            List<AlumnoCursoViewModel> alumnos = await _cursoAlumnosService.GetInscriptosRowsAsync(cursoId);

            return Json(new { data = alumnos ?? [] });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InscribirAlumno(int cursoId, string alumnoId)
        {
            if (string.IsNullOrEmpty(alumnoId))
            {
                return Json(new { success = false, message = "Seleccione un alumno." });
            }

            (bool Success, string? Message) = await _cursoAlumnosService.InscribirAlumnoAsync(cursoId, alumnoId);

            return Json(new
            {
                success = Success,
                message = Message
            });
        }

        public async Task<IActionResult> Acta(int id)
        {
            if (User.IsInRole(RoleConstants.Profesor))
            {
                string? profesorId = _userManager.GetUserId(User);

                bool pertenece = await _service.CursoPerteneceProfesorAsync(id, profesorId!);

                if (!pertenece)
                    return Forbid();
            }

            CursoActaViewModel? model = await _cursoAlumnosService.GetActaModalAsync(id);

            if (model == null) return NotFound();

            return PartialView("_ActaCurso", model);
        }

        public async Task<IActionResult> GetActa(int cursoId)
        {
            List<ActaAlumnoViewModel> data = await _cursoAlumnosService.GetActaRowsAsync(cursoId);
            return Json(new { data });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarActa(UpdateActaViewModel model)
        {
            if (User.IsInRole(RoleConstants.Profesor))
            {
                string? profesorId = _userManager.GetUserId(User);

                bool pertenece = await _service.CursoPerteneceProfesorAsync(model.CursoId, profesorId!);

                if (!pertenece)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No posee permisos para modificar este curso."
                    });
                }
            }

            (bool Success, string? Message) = await _cursoAlumnosService.UpdateActaAsync(model);

            return Json(new
            {
                success = Success,
                message = Message
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoAlumno(int id, EstadoAcademico estado)
        {
            (bool Success, string Message) = await _cursoAlumnosService.CambiarEstadoAlumnoAsync(id, estado);

            return Json(new
            {
                success = Success,
                message = Message
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            (bool Success, string? Message) = await _service.DarBajaAsync(id);

            return Json(new
            {
                success = Success,
                message = Message
            });
        }

        public async Task<IActionResult> Revisar(int id)
        {
            CursoDetailsViewModel? model = await _service.GetDetailsByIdAsync(id);

            if (model == null)
                return NotFound();

            model.PuedeRevisar = true;

            return PartialView("_RevisionCurso", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Autorizar(int id)
        {
            (bool Success, string Message) = await _service.AutorizarAsync(id);

            return Json(new
            {
                success = Success,
                message = Message
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(int id, string motivo)
        {
            (bool Success, string Message) = await _service.RechazarAsync(id, motivo);

            return Json(new
            {
                success = Success,
                message = Message
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(int id)
        {
            if (User.IsInRole(RoleConstants.Profesor))
            {
                string? profesorId = _userManager.GetUserId(User);

                bool pertenece = await _service.CursoPerteneceProfesorAsync(id, profesorId!);

                if (!pertenece)
                {
                    return Json(new
                    {
                        success = false,
                        message = "No posee permisos para finalizar este curso."
                    });
                }
            }

            (bool Success, string Message) = await _service.FinalizarAsync(id);

            return Json(new
            {
                success = Success,
                message = Message
            });
        }

        private async Task CargarCombos()
        {
            ViewBag.TiposAprobacion = Enum.GetValues<TipoAprobacion>().Cast<TipoAprobacion>().Select(t => new SelectListItem { Value = t.ToString(), Text = t.ToString() }).ToList();
            List<ApplicationUser> usuarios = [.. _userManager.Users];
            List<SelectListItem> profesores = [];

            foreach (ApplicationUser user in usuarios)
            {
                if (await _userManager.IsInRoleAsync(user, "Profesor"))
                {
                    profesores.Add(new SelectListItem
                    {
                        Value = user.Id,
                        Text = $"{user.Nombre} {user.Apellido}"
                    });
                }
            }

            ViewBag.Profesores = profesores;
        }

        private void SetToast(string message, string type)
        {
            TempData["ToastMessage"] = message;
            TempData["ToastClass"] = type;
        }
    }
}
