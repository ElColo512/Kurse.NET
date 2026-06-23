using AutoMapper;
using Kurse.Mappers;
using Kurse.Models.Entities;
using Kurse.Models.Enums;
using Kurse.Models.Helpers;
using Kurse.Repositories.Interfaces;
using Kurse.Services.Implementations;
using Kurse.Services.Interfaces;
using Kurse.ViewModels;
using Kurse.ViewModels.Actas;
using Kurse.ViewModels.Alumno;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Tests.CursoAlumnosServiceTests
{
    public class CursoAlumnosServiceTests
    {
        private readonly Mock<ICursoAlumnosRepository> _cursoAlumnosRepositoryMock;
        private readonly Mock<ICursoRepository> _cursoRepositoryMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IPdfService> _pdfServiceMock;
        private readonly IMapper _mapper;
        private readonly ICursoAlumnosService _service;

        public CursoAlumnosServiceTests()
        {
            _cursoAlumnosRepositoryMock = new Mock<ICursoAlumnosRepository>();
            _cursoRepositoryMock = new Mock<ICursoRepository>();
            Mock<IUserStore<ApplicationUser>> userStore = new();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            _pdfServiceMock = new Mock<IPdfService>();
            _mapper = CreateMapper();
            _service = new CursoAlumnosService(_cursoAlumnosRepositoryMock.Object, _cursoRepositoryMock.Object, _userManagerMock.Object, _mapper, _pdfServiceMock.Object);
        }

        [Fact]
        public async Task GetInscriptosModalAsync_CursoInexistente_DeberiaRetornarNull()
        {
            // Arrange
            _cursoRepositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync((Curso?)null);

            // Act
            CursoInscriptosViewModel? resultado = await _service.GetInscriptosModalAsync(1);

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public async Task GetInscriptosModalAsync_DeberiaRetornarDatosCorrectos()
        {
            // Arrange

            Curso curso = new()
            {
                CursoId = 1,
                NombreCurso = "ASP.NET Core",
                EstadoCurso = EstadoCurso.Autorizado,
                CursoAlumnos =
                [new CursoAlumnos
                {
                    AlumnoId = "1",
                    Alumno = new ApplicationUser
                    {
                        Id = "1",
                        Nombre = "Juan",
                        Apellido = "Pérez",
                        Email = "juan@test.com"
                    }
                }]
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync(curso);
            _userManagerMock.Setup(u => u.GetUsersInRoleAsync(RoleConstants.Alumno)).ReturnsAsync(
                [new ApplicationUser
                {
                    Id = "1",
                    Nombre = "Juan",
                    Apellido = "Pérez"
                },
                new ApplicationUser
                {
                    Id = "2",
                    Nombre = "Pedro",
                    Apellido = "Gómez"
                },
                new ApplicationUser
                {
                    Id = "3",
                    Nombre = "María",
                    Apellido = "López"
                }
                ]);

            // Act

            CursoInscriptosViewModel? resultado = await _service.GetInscriptosModalAsync(1);

            // Assert

            Assert.NotNull(resultado);
            Assert.Equal(1, resultado.CursoId);
            Assert.Equal("ASP.NET Core", resultado.NombreCurso);
            Assert.False(resultado.EstaFinalizado);
            Assert.Single(resultado.Alumnos);
            Assert.Equal("Juan Pérez", resultado.Alumnos.First().NombreCompleto);
            Assert.Equal(2, resultado.AlumnosDisponibles.Count);
            Assert.Contains(resultado.AlumnosDisponibles, a => a.Value == "2" && a.Text == "Pedro Gómez");
            Assert.Contains(resultado.AlumnosDisponibles, a => a.Value == "3" && a.Text == "María López");
        }

        [Fact]
        public async Task GetInscriptosModalAsync_CursoFinalizado_DeberiaIndicarlo()
        {
            Curso curso = new()
            {
                CursoId = 1,
                NombreCurso = "ASP.NET Core",
                EstadoCurso = EstadoCurso.Finalizado,
                CursoAlumnos = []
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync(curso);
            _userManagerMock.Setup(u => u.GetUsersInRoleAsync(RoleConstants.Alumno)).ReturnsAsync([]);

            CursoInscriptosViewModel? resultado = await _service.GetInscriptosModalAsync(1);

            Assert.NotNull(resultado);
            Assert.True(resultado.EstaFinalizado);
        }

        [Fact]
        public async Task GetAlumnosDisponiblesAsync_CursoInexistente_DeberiaRetornarListaVacia()
        {
            // Arrange
            _cursoRepositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync((Curso?)null);

            // Act
            List<SelectListItem> resultado = await _service.GetAlumnosDisponiblesAsync(1);

            // Assert
            Assert.Empty(resultado);

            _userManagerMock.Verify(u => u.GetUsersInRoleAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetAlumnosDisponiblesAsync_SinInscriptos_DeberiaRetornarTodosLosAlumnos()
        {
            // Arrange
            Curso curso = new()
            {
                CursoId = 1,
                CursoAlumnos = []
            };

            IList<ApplicationUser> alumnos =
                [new ApplicationUser
                {
                    Id = "1",
                    Nombre = "Juan",
                    Apellido = "Pérez"
                },
                new ApplicationUser
                {
                    Id = "2",
                    Nombre = "María",
                    Apellido = "López"
                }];

            _cursoRepositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync(curso);
            _userManagerMock.Setup(u => u.GetUsersInRoleAsync(RoleConstants.Alumno)).ReturnsAsync(alumnos);

            // Act
            List<SelectListItem> resultado = await _service.GetAlumnosDisponiblesAsync(1);

            // Assert
            Assert.Equal(2, resultado.Count);
            Assert.Contains(resultado, a => a.Value == "1" && a.Text == "Juan Pérez");
            Assert.Contains(resultado, a => a.Value == "2" && a.Text == "María López");
        }

        [Fact]
        public async Task GetAlumnosDisponiblesAsync_ConInscriptos_DeberiaExcluirlos()
        {
            // Arrange
            Curso curso = new()
            {
                CursoId = 1,
                CursoAlumnos =
                [new CursoAlumnos
                {
                    AlumnoId = "1"
                },
                    new CursoAlumnos
                    {
                        AlumnoId = "3"
                    }]
            };

            IList<ApplicationUser> alumnos =
                [new ApplicationUser
                {
                    Id = "1",
                    Nombre = "Juan",
                    Apellido = "Pérez"
                },
                new ApplicationUser
                {
                    Id = "2",
                    Nombre = "María",
                    Apellido = "López"
                },
                new ApplicationUser
                {
                    Id = "3",
                    Nombre = "Pedro",
                    Apellido = "Gómez"
                }];

            _cursoRepositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync(curso);
            _userManagerMock.Setup(u => u.GetUsersInRoleAsync(RoleConstants.Alumno)).ReturnsAsync(alumnos);

            // Act
            List<SelectListItem> resultado = await _service.GetAlumnosDisponiblesAsync(1);

            // Assert
            Assert.Single(resultado);
            Assert.Equal("2", resultado[0].Value);
            Assert.Equal("María López", resultado[0].Text);
        }

        [Fact]
        public async Task GetAlumnosDisponiblesAsync_TodosInscriptos_DeberiaRetornarListaVacia()
        {
            // Arrange
            Curso curso = new()
            {
                CursoId = 1,
                CursoAlumnos =
                [new CursoAlumnos
                {
                AlumnoId = "1"
                }]
            };

            IList<ApplicationUser> alumnos =
                [new ApplicationUser
                {
                    Id = "1",
                    Nombre = "Juan",
                    Apellido = "Pérez"
                }];

            _cursoRepositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync(curso);
            _userManagerMock.Setup(u => u.GetUsersInRoleAsync(RoleConstants.Alumno)).ReturnsAsync(alumnos);

            // Act
            List<SelectListItem> resultado = await _service.GetAlumnosDisponiblesAsync(1);

            // Assert
            Assert.Empty(resultado);
        }

        [Fact]
        public async Task InscribirAlumnoAsync_CursoInexistente_DeberiaRetornarError()
        {
            // Arrange
            _cursoRepositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync((Curso?)null);

            // Act
            (bool Success, string? Message) = await _service.InscribirAlumnoAsync(1, "alumno1");

            // Assert
            Assert.False(Success);
            Assert.Equal("Curso no encontrado.", Message);

            _cursoAlumnosRepositoryMock.Verify(r => r.AddAsync(It.IsAny<CursoAlumnos>()), Times.Never);
        }

        [Fact]
        public async Task InscribirAlumnoAsync_CursoFinalizado_DeberiaRetornarError()
        {
            // Arrange
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Finalizado
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync(curso);

            // Act
            (bool Success, string? Message) = await _service.InscribirAlumnoAsync(1, "alumno1");

            // Assert
            Assert.False(Success);
            Assert.Equal("No es posible inscribir alumnos en un curso finalizado.", Message);

            _cursoAlumnosRepositoryMock.Verify(r => r.AddAsync(It.IsAny<CursoAlumnos>()), Times.Never);
        }

        [Fact]
        public async Task InscribirAlumnoAsync_AlumnoYaInscripto_DeberiaRetornarError()
        {
            // Arrange
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Autorizado,
                CursoAlumnos =
                [new CursoAlumnos
                {
                    AlumnoId = "alumno1"
                }]
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync(curso);

            // Act
            (bool Success, string? Message) = await _service.InscribirAlumnoAsync(1, "alumno1");

            // Assert
            Assert.False(Success);
            Assert.Equal("El alumno ya está inscripto.", Message);

            _cursoAlumnosRepositoryMock.Verify(r => r.AddAsync(It.IsAny<CursoAlumnos>()), Times.Never);
        }

        [Fact]
        public async Task InscribirAlumnoAsync_SinCupos_DeberiaRetornarError()
        {
            // Arrange
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Autorizado,
                CupoMaximo = 2,
                CursoAlumnos = [new CursoAlumnos(), new CursoAlumnos()]
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync(curso);

            // Act
            (bool Success, string? Message) = await _service.InscribirAlumnoAsync(1, "alumno3");

            // Assert
            Assert.False(Success);
            Assert.Equal("No hay cupos disponibles.", Message);

            _cursoAlumnosRepositoryMock.Verify(r => r.AddAsync(It.IsAny<CursoAlumnos>()), Times.Never);
        }

        [Fact]
        public async Task InscribirAlumnoAsync_DeberiaInscribirAlumno()
        {
            // Arrange
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Autorizado,
                CupoMaximo = 30,
                CursoAlumnos = []
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync(curso);

            // Act
            (bool Success, string? Message) = await _service.InscribirAlumnoAsync(1, "alumno1");

            // Assert
            Assert.True(Success);
            Assert.Equal("Alumno inscripto correctamente.", Message);

            _cursoAlumnosRepositoryMock.Verify(r => r.AddAsync(It.Is<CursoAlumnos>(ca => ca.CursoId == 1 && ca.AlumnoId == "alumno1" && ca.Estado == EstadoAcademico.Inscripto)), Times.Once);
            _cursoAlumnosRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }


        [Fact]
        public async Task GetInscriptosRowsAsync_CursoInexistente_DeberiaRetornarListaVacia()
        {
            // Arrange
            _cursoRepositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync((Curso?)null);

            // Act
            List<AlumnoCursoViewModel> resultado = await _service.GetInscriptosRowsAsync(1);

            // Assert
            Assert.Empty(resultado);
        }


        [Fact]
        public async Task GetInscriptosRowsAsync_DeberiaRetornarAlumnosMapeados()
        {
            // Arrange
            Curso curso = new()
            {
                CursoAlumnos =
                [new CursoAlumnos
                {
                    CursoAlumnosId = 1,
                    Asistencia = 8,
                    NotaFinal = 9,
                    Estado = EstadoAcademico.Aprobado,
                    Alumno = new ApplicationUser
                    {
                        Nombre = "Juan",
                        Apellido = "Pérez",
                        Email = "juan@test.com"
                    }
                },
                    new CursoAlumnos
                    {
                        CursoAlumnosId = 2,
                        Asistencia = 5,
                        NotaFinal = 6,
                        Estado = EstadoAcademico.Inscripto,
                        Alumno = new ApplicationUser
                        {
                            Nombre = "María",
                            Apellido = "López",
                            Email = "maria@test.com"
                        }
                    }
                ]
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync(curso);

            // Act
            List<AlumnoCursoViewModel> resultado = await _service.GetInscriptosRowsAsync(1);

            // Assert
            Assert.Equal(2, resultado.Count);
            Assert.Equal("Juan Pérez", resultado[0].NombreCompleto);
            Assert.Equal("María López", resultado[1].NombreCompleto);
        }


        [Fact]
        public async Task GetActaModalAsync_CursoInexistente_DeberiaRetornarNull()
        {
            // Arrange
            _cursoRepositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync((Curso?)null);

            // Act
            CursoActaViewModel? resultado = await _service.GetActaModalAsync(1);

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public async Task GetActaModalAsync_DeberiaRetornarActaCorrectamente()
        {
            // Arrange
            Curso curso = new()
            {
                CursoId = 1,
                NombreCurso = "ASP.NET Core",
                EstadoCurso = EstadoCurso.Autorizado,
                CursoAlumnos =
                [
                    new CursoAlumnos
                    {
                        CursoAlumnosId = 1,
                        Asistencia = 8,
                        NotaFinal = 9,
                        Estado = EstadoAcademico.Aprobado,
                        Alumno = new ApplicationUser
                        {
                            Nombre = "Juan",
                            Apellido = "Pérez"
                        }
                    }
                ]
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync(curso);

            // Act
            CursoActaViewModel? resultado = await _service.GetActaModalAsync(1);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(1, resultado.CursoId);
            Assert.Equal("ASP.NET Core", resultado.NombreCurso);
            Assert.False(resultado.EstaFinalizado);
            Assert.Single(resultado.Alumnos);
            Assert.Equal("Juan Pérez", resultado.Alumnos.First().NombreCompleto);
        }

        [Fact]
        public async Task GetActaModalAsync_CursoFinalizado_DeberiaIndicarlo()
        {
            // Arrange
            Curso curso = new()
            {
                CursoId = 1,
                NombreCurso = "ASP.NET Core",
                EstadoCurso = EstadoCurso.Finalizado,
                CursoAlumnos = []
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync(curso);

            // Act
            CursoActaViewModel? resultado = await _service.GetActaModalAsync(1);

            // Assert
            Assert.NotNull(resultado);
            Assert.True(resultado.EstaFinalizado);
        }

        [Fact]
        public async Task UpdateActaAsync_CursoInexistente_DeberiaRetornarError()
        {
            UpdateActaViewModel model = new()
            {
                CursoId = 1,
                Alumnos = []
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Curso?)null);

            (bool Success, string? Message) = await _service.UpdateActaAsync(model);

            Assert.False(Success);
            Assert.Equal("Curso no encontrado.", Message);

            _cursoRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateActaAsync_CursoFinalizado_DeberiaRetornarError()
        {
            UpdateActaViewModel model = new()
            {
                CursoId = 1,
                Alumnos = []
            };

            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Finalizado,
                FechaFin = DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(curso);

            (bool Success, string? Message) = await _service.UpdateActaAsync(model);

            Assert.False(Success);
            Assert.Equal("El curso se encuentra finalizado y el acta no puede modificarse.", Message);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(11)]
        public async Task UpdateActaAsync_NotaInvalida_DeberiaRetornarError(decimal nota)
        {
            UpdateActaViewModel model = new()
            {
                CursoId = 1,
                Alumnos =
                [new UpdateActaAlumnoViewModel
                {
                    CursoAlumnoId = 1,
                    NotaFinal = nota
                }]
            };

            Curso curso = new()
            {
                CantidadClases = 10,
                FechaFin = DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(curso);
            _cursoAlumnosRepositoryMock.Setup(r => r.GetCursoAlumnosByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync([]);

            (bool Success, string? Message) = await _service.UpdateActaAsync(model);

            Assert.False(Success);
            Assert.Equal("La nota debe estar entre 0 y 10.", Message);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(11)]
        public async Task UpdateActaAsync_AsistenciaInvalida_DeberiaRetornarError(int asistencia)
        {
            UpdateActaViewModel model = new()
            {
                CursoId = 1,
                Alumnos =
                [new UpdateActaAlumnoViewModel
                {
                    CursoAlumnoId = 1,
                    Asistencias = asistencia
                }]
            };

            Curso curso = new()
            {
                CantidadClases = 10,
                FechaFin = DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(curso);
            _cursoAlumnosRepositoryMock.Setup(r => r.GetCursoAlumnosByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync([]);

            (bool Success, string? Message) = await _service.UpdateActaAsync(model);

            Assert.False(Success);
            Assert.Contains("La asistencia no puede superar", Message);
        }

        [Fact]
        public async Task UpdateActaAsync_AlumnoDadoDeBaja_NoDeberiaModificarse()
        {
            CursoAlumnos entity = new()
            {
                CursoAlumnosId = 1,
                Estado = EstadoAcademico.Baja,
                NotaFinal = null,
                Asistencia = null
            };

            UpdateActaViewModel model = new()
            {
                CursoId = 1,
                Alumnos =
                [new UpdateActaAlumnoViewModel
                {
                    CursoAlumnoId = 1,
                    NotaFinal = 10,
                    Asistencias = 8
                }]
            };

            Curso curso = new()
            {
                CantidadClases = 10,
                FechaFin = DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(curso);
            _cursoAlumnosRepositoryMock.Setup(r => r.GetCursoAlumnosByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync([entity]);

            (bool Success, string? Message) = await _service.UpdateActaAsync(model);

            Assert.True(Success);
            Assert.Null(entity.NotaFinal);
            Assert.Null(entity.Asistencia);
        }

        [Fact]
        public async Task UpdateActaAsync_DeberiaActualizarActaCorrectamente()
        {
            CursoAlumnos entity = new()
            {
                CursoAlumnosId = 1,
                Estado = EstadoAcademico.Inscripto
            };

            UpdateActaViewModel model = new()
            {
                CursoId = 1,
                Alumnos =
                [new UpdateActaAlumnoViewModel
                {
                    CursoAlumnoId = 1,
                    NotaFinal = 9,
                    Asistencias = 8
                }]
            };

            Curso curso = new()
            {
                CantidadClases = 10,
                FechaFin = DateOnly.FromDateTime(DateTime.Today.AddDays(30))
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(curso);
            _cursoAlumnosRepositoryMock.Setup(r => r.GetCursoAlumnosByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync([entity]);

            (bool Success, string? Message) = await _service.UpdateActaAsync(model);

            Assert.True(Success);
            Assert.Equal("Acta guardada correctamente.", Message);
            Assert.Equal(9, entity.NotaFinal);
            Assert.Equal(8, entity.Asistencia);

            _cursoRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetActaRowsAsync_CursoInexistente_DeberiaRetornarListaVacia()
        {
            // Arrange
            _cursoRepositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync((Curso?)null);

            // Act
            List<ActaAlumnoViewModel> resultado = await _service.GetActaRowsAsync(1);

            // Assert
            Assert.Empty(resultado);

            _cursoRepositoryMock.Verify(r => r.GetByIdWithAlumnosAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetActaRowsAsync_DeberiaRetornarAlumnosMapeados()
        {
            // Arrange
            Curso curso = new()
            {
                CursoAlumnos =
                [new CursoAlumnos
                {
                    CursoAlumnosId = 1,
                    Asistencia = 9,
                    NotaFinal = 8,
                    Estado = EstadoAcademico.Aprobado,
                    Alumno = new ApplicationUser
                    {
                        Nombre = "Juan",
                        Apellido = "Pérez"
                    }
                },
                    new CursoAlumnos
                    {
                        CursoAlumnosId = 2,
                        Asistencia = 6,
                        NotaFinal = 5,
                        Estado = EstadoAcademico.Desaprobado,
                        Alumno = new ApplicationUser
                        {
                            Nombre = "María",
                            Apellido = "López"
                        }
                    }
                ]
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdWithAlumnosAsync(1)).ReturnsAsync(curso);

            // Act
            List<ActaAlumnoViewModel> resultado = await _service.GetActaRowsAsync(1);

            // Assert
            Assert.Equal(2, resultado.Count);
            Assert.Equal(1, resultado[0].CursoAlumnoId);
            Assert.Equal("Juan Pérez", resultado[0].NombreCompleto);
            Assert.Equal(9, resultado[0].Asistencias);
            Assert.Equal(8, resultado[0].NotaFinal);
            Assert.Equal("Aprobado", resultado[0].Estado);
            Assert.Equal(2, resultado[1].CursoAlumnoId);
            Assert.Equal("María López", resultado[1].NombreCompleto);
            Assert.Equal(6, resultado[1].Asistencias);
            Assert.Equal(5, resultado[1].NotaFinal);
            Assert.Equal("Desaprobado", resultado[1].Estado);

            _cursoRepositoryMock.Verify(r => r.GetByIdWithAlumnosAsync(1), Times.Once);
        }

        [Fact]
        public async Task CambiarEstadoAlumnoAsync_AlumnoInexistente_DeberiaRetornarError()
        {
            // Arrange
            _cursoAlumnosRepositoryMock.Setup(r => r.GetCursoAlumnoByIdAsync(1)).ReturnsAsync((CursoAlumnos?)null);

            // Act
            (bool Success, string? Message) = await _service.CambiarEstadoAlumnoAsync(1, EstadoAcademico.Baja);

            // Assert
            Assert.False(Success);
            Assert.Equal("Alumno no encontrado.", Message);

            _cursoAlumnosRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CambiarEstadoAlumnoAsync_CursoFinalizado_DeberiaRetornarError()
        {
            // Arrange
            CursoAlumnos entity = new()
            {
                Curso = new Curso
                {
                    EstadoCurso = EstadoCurso.Finalizado
                }
            };

            _cursoAlumnosRepositoryMock.Setup(r => r.GetCursoAlumnoByIdAsync(1)).ReturnsAsync(entity);

            // Act
            (bool Success, string? Message) = await _service.CambiarEstadoAlumnoAsync(1, EstadoAcademico.Baja);

            // Assert
            Assert.False(Success);
            Assert.Equal("No es posible modificar alumnos de un curso finalizado.", Message);

            _cursoAlumnosRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task CambiarEstadoAlumnoAsync_DeberiaActualizarEstado()
        {
            // Arrange
            CursoAlumnos entity = new()
            {
                Estado = EstadoAcademico.Inscripto,
                Curso = new Curso
                {
                    EstadoCurso = EstadoCurso.Autorizado
                }
            };

            _cursoAlumnosRepositoryMock.Setup(r => r.GetCursoAlumnoByIdAsync(1)).ReturnsAsync(entity);

            // Act
            (bool Success, string? Message) = await _service.CambiarEstadoAlumnoAsync(1, EstadoAcademico.Baja);

            // Assert
            Assert.True(Success);
            Assert.Equal("Estado actualizado correctamente.", Message);
            Assert.Equal(EstadoAcademico.Baja, entity.Estado);

            _cursoAlumnosRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task InscribirseAsync_CursoInexistente_DeberiaRetornarError()
        {
            // Arrange
            _cursoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Curso?)null);

            // Act
            (bool Success, string? Message) = await _service.InscribirseAsync(1, "alumno1");

            // Assert
            Assert.False(Success);
            Assert.Equal("Curso no encontrado.", Message);

            _cursoAlumnosRepositoryMock.Verify(r => r.InscribirAlumnoAsync(It.IsAny<CursoAlumnos>()), Times.Never);
            _cursoAlumnosRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task InscribirseAsync_CursoNoAutorizado_DeberiaRetornarError()
        {
            // Arrange
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Pendiente
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(curso);

            // Act
            (bool Success, string? Message) = await _service.InscribirseAsync(1, "alumno1");

            // Assert
            Assert.False(Success);
            Assert.Equal("El curso no se encuentra disponible.", Message);

            _cursoAlumnosRepositoryMock.Verify(r => r.InscribirAlumnoAsync(It.IsAny<CursoAlumnos>()), Times.Never);
        }

        [Fact]
        public async Task InscribirseAsync_AlumnoYaInscripto_DeberiaRetornarError()
        {
            // Arrange
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Autorizado,
                FechaFin = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
                CursoAlumnos = []
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(curso);
            _cursoAlumnosRepositoryMock.Setup(r => r.AlumnoYaInscriptoAsync(1, "alumno1")).ReturnsAsync(true);

            // Act
            (bool Success, string? Message) = await _service.InscribirseAsync(1, "alumno1");

            // Assert
            Assert.False(Success);
            Assert.Equal("Ya te encuentras inscripto.", Message);

            _cursoAlumnosRepositoryMock.Verify(r => r.InscribirAlumnoAsync(It.IsAny<CursoAlumnos>()), Times.Never);
        }

        [Fact]
        public async Task InscribirseAsync_SinCupos_DeberiaRetornarError()
        {
            // Arrange
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Autorizado,
                FechaFin = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
                CupoMaximo = 2,
                CursoAlumnos =
                [new CursoAlumnos { Estado = EstadoAcademico.Inscripto }, new CursoAlumnos { Estado = EstadoAcademico.Aprobado }]
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(curso);
            _cursoAlumnosRepositoryMock.Setup(r => r.AlumnoYaInscriptoAsync(1, "alumno1")).ReturnsAsync(false);

            // Act
            (bool Success, string? Message) = await _service.InscribirseAsync(1, "alumno1");

            // Assert
            Assert.False(Success);
            Assert.Equal("No hay cupos disponibles.", Message);

            _cursoAlumnosRepositoryMock.Verify(r => r.InscribirAlumnoAsync(It.IsAny<CursoAlumnos>()), Times.Never);
        }

        [Fact]
        public async Task InscribirseAsync_DeberiaInscribirAlumno()
        {
            // Arrange
            Curso curso = new()
            {
                EstadoCurso = EstadoCurso.Autorizado,
                FechaFin = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
                CupoMaximo = 10,
                CursoAlumnos =
                [new CursoAlumnos
                {
                    Estado = EstadoAcademico.Inscripto
                }]
            };

            _cursoRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(curso);
            _cursoAlumnosRepositoryMock.Setup(r => r.AlumnoYaInscriptoAsync(1, "alumno1")).ReturnsAsync(false);

            // Act
            (bool Success, string? Message) = await _service.InscribirseAsync(1, "alumno1");

            // Assert
            Assert.True(Success);
            Assert.Equal("Inscripción realizada correctamente.", Message);

            _cursoAlumnosRepositoryMock.Verify(r => r.InscribirAlumnoAsync(It.Is<CursoAlumnos>(ca => ca.CursoId == 1 && ca.AlumnoId == "alumno1" && ca.Estado == EstadoAcademico.Inscripto)), Times.Once);
            _cursoAlumnosRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetHistoriaAcademicaAsync_SinCursos_DeberiaRetornarValoresEnCero()
        {
            // Arrange
            _cursoAlumnosRepositoryMock.Setup(r => r.GetHistoriaAcademicaAsync("alumno1")).ReturnsAsync([]);

            // Act
            HistoriaAcademicaViewModel resultado = await _service.GetHistoriaAcademicaAsync("alumno1");

            // Assert
            Assert.Equal(0, resultado.TotalCursos);
            Assert.Equal(0, resultado.CursosAprobados);
            Assert.Equal(0, resultado.CursosEnCurso);
            Assert.Equal(0, resultado.PromedioGeneral);
            Assert.Empty(resultado.CursosActivos);
            Assert.Empty(resultado.CursosFinalizados);
        }

        [Fact]
        public async Task GetHistoriaAcademicaAsync_DeberiaCalcularIndicadoresCorrectamente()
        {
            // Arrange
            List<CursoAlumnos> inscripciones =
                [new CursoAlumnos
                {
                    Estado = EstadoAcademico.Aprobado,
                    NotaFinal = 8,
                    Curso = new Curso
                    {
                        NombreCurso = "C#"
                    }
                },
                new CursoAlumnos
                {
                    Estado = EstadoAcademico.Aprobado,
                    NotaFinal = 10,
                    Curso = new Curso
                    {
                        NombreCurso = "SQL"
                    }
                },
                new CursoAlumnos
                {
                    Estado = EstadoAcademico.Inscripto,
                    Curso = new Curso
                    {
                        NombreCurso = "ASP.NET"
                    }
                }];

            _cursoAlumnosRepositoryMock.Setup(r => r.GetHistoriaAcademicaAsync("alumno1")).ReturnsAsync(inscripciones);

            // Act
            HistoriaAcademicaViewModel resultado = await _service.GetHistoriaAcademicaAsync("alumno1");

            // Assert
            Assert.Equal(3, resultado.TotalCursos);
            Assert.Equal(2, resultado.CursosAprobados);
            Assert.Equal(1, resultado.CursosEnCurso);
            Assert.Equal(9, resultado.PromedioGeneral);
            Assert.Single(resultado.CursosActivos);
            Assert.Equal(2, resultado.CursosFinalizados.Count);
        }

        [Fact]
        public async Task GetHistoriaAcademicaAsync_DeberiaIgnorarCursosSinNotaEnPromedio()
        {
            // Arrange
            List<CursoAlumnos> inscripciones =
                [new CursoAlumnos
                {
                    Estado = EstadoAcademico.Aprobado,
                    NotaFinal = 8
                },
                new CursoAlumnos
                {
                    Estado = EstadoAcademico.Inscripto,
                    NotaFinal = null
                }];

            _cursoAlumnosRepositoryMock.Setup(r => r.GetHistoriaAcademicaAsync("alumno1")).ReturnsAsync(inscripciones);

            // Act
            HistoriaAcademicaViewModel resultado = await _service.GetHistoriaAcademicaAsync("alumno1");

            // Assert
            Assert.Equal(8, resultado.PromedioGeneral);
        }


        [Fact]
        public async Task GenerarCertificadoAsync_InscripcionInexistente_DeberiaRetornarNull()
        {
            // Arrange
            _cursoAlumnosRepositoryMock.Setup(r => r.GetCertificadoAsync("alumno1", 1)).ReturnsAsync((CursoAlumnos?)null);

            // Act
            byte[]? resultado = await _service.GenerarCertificadoAsync("alumno1", 1);

            // Assert
            Assert.Null(resultado);

            _pdfServiceMock.Verify(p => p.GenerateCertificate(It.IsAny<CertificadoViewModel>()), Times.Never);
        }

        [Theory]
        [InlineData(EstadoAcademico.Inscripto, EstadoCurso.Finalizado)]
        [InlineData(EstadoAcademico.Desaprobado, EstadoCurso.Finalizado)]
        [InlineData(EstadoAcademico.Aprobado, EstadoCurso.Autorizado)]
        public async Task GenerarCertificadoAsync_NoCumpleCondiciones_DeberiaRetornarNull(EstadoAcademico estadoAlumno, EstadoCurso estadoCurso)
        {
            // Arrange
            CursoAlumnos inscripcion = new()
            {
                Estado = estadoAlumno,
                Curso = new Curso
                {
                    EstadoCurso = estadoCurso
                },
                Alumno = new ApplicationUser()
            };

            _cursoAlumnosRepositoryMock.Setup(r => r.GetCertificadoAsync("alumno1", 1)).ReturnsAsync(inscripcion);

            // Act
            byte[]? resultado = await _service.GenerarCertificadoAsync("alumno1", 1);

            // Assert
            Assert.Null(resultado);

            _pdfServiceMock.Verify(p => p.GenerateCertificate(It.IsAny<CertificadoViewModel>()), Times.Never);
        }

        [Fact]
        public async Task GenerarCertificadoAsync_DeberiaGenerarPdf()
        {
            // Arrange
            byte[] pdfEsperado = [1, 2, 3];

            CursoAlumnos inscripcion = new()
            {
                Estado = EstadoAcademico.Aprobado,
                Alumno = new ApplicationUser
                {
                    Nombre = "Juan",
                    Apellido = "Pérez",
                    Legajo = "12345"
                },

                Curso = new Curso
                {
                    NombreCurso = "ASP.NET Core",
                    EstadoCurso = EstadoCurso.Finalizado,
                    FechaInicio = new DateOnly(2026, 1, 10),
                    FechaFin = new DateOnly(2026, 3, 15),
                    DuracionHoras = 80,
                    Profesor = new ApplicationUser
                    {
                        Nombre = "Ana",
                        Apellido = "García"
                    }
                }
            };

            _cursoAlumnosRepositoryMock.Setup(r => r.GetCertificadoAsync("alumno1", 1)).ReturnsAsync(inscripcion);
            _pdfServiceMock.Setup(p => p.GenerateCertificate(It.IsAny<CertificadoViewModel>())).Returns(pdfEsperado);

            // Act
            byte[]? resultado = await _service.GenerarCertificadoAsync("alumno1", 1);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(pdfEsperado, resultado);

            _pdfServiceMock.Verify(p => p.GenerateCertificate(It.Is<CertificadoViewModel>(c => c.Alumno == "Juan Pérez" && c.Legajo == "12345" && c.Curso == "ASP.NET Core" && c.Profesor == "Ana García" && c.DuracionHoras == 80)), Times.Once);
        }

        private static IMapper CreateMapper()
        {
            MapperConfigurationExpression expression = new();

            expression.AddMaps(typeof(CursoAlumnosProfile).Assembly);

            MapperConfiguration configuration =
                new(expression, NullLoggerFactory.Instance);

            configuration.AssertConfigurationIsValid();

            return configuration.CreateMapper();
        }
    }
}
