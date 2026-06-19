using Microsoft.EntityFrameworkCore;
using Unilife.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Unilife.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tarea> Tareas { get; set; }
        public DbSet<Lugar> Lugares { get; set; }
        public DbSet<Evento> Eventos { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<CursoAlumno> CursoAlumnos { get; set; }
        public DbSet<HorarioCurso> HorariosCurso { get; set; }
        public DbSet<CarreraRegistrada> CarrerasRegistradas { get; set; }
        public DbSet<Modulo> Modulos { get; set; }
        public DbSet<Actividad> Actividades { get; set; }
        public DbSet<ValoracionLugar> ValoracionesLugar { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ValoracionLugar>()
                .HasIndex(v => new { v.UsuarioId, v.LugarId })
                .IsUnique();

            builder.Entity<ValoracionLugar>()
                .HasOne(v => v.Lugar)
                .WithMany()
                .HasForeignKey(v => v.LugarId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CursoAlumno>()
                .HasKey(ca => new { ca.CursoId, ca.AlumnoId });

            builder.Entity<CursoAlumno>()
                .HasOne(ca => ca.Curso)
                .WithMany(c => c.Participantes)
                .HasForeignKey(ca => ca.CursoId);

            builder.Entity<CursoAlumno>()
                .HasOne(ca => ca.Alumno)
                .WithMany()
                .HasForeignKey(ca => ca.AlumnoId);

            builder.Entity<Curso>()
                .HasOne(c => c.DocenteUser)
                .WithMany()
                .HasForeignKey(c => c.DocenteId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<HorarioCurso>()
                .HasOne(h => h.Curso)
                .WithMany(c => c.Horarios)
                .HasForeignKey(h => h.CursoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<HorarioCurso>()
                .Ignore(h => h.CodigoAula);

            builder.Entity<Modulo>()
                .HasOne(m => m.Curso)
                .WithMany(c => c.Modulos)
                .HasForeignKey(m => m.CursoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Actividad>()
                .HasOne(a => a.Modulo)
                .WithMany(m => m.Actividades)
                .HasForeignKey(a => a.ModuloId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
