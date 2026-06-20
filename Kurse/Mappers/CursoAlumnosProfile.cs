using AutoMapper;
using Kurse.Models.Entities;
using Kurse.Models.Enums;
using Kurse.ViewModels;
using Kurse.ViewModels.Actas;
using Kurse.ViewModels.Alumno;

namespace Kurse.Mappers
{
    public class CursoAlumnosProfile : Profile
    {
        public CursoAlumnosProfile()
        {
            CreateMap<CursoAlumnos, AlumnoCursoViewModel>()
                .ForMember(
                dest => dest.NombreCompleto,
                opt => opt.MapFrom(src => $"{src.Alumno.Nombre} {src.Alumno.Apellido}"))
            .ForMember(
                dest => dest.Email,
                opt => opt.MapFrom(src => src.Alumno.Email));

            CreateMap<CursoAlumnos, ActaAlumnoViewModel>()
                .ForMember(
                dest => dest.CursoAlumnoId,
                opt => opt.MapFrom(src => src.CursoAlumnosId)
                )
                .ForMember(
                dest => dest.NombreCompleto,
                opt => opt.MapFrom(src => $"{src.Alumno.Nombre} {src.Alumno.Apellido}")
                )
                .ForMember(
                dest => dest.Asistencias,
                opt => opt.MapFrom(src => src.Asistencia)
                )
                .ForMember(
                dest => dest.Estado,
                opt => opt.MapFrom(src => src.Estado.ToString()));

            CreateMap<CursoAlumnos, HistoriaCursoViewModel>()
                .ForMember(
                dest => dest.NombreCurso,
                opt => opt.MapFrom(src => src.Curso.NombreCurso))
                .ForMember(
                dest => dest.Profesor,
                opt => opt.MapFrom(src => $"{src.Curso.Profesor.Nombre} {src.Curso.Profesor.Apellido}"))
                .ForMember(
                dest => dest.FechaInicio,
                opt => opt.MapFrom(src => src.Curso.FechaInicio))
                .ForMember(
                dest => dest.FechaFin,
                opt => opt.MapFrom(src => src.Curso.FechaFin))
                .ForMember(
                dest => dest.PuedeDescargarCertificado,
                opt => opt.MapFrom(src => src.Estado == EstadoAcademico.Aprobado && src.Curso.EstadoCurso == EstadoCurso.Finalizado))
                .ForMember(
                dest => dest.PorcentajeAsistencia,
                opt => opt.MapFrom(src => src.Curso.CantidadClases > 0 ? (src.Asistencia ?? 0) * 100 / src.Curso.CantidadClases : 0))
                .ForMember(
                dest => dest.CantidadClases,
                opt => opt.MapFrom(src => src.Curso.CantidadClases));
        }
    }
}
