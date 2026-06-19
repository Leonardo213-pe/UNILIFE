using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilife.Data;
using Unilife.Models;
using Microsoft.AspNetCore.Hosting;

namespace Unilife.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment env)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _env = env;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true
            );

            if (resultado.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && await _userManager.IsInRoleAsync(user, "Coordinador"))
                    return RedirectToAction("Index", "Coordinador");

                return RedirectToAction("Index", "Home");
            }

            if (resultado.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Cuenta bloqueada por demasiados intentos. Intenta en 10 minutos.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(EditarPerfilViewModel vm)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Perfil));

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return RedirectToAction("Login");

            usuario.Nombre   = vm.Nombre;
            usuario.Apellido = vm.Apellido;
            usuario.Email    = vm.Email;
            usuario.UserName = vm.Email;

            await _userManager.UpdateAsync(usuario);

            if (!string.IsNullOrWhiteSpace(vm.NuevaContrasena))
            {
                if (string.IsNullOrWhiteSpace(vm.ContrasenaActual) ||
                    !await _userManager.CheckPasswordAsync(usuario, vm.ContrasenaActual))
                {
                    TempData["ErrorPerfil"] = "La contraseña actual es incorrecta.";
                    return RedirectToAction(nameof(Perfil));
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                await _userManager.ResetPasswordAsync(usuario, token, vm.NuevaContrasena);
            }

            TempData["Toast"] = "Perfil actualizado correctamente.";
            return RedirectToAction(nameof(Perfil));
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return RedirectToAction("Login");

            var roles    = await _userManager.GetRolesAsync(usuario);
            var rol      = roles.FirstOrDefault() ?? "Sin rol";
            bool esDoc   = rol == "Docente";
            bool esCoor  = rol == "Coordinador";

            var modelo = new PerfilViewModel
            {
                Nombre        = usuario.Nombre,
                Apellido      = usuario.Apellido,
                Email         = usuario.Email ?? "",
                Carrera       = usuario.Carrera,
                Rol           = rol,
                EsDocente     = esDoc,
                EsCoordinador = esCoor,
                FotoPerfil    = usuario.FotoPerfil,
            };

            if (esDoc)
            {
                modelo.MisCursos = await _context.Cursos
                    .Include(c => c.Participantes)
                    .Include(c => c.Horarios)
                    .Where(c => c.DocenteId == usuario.Id)
                    .OrderBy(c => c.Carrera).ThenBy(c => c.Semestre)
                    .ToListAsync();
            }
            else if (esCoor)
            {
                modelo.TotalDocentes  = (await _userManager.GetUsersInRoleAsync("Docente")).Count;
                modelo.TotalAlumnos   = (await _userManager.GetUsersInRoleAsync("Alumno")).Count;
                modelo.TotalUsuarios  = await _userManager.Users.CountAsync();
                modelo.TotalCursos    = await _context.Cursos.CountAsync();
                modelo.TotalCarreras  = await _context.CarrerasRegistradas.CountAsync();
                modelo.TotalEventos   = await _context.Eventos.CountAsync();
            }
            else
            {
                // Alumno: cursos inscritos y tareas próximas
                modelo.CursosInscritos = await _context.Cursos
                    .Include(c => c.DocenteUser)
                    .Where(c => c.Participantes.Any(p => p.AlumnoId == usuario.Id))
                    .OrderBy(c => c.Semestre)
                    .ToListAsync();

                modelo.TareasProximas = await _context.Tareas
                    .Where(t => t.UsuarioId == usuario.Id && t.Estado != "Completada")
                    .OrderBy(t => t.FechaEntrega)
                    .Take(5)
                    .ToListAsync();
            }

            return View(modelo);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubirFoto(IFormFile foto)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return RedirectToAction("Login");

            if (foto != null && foto.Length > 0)
            {
                var extPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var ext = Path.GetExtension(foto.FileName).ToLowerInvariant();
                if (!extPermitidas.Contains(ext))
                {
                    TempData["ErrorPerfil"] = "Solo se permiten imágenes JPG, PNG o WEBP.";
                    return RedirectToAction(nameof(Perfil));
                }
                if (foto.Length > 5 * 1024 * 1024)
                {
                    TempData["ErrorPerfil"] = "La imagen no puede superar 5 MB.";
                    return RedirectToAction(nameof(Perfil));
                }

                var dir = Path.Combine(_env.WebRootPath, "fotos-perfil");
                Directory.CreateDirectory(dir);

                if (!string.IsNullOrEmpty(usuario.FotoPerfil))
                {
                    var anterior = Path.Combine(_env.WebRootPath, usuario.FotoPerfil.TrimStart('/'));
                    if (System.IO.File.Exists(anterior))
                        System.IO.File.Delete(anterior);
                }

                var fileName = $"{usuario.Id}{ext}";
                using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
                await foto.CopyToAsync(stream);

                usuario.FotoPerfil = $"/fotos-perfil/{fileName}";
                await _userManager.UpdateAsync(usuario);
                TempData["Toast"] = "Foto de perfil actualizada.";
            }

            return RedirectToAction(nameof(Perfil));
        }
    }
}