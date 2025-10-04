using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers;

public class CursosController : Controller
{
    private readonly ApplicationDbContext _context;

    public CursosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Cursos
    public async Task<IActionResult> Index(string? nombre, int? creditosMin, int? creditosMax, TimeSpan? horarioInicio, TimeSpan? horarioFin)
    {
        var query = _context.Cursos
            .Where(c => c.Activo)
            .AsQueryable();

        if (!string.IsNullOrEmpty(nombre))
            query = query.Where(c => c.Nombre.Contains(nombre));

        if (creditosMin.HasValue)
        {
            if (creditosMin.Value < 0)
                ModelState.AddModelError("creditosMin", "Los créditos no pueden ser negativos.");
            else
                query = query.Where(c => c.Creditos >= creditosMin);
        }

        if (creditosMax.HasValue)
            query = query.Where(c => c.Creditos <= creditosMax);

        if (horarioInicio.HasValue && horarioFin.HasValue)
        {
            if (horarioFin <= horarioInicio)
                ModelState.AddModelError("Horario", "El horario final no puede ser menor o igual al inicial.");
            else
                query = query.Where(c => c.HorarioInicio >= horarioInicio && c.HorarioFin <= horarioFin);
        }

        var cursos = await query.ToListAsync();

        return View(cursos);
    }

    // GET: /Cursos/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var curso = await _context.Cursos.FindAsync(id);
        if (curso == null || !curso.Activo)
            return NotFound();

        return View(curso);
    }
}