using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers
{
    [Authorize] // Requiere sesión para cualquier acción
    public class MatriculasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MatriculasController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: /Matriculas/Inscribirse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribirse(int cursoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para inscribirte.";
                return RedirectToAction("Login", "Account");
            }

            var curso = await _context.Cursos.FindAsync(cursoId);
            if (curso == null || !curso.Activo)
            {
                TempData["ErrorMessage"] = "El curso no existe o está inactivo.";
                return RedirectToAction("Index", "Cursos");
            }

            // Validar que el usuario no esté ya inscrito en el curso
            bool yaMatriculado = await _context.Matriculas
                .AnyAsync(m => m.CursoId == cursoId && m.UsuarioId == user.Id && m.Estado != EstadoMatricula.Cancelada);

            if (yaMatriculado)
            {
                TempData["ErrorMessage"] = "Ya estás inscrito en este curso.";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }

            // Validar cupo máximo
            int inscritos = await _context.Matriculas
                .CountAsync(m => m.CursoId == cursoId && m.Estado != EstadoMatricula.Cancelada);

            if (inscritos >= curso.CupoMaximo)
            {
                TempData["ErrorMessage"] = "El curso ya alcanzó el cupo máximo.";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }

            // Validar solapamiento de horario
            var matriculasUsuario = await _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.UsuarioId == user.Id && m.Estado != EstadoMatricula.Cancelada)
                .ToListAsync();

            bool horarioSolapado = matriculasUsuario.Any(m =>
                (curso.HorarioInicio < m.Curso.HorarioFin) &&
                (m.Curso.HorarioInicio < curso.HorarioFin));

            if (horarioSolapado)
            {
                TempData["ErrorMessage"] = "Tienes otro curso en el mismo horario.";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }

            // Crear la matrícula
            var matricula = new Matricula
            {
                CursoId = cursoId,
                UsuarioId = user.Id,
                FechaRegistro = DateTime.Now,
                Estado = EstadoMatricula.Pendiente
            };

            _context.Matriculas.Add(matricula);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Te has inscrito exitosamente al curso {curso.Nombre}.";
            return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
        }
    }
}