using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilife.Data;
using Unilife.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Unilife.Services;

namespace Unilife.Controllers
{
    [Authorize]
    public class EventosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RecomendadorEventosService _recomendador;

        public EventosController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RecomendadorEventosService recomendador)
        {
            _context      = context;
            _userManager  = userManager;
            _recomendador = recomendador;
        }

        [Authorize(Roles = "Alumno")]
        public async Task<IActionResult> Recomendados()
        {
            var usuario = await _userManager.GetUserAsync(User);
            var lista   = await _recomendador.ObtenerEventosRecomendadosAsync(usuario?.Carrera, 6);
            return View(lista);
        }

        public async Task<IActionResult> Index(string tipoEvento, string buscar)
        {
            try
            {
                var todos = await _context.Eventos.ToListAsync();

                IEnumerable<Evento> eventos = todos;

                if (!User.IsInRole("Coordinador"))
                {
                    var usuario = await _userManager.GetUserAsync(User);
                    var carrera = usuario?.Carrera;

                    eventos = string.IsNullOrEmpty(carrera)
                        ? todos.Where(e => e.EsGeneral)
                        : todos.Where(e => e.EsGeneral || e.Carrera == carrera);
                }

                if (!string.IsNullOrEmpty(tipoEvento))
                    eventos = eventos.Where(e => e.TipoEvento == tipoEvento);

                if (!string.IsNullOrEmpty(buscar))
                    eventos = eventos.Where(e =>
                        e.Titulo.Contains(buscar, StringComparison.OrdinalIgnoreCase) ||
                        e.Descripcion.Contains(buscar, StringComparison.OrdinalIgnoreCase));

                ViewBag.TipoEvento = tipoEvento;
                ViewBag.Buscar = buscar;

                return View(eventos.OrderBy(e => e.Fecha).ThenBy(e => e.Hora).ToList());
            }
            catch
            {
                ViewBag.TipoEvento = tipoEvento;
                ViewBag.Buscar = buscar;
                return View(new List<Evento>());
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.Id == id);

            if (evento == null) return NotFound();

            return View(evento);
        }

        [Authorize(Roles = "Coordinador")]
        public IActionResult Create()
        {
            ViewBag.Carreras = Carreras.Todas;
            return View();
        }

        [Authorize(Roles = "Coordinador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Evento evento)
        {
            if (evento.EsGeneral)
                evento.Carrera = null;

            if (ModelState.IsValid)
            {
                _context.Eventos.Add(evento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Carreras = Carreras.Todas;
            return View(evento);
        }

        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var evento = await _context.Eventos.FindAsync(id);

            if (evento == null) return NotFound();

            ViewBag.Carreras = Carreras.Todas;
            return View(evento);
        }

        [Authorize(Roles = "Coordinador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Evento evento)
        {
            if (id != evento.Id) return NotFound();

            if (evento.EsGeneral)
                evento.Carrera = null;

            if (ModelState.IsValid)
            {
                _context.Eventos.Update(evento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Carreras = Carreras.Todas;
            return View(evento);
        }

        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.Id == id);

            if (evento == null) return NotFound();

            return View(evento);
        }

        [Authorize(Roles = "Coordinador")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var evento = await _context.Eventos.FindAsync(id);

            if (evento != null)
            {
                _context.Eventos.Remove(evento);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
