using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilife.Data;
using Unilife.Models;

namespace Unilife.Controllers
{
    [Authorize(Roles = "Docente")]
    public class DocenteController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocenteController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db          = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);
            var userId  = usuario!.Id;
            var hoy     = DateTime.Today;

            var cursos = await _db.Cursos
                .Include(c => c.Horarios)
                .Include(c => c.Participantes)
                .Include(c => c.Modulos)
                .Where(c => c.DocenteId == userId)
                .ToListAsync();

            var eventosProximos = await _db.Eventos
                .Where(e => e.Fecha >= hoy && e.Fecha <= hoy.AddDays(14))
                .OrderBy(e => e.Fecha)
                .Take(5)
                .ToListAsync();

            ViewBag.Usuario         = usuario;
            ViewBag.Cursos          = cursos;
            ViewBag.EventosProximos = eventosProximos;
            ViewBag.TotalCursos     = cursos.Count;
            ViewBag.TotalAlumnos    = cursos.Sum(c => c.Participantes.Count);
            ViewBag.TotalModulos    = cursos.Sum(c => c.Modulos.Count);

            return View();
        }
    }
}
