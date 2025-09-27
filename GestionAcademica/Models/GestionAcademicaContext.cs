using Microsoft.EntityFrameworkCore;

namespace GestionAcademica.Models
{
    public class GestionAcademicaContext : DbContext
    {
        public GestionAcademicaContext(DbContextOptions<GestionAcademicaContext> options)
            : base(options)
        {
        }

        // DbSet para cada tabla
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Docente> Docentes { get; set; }
        public DbSet<Materia> Materias { get; set; }
        public DbSet<Asignacion> Asignaciones { get; set; }
        public DbSet<TipoAsignacion> TiposAsignacion { get; set; }
        public DbSet<Calificacion> Calificaciones { get; set; }
        public DbSet<Inscripcion> Inscripciones { get; set; }
        public DbSet<Facultad> Facultades { get; set; }
        public DbSet<Carrera> Carreras { get; set; }
        public DbSet<EstudianteMateria> EstudianteMaterias { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de la tabla EstudianteMaterias
            modelBuilder.Entity<EstudianteMateria>(entity =>
            {
                entity.ToTable("EstudianteMaterias");
                entity.HasKey(e => e.EstudianteMateriaID);

                entity.Property(e => e.EstudianteMateriaID)
                    .HasColumnName("EstudianteMateriaID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.FechaInscripcion)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.Activa)
                    .HasDefaultValue(true);

                entity.Property(e => e.Observaciones)
                    .HasMaxLength(500);

                // Relación con Estudiante
                entity.HasOne(e => e.Estudiante)
                    .WithMany()
                    .HasForeignKey(e => e.EstudianteID)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Materia
                entity.HasOne(e => e.Materia)
                    .WithMany()
                    .HasForeignKey(e => e.MateriaID)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índice único para evitar inscripciones duplicadas
                entity.HasIndex(e => new { e.EstudianteID, e.MateriaID })
                    .IsUnique()
                    .HasDatabaseName("IX_EstudianteMaterias_Unique");
            });
            base.OnModelCreating(modelBuilder);

            // Configuración de la tabla Estudiantes
            modelBuilder.Entity<Estudiante>(entity =>
            {
                entity.ToTable("Estudiantes");
                entity.HasKey(e => e.EstudianteID);

                entity.Property(e => e.EstudianteID)
                    .HasColumnName("EstudianteID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Carnet)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(e => e.Nombres)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Apellidos)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasMaxLength(150);

                entity.Property(e => e.Telefono)
                    .HasMaxLength(20);

                entity.Property(e => e.FechaNacimiento)
                    .HasColumnType("date");

                entity.Property(e => e.FechaIngreso)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.Activo)
                    .HasDefaultValue(true);

                // Índices únicos
                entity.HasIndex(e => e.Carnet)
                    .IsUnique()
                    .HasDatabaseName("IX_Estudiantes_Carnet");

                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Estudiantes_Email")
                    .HasFilter("Email IS NOT NULL");
                // En la configuración de la entidad Estudiante, agregar:
                entity.HasOne(e => e.Carrera)
                    .WithMany(c => c.Estudiantes)
                    .HasForeignKey(e => e.CarreraID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de la tabla Docentes
            modelBuilder.Entity<Docente>(entity =>
            {
                entity.ToTable("Docentes");
                entity.HasKey(e => e.DocenteID);

                entity.Property(e => e.DocenteID)
                    .HasColumnName("DocenteID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Nombres)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Apellidos)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Email)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(e => e.Telefono)
                    .HasMaxLength(20);

                entity.Property(e => e.FechaRegistro)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.Activo)
                    .HasDefaultValue(true);

                entity.Property(e => e.Contraseña)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(e => e.Rol)
                    .HasMaxLength(20)
                    .HasDefaultValue("Docente");

                // Índice único para email
                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Docentes_Email");
            });

            // Configuración de la tabla Materias
            modelBuilder.Entity<Materia>(entity =>
            {
                entity.ToTable("Materias");
                entity.HasKey(e => e.MateriaID);

                entity.Property(e => e.MateriaID)
                    .HasColumnName("MateriaID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Nombre)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Codigo)
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(e => e.Creditos)
                    .HasDefaultValue(3);

                entity.Property(e => e.Periodo)
                    .HasMaxLength(20);

                entity.Property(e => e.Activa)
                    .HasDefaultValue(true);

                // Relación con Docente
                entity.HasOne(e => e.Docente)
                    .WithMany()
                    .HasForeignKey(e => e.DocenteID)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índice único para el código
                entity.HasIndex(e => e.Codigo)
                    .IsUnique()
                    .HasDatabaseName("IX_Materias_Codigo");
            });

            // Configuración de la tabla Asignaciones
            modelBuilder.Entity<Asignacion>(entity =>
            {
                entity.ToTable("Asignaciones");
                entity.HasKey(e => e.AsignacionID);

                entity.Property(e => e.AsignacionID)
                    .HasColumnName("AsignacionID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Nombre)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(1000);

                entity.Property(e => e.Porcentaje)
                    .HasColumnType("decimal(5,2)")
                    .HasDefaultValue(0);

                entity.Property(e => e.FechaCreacion)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.FechaVencimiento)
                    .HasColumnType("datetime");

                entity.Property(e => e.NotaMaxima)
                    .HasColumnType("decimal(5,2)")
                    .HasDefaultValue(10);

                entity.Property(e => e.Activa)
                    .HasDefaultValue(true);

                // Relación con Materia
                entity.HasOne(e => e.Materia)
                    .WithMany(m => m.Asignaciones)
                    .HasForeignKey(e => e.MateriaID)
                    .OnDelete(DeleteBehavior.Cascade);
                // Agregar esto en OnModelCreating después de las configuraciones existentes:

                // Configuración de la tabla Facultades
                modelBuilder.Entity<Facultad>(entity =>
                {
                    entity.ToTable("Facultades");
                    entity.HasKey(e => e.FacultadID);

                    entity.Property(e => e.Codigo)
                        .HasMaxLength(10)
                        .IsRequired();

                    entity.Property(e => e.Nombre)
                        .HasMaxLength(100)
                        .IsRequired();

                    entity.Property(e => e.Descripcion)
                        .HasMaxLength(500);
                });

                // Configuración de la tabla Carreras (actualizar la existente si ya la tienes)
                modelBuilder.Entity<Carrera>(entity =>
                {
                    entity.ToTable("Carreras");
                    entity.HasKey(e => e.CarreraID);

                    // Relación con Facultad
                    entity.HasOne(e => e.Facultad)
                        .WithMany(f => f.Carreras)
                        .HasForeignKey(e => e.FacultadID)
                        .OnDelete(DeleteBehavior.Restrict);
                });
            });
        }
    }
}