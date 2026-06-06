using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Unilife.Data;
using Unilife.Models;
using Unilife.Models.Api;

namespace Unilife.Controllers.Api
{
    [ApiController]
    [Route("api/cursos")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CursosApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CursosApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context     = context;
            _userManager = userManager;
        }

        /// <summary>Lista cursos. Alumno ve solo sus inscritos; Docente los suyos; Coordinador todos.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? carrera, [FromQuery] string? semestre)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rol    = User.FindFirstValue(ClaimTypes.Role);

            IQueryable<Curso> query = _context.Cursos
                .Include(c => c.DocenteUser)
                .Include(c => c.Participantes);

            if (rol == "Alumno")
                query = query.Where(c => c.Participantes.Any(p => p.AlumnoId == userId));
            else if (rol == "Docente")
                query = query.Where(c => c.DocenteId == userId);

            if (!string.IsNullOrEmpty(carrera))
                query = query.Where(c => c.Carrera == carrera);
            if (!string.IsNullOrEmpty(semestre))
                query = query.Where(c => c.Semestre == semestre);

            var cursos = await query.OrderBy(c => c.Carrera).ThenBy(c => c.Semestre).ToListAsync();

            return Ok(cursos.Select(c => new CursoDto(
                Id:            c.Id,
                Nombre:        c.Nombre,
                Descripcion:   c.Descripcion,
                Carrera:       c.Carrera,
                Semestre:      c.Semestre,
                DocenteNombre: c.DocenteUser != null ? $"{c.DocenteUser.Nombre} {c.DocenteUser.Apellido}" : null,
                TotalAlumnos:  c.Participantes.Count)));
        }

        /// <summary>Detalle de un curso incluyendo módulos.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.DocenteUser)
                .Include(c => c.Participantes)
                .Include(c => c.Modulos.OrderBy(m => m.Orden))
                    .ThenInclude(m => m.Actividades)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null) return NotFound(new { mensaje = "Curso no encontrado." });

            return Ok(new
            {
                curso.Id,
                curso.Nombre,
                curso.Descripcion,
                curso.Carrera,
                curso.Semestre,
                Docente = curso.DocenteUser != null
                    ? $"{curso.DocenteUser.Nombre} {curso.DocenteUser.Apellido}"
                    : null,
                TotalAlumnos = curso.Participantes.Count,
                Modulos = curso.Modulos.Select(m => new ModuloDto(
                    Id:               m.Id,
                    Nombre:           m.Nombre,
                    Color:            m.Color,
                    Orden:            m.Orden,
                    TotalActividades: m.Actividades.Count))
            });
        }

        /// <summary>Crea un nuevo curso. Solo Coordinador.</summary>
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Coordinador")]
        public async Task<IActionResult> Create([FromBody] CursoRequest req)
        {
            var curso = new Curso
            {
                Nombre      = req.Nombre,
                Descripcion = req.Descripcion,
                Carrera     = req.Carrera,
                Semestre    = req.Semestre,
                DocenteId   = string.IsNullOrEmpty(req.DocenteId) ? null : req.DocenteId
            };

            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = curso.Id },
                new { curso.Id, curso.Nombre, curso.Carrera, curso.Semestre });
        }

        /// <summary>Actualiza un curso. Solo Coordinador.</summary>
        [HttpPut("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Coordinador")]
        public async Task<IActionResult> Update(int id, [FromBody] CursoRequest req)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound(new { mensaje = "Curso no encontrado." });

            curso.Nombre      = req.Nombre;
            curso.Descripcion = req.Descripcion;
            curso.Carrera     = req.Carrera;
            curso.Semestre    = req.Semestre;
            curso.DocenteId   = string.IsNullOrEmpty(req.DocenteId) ? null : req.DocenteId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Elimina un curso. Solo Coordinador.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Coordinador")]
        public async Task<IActionResult> Delete(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Participantes)
                .Include(c => c.Horarios)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null) return NotFound(new { mensaje = "Curso no encontrado." });

            _context.CursoAlumnos.RemoveRange(curso.Participantes);
            _context.HorariosCurso.RemoveRange(curso.Horarios);
            _context.Cursos.Remove(curso);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>Lista los módulos de un curso con sus actividades.</summary>
        [HttpGet("{id:int}/modulos")]
        public async Task<IActionResult> GetModulos(int id)
        {
            var modulos = await _context.Modulos
                .Include(m => m.Actividades.OrderBy(a => a.Orden))
                .Where(m => m.CursoId == id)
                .OrderBy(m => m.Orden)
                .ToListAsync();

            return Ok(modulos.Select(m => new
            {
                m.Id,
                m.Nombre,
                m.Color,
                m.Orden,
                Actividades = m.Actividades.Select(a => new ActividadDto(
                    Id:           a.Id,
                    Tipo:         a.Tipo,
                    Titulo:       a.Titulo,
                    Descripcion:  a.Descripcion,
                    Url:          a.Url,
                    FechaEntrega: a.FechaEntrega,
                    Orden:        a.Orden))
            }));
        }
    }
}
