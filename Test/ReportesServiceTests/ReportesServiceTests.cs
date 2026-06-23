using AutoMapper;
using Kurse.Mappers;
using Kurse.Models.Entities;
using Kurse.Models.Enums;
using Kurse.Repositories.Interfaces;
using Kurse.Services.Implementations;
using Kurse.ViewModels.Reportes;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Tests.ReportesServiceTests
{
    public class ReportesServiceTests
    {
        private readonly Mock<IReportesRepository> _repositoryMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly ReportesService _service;
        private readonly IMapper _mapper;

        public ReportesServiceTests()
        {
            _repositoryMock = new Mock<IReportesRepository>();
            Mock<IUserStore<ApplicationUser>> userStore = new();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            _mapper = CreateMapper();
            _service = new ReportesService(_repositoryMock.Object, _userManagerMock.Object, _mapper);
        }

        [Fact]
        public void AutoMapper_DeberiaTenerConfiguracionValida()
        {
            MapperConfigurationExpression expression = new();

            expression.AddProfile<ReporteProfile>();

            MapperConfiguration configuration = new(expression, NullLoggerFactory.Instance);

            configuration.AssertConfigurationIsValid();
        }


        [Fact]
        public async Task GetNotasAsync_DeberiaRetornarNotasMapeadas()
        {
            // Arrange
            List<CursoAlumnos> entities =
                [new()
                {
                    Alumno = new ApplicationUser
                    {
                        Legajo = "0001",
                        Nombre = "Juan",
                        Apellido = "Perez"
                    },
                    Curso = new Curso
                    {
                        NombreCurso = "Programación en C#",
                        CantidadClases = 10
                    },
                    NotaFinal = 8,
                    Asistencia = 9,
                    Estado = EstadoAcademico.Aprobado
                }];

            _repositoryMock.Setup(r => r.GetNotasAsync(1)).ReturnsAsync(entities);

            // Act
            List<ReporteNotasViewModel> resultado = await _service.GetNotasAsync(1);

            // Assert
            Assert.Single(resultado);

            ReporteNotasViewModel nota = resultado.First();

            Assert.Equal("0001", nota.Legajo);
            Assert.Equal("Juan Perez", nota.Alumno);
            Assert.Equal("Programación en C#", nota.Curso);
            Assert.Equal(8, nota.NotaFinal);
            Assert.Equal(9, nota.ClasesAsistidas);
            Assert.Equal(10, nota.TotalClases);
            Assert.Equal("Aprobado", nota.Estado);
        }

        [Fact]
        public async Task GetInscriptosAsync_DeberiaRetornarInscriptosMapeados()
        {
            // Arrange
            List<CursoAlumnos> entities =
            [new()
            {
                Alumno = new ApplicationUser
                {
                    Legajo = "0002",
                    Nombre = "Ana",
                    Apellido = "Gomez",
                    Email = "ana@test.com"
                },
                Curso = new Curso
                {
                    NombreCurso = "Excel Avanzado"
                },
                FechaInscripcion = new DateTime(2026, 6, 1),
                Estado = EstadoAcademico.Inscripto
            }];

            _repositoryMock.Setup(r => r.GetInscriptosAsync(1)).ReturnsAsync(entities);

            // Act
            List<ReporteInscriptosViewModel> resultado = await _service.GetInscriptosAsync(1);

            // Assert
            Assert.Single(resultado);

            ReporteInscriptosViewModel inscripto = resultado.First();

            Assert.Equal("0002", inscripto.Legajo);
            Assert.Equal("Ana Gomez", inscripto.Alumno);
            Assert.Equal("Excel Avanzado", inscripto.Curso);
            Assert.Equal("ana@test.com", inscripto.Email);
            Assert.Equal(new DateOnly(2026, 6, 1), inscripto.FechaInscripcion);
            Assert.Equal("Inscripto", inscripto.EstadoAcademico);
        }

        [Fact]
        public async Task GetCursosAsync_DeberiaRetornarCursosMapeados()
        {
            // Arrange
            ReporteFiltroViewModel filtro = new()
            {
                FechaDesde = new DateOnly(2026, 1, 1),
                FechaHasta = new DateOnly(2026, 12, 31)
            };

            List<Curso> cursos =
            [new()
            {
                Codigo = "CUR-001",
                NombreCurso = "Programación en C#",
                FechaInicio = new DateOnly(2026, 6, 1),
                FechaFin = new DateOnly(2026, 6, 30),
                HoraInicio = new TimeOnly(18, 0),
                HoraFin = new TimeOnly(20, 0),
                DuracionHoras = 40,
                EstadoCurso = EstadoCurso.Autorizado,

                Profesor = new ApplicationUser
                {
                    Nombre = "Juan",
                    Apellido = "Pérez"
                },

                CursoAlumnos =
                [
                    new CursoAlumnos(),
                    new CursoAlumnos()
                ],

                CursoDias =
                [
                    new CursoDia
                    {
                        DiaSemana = DiaSemana.Lunes
                    },
                    new CursoDia
                    {
                        DiaSemana = DiaSemana.Miercoles
                    }
                 ]
            }];

            _repositoryMock.Setup(r => r.GetCursosAsync(filtro.FechaDesde, filtro.FechaHasta)).ReturnsAsync(cursos);

            // Act
            List<ReporteCursoViewModel> resultado = await _service.GetCursosAsync(filtro);

            // Assert
            Assert.Single(resultado);

            ReporteCursoViewModel curso = resultado.First();

            Assert.Equal("CUR-001", curso.Codigo);
            Assert.Equal("Programación en C#", curso.Curso);
            Assert.Equal("Juan Pérez", curso.Profesor);
            Assert.Equal(new DateOnly(2026, 6, 1), curso.FechaInicio);
            Assert.Equal(new DateOnly(2026, 6, 30), curso.FechaFin);
            Assert.Equal(new TimeOnly(18, 0), curso.HoraInicio);
            Assert.Equal(new TimeOnly(20, 0), curso.HoraFin);
            Assert.Equal(40, curso.DuracionHoras);
            Assert.Equal(EstadoCurso.Autorizado.ToString(), curso.Estado);
            Assert.Equal(2, curso.CantidadInscriptos);
            Assert.Equal("Lun - Mié", curso.DiasDictado);
        }

        [Fact]
        public async Task GetDashboardAsync_DeberiaRetornarIndicadoresCorrectos()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetTotalAlumnosAsync()).ReturnsAsync(100);
            _repositoryMock.Setup(r => r.GetTotalProfesoresAsync()).ReturnsAsync(8);
            _repositoryMock.Setup(r => r.GetTotalCursosAsync()).ReturnsAsync(20);
            _repositoryMock.Setup(r => r.GetTotalInscripcionesAsync()).ReturnsAsync(250);

            // Act
            ReportesDashboardViewModel resultado = await _service.GetDashboardAsync();

            // Assert
            Assert.Equal(100, resultado.TotalAlumnos);
            Assert.Equal(8, resultado.TotalProfesores);
            Assert.Equal(20, resultado.TotalCursos);
            Assert.Equal(250, resultado.TotalInscripciones);
        }

        [Fact]
        public async Task GetNotasAsync_SinDatos_DeberiaRetornarListaVacia()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetNotasAsync(null)).ReturnsAsync([]);

            // Act
            List<ReporteNotasViewModel> resultado = await _service.GetNotasAsync(null);

            // Assert
            Assert.Empty(resultado);
        }

        [Fact]
        public async Task GetCursosAsync_SinResultados_DeberiaRetornarListaVacia()
        {
            // Arrange
            ReporteFiltroViewModel filtro = new();

            _repositoryMock.Setup(r => r.GetCursosAsync(filtro.FechaDesde, filtro.FechaHasta)).ReturnsAsync([]);

            // Act
            List<ReporteCursoViewModel> resultado = await _service.GetCursosAsync(filtro);

            // Assert
            Assert.Empty(resultado);
        }

        private static IMapper CreateMapper()
        {
            MapperConfigurationExpression expression = new();

            expression.AddProfile<ReporteProfile>();

            MapperConfiguration configuration = new(expression, NullLoggerFactory.Instance);

            configuration.AssertConfigurationIsValid();

            return configuration.CreateMapper();
        }
    }
}
