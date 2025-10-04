using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Models;


namespace PortalAcademico.Data;


public static class SeedData
{
public static async Task InitializeAsync(IServiceProvider services)
{
using var scope = services.CreateScope();
var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();


// Aplicar migraciones
await ctx.Database.MigrateAsync();


// Crear rol Coordinador si no existe
if (!await roleManager.RoleExistsAsync("Coordinador"))
await roleManager.CreateAsync(new IdentityRole("Coordinador"));


// Crear usuario coordinador
var coordEmail = "coordinador@uni.edu";
var coord = await userManager.FindByEmailAsync(coordEmail);
if (coord == null)
    {
    coord = new ApplicationUser { UserName = coordEmail, Email = coordEmail, EmailConfirmed = true };
    await userManager.CreateAsync(coord, "Coordinador!123");
    await userManager.AddToRoleAsync(coord, "Coordinador");
    }


// Seed cursos si no existen
if (!ctx.Cursos.Any())
{
    var cursos = new[]
        {
            new Curso { Codigo = "MAT101", Nombre = "Matemáticas I", Creditos = 3, CupoMaximo = 30, HorarioInicio = TimeSpan.Parse("08:00"), HorarioFin = TimeSpan.Parse("09:30"), Activo = true },
            new Curso { Codigo = "PROG201", Nombre = "Programación II", Creditos = 4, CupoMaximo = 25, HorarioInicio = TimeSpan.Parse("10:00"), HorarioFin = TimeSpan.Parse("12:00"), Activo = true },
            new Curso { Codigo = "FIS150", Nombre = "Física Básica", Creditos = 3, CupoMaximo = 20, HorarioInicio = TimeSpan.Parse("13:00"), HorarioFin = TimeSpan.Parse("14:30"), Activo = true }
        };
        ctx.Cursos.AddRange(cursos);
        await ctx.SaveChangesAsync();
        }
    }
}