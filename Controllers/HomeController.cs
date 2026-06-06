using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilife.Data;
using Unilife.Models;

namespace Unilife.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var usuario = await _userManager.GetUserAsync(User);
        var hoy = DateTime.Today;
        bool esDocente      = User.IsInRole("Docente");
        bool esCoordinador  = User.IsInRole("Coordinador");

        var tareasUsuario = await _context.Tareas
            .Where(t => t.UsuarioId == userId)
            .ToListAsync();

        // Coordinador y Docente ven todos los eventos; Alumno solo generales o de su carrera
        var carreraUsuario = usuario?.Carrera ?? "";
        var eventosProximos = (esDocente || esCoordinador)
            ? await _context.Eventos.Where(e => e.Fecha >= hoy).ToListAsync()
            : await _context.Eventos
                .Where(e => e.Fecha >= hoy && (e.EsGeneral || e.Carrera == carreraUsuario))
                .ToListAsync();

        var completadas = tareasUsuario.Count(t => t.Completada);
        var total = tareasUsuario.Count;

        var modelo = new DashboardViewModel
        {
            Nombre      = usuario?.Nombre ?? "Usuario",
            EsDocente   = esDocente,
            Completadas = completadas,
            Pendientes  = total - completadas,
            EventosProximos = eventosProximos.Count,
            Progreso    = total == 0 ? 0 : (int)Math.Round(completadas * 100.0 / total),

            ProximasTareas = tareasUsuario
                .Where(t => !t.Completada)
                .OrderBy(t => t.FechaEntrega)
                .Take(4)
                .ToList(),

            Recordatorios = eventosProximos
                .OrderBy(e => e.Fecha)
                .ThenBy(e => e.Hora)
                .Take(5)
                .ToList()
        };

        if (esDocente)
        {
            var misCursos = await _context.Cursos
                .Include(c => c.Horarios)
                .Include(c => c.Participantes)
                .Where(c => c.DocenteId == userId)
                .ToListAsync();

            modelo.MisCursos    = misCursos;
            modelo.TotalAlumnos = misCursos.Sum(c => c.Participantes.Count);
            modelo.Horario      = misCursos
                .OrderBy(c => c.Horarios.Min(h => (TimeSpan?)h.HoraInicio) ?? TimeSpan.Zero)
                .Take(5)
                .ToList();
        }
        else
        {
            // Alumno: solo cursos en los que está inscrito
            var cursosInscritos = await _context.Cursos
                .Include(c => c.DocenteUser)
                .Include(c => c.Horarios)
                .Include(c => c.Participantes)
                .Where(c => c.Participantes.Any(p => p.AlumnoId == userId))
                .ToListAsync();

            modelo.MisCursos = cursosInscritos;
            modelo.Horario   = cursosInscritos
                .OrderBy(c => c.Horarios.Min(h => (TimeSpan?)h.HoraInicio) ?? TimeSpan.Zero)
                .ToList();
        }

        return View(modelo);
    }

[AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
