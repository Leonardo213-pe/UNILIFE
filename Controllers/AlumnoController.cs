using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilife.Data;
using Unilife.Models;
using Unilife.Services;

namespace Unilife.Controllers
{
    [Authorize(Roles = "Alumno")]
    public class AlumnoController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RecomendadorEventosService _recEventos;
        private readonly RecomendadorLugaresService _recLugares;

        public AlumnoController(ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RecomendadorEventosService recEventos,
            RecomendadorLugaresService recLugares)
        {
            _db          = db;
            _userManager = userManager;
            _recEventos  = recEventos;
            _recLugares  = recLugares;
        }

        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);
            var userId  = usuario!.Id;
            var hoy     = DateTime.Today;

            // Cursos inscritos
            var cursos = await _db.Cursos
                .Include(c => c.DocenteUser)
                .Include(c => c.Horarios)
                .Where(c => c.Participantes.Any(p => p.AlumnoId == userId))
                .ToListAsync();

            // Tareas pendientes y vencidas
            var tareas = await _db.Tareas
                .Where(t => t.UsuarioId == userId && t.Estado != "Completada")
                .OrderBy(t => t.FechaEntrega)
                .Take(5)
                .ToListAsync();

            // Recomendaciones (se obtienen en paralelo para no bloquear)
            var eventosTask  = _recEventos.ObtenerEventosRecomendadosAsync(usuario.Carrera, 3);
            var lugaresTask  = _recLugares.ObtenerRecomendacionesAsync(userId, 3);
            await Task.WhenAll(eventosTask, lugaresTask);

            // Métricas
            var totalTareas     = await _db.Tareas.CountAsync(t => t.UsuarioId == userId);
            var completadas     = await _db.Tareas.CountAsync(t => t.UsuarioId == userId && t.Estado == "Completada");
            var tareasVencidas  = tareas.Count(t => t.EsVencida);

            ViewBag.Usuario         = usuario;
            ViewBag.Cursos          = cursos;
            ViewBag.Tareas          = tareas;
            ViewBag.EventosRec      = eventosTask.Result;
            ViewBag.LugaresRec      = lugaresTask.Result;
            ViewBag.TotalCursos     = cursos.Count;
            ViewBag.TotalTareas     = totalTareas;
            ViewBag.Completadas     = completadas;
            ViewBag.TareasVencidas  = tareasVencidas;
            ViewBag.Progreso        = totalTareas == 0 ? 0 : (int)Math.Round(completadas * 100.0 / totalTareas);

            return View();
        }
    }
}
