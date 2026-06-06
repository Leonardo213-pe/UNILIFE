using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilife.Data;
using Unilife.Models;

namespace Unilife.ViewComponents
{
    public class NotifBadgeViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotifBadgeViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = _userManager.GetUserId(HttpContext.User);
            if (userId == null) return Content("");

            var hoy = DateTime.Today;
            int count = 0;

            bool esDocente = HttpContext.User.IsInRole("Docente");
            bool esCoor    = HttpContext.User.IsInRole("Coordinador");

            if (!esDocente && !esCoor)
            {
                // Alumno: tareas vencidas o próximas (en los próximos 3 días)
                count = await _context.Tareas
                    .CountAsync(t => t.UsuarioId == userId && !t.Completada
                                  && t.FechaEntrega >= hoy && t.FechaEntrega <= hoy.AddDays(3));
            }
            else
            {
                // Docente / Coordinador: eventos próximos en los siguientes 7 días
                count = await _context.Eventos
                    .CountAsync(e => e.Fecha >= hoy && e.Fecha <= hoy.AddDays(7));
            }

            return View(count);
        }
    }
}
