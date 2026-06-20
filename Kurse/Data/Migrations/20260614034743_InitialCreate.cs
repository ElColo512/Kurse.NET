using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kurse.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Apellido",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Carrera",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Dni",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Legajo",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Titulo",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.CreateTable(
                name: "Cursos",
                columns: table => new
                {
                    CursoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdProfesor = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreCurso = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContenidoCurso = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeOnly>(type: "time", nullable: false),
                    TipoAprobacion = table.Column<int>(type: "int", nullable: false),
                    CupoMaximo = table.Column<int>(type: "int", nullable: false),
                    Arancelado = table.Column<bool>(type: "bit", nullable: false),
                    Arancel = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LugarDictado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CantidadClases = table.Column<int>(type: "int", nullable: false),
                    DuracionHoras = table.Column<int>(type: "int", nullable: false),
                    EstadoCurso = table.Column<int>(type: "int", nullable: false),
                    MotivoRechazo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cursos", x => x.CursoId);
                    table.ForeignKey(
                        name: "FK_Cursos_AspNetUsers_IdProfesor",
                        column: x => x.IdProfesor,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CursoAlumnos",
                columns: table => new
                {
                    CursoAlumnosId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlumnoId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    Asistencia = table.Column<int>(type: "int", nullable: true),
                    NotaFinal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    FechaInscripcion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaBaja = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoBaja = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CursoAlumnos", x => x.CursoAlumnosId);
                    table.ForeignKey(
                        name: "FK_CursoAlumnos_AspNetUsers_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CursoAlumnos_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "CursoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CursoDia",
                columns: table => new
                {
                    CursoDiaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CursoDia", x => x.CursoDiaId);
                    table.ForeignKey(
                        name: "FK_CursoDia_Cursos_CursoId",
                        column: x => x.CursoId,
                        principalTable: "Cursos",
                        principalColumn: "CursoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CursoAlumnos_AlumnoId",
                table: "CursoAlumnos",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_CursoAlumnos_CursoId",
                table: "CursoAlumnos",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_CursoDia_CursoId",
                table: "CursoDia",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_IdProfesor",
                table: "Cursos",
                column: "IdProfesor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CursoAlumnos");

            migrationBuilder.DropTable(
                name: "CursoDia");

            migrationBuilder.DropTable(
                name: "Cursos");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Apellido",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Carrera",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Dni",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Legajo",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Titulo",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
