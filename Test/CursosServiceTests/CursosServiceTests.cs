using AutoMapper;
using Kurse.Mappers;
using Kurse.Models.Entities;
using Kurse.Models.Enums;
using Kurse.Repositories.Interfaces;
using Kurse.Services.Implementations;
using Kurse.ViewModels;
using Kurse.ViewModels.Cursos;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Tests.CursosServiceTests
{
    public class CursosServiceTests
    {
        private readonly Mock<ICursoRepository> _repositoryMock;
        private readonly CursoService _service;
        private readonly IMapper _mapper;

        public CursosServiceTests()
        {
            _repositoryMock = new Mock<ICursoRepository>();
            _mapper = CreateMapper();
            _service = new CursoService(_repositoryMock.Object, _mapper);
        }

        [Fact]
        public void AutoMapper_DeberiaTenerConfiguracionValida()
        {
            MapperConfigurationExpression expression = new();

            expression.AddProfile<CursoProfile>();

            MapperConfiguration configuration = new(expression, NullLoggerFactory.Instance);

            configuration.AssertConfigurationIsValid();
        }

        [Fact]
        public async Task GetAllAsync_DeberiaRetornarCursosMapeados()
        {
            // Arrange
            List<Curso> cursos =
            [new Curso
            {
                CursoId = 1,
                NombreCurso = "C# Básico",
                EstadoCurso = EstadoCurso.Autorizado,
                Profesor = new ApplicationUser
                {
                    Nombre = "Juan",
                    Apellido = "Pérez"
                },
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

            _repositoryMock.Setup(r => r.GetAllAsync(false, "prof1")).ReturnsAsync(cursos);

            // Act
            List<CursoViewModel> resultado = await _service.GetAllAsync(false, "prof1");

            // Assert
            Assert.Single(resultado);

            CursoViewModel curso = resultado.First();

            Assert.Equal(1, curso.CursoId);
            Assert.Equal("C# Básico", curso.NombreCurso);

            _repositoryMock.Verify(r => r.GetAllAsync(false, "prof1"), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_SinCursos_DeberiaRetornarListaVacia()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetAllAsync(true, "")).ReturnsAsync([]);

            // Act
            List<CursoViewModel> resultado = await _service.GetAllAsync(true, "");

            // Assert
            Assert.Empty(resultado);

            _repositoryMock.Verify(r => r.GetAllAsync(true, ""), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_SiEsProfesor_DeberiaCrearCursoPendiente()
        {
            // Arrange
            CreateCursoViewModel model = new()
            {
                NombreCurso = "C# Básico",
                FechaInicio = DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
                FechaFin = DateOnly.FromDateTime(DateTime.Today.AddDays(20)),
                DiasDictado = [DiaSemana.Lunes]
            };

            Curso? cursoGuardado = null;

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Curso>())).Callback<Curso>(c => cursoGuardado = c).Returns(Task.CompletedTask);

            // Act
            await _service.CreateAsync(model, true);

            // Assert
            Assert.NotNull(cursoGuardado);
            Assert.Equal(EstadoCurso.Pendiente, cursoGuardado!.EstadoCurso);

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Curso>()), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_SiNoEsProfesor_DeberiaCrearCursoAutorizado()
        {
            // Arrange
            CreateCursoViewModel model = new()
            {
                NombreCurso = "ASP.NET Core",
                FechaInicio = DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
                FechaFin = DateOnly.FromDateTime(DateTime.Today.AddDays(20)),
                DiasDictado = [DiaSemana.Martes]
            };

            Curso? cursoGuardado = null;

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Curso>())).Callback<Curso>(c => cursoGuardado = c).Returns(Task.CompletedTask);

            // Act
            await _service.CreateAsync(model, false);

            // Assert
            Assert.NotNull(cursoGuardado);
            Assert.Equal(EstadoCurso.Autorizado, cursoGuardado!.EstadoCurso);

            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_DeberiaGuardarDiasDeDictado()
        {
            // Arrange
            CreateCursoViewModel model = new()
            {
                NombreCurso = "SQL Server",
                FechaInicio = DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
                FechaFin = DateOnly.FromDateTime(DateTime.Today.AddDays(20)),
                DiasDictado =
                [
                    DiaSemana.Lunes,
                    DiaSemana.Miercoles,
                    DiaSemana.Viernes
                ]
            };

            Curso? cursoGuardado = null;

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Curso>())).Callback<Curso>(c => cursoGuardado = c).Returns(Task.CompletedTask);

            // Act
            await _service.CreateAsync(model, true);

            // Assert
            Assert.NotNull(cursoGuardado);
            Assert.Equal(3, cursoGuardado!.CursoDias.Count);
            Assert.Contains(cursoGuardado.CursoDias, d => d.DiaSemana == DiaSemana.Lunes);
            Assert.Contains(cursoGuardado.CursoDias, d => d.DiaSemana == DiaSemana.Miercoles);
            Assert.Contains(cursoGuardado.CursoDias, d => d.DiaSemana == DiaSemana.Viernes);
        }

        [Fact]
        public async Task UpdateAsync_CursoInexistente_DeberiaRetornarFalse()
        {
            // Arrange
            EditCursoViewModel model = new()
            {
                CursoId = 1
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(model.CursoId)).ReturnsAsync((Curso?)null);

            // Act
            bool resultado = await _service.UpdateAsync(model);

            // Assert
            Assert.False(resultado);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Curso>()), Times.Never);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }


        [Fact]
        public async Task UpdateAsync_CursoExistente_DeberiaActualizarCurso()
        {
            // Arrange
            EditCursoViewModel model = new()
            {
                CursoId = 1,
                NombreCurso = "Curso Actualizado",
                DiasDictado = [DiaSemana.Martes]
            };

            Curso curso = new()
            {
                CursoId = 1,
                NombreCurso = "Curso Original",
                CursoDias = []
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(model.CursoId)).ReturnsAsync(curso);

            // Act
            bool resultado = await _service.UpdateAsync(model);

            // Assert
            Assert.True(resultado);
            Assert.Equal("Curso Actualizado", curso.NombreCurso);

            _repositoryMock.Verify(r => r.UpdateAsync(curso), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_DeberiaActualizarDiasDictado()
        {
            // Arrange
            EditCursoViewModel model = new()
            {
                CursoId = 1,
                DiasDictado =
                [
                    DiaSemana.Lunes,
                    DiaSemana.Viernes
                ]
            };

            Curso curso = new()
            {
                CursoId = 1,
                CursoDias =
                [
                    new CursoDia
                    {
                        CursoId = 1,
                        DiaSemana = DiaSemana.Miercoles
                    }
                ]
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(model.CursoId)).ReturnsAsync(curso);

            // Act
            await _service.UpdateAsync(model);

            // Assert
            Assert.Equal(2, curso.CursoDias.Count);
            Assert.Contains(curso.CursoDias, d => d.DiaSemana == DiaSemana.Lunes);
            Assert.Contains(curso.CursoDias, d => d.DiaSemana == DiaSemana.Viernes);
            Assert.DoesNotContain(curso.CursoDias, d => d.DiaSemana == DiaSemana.Miercoles);
        }

        [Fact]
        public async Task GetEditByIdAsync_CursoInexistente_DeberiaRetornarNull()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Curso?)null);

            // Act
            EditCursoViewModel? resultado = await _service.GetEditByIdAsync(1);

            // Assert
            Assert.Null(resultado);

            _repositoryMock.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetEditByIdAsync_CursoExistente_DeberiaRetornarViewModel()
        {
            // Arrange
            Curso curso = new()
            {
                CursoId = 1,
                NombreCurso = "C# Avanzado",

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
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(curso);

            // Act
            EditCursoViewModel? resultado = await _service.GetEditByIdAsync(1);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(1, resultado!.CursoId);
            Assert.Equal("C# Avanzado", resultado.NombreCurso);
            Assert.Equal(2, resultado.DiasDictado.Count);
            Assert.Contains(DiaSemana.Lunes, resultado.DiasDictado);
            Assert.Contains(DiaSemana.Miercoles, resultado.DiasDictado);
        }

        [Fact]
        public async Task GetDetailsByIdAsync_CursoInexistente_DeberiaRetornarNull()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Curso?)null);

            // Act
            CursoDetailsViewModel? resultado = await _service.GetDetailsByIdAsync(1);

            // Assert
            Assert.Null(resultado);

            _repositoryMock.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetDetailsByIdAsync_CursoExistente_DeberiaRetornarViewModel()
        {
            // Arrange
            Curso curso = new()
            {
                CursoId = 1,
                NombreCurso = "ASP.NET Core",

                Profesor = new ApplicationUser
                {
                    Nombre = "Juan",
                    Apellido = "Pérez"
                },

                EstadoCurso = EstadoCurso.Autorizado,

                CursoDias =
                [
                    new CursoDia
                    {
                        DiaSemana = DiaSemana.Martes
                    },
                    new CursoDia
                    {
                        DiaSemana = DiaSemana.Jueves
                    }
                ]
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(curso);

            // Act
            CursoDetailsViewModel? resultado = await _service.GetDetailsByIdAsync(1);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(1, resultado!.CursoId);
            Assert.Equal("ASP.NET Core", resultado.NombreCurso);
            Assert.Equal("Juan Pérez", resultado.Profesor);
            Assert.Equal(EstadoCurso.Autorizado.ToString(), resultado.EstadoCurso);
            Assert.Equal(2, resultado.DiasDictado.Count);
            Assert.Contains(DiaSemana.Martes, resultado.DiasDictado);
            Assert.Contains(DiaSemana.Jueves, resultado.DiasDictado);
        }

        [Fact]
        public async Task DarBajaAsync_CursoInexistente_DeberiaRetornarError()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Curso?)null);

            (bool Success, string? Message) = await _service.DarBajaAsync(1);

            Assert.False(Success);
            Assert.Equal("Curso no encontrado.", Message);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Curso>()), Times.Never);
        }

        [Fact]
        public async Task DarBajaAsync_CursoYaCancelado_DeberiaRetornarError()
        {
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Cancelado
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(curso);

            (bool Success, string? Message) = await _service.DarBajaAsync(1);

            Assert.False(Success);
            Assert.Equal("El curso ya está dado de baja.", Message);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Curso>()), Times.Never);
        }

        [Fact]
        public async Task DarBajaAsync_CursoConAlumnosActivos_DeberiaRetornarError()
        {
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Autorizado
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(curso);
            _repositoryMock.Setup(r => r.TieneAlumnosActivosAsync(1)).ReturnsAsync(true);

            (bool Success, string? Message) = await _service.DarBajaAsync(1);

            Assert.False(Success);
            Assert.Equal("El curso posee alumnos inscriptos.", Message);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Curso>()), Times.Never);
        }

        [Fact]
        public async Task DarBajaAsync_DeberiaCancelarCurso()
        {
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Autorizado
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(curso);
            _repositoryMock.Setup(r => r.TieneAlumnosActivosAsync(1)).ReturnsAsync(false);

            (bool Success, string? Message) = await _service.DarBajaAsync(1);

            Assert.True(Success);
            Assert.Equal("Curso dado de baja correctamente.", Message);
            Assert.Equal(EstadoCurso.Cancelado, curso.EstadoCurso);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Curso>()), Times.Once);
        }

        [Fact]
        public async Task AutorizarAsync_CursoInexistente_DeberiaRetornarError()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Curso?)null);

            (bool Success, string? Message) = await _service.AutorizarAsync(1);

            Assert.False(Success);
        }

        [Fact]
        public async Task AutorizarAsync_CursoNoPendiente_DeberiaRetornarError()
        {
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Autorizado
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(curso);

            (bool Success, string? Message) = await _service.AutorizarAsync(1);

            Assert.False(Success);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Curso>()), Times.Never);
        }

        [Fact]
        public async Task AutorizarAsync_DeberiaAutorizarCurso()
        {
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Pendiente,
                MotivoRechazo = "Sin documentación"
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(curso);

            (bool Success, string? Message) = await _service.AutorizarAsync(1);

            Assert.True(Success);
            Assert.Equal(EstadoCurso.Autorizado, curso.EstadoCurso);
            Assert.Null(curso.MotivoRechazo);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Curso>()), Times.Once);
        }

        [Fact]
        public async Task RechazarAsync_CursoInexistente_DeberiaRetornarError()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Curso?)null);

            // Act
            (bool Success, string? Message) = await _service.RechazarAsync(1, "Motivo");

            // Assert
            Assert.False(Success);
            Assert.Equal("Curso no encontrado.", Message);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Curso>()), Times.Never);
        }

        [Fact]
        public async Task RechazarAsync_CursoNoPendiente_DeberiaRetornarError()
        {
            // Arrange
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Autorizado
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(curso);

            // Act
            (bool Success, string? Message) = await _service.RechazarAsync(1, "Motivo");

            // Assert
            Assert.False(Success);
            Assert.Equal("Solo pueden rechazarse cursos pendientes.", Message);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Curso>()), Times.Never);
        }

        [Fact]
        public async Task RechazarAsync_MotivoVacio_DeberiaRetornarError()
        {
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Pendiente
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(curso);

            (bool Success, string? Message) = await _service.RechazarAsync(1, "");

            Assert.False(Success);
            Assert.Equal("Debe indicar un motivo.", Message);
        }

        [Fact]
        public async Task RechazarAsync_DeberiaRechazarCurso()
        {
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Pendiente
            };

            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(curso);

            (bool Success, string? Message) = await _service.RechazarAsync(1, "No cumple requisitos");

            Assert.True(Success);
            Assert.Equal(EstadoCurso.Rechazado, curso.EstadoCurso);
            Assert.Equal("No cumple requisitos", curso.MotivoRechazo);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Curso>()), Times.Once);
        }

        [Fact]
        public async Task FinalizarAsync_CursoInexistente_DeberiaRetornarError()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync((Curso?)null);

            // Act
            (bool Success, string? Message) = await _service.FinalizarAsync(1);

            // Assert
            Assert.False(Success);
            Assert.Equal("Curso no encontrado.", Message);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Curso>()), Times.Never);
        }

        [Fact]
        public async Task FinalizarAsync_CursoYaFinalizado_DeberiaRetornarError()
        {
            // Arrange
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Finalizado
            };

            _repositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync(curso);

            // Act
            (bool Success, string? Message) = await _service.FinalizarAsync(1);

            // Assert
            Assert.False(Success);
            Assert.Equal("El curso ya se encuentra finalizado.", Message);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Curso>()), Times.Never);
        }

        [Fact]
        public async Task FinalizarAsync_CursoNoAutorizado_DeberiaRetornarError()
        {
            // Arrange
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Rechazado
            };

            _repositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync(curso);

            // Act
            (bool Success, string? Message) = await _service.FinalizarAsync(1);

            // Assert
            Assert.False(Success);
            Assert.Equal("Solo pueden finalizarse cursos autorizados.", Message);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Curso>()), Times.Never);
        }

        [Fact]
        public async Task FinalizarAsync_ConAlumnosPendientes_DeberiaRetornarError()
        {
            // Arrange
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Autorizado,

                CursoAlumnos =
                [
                    new CursoAlumnos
                    {
                        Estado = EstadoAcademico.Inscripto
                    }
                ]
            };

            _repositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync(curso);

            // Act
            (bool Success, string? Message) = await _service.FinalizarAsync(1);

            // Assert
            Assert.False(Success);
            Assert.Equal("Existen alumnos sin evaluar en el acta.", Message);

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Curso>()), Times.Never);
        }

        [Fact]
        public async Task FinalizarAsync_DeberiaFinalizarCurso()
        {
            // Arrange
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Autorizado,

                CursoAlumnos =
                [
                    new CursoAlumnos
                    {
                        Estado = EstadoAcademico.Aprobado
                    },
                    new CursoAlumnos
                    {
                        Estado = EstadoAcademico.Desaprobado
                    }
                ]
            };

            _repositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync(curso);

            // Act
            (bool Success, string? Message) = await _service.FinalizarAsync(1);

            // Assert
            Assert.True(Success);
            Assert.Equal("Curso finalizado correctamente.", Message);
            Assert.Equal(EstadoCurso.Finalizado, curso.EstadoCurso);

            _repositoryMock.Verify(r => r.UpdateAsync(curso), Times.Once);
        }

        [Fact]
        public async Task CursoPerteneceProfesorAsync_CuandoExisteRelacion_DeberiaRetornarTrue()
        {
            // Arrange
            _repositoryMock.Setup(r => r.CursoPerteneceProfesorAsync(1, "prof1")).ReturnsAsync(true);

            // Act
            bool resultado = await _service.CursoPerteneceProfesorAsync(1, "prof1");

            // Assert
            Assert.True(resultado);

            _repositoryMock.Verify(r => r.CursoPerteneceProfesorAsync(1, "prof1"), Times.Once);
        }

        [Fact]
        public async Task CursoPerteneceProfesorAsync_CuandoNoExisteRelacion_DeberiaRetornarFalse()
        {
            // Arrange
            _repositoryMock.Setup(r => r.CursoPerteneceProfesorAsync(1, "prof1")).ReturnsAsync(false);

            // Act
            bool resultado = await _service.CursoPerteneceProfesorAsync(1, "prof1");

            // Assert
            Assert.False(resultado);

            _repositoryMock.Verify(r => r.CursoPerteneceProfesorAsync(1, "prof1"), Times.Once);
        }

        [Fact]
        public async Task GetOfertaAcademicaAsync_SinCursos_DeberiaRetornarListaVacia()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetCursosDisponiblesAsync()).ReturnsAsync([]);

            // Act
            List<CursoOfertaViewModel> resultado = await _service.GetOfertaAcademicaAsync("alumno1");

            // Assert
            Assert.Empty(resultado);
        }

        [Fact]
        public async Task GetOfertaAcademicaAsync_DeberiaMapearCorrectamente()
        {
            // Arrange
            List<Curso> cursos =
            [
                new Curso
                {
                    CursoId = 1,
                    NombreCurso = "ASP.NET Core",
                    Profesor = new ApplicationUser
                    {
                        Nombre = "Juan",
                        Apellido = "Pérez"
                    },
                    FechaInicio = new DateOnly(2026, 8, 1),
                    FechaFin = new DateOnly(2026, 10, 1),
                    DuracionHoras = 40,
                    LugarDictado = "Aula 1",
                    CupoMaximo = 30,
                    Arancelado = true,
                    Arancel = 15000,
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
                    ],
                    CursoAlumnos = []
                }
            ];

            _repositoryMock.Setup(r => r.GetCursosDisponiblesAsync()).ReturnsAsync(cursos);

            // Act
            List<CursoOfertaViewModel> resultado = await _service.GetOfertaAcademicaAsync("alumno1");

            // Assert
            Assert.Single(resultado);

            CursoOfertaViewModel curso = resultado.First();

            Assert.Equal(1, curso.CursoId);
            Assert.Equal("ASP.NET Core", curso.NombreCurso);
            Assert.Equal("Juan Pérez", curso.Profesor);
            Assert.Equal("Lun - Mié", curso.DiasDictado);
            Assert.Equal(40, curso.DuracionHoras);
            Assert.Equal(30, curso.CupoMaximo);
            Assert.True(curso.Arancelado);
            Assert.Equal(15000, curso.Arancel);
            Assert.False(curso.YaInscripto);
        }

        [Fact]
        public async Task GetOfertaAcademicaAsync_DeberiaCalcularInscripcionCorrectamente()
        {
            // Arrange
            List<Curso> cursos =
            [
                new Curso
                {
                    CursoId = 1,
                    Profesor = new ApplicationUser(),
                    CursoDias = [],
                    CursoAlumnos =
                    [
                        new CursoAlumnos
                        {
                            AlumnoId = "alumno1",
                            Estado = EstadoAcademico.Inscripto
                        },
                        new CursoAlumnos
                        {
                            AlumnoId = "otro",
                            Estado = EstadoAcademico.Aprobado
                        },
                        new CursoAlumnos
                        {
                            AlumnoId = "baja",
                            Estado = EstadoAcademico.Baja
                        }
                    ]
                }
            ];

            _repositoryMock.Setup(r => r.GetCursosDisponiblesAsync()).ReturnsAsync(cursos);

            // Act
            List<CursoOfertaViewModel> resultado = await _service.GetOfertaAcademicaAsync("alumno1");

            // Assert
            CursoOfertaViewModel curso = resultado.First();

            Assert.True(curso.YaInscripto);
            Assert.Equal(2, curso.CantidadInscriptos);
        }

        private static IMapper CreateMapper()
        {
            MapperConfigurationExpression expression = new();

            expression.AddProfile<CursoProfile>();

            MapperConfiguration configuration = new(expression, NullLoggerFactory.Instance);

            configuration.AssertConfigurationIsValid();

            return configuration.CreateMapper();
        }
    }
}
