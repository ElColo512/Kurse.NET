using AutoMapper;
using Kurse.Models.Entities;
using Kurse.Models.Extensions;
using Kurse.ViewModels.Reportes;

namespace Kurse.Mappers
{
    public class ReporteProfile : Profile
    {
        public ReporteProfile()
        {
            CreateMap<Curso, ReporteCursoViewModel>()
                .ForMember(
                    dest => dest.Curso,
                    opt => opt.MapFrom(src => src.NombreCurso))
                .ForMember(
                    dest => dest.Profesor,
                    opt => opt.MapFrom(src => $"{src.Profesor.Nombre} {src.Profesor.Apellido}"))
                .ForMember(
                    dest => dest.DiasDictado,
                    opt => opt.MapFrom(src => string.Join(" - ", src.CursoDias.OrderBy(d => d.DiaSemana).Select(d => d.DiaSemana.ToShortName()))))
                .ForMember(
                    dest => dest.Estado,
                    opt => opt.MapFrom(src => src.EstadoCurso.ToString()))
                .ForMember(
                    dest => dest.CantidadInscriptos,
                    opt => opt.MapFrom(src => src.CursoAlumnos.Count));

            CreateMap<CursoAlumnos, ReporteInscriptosViewModel>()
                 .ForMember(
                    dest => dest.Legajo,
                    opt => opt.MapFrom(src => src.Alumno.Legajo))
                   .ForMember(
                    dest => dest.Email,
                    opt => opt.MapFrom(src => src.Alumno.Email))
                .ForMember(
                    dest => dest.Curso,
                    opt => opt.MapFrom(src => src.Curso.NombreCurso))
                .ForMember(
                    dest => dest.Alumno,
                    opt => opt.MapFrom(src => $"{src.Alumno.Nombre} {src.Alumno.Apellido}"))
                .ForMember(
                    dest => dest.FechaInscripcion,
                    opt => opt.MapFrom(src => DateOnly.FromDateTime(src.FechaInscripcion)))
                .ForMember(
                    dest => dest.EstadoAcademico,
                    opt => opt.MapFrom(src => src.Estado.ToString()));

            CreateMap<CursoAlumnos, ReporteNotasViewModel>()
                 .ForMember(
                    dest => dest.Legajo,
                    opt => opt.MapFrom(src => src.Alumno.Legajo))
                .ForMember(
                    dest => dest.Alumno,
                    opt => opt.MapFrom(src => $"{src.Alumno.Nombre} {src.Alumno.Apellido}"))
                .ForMember(
                    dest => dest.Curso,
                    opt => opt.MapFrom(src => src.Curso.NombreCurso))
                .ForMember(
                    dest => dest.ClasesAsistidas,
                    opt => opt.MapFrom(src => src.Asistencia ?? 0))
                .ForMember(
                    dest => dest.TotalClases,
                    opt => opt.MapFrom(src => src.Curso.CantidadClases))
                .ForMember(
                    dest => dest.Estado,
                    opt => opt.MapFrom(src => src.Estado.ToString()));
        }
    }
}
