using AutoMapper;
using Kurse.Models.Entities;
using Kurse.Models.Enums;
using Kurse.Models.Helpers;
using Kurse.Repositories.Interfaces;
using Kurse.Services.Interfaces;
using Kurse.ViewModels;
using Kurse.ViewModels.Actas;
using Kurse.ViewModels.Alumno;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kurse.Services.Implementations
{
    public class CursoAlumnosService(ICursoAlumnosRepository cursoAlumnosRepository, ICursoRepository cursoRepository, UserManager<ApplicationUser> userManager, IMapper mapper, IPdfService pdfService) : ICursoAlumnosService
    {
        private readonly ICursoAlumnosRepository _cursoAlumnosRepository = cursoAlumnosRepository;
        private readonly ICursoRepository _cursoRepository = cursoRepository;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IMapper _mapper = mapper;
        private readonly IPdfService _pdfService = pdfService;

        public async Task<CursoInscriptosViewModel?> GetInscriptosModalAsync(int cursoId)
        {
            Curso? curso = await _cursoRepository.GetByIdWithAlumnosAsync(cursoId);

            if (curso == null) return null;

            List<SelectListItem> alumnosDisponibles = await GetAlumnosDisponiblesAsync(cursoId);

            return new CursoInscriptosViewModel
            {
                CursoId = curso.CursoId,
                NombreCurso = curso.NombreCurso,
                AlumnosDisponibles = alumnosDisponibles,
                Alumnos = _mapper.Map<List<AlumnoCursoViewModel>>(curso.CursoAlumnos),
                EstaFinalizado = curso.EstadoCurso == EstadoCurso.Finalizado
            };
        }

        public async Task<List<SelectListItem>> GetAlumnosDisponiblesAsync(int cursoId)
        {
            Curso? curso = await _cursoRepository.GetByIdWithAlumnosAsync(cursoId);

            if (curso == null) return [];

            List<string> alumnosInscriptos = [.. curso.CursoAlumnos.Select(ca => ca.AlumnoId)];

            IList<ApplicationUser> alumnos = await _userManager.GetUsersInRoleAsync(RoleConstants.Alumno);

            return [.. alumnos.Where(a => a.Activo && !alumnosInscriptos.Contains(a.Id)).Select(a => new SelectListItem { Value = a.Id, Text = $"{a.Nombre} {a.Apellido}" })];
        }

        public async Task<(bool Success, string Message)> InscribirAlumnoAsync(int cursoId, string alumnoId)
        {
            Curso? curso = await _cursoRepository.GetByIdWithAlumnosAsync(cursoId);

            if (curso == null)
                return (false, "Curso no encontrado.");

            if (curso.EstadoCurso == EstadoCurso.Finalizado)
                return (false, "No es posible inscribir alumnos en un curso finalizado.");

            bool yaInscripto = curso.CursoAlumnos.Any(ca => ca.AlumnoId == alumnoId);

            if (yaInscripto) return (false, "El alumno ya está inscripto.");

            if (curso.CursoAlumnos.Count >= curso.CupoMaximo)
                return (false, "No hay cupos disponibles.");

            await _cursoAlumnosRepository.AddAsync(
                new CursoAlumnos
                {
                    CursoId = cursoId,
                    AlumnoId = alumnoId,
                    FechaInscripcion = DateTime.Now,
                    Estado = EstadoAcademico.Inscripto
                });

            await _cursoAlumnosRepository.SaveChangesAsync();
            return (true, "Alumno inscripto correctamente.");
        }

        public async Task<List<AlumnoCursoViewModel>> GetInscriptosRowsAsync(int cursoId)
        {
            Curso? curso = await _cursoRepository.GetByIdWithAlumnosAsync(cursoId);

            if (curso == null) return [];

            return _mapper.Map<List<AlumnoCursoViewModel>>(curso.CursoAlumnos);
        }

        public async Task<CursoActaViewModel?> GetActaModalAsync(int cursoId)
        {
            Curso? curso = await _cursoRepository.GetByIdWithAlumnosAsync(cursoId);

            if (curso == null) return null;

            return new CursoActaViewModel
            {
                CursoId = curso.CursoId,
                NombreCurso = curso.NombreCurso,
                Alumnos = _mapper.Map<List<ActaAlumnoViewModel>>(curso.CursoAlumnos),
                EstaFinalizado = curso.EstadoCurso == EstadoCurso.Finalizado
            };
        }

        public async Task<(bool Success, string Message)> UpdateActaAsync(UpdateActaViewModel model)
        {
            List<int> ids = [.. model.Alumnos.Select(a => a.CursoAlumnoId)];

            List<CursoAlumnos> entities = await _cursoAlumnosRepository.GetCursoAlumnosByIdsAsync(ids);
            Curso? curso = await _cursoRepository.GetByIdAsync(model.CursoId);

            if (curso == null)
                return (false, "Curso no encontrado.");

            if (curso.FechaFin < DateOnly.FromDateTime(DateTime.Today))
                return (false, "El curso ya finalizó.");

            if (curso.EstadoCurso == EstadoCurso.Finalizado)
                return (false, "El curso se encuentra finalizado y el acta no puede modificarse.");

            foreach (var alumno in model.Alumnos)
            {
                if (alumno.NotaFinal.HasValue && (alumno.NotaFinal < 0 || alumno.NotaFinal > 10))
                    return (false, "La nota debe estar entre 0 y 10.");

                if (alumno.Asistencias.HasValue && (alumno.Asistencias < 0 || alumno.Asistencias > curso.CantidadClases))
                    return (false, $"La asistencia no puede superar la cantidad de clases máxima del curso que es: {curso.CantidadClases}.");

                CursoAlumnos? entity = entities.FirstOrDefault(e => e.CursoAlumnosId == alumno.CursoAlumnoId);

                if (entity == null) continue;

                if (entity.Estado == EstadoAcademico.Baja) continue;

                entity.Asistencia = alumno.Asistencias;
                entity.NotaFinal = alumno.NotaFinal;
                entity.Estado = CalcularEstado(curso, entity);
            }

            await _cursoRepository.SaveChangesAsync();
            return (true, "Acta guardada correctamente.");
        }

        public async Task<List<ActaAlumnoViewModel>> GetActaRowsAsync(int cursoId)
        {
            Curso? curso = await _cursoRepository.GetByIdWithAlumnosAsync(cursoId);

            if (curso == null) return [];

            return _mapper.Map<List<ActaAlumnoViewModel>>(curso.CursoAlumnos);
        }

        public async Task<(bool Success, string Message)> CambiarEstadoAlumnoAsync(int cursoAlumnoId, EstadoAcademico estado)
        {
            CursoAlumnos? entity = await _cursoAlumnosRepository.GetCursoAlumnoByIdAsync(cursoAlumnoId);

            if (entity == null)
                return (false, "Alumno no encontrado.");

            if (entity.Curso.EstadoCurso == EstadoCurso.Finalizado)
                return (false, "No es posible modificar alumnos de un curso finalizado.");

            entity.Estado = estado;

            await _cursoAlumnosRepository.SaveChangesAsync();

            return (true, "Estado actualizado correctamente.");
        }

        public async Task<(bool Success, string Message)> InscribirseAsync(int cursoId, string alumnoId)
        {
            Curso? curso = await _cursoRepository.GetByIdAsync(cursoId);

            if (curso == null)
                return (false, "Curso no encontrado.");

            if (curso.EstadoCurso != EstadoCurso.Autorizado)
                return (false, "El curso no se encuentra disponible.");

            if (curso.FechaFin < DateOnly.FromDateTime(DateTime.Today))
                return (false, "El curso ya finalizó.");

            bool yaInscripto = await _cursoAlumnosRepository.AlumnoYaInscriptoAsync(cursoId, alumnoId);

            if (yaInscripto)
                return (false, "Ya te encuentras inscripto.");

            int inscriptos = curso.CursoAlumnos.Count(x => x.Estado != EstadoAcademico.Baja);

            if (inscriptos >= curso.CupoMaximo)
                return (false, "No hay cupos disponibles.");

            await _cursoAlumnosRepository.InscribirAlumnoAsync(
                new CursoAlumnos
                {
                    CursoId = cursoId,
                    AlumnoId = alumnoId,
                    FechaInscripcion = DateTime.Now,
                    Estado = EstadoAcademico.Inscripto
                });

            await _cursoAlumnosRepository.SaveChangesAsync();
            return (true, "Inscripción realizada correctamente.");
        }

        public async Task<HistoriaAcademicaViewModel> GetHistoriaAcademicaAsync(string alumnoId)
        {
            List<CursoAlumnos> inscripciones = await _cursoAlumnosRepository.GetHistoriaAcademicaAsync(alumnoId);

            HistoriaAcademicaViewModel model = new()
            {
                TotalCursos = inscripciones.Count,
                CursosAprobados = inscripciones.Count(c => c.Estado == EstadoAcademico.Aprobado),
                CursosEnCurso = inscripciones.Count(c => c.Estado == EstadoAcademico.Inscripto)
            };

            List<decimal> notas = [.. inscripciones.Where(c => c.NotaFinal.HasValue).Select(c => c.NotaFinal!.Value)];

            model.PromedioGeneral = notas.Count != 0 ? Math.Round(notas.Average(), 2) : 0;
            model.CursosActivos = [.. inscripciones.Where(c => c.Estado == EstadoAcademico.Inscripto).Select(c => _mapper.Map<HistoriaCursoViewModel>(c))];
            model.CursosFinalizados = [.. inscripciones.Where(c => c.Estado != EstadoAcademico.Inscripto).Select(c => _mapper.Map<HistoriaCursoViewModel>(c))];

            return model;
        }

        private static EstadoAcademico CalcularEstado(Curso curso, CursoAlumnos alumno)
        {
            decimal porcentajeAsistencia = alumno.Asistencia.HasValue && curso.CantidadClases > 0 ? (alumno.Asistencia.Value * 100m) / curso.CantidadClases : 0;

            return curso.TipoAprobacion switch
            {
                TipoAprobacion.Examen when !alumno.NotaFinal.HasValue => EstadoAcademico.Inscripto,
                TipoAprobacion.Examen => alumno.NotaFinal >= 4 ? EstadoAcademico.Aprobado : EstadoAcademico.Desaprobado,
                TipoAprobacion.Asistencia when !alumno.Asistencia.HasValue => EstadoAcademico.Inscripto,
                TipoAprobacion.Asistencia => porcentajeAsistencia >= 80 ? EstadoAcademico.Aprobado : EstadoAcademico.Desaprobado,
                _ => EstadoAcademico.Inscripto
            };
        }

        public async Task<byte[]?> GenerarCertificadoAsync(string alumnoId, int cursoId)
        {
            CursoAlumnos? inscripcion =
                await _cursoAlumnosRepository.GetCertificadoAsync(alumnoId, cursoId);

            if (inscripcion == null)
                return null;

            bool puedeEmitirse =
                inscripcion.Estado == EstadoAcademico.Aprobado &&
                inscripcion.Curso.EstadoCurso == EstadoCurso.Finalizado;

            if (!puedeEmitirse)
                return null;

            CertificadoViewModel model = new()
            {
                Alumno = $"{inscripcion.Alumno.Nombre} {inscripcion.Alumno.Apellido}",
                Legajo = inscripcion.Alumno.Legajo,
                Curso = inscripcion.Curso.NombreCurso,
                Profesor = $"{inscripcion.Curso.Profesor.Nombre} {inscripcion.Curso.Profesor.Apellido}",
                FechaInicio = inscripcion.Curso.FechaInicio,
                FechaFin = inscripcion.Curso.FechaFin,
                DuracionHoras = inscripcion.Curso.DuracionHoras,
                FechaEmision = DateOnly.FromDateTime(DateTime.Today)
            };

            return _pdfService.GenerateCertificate(model);
        }
    }
}
