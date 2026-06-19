using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilife.Data;
using Unilife.Models;

namespace Unilife.Controllers
{
    [Authorize(Roles = "Alumno")]
    public class TareasController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public TareasController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? estado)
        {
            var userId = _userManager.GetUserId(User)!;
            var query = _db.Tareas.Where(t => t.UsuarioId == userId);

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(t => t.Estado == estado);

            ViewBag.EstadoFiltro = estado ?? "";

            var tareas = await query.OrderBy(t => t.FechaEntrega).ToListAsync();
            return View(tareas);
        }

        public IActionResult Create() => View(new Tarea { FechaEntrega = DateTime.Today.AddDays(7) });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tarea tarea)
        {
            tarea.UsuarioId = _userManager.GetUserId(User)!;
            if (string.IsNullOrEmpty(tarea.Estado)) tarea.Estado = "Pendiente";

            ModelState.Remove("UsuarioId");
            if (!ModelState.IsValid) return View(tarea);

            _db.Tareas.Add(tarea);
            await _db.SaveChangesAsync();
            TempData["Toast"] = "Tarea creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var tarea = await _db.Tareas.FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == userId);
            if (tarea == null) return NotFound();
            return View(tarea);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tarea tarea)
        {
            var userId = _userManager.GetUserId(User)!;
            var existing = await _db.Tareas.FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == userId);
            if (existing == null) return NotFound();

            ModelState.Remove("UsuarioId");
            if (!ModelState.IsValid) return View(tarea);

            existing.Titulo      = tarea.Titulo;
            existing.Curso       = tarea.Curso;
            existing.FechaEntrega = tarea.FechaEntrega;
            existing.Prioridad   = tarea.Prioridad;
            existing.Estado      = tarea.Estado;

            await _db.SaveChangesAsync();
            TempData["Toast"] = "Tarea actualizada.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var tarea = await _db.Tareas.FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == userId);
            if (tarea == null) return NotFound();
            return View(tarea);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var tarea = await _db.Tareas.FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == userId);
            if (tarea == null) return NotFound();

            _db.Tareas.Remove(tarea);
            await _db.SaveChangesAsync();
            TempData["Toast"] = "Tarea eliminada.";
            return RedirectToAction(nameof(Index));
        }
    }
}
