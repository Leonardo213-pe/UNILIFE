using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilife.Data;
using Unilife.Models;

namespace Unilife.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoordinadorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var alumnos = await _userManager.GetUsersInRoleAsync("Alumno");
            var docentes = await _userManager.GetUsersInRoleAsync("Docente");
            var tareas = await _context.Tareas.ToListAsync();
            var cursos = await _context.Cursos.Include(c => c.Participantes).ToListAsync();
            var hoy = DateTime.Today;

            var carreraConMas = cursos
                .GroupBy(c => c.Carrera)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "—";

            var vm = new CoordinadorDashboardViewModel
            {
                TotalAlumnos = alumnos.Count,
                TotalDocentes = docentes.Count,
                TotalCursos = cursos.Count,
                TotalEventos = await _context.Eventos.CountAsync(),
                TotalLugares = await _context.Lugares.CountAsync(),
                TareasTotales = tareas.Count,
                TareasCompletadas = tareas.Count(t => t.Completada),
                EventosProximos = await _context.Eventos.CountAsync(e => e.Fecha >= hoy && e.Fecha <= hoy.AddDays(7)),
                CursosConAlumnos = cursos.Count(c => c.Participantes.Any()),
                TotalInscripciones = cursos.Sum(c => c.Participantes.Count),
                CarreraConMasCursos = carreraConMas
            };

            return View(vm);
        }

        // ─── Usuarios ────────────────────────────────────────────────────────────

        public async Task<IActionResult> Usuarios(string? buscar, string? filtroRol)
        {
            var usuarios = _userManager.Users.ToList();
            var resultado = new List<(ApplicationUser Usuario, string Rol)>();

            foreach (var u in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var rol = roles.FirstOrDefault() ?? "Sin rol";
                if (rol == "Coordinador") continue;
                resultado.Add((u, rol));
            }

            if (!string.IsNullOrEmpty(buscar))
                resultado = resultado.Where(r =>
                    r.Usuario.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase) ||
                    r.Usuario.Apellido.Contains(buscar, StringComparison.OrdinalIgnoreCase) ||
                    (r.Usuario.Email ?? "").Contains(buscar, StringComparison.OrdinalIgnoreCase)
                ).ToList();

            if (!string.IsNullOrEmpty(filtroRol))
                resultado = resultado.Where(r => r.Rol == filtroRol).ToList();

            ViewBag.Buscar = buscar;
            ViewBag.FiltroRol = filtroRol;
            return View(resultado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCarrera(string nombre, string grupo)
        {
            if (!string.IsNullOrWhiteSpace(nombre) && !string.IsNullOrWhiteSpace(grupo))
            {
                var yaExiste = await _context.CarrerasRegistradas
                    .AnyAsync(c => c.Nombre.ToLower() == nombre.Trim().ToLower());
                if (!yaExiste)
                {
                    _context.CarrerasRegistradas.Add(new CarreraRegistrada
                    {
                        Nombre = nombre.Trim(),
                        Grupo = grupo,
                        EsMedica = grupo == "Ciencias Médicas"
                    });
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Index", "Cursos");
        }

        public IActionResult CrearUsuario()
        {
            ViewBag.Carreras = Carreras.Todas;
            return View(new CrearUsuarioViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUsuario(CrearUsuarioViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Carreras = Carreras.Todas;
                return View(vm);
            }

            var usuario = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                EmailConfirmed = true,
                Nombre = vm.Nombre,
                Apellido = vm.Apellido,
                Carrera = vm.Carrera
            };

            var resultado = await _userManager.CreateAsync(usuario, vm.Password);

            if (resultado.Succeeded)
            {
                await _userManager.AddToRoleAsync(usuario, vm.Rol);
                return RedirectToAction(nameof(Usuarios));
            }

            foreach (var error in resultado.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            ViewBag.Carreras = Carreras.Todas;
            return View(vm);
        }

        public async Task<IActionResult> EditarUsuario(string id)
        {
            if (id == null) return NotFound();

            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(usuario);

            var vm = new CrearUsuarioViewModel
            {
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email ?? string.Empty,
                Rol = roles.FirstOrDefault() ?? "Alumno",
                Carrera = usuario.Carrera,
                Password = "placeholder"
            };

            ViewBag.UserId = id;
            ViewBag.Carreras = Carreras.Todas;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(string id, CrearUsuarioViewModel vm)
        {
            ModelState.Remove("Password");

            if (!ModelState.IsValid)
            {
                ViewBag.UserId = id;
                ViewBag.Carreras = Carreras.Todas;
                return View(vm);
            }

            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            usuario.Nombre = vm.Nombre;
            usuario.Apellido = vm.Apellido;
            usuario.Email = vm.Email;
            usuario.UserName = vm.Email;
            usuario.Carrera = vm.Carrera;

            await _userManager.UpdateAsync(usuario);

            var rolesActuales = await _userManager.GetRolesAsync(usuario);
            await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            await _userManager.AddToRoleAsync(usuario, vm.Rol);

            // Cambio de contraseña opcional
            var nuevaPass = Request.Form["NuevaContrasena"].ToString();
            if (!string.IsNullOrEmpty(nuevaPass))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                var passResult = await _userManager.ResetPasswordAsync(usuario, token, nuevaPass);
                if (!passResult.Succeeded)
                {
                    foreach (var err in passResult.Errors)
                        ModelState.AddModelError(string.Empty, err.Description);
                    ViewBag.UserId = id;
                    ViewBag.Carreras = Carreras.Todas;
                    return View(vm);
                }
            }

            return RedirectToAction(nameof(Usuarios));
        }

        public async Task<IActionResult> EliminarUsuario(string id)
        {
            if (id == null) return NotFound();

            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(usuario);
            ViewBag.Rol = roles.FirstOrDefault() ?? "Sin rol";

            return View(usuario);
        }

        [HttpPost, ActionName("EliminarUsuario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUsuarioConfirmado(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario != null)
                await _userManager.DeleteAsync(usuario);

            return RedirectToAction(nameof(Usuarios));
        }
    }
}
