using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Cursos
        public async Task<IActionResult> Index(string nombre, int? creditosMin, int? creditosMax, TimeSpan? horarioInicio, TimeSpan? horarioFin)
        {
            // Validaciones básicas del lado servidor
            if (creditosMin < 0 || creditosMax < 0)
            {
                ModelState.AddModelError("", "Los créditos no pueden ser negativos.");
            }

            if (horarioInicio.HasValue && horarioFin.HasValue && horarioFin <= horarioInicio)
            {
                ModelState.AddModelError("", "El horario final no puede ser anterior o igual al horario inicial.");
            }

            var cursos = _context.Cursos.AsQueryable();

            // Solo cursos activos
            cursos = cursos.Where(c => c.Activo);

            // Filtros dinámicos
            if (!string.IsNullOrEmpty(nombre))
                cursos = cursos.Where(c => c.Nombre.Contains(nombre));

            if (creditosMin.HasValue)
                cursos = cursos.Where(c => c.Creditos >= creditosMin.Value);

            if (creditosMax.HasValue)
                cursos = cursos.Where(c => c.Creditos <= creditosMax.Value);

            if (horarioInicio.HasValue)
                cursos = cursos.Where(c => c.HorarioInicio >= horarioInicio.Value);

            if (horarioFin.HasValue)
                cursos = cursos.Where(c => c.HorarioFin <= horarioFin.Value);

            // Enviar filtros a la vista (para que se mantengan en los inputs)
            ViewData["nombre"] = nombre;
            ViewData["creditosMin"] = creditosMin;
            ViewData["creditosMax"] = creditosMax;

            // Retornar la lista
            return View(await cursos.ToListAsync());
        }

        // GET: /Cursos/Detalle/5
        public async Task<IActionResult> Detalle(int? id)
        {
            if (id == null)
                return NotFound();

            var curso = await _context.Cursos
                .FirstOrDefaultAsync(m => m.Id == id);

            if (curso == null)
                return NotFound();

            return View(curso);
        }

        // GET: /Cursos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Cursos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Codigo,Nombre,Creditos,CupoMaximo,HorarioInicio,HorarioFin,Activo")] Curso curso)
        {
            // Validaciones servidor
            if (curso.Creditos <= 0)
            {
                ModelState.AddModelError("Creditos", "Los créditos deben ser mayores a 0.");
            }

            if (curso.HorarioInicio >= curso.HorarioFin)
            {
                ModelState.AddModelError("HorarioFin", "El horario de fin debe ser posterior al inicio.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(curso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(curso);
        }
    }
}