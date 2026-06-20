using AutoMapper;
using Kurse.Models.Entities;
using Kurse.Models.Enums;
using Kurse.ViewModels;
using Kurse.ViewModels.Cursos;

namespace Kurse.Mappers
{
    public class CursoProfile : Profile
    {
        public CursoProfile()
        {
            CreateMap<Curso, CursoViewModel>()
                .ForMember(
                dest => dest.Profesor,
                opt => opt.MapFrom(src => $"{src.Profesor.Nombre} {src.Profesor.Apellido}"))
                .ForMember(
                dest => dest.EstadoCurso,
                opt => opt.MapFrom(src => src.EstadoCurso.ToString()))
                .ForMember(
                dest => dest.Anotados,
                opt => opt.MapFrom(src => src.CursoAlumnos.Count(ca => ca.Estado != EstadoAcademico.Baja)));

            CreateMap<Curso, CursoDetailsViewModel>()
                .ForMember(
                dest => dest.Profesor,
                opt => opt.MapFrom(src => $"{src.Profesor.Nombre} {src.Profesor.Apellido}"))
                .ForMember(
                dest => dest.EstadoCurso,
                opt => opt.MapFrom(src => src.EstadoCurso.ToString()))
                .ForMember(dest => dest.DiasDictado, opt => opt.Ignore())
                .ForMember(dest => dest.PuedeRevisar, opt => opt.Ignore());

            CreateMap<Curso, EditCursoViewModel>()
                .ForMember(dest => dest.DiasDictado, opt => opt.Ignore());

            CreateMap<CreateCursoViewModel, Curso>()
                .ForMember(d => d.CursoId, o => o.Ignore())
                .ForMember(d => d.CursoDias, o => o.Ignore())
                .ForMember(d => d.EstadoCurso, o => o.Ignore())
                .ForMember(d => d.MotivoRechazo, o => o.Ignore())
                .ForMember(d => d.Profesor, o => o.Ignore())
                .ForMember(d => d.CursoAlumnos, o => o.Ignore());

            CreateMap<EditCursoViewModel, Curso>()
                .ForMember(d => d.CursoDias, o => o.Ignore())
                .ForMember(d => d.MotivoRechazo, o => o.Ignore())
                .ForMember(d => d.Profesor, o => o.Ignore())
                .ForMember(d => d.CursoAlumnos, o => o.Ignore());
        }
    }
}
