using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Unilife.Data;
using Unilife.Models;

namespace Unilife.Controllers
{
    [Authorize]
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public CursosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        public async Task<IActionResult> Index(string? carrera, string? semestre)
        {
            bool esDocente = User.IsInRole("Docente");
            var userId = _userManager.GetUserId(User);

            // Alumno: muestra directamente sus cursos inscritos, sin navegación
            if (!esDocente && !User.IsInRole("Coordinador"))
            {
                var cursosAlumno = await _context.Cursos
                    .Include(c => c.DocenteUser)
                    .Include(c => c.Horarios)
                    .Include(c => c.Participantes)
                    .Where(c => c.Participantes.Any(p => p.AlumnoId == userId))
                    .OrderBy(c => c.Carrera).ThenBy(c => c.Semestre)
                    .ToListAsync();

                ViewBag.Modo = "alumno";
                return View(cursosAlumno);
            }

            if (User.IsInRole("Coordinador"))
                await CargarViewBagAsync();
            else
                ViewBag.CarrerasDb = await _context.CarrerasRegistradas
                    .OrderBy(c => c.Grupo).ThenBy(c => c.Nombre).ToListAsync();

            // Nivel 1 — elegir carrera
            if (string.IsNullOrEmpty(carrera))
            {
                if (esDocente)
                {
                    // Docente: solo las carreras donde enseña
                    var datosDocente = await _context.Cursos
                        .Where(c => c.DocenteId == userId)
                        .Select(c => new { c.Carrera })
                        .ToListAsync();

                    ViewBag.Modo = "carreras";
                    ViewBag.ModoDocente = true;
                    ViewBag.CursosPorCarrera = datosDocente
                        .GroupBy(c => c.Carrera)
                        .ToDictionary(g => g.Key, g => g.Count());
                    return View(new List<Curso>());
                }

                var datos = await _context.Cursos
                    .Select(c => new { c.Carrera })
                    .ToListAsync();

                ViewBag.Modo = "carreras";
                ViewBag.CursosPorCarrera = datos
                    .GroupBy(c => c.Carrera)
                    .ToDictionary(g => g.Key, g => g.Count());
                return View(new List<Curso>());
            }

            // Docente: saltar semestres, mostrar todos sus cursos en esa carrera
            if (esDocente)
            {
                var cursosDocente = await _context.Cursos
                    .Include(c => c.Horarios)
                    .Include(c => c.Participantes)
                    .Where(c => c.Carrera == carrera && c.DocenteId == userId)
                    .OrderBy(c => c.Semestre)
                    .ToListAsync();

                ViewBag.Modo = "cursos";
                ViewBag.ModoDocente = true;
                ViewBag.CarreraSeleccionada = carrera;
                ViewBag.SemestreSeleccionado = "";
                return View(cursosDocente);
            }

            // Nivel 2 — elegir semestre (Coordinador / Alumno)
            if (string.IsNullOrEmpty(semestre))
            {
                var datos = await _context.Cursos
                    .Where(c => c.Carrera == carrera)
                    .Select(c => new { c.Semestre })
                    .ToListAsync();

                ViewBag.Modo = "semestres";
                ViewBag.CarreraSeleccionada = carrera;
                ViewBag.CursosPorSemestre = datos
                    .GroupBy(c => c.Semestre)
                    .ToDictionary(g => g.Key, g => g.Count());
                return View(new List<Curso>());
            }

            // Nivel 3 — ver cursos
            var cursos = await _context.Cursos
                .Include(c => c.DocenteUser)
                .Include(c => c.Horarios)
                .Include(c => c.Participantes)
                .Where(c => c.Carrera == carrera && c.Semestre == semestre)
                .ToListAsync();

            ViewBag.Modo = "cursos";
            ViewBag.CarreraSeleccionada = carrera;
            ViewBag.SemestreSeleccionado = semestre;
            ViewBag.MostrarModal = TempData["MostrarModal"] != null;
            return View(cursos);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos
                .Include(c => c.DocenteUser)
                .Include(c => c.Horarios)
                .Include(c => c.Participantes)
                    .ThenInclude(p => p.Alumno)
                .Include(c => c.Modulos.OrderBy(m => m.Orden))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null) return NotFound();

            return View(curso);
        }

        public async Task<IActionResult> DetalleModulo(int? id)
        {
            if (id == null) return NotFound();

            var modulo = await _context.Modulos
                .Include(m => m.Curso)
                .Include(m => m.Actividades.OrderBy(a => a.Orden))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (modulo == null) return NotFound();

            return View("Modulo", modulo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coordinador,Docente")]
        public async Task<IActionResult> CrearActividad(int moduloId, string tipo, string titulo,
            string? descripcion, string? url, DateTime? fechaEntrega, IFormFile? archivo)
        {
            if (!string.IsNullOrWhiteSpace(titulo))
            {
                string? archivoUrl = url;

                if (archivo != null && archivo.Length > 0)
                {
                    var extPermitidas = new[] { ".pdf", ".docx", ".doc", ".pptx", ".ppt", ".xlsx", ".xls", ".txt", ".zip", ".png", ".jpg", ".jpeg" };
                    var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
                    if (!extPermitidas.Contains(ext))
                    {
                        TempData["ErrorActividad"] = $"Tipo de archivo no permitido: {ext}";
                        return RedirectToAction(nameof(DetalleModulo), new { id = moduloId });
                    }
                    if (archivo.Length > 20 * 1024 * 1024) // 20 MB
                    {
                        TempData["ErrorActividad"] = "El archivo no puede superar 20 MB.";
                        return RedirectToAction(nameof(DetalleModulo), new { id = moduloId });
                    }

                    var dir = Path.Combine(_env.ContentRootPath, "PrivateUploads", moduloId.ToString());
                    Directory.CreateDirectory(dir);
                    var fileName = Guid.NewGuid() + ext;
                    using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
                    await archivo.CopyToAsync(stream);
                    archivoUrl = $"/Cursos/Descargar?path={Uri.EscapeDataString($"{moduloId}/{fileName}")}";
                }

                var orden = await _context.Actividades.CountAsync(a => a.ModuloId == moduloId);
                _context.Actividades.Add(new Actividad
                {
                    ModuloId = moduloId,
                    Tipo = tipo,
                    Titulo = titulo.Trim(),
                    Descripcion = descripcion ?? "",
                    Url = archivoUrl,
                    FechaEntrega = fechaEntrega,
                    Orden = orden
                });
                await _context.SaveChangesAsync();
                TempData["Toast"] = "Actividad agregada correctamente.";
            }
            return RedirectToAction(nameof(DetalleModulo), new { id = moduloId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coordinador,Docente")]
        public async Task<IActionResult> EliminarActividad(int id, int moduloId)
        {
            var act = await _context.Actividades.FindAsync(id);
            if (act != null)
            {
                _context.Actividades.Remove(act);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(DetalleModulo), new { id = moduloId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coordinador,Docente")]
        public async Task<IActionResult> CrearModulo(int cursoId, string nombre, string color)
        {
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var orden = await _context.Modulos.CountAsync(m => m.CursoId == cursoId);
                _context.Modulos.Add(new Modulo
                {
                    CursoId = cursoId,
                    Nombre = nombre.Trim().ToUpper(),
                    Color = string.IsNullOrEmpty(color) ? "teal" : color,
                    Orden = orden
                });
                await _context.SaveChangesAsync();
                TempData["Toast"] = "Sección creada correctamente.";
            }
            return RedirectToAction(nameof(Details), new { id = cursoId });
        }

        // POST: crea el curso desde el modal en Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> Create(CursoFormViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var curso = new Curso
                {
                    Nombre = vm.Nombre,
                    Descripcion = vm.Descripcion,
                    Carrera = vm.Carrera,
                    Semestre = vm.Semestre,
                    DocenteId = string.IsNullOrEmpty(vm.DocenteId) ? null : vm.DocenteId
                };

                _context.Cursos.Add(curso);
                await _context.SaveChangesAsync();

                foreach (var alumnoId in vm.AlumnoIds)
                    _context.CursoAlumnos.Add(new CursoAlumno { CursoId = curso.Id, AlumnoId = alumnoId });

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { carrera = vm.Carrera, semestre = vm.Semestre });
            }

            TempData["MostrarModal"] = true;
            await CargarViewBagAsync();
            var cursos = await _context.Cursos
                .Include(c => c.DocenteUser).Include(c => c.Horarios).Include(c => c.Participantes)
                .Where(c => c.Carrera == vm.Carrera && c.Semestre == vm.Semestre)
                .ToListAsync();
            ViewBag.Modo = "cursos";
            ViewBag.CarreraSeleccionada = vm.Carrera;
            ViewBag.SemestreSeleccionado = vm.Semestre;
            ViewBag.MostrarModal = true;
            return View("Index", cursos);
        }

        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos
                .Include(c => c.Participantes)
                .Include(c => c.Horarios)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null) return NotFound();

            var vm = new CursoFormViewModel
            {
                Id = curso.Id,
                Nombre = curso.Nombre,
                Descripcion = curso.Descripcion,
                Carrera = curso.Carrera,
                Semestre = curso.Semestre,
                DocenteId = curso.DocenteId,
                AlumnoIds = curso.Participantes.Select(p => p.AlumnoId).ToList()
            };

            await CargarViewBagAsync();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> Edit(int id, CursoFormViewModel vm)
        {
            if (id != vm.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var curso = await _context.Cursos
                    .Include(c => c.Participantes)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (curso == null) return NotFound();

                curso.Nombre = vm.Nombre;
                curso.Descripcion = vm.Descripcion;
                curso.Carrera = vm.Carrera;
                curso.Semestre = vm.Semestre;
                curso.DocenteId = string.IsNullOrEmpty(vm.DocenteId) ? null : vm.DocenteId;

                _context.CursoAlumnos.RemoveRange(curso.Participantes);
                foreach (var alumnoId in vm.AlumnoIds)
                    _context.CursoAlumnos.Add(new CursoAlumno { CursoId = curso.Id, AlumnoId = alumnoId });

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = curso.Id });
            }

            await CargarViewBagAsync();
            return View(vm);
        }

        // ─── Horarios ─────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> AgregarHorario(HorarioCurso horario)
        {
            if (ModelState.IsValid)
            {
                _context.HorariosCurso.Add(horario);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id = horario.CursoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> EliminarHorario(int id, int cursoId)
        {
            var horario = await _context.HorariosCurso.FindAsync(id);
            if (horario != null)
            {
                _context.HorariosCurso.Remove(horario);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id = cursoId });
        }

        // ─── Delete ───────────────────────────────────────────────────────────────

        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos
                .Include(c => c.DocenteUser)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null) return NotFound();

            return View(curso);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Participantes)
                .Include(c => c.Horarios)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso != null)
            {
                _context.CursoAlumnos.RemoveRange(curso.Participantes);
                _context.HorariosCurso.RemoveRange(curso.Horarios);
                _context.Cursos.Remove(curso);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Descargar(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return NotFound();

            // Solo permite nombres de archivo seguros (sin ../)
            var safePath = path.Replace("..", "").TrimStart('/');
            var fullPath = Path.Combine(_env.ContentRootPath, "PrivateUploads", safePath);

            if (!System.IO.File.Exists(fullPath)) return NotFound();

            var ext = Path.GetExtension(fullPath).ToLowerInvariant();
            var mime = ext switch
            {
                ".pdf"  => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc"  => "application/msword",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".ppt"  => "application/vnd.ms-powerpoint",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".xls"  => "application/vnd.ms-excel",
                ".zip"  => "application/zip",
                ".png"  => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                _       => "application/octet-stream"
            };

            return PhysicalFile(fullPath, mime, Path.GetFileName(fullPath));
        }

        private async Task CargarViewBagAsync()
        {
            ViewBag.CarrerasDb = await _context.CarrerasRegistradas
                .OrderBy(c => c.Grupo).ThenBy(c => c.Nombre).ToListAsync();
            ViewBag.Docentes = await _userManager.GetUsersInRoleAsync("Docente");
            ViewBag.Alumnos = await _userManager.GetUsersInRoleAsync("Alumno");
        }

        private bool CursoExists(int id) =>
            _context.Cursos.Any(e => e.Id == id);
    }
}
