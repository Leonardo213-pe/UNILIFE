using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilife.Data;
using Unilife.Models;
using Unilife.Services;

namespace Unilife.Controllers
{
    [Authorize]
    public class LugaresController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RecomendadorLugaresService _recomendador;

        public LugaresController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RecomendadorLugaresService recomendador)
        {
            _context      = context;
            _userManager  = userManager;
            _recomendador = recomendador;
        }

        [Authorize(Roles = "Alumno")]
        public async Task<IActionResult> Recomendados()
        {
            var userId = _userManager.GetUserId(User)!;
            var lista  = await _recomendador.ObtenerRecomendacionesAsync(userId, 6);
            return View(lista);
        }

        public async Task<IActionResult> Index(string tipo, string buscar)
        {
            var lugares = _context.Lugares.AsQueryable();

            if (!string.IsNullOrWhiteSpace(tipo))
                lugares = lugares.Where(l => l.Tipo == tipo);

            if (!string.IsNullOrWhiteSpace(buscar))
                lugares = lugares.Where(l =>
                    l.Nombre.Contains(buscar) ||
                    l.Direccion.Contains(buscar) ||
                    l.Descripcion.Contains(buscar));

            ViewBag.Tipo   = tipo;
            ViewBag.Buscar = buscar;

            var lista = await lugares.OrderByDescending(l => l.Calificacion).ToListAsync();

            // Promedios de valoraciones por usuario
            var promedios = await _context.ValoracionesLugar
                .GroupBy(v => v.LugarId)
                .Select(g => new { LugarId = g.Key, Promedio = g.Average(v => (double)v.Puntaje), Total = g.Count() })
                .ToDictionaryAsync(x => x.LugarId, x => (x.Promedio, x.Total));

            ViewBag.Promedios = promedios;

            // Valoración del usuario actual
            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                var misValoraciones = await _context.ValoracionesLugar
                    .Where(v => v.UsuarioId == userId)
                    .ToDictionaryAsync(v => v.LugarId, v => v.Puntaje);
                ViewBag.MisValoraciones = misValoraciones;
            }
            else
            {
                ViewBag.MisValoraciones = new Dictionary<int, int>();
            }

            return View(lista);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Valorar(int lugarId, int puntaje)
        {
            if (puntaje < 1 || puntaje > 5) return BadRequest();

            var userId = _userManager.GetUserId(User)!;

            var existing = await _context.ValoracionesLugar
                .FirstOrDefaultAsync(v => v.UsuarioId == userId && v.LugarId == lugarId);

            if (existing != null)
            {
                existing.Puntaje         = puntaje;
                existing.FechaValoracion = DateTime.Now;
            }
            else
            {
                _context.ValoracionesLugar.Add(new ValoracionLugar
                {
                    UsuarioId        = userId,
                    LugarId          = lugarId,
                    Puntaje          = puntaje,
                    FechaValoracion  = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            TempData["Toast"] = "Valoración guardada.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var lugar = await _context.Lugares.FirstOrDefaultAsync(l => l.Id == id);
            if (lugar == null) return NotFound();
            return View(lugar);
        }

        [Authorize(Roles = "Coordinador")]
        public IActionResult Create() => View();

        [Authorize(Roles = "Coordinador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lugar lugar)
        {
            ModelState.Remove("Id");
            if (!ModelState.IsValid) return View(lugar);

            _context.Lugares.Add(lugar);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var lugar = await _context.Lugares.FindAsync(id);
            if (lugar == null) return NotFound();
            return View(lugar);
        }

        [Authorize(Roles = "Coordinador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Lugar lugar)
        {
            if (id != lugar.Id) return NotFound();
            ModelState.Remove("Id");
            if (!ModelState.IsValid) return View(lugar);

            _context.Lugares.Update(lugar);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var lugar = await _context.Lugares.FirstOrDefaultAsync(l => l.Id == id);
            if (lugar == null) return NotFound();
            return View(lugar);
        }

        [Authorize(Roles = "Coordinador")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lugar = await _context.Lugares.FindAsync(id);
            if (lugar != null) { _context.Lugares.Remove(lugar); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }
    }
}
