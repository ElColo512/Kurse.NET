using AutoMapper;
using Kurse.Models.Entities;
using Kurse.Models.Enums;
using Kurse.Models.Extensions;
using Kurse.Repositories.Interfaces;
using Kurse.Services.Interfaces;
using Kurse.ViewModels;
using Kurse.ViewModels.Cursos;

namespace Kurse.Services.Implementations
{
    public class CursoService(ICursoRepository repository, IMapper mapper) : ICursoService
    {
        private readonly ICursoRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<List<CursoViewModel>> GetAllAsync(bool mostrarInactivos, string profesorId)
        {
            List<Curso> cursos = await _repository.GetAllAsync(mostrarInactivos, profesorId);
            return _mapper.Map<List<CursoViewModel>>(cursos);
        }

        public async Task<(bool Success, string Message)> CreateAsync(CreateCursoViewModel model, bool esProfesor)
        {
            DateOnly hoy = DateOnly.FromDateTime(DateTime.Today);

            if (model.FechaInicio < hoy)
                return (false, "La fecha de inicio no puede ser anterior a hoy.");

            if (model.FechaFin <= model.FechaInicio)
                return (false, "La fecha de finalización debe ser posterior a la fecha de inicio.");

            Curso curso = _mapper.Map<Curso>(model);
            curso.CursoDias = [.. model.DiasDictado.Select(d => new CursoDia { DiaSemana = d })];
            curso.EstadoCurso = esProfesor ? EstadoCurso.Pendiente : EstadoCurso.Autorizado;

            await _repository.AddAsync(curso);
            await _repository.SaveChangesAsync();

            return (true, "Curso creado correctamente.");
        }

        public async Task<bool> UpdateAsync(EditCursoViewModel model)
        {
            Curso? curso = await _repository.GetByIdAsync(model.CursoId);

            if (curso == null) return false;

            _mapper.Map(model, curso);
            curso.CursoDias.Clear();
            curso.CursoDias = [.. model.DiasDictado.Select(d => new CursoDia { CursoId = curso.CursoId, DiaSemana = d })];
            await _repository.UpdateAsync(curso);
            await _repository.SaveChangesAsync();
            return true;
        }

        public async Task<EditCursoViewModel?> GetEditByIdAsync(int id)
        {
            Curso? curso = await _repository.GetByIdAsync(id);

            if (curso == null) return null;

            EditCursoViewModel model = _mapper.Map<EditCursoViewModel>(curso);
            model.DiasDictado = [.. curso.CursoDias.Select(cd => cd.DiaSemana)];

            return model;
        }

        public async Task<CursoDetailsViewModel?> GetDetailsByIdAsync(int id)
        {
            Curso? curso = await _repository.GetByIdAsync(id);

            if (curso == null) return null;

            CursoDetailsViewModel model = _mapper.Map<CursoDetailsViewModel>(curso);
            model.DiasDictado = [.. curso.CursoDias.Select(cd => cd.DiaSemana)];

            return model;
        }

        public async Task<(bool Success, string Message)> DarBajaAsync(int id)
        {
            Curso? curso = await _repository.GetByIdAsync(id);

            if (curso == null)
                return (false, "Curso no encontrado.");

            if (curso.EstadoCurso == EstadoCurso.Cancelado)
                return (false, "El curso ya está dado de baja.");

            bool tieneAlumnos = await _repository.TieneAlumnosActivosAsync(id);

            if (tieneAlumnos)
                return (false, "El curso posee alumnos inscriptos.");

            curso.EstadoCurso = EstadoCurso.Cancelado;

            await _repository.UpdateAsync(curso);

            return (true, "Curso dado de baja correctamente.");
        }

        public async Task<(bool Success, string Message)> AutorizarAsync(int cursoId)
        {
            Curso? curso = await _repository.GetByIdAsync(cursoId);

            if (curso == null)
                return (false, "Curso no encontrado.");

            if (curso.EstadoCurso != EstadoCurso.Pendiente)
                return (false, "Solo pueden autorizarse cursos pendientes.");

            curso.EstadoCurso = EstadoCurso.Autorizado;
            curso.MotivoRechazo = null;
            await _repository.UpdateAsync(curso);

            return (true, "Curso autorizado correctamente.");
        }

        public async Task<(bool Success, string Message)> RechazarAsync(int cursoId, string motivo)
        {
            Curso? curso = await _repository.GetByIdAsync(cursoId);

            if (curso == null)
                return (false, "Curso no encontrado.");

            if (curso.EstadoCurso != EstadoCurso.Pendiente)
                return (false, "Solo pueden rechazarse cursos pendientes.");

            if (string.IsNullOrWhiteSpace(motivo))
                return (false, "Debe indicar un motivo.");

            curso.EstadoCurso = EstadoCurso.Rechazado;
            curso.MotivoRechazo = motivo;
            await _repository.UpdateAsync(curso);

            return (true, "Curso rechazado correctamente.");
        }

        public async Task<(bool Success, string Message)> FinalizarAsync(int cursoId)
        {
            Curso? curso = await _repository.GetByIdWithAlumnosAsync(cursoId);

            if (curso == null)
                return (false, "Curso no encontrado.");

            if (curso.EstadoCurso == EstadoCurso.Finalizado)
                return (false, "El curso ya se encuentra finalizado.");

            if (curso.EstadoCurso != EstadoCurso.Autorizado)
                return (false, "Solo pueden finalizarse cursos autorizados.");

            bool tienePendientes = curso.CursoAlumnos.Any(a => a.Estado == EstadoAcademico.Inscripto);

            if (tienePendientes)
                return (false, "Existen alumnos sin evaluar en el acta.");

            curso.EstadoCurso = EstadoCurso.Finalizado;

            await _repository.UpdateAsync(curso);

            return (true, "Curso finalizado correctamente.");
        }

        public async Task<List<CursoOfertaViewModel>> GetOfertaAcademicaAsync(string alumnoId)
        {
            List<Curso> cursos = await _repository.GetCursosDisponiblesAsync();

            return [.. cursos
                .Select(c => new CursoOfertaViewModel
                {
                    CursoId = c.CursoId,
                    NombreCurso = c.NombreCurso,
                    Profesor = $"{c.Profesor.Nombre} {c.Profesor.Apellido}",
                    FechaInicio = c.FechaInicio,
                    FechaFin = c.FechaFin,
                    HoraInicio = c.HoraInicio,
                    HoraFin = c.HoraFin,
                    DuracionHoras = c.DuracionHoras,
                    DiasDictado = string.Join(" - ", c.CursoDias.OrderBy(d => d.DiaSemana).Select(d => d.DiaSemana.ToShortName())),
                    LugarDictado = c.LugarDictado,
                    CupoMaximo = c.CupoMaximo,
                    CantidadInscriptos = c.CursoAlumnos.Count(a => a.Estado != EstadoAcademico.Baja),
                    Arancelado = c.Arancelado,
                    Arancel = c.Arancel,
                    YaInscripto = c.CursoAlumnos.Any(x => x.AlumnoId == alumnoId && x.Estado != EstadoAcademico.Baja)
                })
                .OrderBy(c => c.FechaInicio)];
        }

        public async Task<bool> CursoPerteneceProfesorAsync(int cursoId, string profesorId) => await _repository.CursoPerteneceProfesorAsync(cursoId, profesorId);
    }
}
