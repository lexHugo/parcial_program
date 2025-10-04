using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Models;

namespace PortalAcademico.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Curso> Cursos { get; set; } = null!;
    public DbSet<Matricula> Matriculas { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);


    // Codigo unico en Curso
    builder.Entity<Curso>()
    .HasIndex(c => c.Codigo)
    .IsUnique();


    // Check constraints: Creditos > 0, HorarioInicio < HorarioFin
    builder.Entity<Curso>()
    .HasCheckConstraint("CK_Curso_Creditos_Positive", "Creditos > 0")
    .HasCheckConstraint("CK_Curso_Horario", "HorarioInicio < HorarioFin");


    // Un usuario no puede matricularse mas de una vez en el mismo curso
    builder.Entity<Matricula>()
    .HasIndex(m => new { m.CursoId, m.UsuarioId })
    .IsUnique();


    // Relaciones
    builder.Entity<Matricula>()
    .HasOne(m => m.Curso)
    .WithMany(c => c.Matriculas)
    .HasForeignKey(m => m.CursoId);
    }
}
