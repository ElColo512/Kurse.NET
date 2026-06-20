using AutoMapper;
using Kurse.Models.Entities;
using Kurse.Repositories.Interfaces;
using Kurse.Services.Interfaces;
using Kurse.ViewModels.Reportes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Kurse.Services.Implementations
{
    public class ReportesService(IReportesRepository repository, UserManager<ApplicationUser> userManager, IMapper mapper) : IReportesService
    {
        private readonly IReportesRepository _repository = repository;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IMapper _mapper = mapper;

        public async Task<List<ReporteUsuarioViewModel>> GetUsuariosAsync(string? rol)
        {
            List<ApplicationUser> usuarios = await _userManager.Users.ToListAsync();

            List<ReporteUsuarioViewModel> resultado = [];

            foreach (ApplicationUser usuario in usuarios)
            {
                IList<string> roles = await _userManager.GetRolesAsync(usuario);

                string rolUsuario = roles.FirstOrDefault() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(rol) && rolUsuario != rol) continue;

                resultado.Add(
                    new ReporteUsuarioViewModel
                    {
                        Legajo = usuario.Legajo,
                        NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                        Dni = usuario.Dni,
                        Email = usuario.Email ?? string.Empty,
                        Telefono = usuario.PhoneNumber ?? string.Empty,
                        Rol = rolUsuario,
                        Activo = usuario.Activo
                    });
            }

            return resultado;
        }

        public async Task<List<ReporteCursoViewModel>> GetCursosAsync(ReporteFiltroViewModel filtro)
        {
            List<Curso> cursos = await _repository.GetCursosAsync(filtro.FechaDesde, filtro.FechaHasta);
            return _mapper.Map<List<ReporteCursoViewModel>>(cursos);
        }

        public async Task<ReportesDashboardViewModel> GetDashboardAsync()
        {
            return new ReportesDashboardViewModel
            {
                TotalAlumnos = await _repository.GetTotalAlumnosAsync(),
                TotalProfesores = await _repository.GetTotalProfesoresAsync(),
                TotalCursos = await _repository.GetTotalCursosAsync(),
                TotalInscripciones = await _repository.GetTotalInscripcionesAsync()
            };
        }

        public async Task<List<ReporteInscriptosViewModel>> GetInscriptosAsync(int? cursoId)
        {
            List<CursoAlumnos> entities = await _repository.GetInscriptosAsync(cursoId);
            return _mapper.Map<List<ReporteInscriptosViewModel>>(entities);
        }

        public async Task<List<ReporteNotasViewModel>> GetNotasAsync(int? cursoId)
        {
            List<CursoAlumnos> entities = await _repository.GetNotasAsync(cursoId);
            return _mapper.Map<List<ReporteNotasViewModel>>(entities);
        }

        public async Task<List<SelectListItem>> GetSelectAsync(string? profesorId = null)
        {
            List<Curso> cursos = await _repository.GetAllCursosAsync(profesorId);

            return [.. cursos.Select(c =>
            new SelectListItem
            {
                Value = c.CursoId.ToString(),
                Text = c.NombreCurso
            })
            ];
        }
    }
}
