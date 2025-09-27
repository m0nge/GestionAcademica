using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionAcademica.Migrations
{
    /// <inheritdoc />
    public partial class Add2FAAndSecurityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Materias");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Materias");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Asignaciones");

            migrationBuilder.DropColumn(
                name: "FechaAsignacion",
                table: "Asignaciones");

            migrationBuilder.DropColumn(
                name: "FechaEntrega",
                table: "Asignaciones");

            migrationBuilder.DropColumn(
                name: "PuntuacionMaxima",
                table: "Asignaciones");

            migrationBuilder.RenameColumn(
                name: "Titulo",
                table: "Asignaciones",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "TareaID",
                table: "Asignaciones",
                newName: "AsignacionID");

            migrationBuilder.AddColumn<int>(
                name: "CarreraID",
                table: "Materias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Creditos",
                table: "Materias",
                type: "int",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<string>(
                name: "Periodo",
                table: "Materias",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CarreraID",
                table: "Estudiantes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BloqueadoHasta",
                table: "Docentes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FacultadID",
                table: "Docentes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IntentosFallidos",
                table: "Docentes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TwoFactorCode",
                table: "Docentes",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TwoFactorCodeExpiry",
                table: "Docentes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "Docentes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UltimaIP",
                table: "Docentes",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoAcceso",
                table: "Docentes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimiento",
                table: "Asignaciones",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "NotaMaxima",
                table: "Asignaciones",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 10m);

            migrationBuilder.AddColumn<decimal>(
                name: "Porcentaje",
                table: "Asignaciones",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TipoAsignacionID",
                table: "Asignaciones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Calificaciones",
                columns: table => new
                {
                    CalificacionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AsignacionID = table.Column<int>(type: "int", nullable: false),
                    EstudianteID = table.Column<int>(type: "int", nullable: false),
                    Nota = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaCalificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calificaciones", x => x.CalificacionID);
                    table.ForeignKey(
                        name: "FK_Calificaciones_Asignaciones_AsignacionID",
                        column: x => x.AsignacionID,
                        principalTable: "Asignaciones",
                        principalColumn: "AsignacionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Calificaciones_Estudiantes_EstudianteID",
                        column: x => x.EstudianteID,
                        principalTable: "Estudiantes",
                        principalColumn: "EstudianteID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EstudianteMaterias",
                columns: table => new
                {
                    EstudianteMateriaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstudianteID = table.Column<int>(type: "int", nullable: false),
                    MateriaID = table.Column<int>(type: "int", nullable: false),
                    FechaInscripcion = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    Activa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstudianteMaterias", x => x.EstudianteMateriaID);
                    table.ForeignKey(
                        name: "FK_EstudianteMaterias_Estudiantes_EstudianteID",
                        column: x => x.EstudianteID,
                        principalTable: "Estudiantes",
                        principalColumn: "EstudianteID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EstudianteMaterias_Materias_MateriaID",
                        column: x => x.MateriaID,
                        principalTable: "Materias",
                        principalColumn: "MateriaID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Facultades",
                columns: table => new
                {
                    FacultadID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facultades", x => x.FacultadID);
                });

            migrationBuilder.CreateTable(
                name: "Inscripciones",
                columns: table => new
                {
                    InscripcionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstudianteID = table.Column<int>(type: "int", nullable: false),
                    MateriaID = table.Column<int>(type: "int", nullable: false),
                    FechaInscripcion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inscripciones", x => x.InscripcionID);
                    table.ForeignKey(
                        name: "FK_Inscripciones_Estudiantes_EstudianteID",
                        column: x => x.EstudianteID,
                        principalTable: "Estudiantes",
                        principalColumn: "EstudianteID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inscripciones_Materias_MateriaID",
                        column: x => x.MateriaID,
                        principalTable: "Materias",
                        principalColumn: "MateriaID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TiposAsignacion",
                columns: table => new
                {
                    TipoAsignacionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposAsignacion", x => x.TipoAsignacionID);
                });

            migrationBuilder.CreateTable(
                name: "Carreras",
                columns: table => new
                {
                    CarreraID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacultadID = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DuracionAnios = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carreras", x => x.CarreraID);
                    table.ForeignKey(
                        name: "FK_Carreras_Facultades_FacultadID",
                        column: x => x.FacultadID,
                        principalTable: "Facultades",
                        principalColumn: "FacultadID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarreraMateria",
                columns: table => new
                {
                    CarreraMateriaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarreraID = table.Column<int>(type: "int", nullable: false),
                    MateriaID = table.Column<int>(type: "int", nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Semestre = table.Column<int>(type: "int", nullable: false),
                    EsObligatoria = table.Column<bool>(type: "bit", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarreraMateria", x => x.CarreraMateriaID);
                    table.ForeignKey(
                        name: "FK_CarreraMateria_Carreras_CarreraID",
                        column: x => x.CarreraID,
                        principalTable: "Carreras",
                        principalColumn: "CarreraID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarreraMateria_Materias_MateriaID",
                        column: x => x.MateriaID,
                        principalTable: "Materias",
                        principalColumn: "MateriaID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Materias_CarreraID",
                table: "Materias",
                column: "CarreraID");

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_CarreraID",
                table: "Estudiantes",
                column: "CarreraID");

            migrationBuilder.CreateIndex(
                name: "IX_Docentes_FacultadID",
                table: "Docentes",
                column: "FacultadID");

            migrationBuilder.CreateIndex(
                name: "IX_Asignaciones_TipoAsignacionID",
                table: "Asignaciones",
                column: "TipoAsignacionID");

            migrationBuilder.CreateIndex(
                name: "IX_Calificaciones_AsignacionID",
                table: "Calificaciones",
                column: "AsignacionID");

            migrationBuilder.CreateIndex(
                name: "IX_Calificaciones_EstudianteID",
                table: "Calificaciones",
                column: "EstudianteID");

            migrationBuilder.CreateIndex(
                name: "IX_CarreraMateria_CarreraID",
                table: "CarreraMateria",
                column: "CarreraID");

            migrationBuilder.CreateIndex(
                name: "IX_CarreraMateria_MateriaID",
                table: "CarreraMateria",
                column: "MateriaID");

            migrationBuilder.CreateIndex(
                name: "IX_Carreras_FacultadID",
                table: "Carreras",
                column: "FacultadID");

            migrationBuilder.CreateIndex(
                name: "IX_EstudianteMaterias_MateriaID",
                table: "EstudianteMaterias",
                column: "MateriaID");

            migrationBuilder.CreateIndex(
                name: "IX_EstudianteMaterias_Unique",
                table: "EstudianteMaterias",
                columns: new[] { "EstudianteID", "MateriaID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_EstudianteID",
                table: "Inscripciones",
                column: "EstudianteID");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_MateriaID",
                table: "Inscripciones",
                column: "MateriaID");

            migrationBuilder.AddForeignKey(
                name: "FK_Asignaciones_TiposAsignacion_TipoAsignacionID",
                table: "Asignaciones",
                column: "TipoAsignacionID",
                principalTable: "TiposAsignacion",
                principalColumn: "TipoAsignacionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Docentes_Facultades_FacultadID",
                table: "Docentes",
                column: "FacultadID",
                principalTable: "Facultades",
                principalColumn: "FacultadID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Estudiantes_Carreras_CarreraID",
                table: "Estudiantes",
                column: "CarreraID",
                principalTable: "Carreras",
                principalColumn: "CarreraID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Materias_Carreras_CarreraID",
                table: "Materias",
                column: "CarreraID",
                principalTable: "Carreras",
                principalColumn: "CarreraID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asignaciones_TiposAsignacion_TipoAsignacionID",
                table: "Asignaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Docentes_Facultades_FacultadID",
                table: "Docentes");

            migrationBuilder.DropForeignKey(
                name: "FK_Estudiantes_Carreras_CarreraID",
                table: "Estudiantes");

            migrationBuilder.DropForeignKey(
                name: "FK_Materias_Carreras_CarreraID",
                table: "Materias");

            migrationBuilder.DropTable(
                name: "Calificaciones");

            migrationBuilder.DropTable(
                name: "CarreraMateria");

            migrationBuilder.DropTable(
                name: "EstudianteMaterias");

            migrationBuilder.DropTable(
                name: "Inscripciones");

            migrationBuilder.DropTable(
                name: "TiposAsignacion");

            migrationBuilder.DropTable(
                name: "Carreras");

            migrationBuilder.DropTable(
                name: "Facultades");

            migrationBuilder.DropIndex(
                name: "IX_Materias_CarreraID",
                table: "Materias");

            migrationBuilder.DropIndex(
                name: "IX_Estudiantes_CarreraID",
                table: "Estudiantes");

            migrationBuilder.DropIndex(
                name: "IX_Docentes_FacultadID",
                table: "Docentes");

            migrationBuilder.DropIndex(
                name: "IX_Asignaciones_TipoAsignacionID",
                table: "Asignaciones");

            migrationBuilder.DropColumn(
                name: "CarreraID",
                table: "Materias");

            migrationBuilder.DropColumn(
                name: "Creditos",
                table: "Materias");

            migrationBuilder.DropColumn(
                name: "Periodo",
                table: "Materias");

            migrationBuilder.DropColumn(
                name: "CarreraID",
                table: "Estudiantes");

            migrationBuilder.DropColumn(
                name: "BloqueadoHasta",
                table: "Docentes");

            migrationBuilder.DropColumn(
                name: "FacultadID",
                table: "Docentes");

            migrationBuilder.DropColumn(
                name: "IntentosFallidos",
                table: "Docentes");

            migrationBuilder.DropColumn(
                name: "TwoFactorCode",
                table: "Docentes");

            migrationBuilder.DropColumn(
                name: "TwoFactorCodeExpiry",
                table: "Docentes");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "Docentes");

            migrationBuilder.DropColumn(
                name: "UltimaIP",
                table: "Docentes");

            migrationBuilder.DropColumn(
                name: "UltimoAcceso",
                table: "Docentes");

            migrationBuilder.DropColumn(
                name: "FechaVencimiento",
                table: "Asignaciones");

            migrationBuilder.DropColumn(
                name: "NotaMaxima",
                table: "Asignaciones");

            migrationBuilder.DropColumn(
                name: "Porcentaje",
                table: "Asignaciones");

            migrationBuilder.DropColumn(
                name: "TipoAsignacionID",
                table: "Asignaciones");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Asignaciones",
                newName: "Titulo");

            migrationBuilder.RenameColumn(
                name: "AsignacionID",
                table: "Asignaciones",
                newName: "TareaID");

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Materias",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Materias",
                type: "datetime",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Asignaciones",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pendiente");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAsignacion",
                table: "Asignaciones",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEntrega",
                table: "Asignaciones",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "PuntuacionMaxima",
                table: "Asignaciones",
                type: "int",
                nullable: false,
                defaultValue: 10);
        }
    }
}
