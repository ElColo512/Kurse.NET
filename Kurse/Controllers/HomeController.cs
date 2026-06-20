using Kurse.Models;
using Kurse.Models.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Kurse.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.IsInRole(RoleConstants.Alumno))
            {
                return RedirectToAction("OfertaAcademica", "Alumnos");
            }

            return RedirectToAction("Index", "Cursos");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
