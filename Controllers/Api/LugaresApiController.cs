using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unilife.Data;
using Unilife.Models;
using Unilife.Models.Api;

namespace Unilife.Controllers.Api
{
    /// <summary>Lugares: listado, detalle y valoraciones.</summary>
    [ApiController]
    [Route("api/lugares")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Produces("application/json")]
    public class LugaresApiController : ControllerBase
    {
        private readonly ApplicationDbContext        _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public LugaresApiController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db          = db;
            _userManager = userManager;
        }

        /// <summary>Lista todos los lugares con promedio de valoraciones y la valoración del usuario.</summary>
        /// <param name="page">Página (base 1).</param>
        /// <param name="pageSize">Resultados por página (1-50).</param>
        /// <param name="tipo">Filtro opcional por tipo (Cafetería, Biblioteca, etc.).</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<LugarConPromedioDto>), 200)]
        public async Task<IActionResult> GetLugares(
            [FromQuery] int    page     = 1,
            [FromQuery] int    pageSize = 10,
            [FromQuery] string? tipo    = null)
        {
            page     = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var userId = _userManager.GetUserId(User);

            var query = _db.Lugares.AsQueryable();
            if (!string.IsNullOrEmpty(tipo))
                query = query.Where(l => l.Tipo == tipo);

            var total = await query.CountAsync();

            var lugares = await query
                .OrderByDescending(l => l.Calificacion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var ids = lugares.Select(l => l.Id).ToList();

            var promedios = await _db.ValoracionesLugar
                .Where(v => ids.Contains(v.LugarId))
                .GroupBy(v => v.LugarId)
                .Select(g => new { LugarId = g.Key, Avg = g.Average(v => (double)v.Puntaje), Total = g.Count() })
                .ToDictionaryAsync(x => x.LugarId);

            Dictionary<int, int> misValoraciones = new();
            if (userId != null)
            {
                misValoraciones = await _db.ValoracionesLugar
                    .Where(v => v.UsuarioId == userId && ids.Contains(v.LugarId))
                    .ToDictionaryAsync(v => v.LugarId, v => v.Puntaje);
            }

            var dtos = lugares.Select(l => new LugarConPromedioDto(
                l.Id, l.Nombre, l.Tipo, l.Direccion,
                l.Distancia, l.PrecioPromedio, l.Calificacion, l.Descripcion,
                promedios.TryGetValue(l.Id, out var p) ? p.Avg   : null,
                promedios.TryGetValue(l.Id, out var p2) ? p2.Total : 0,
                misValoraciones.TryGetValue(l.Id, out var mv) ? mv : null
            ));

            return Ok(new PagedResult<LugarConPromedioDto>(dtos, total, page, pageSize));
        }

        /// <summary>Detalle de un lugar.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(LugarConPromedioDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetLugar(int id)
        {
            var lugar = await _db.Lugares.FindAsync(id);
            if (lugar == null) return NotFound(new { message = "Lugar no encontrado." });

            var userId = _userManager.GetUserId(User);

            var prom = await _db.ValoracionesLugar
                .Where(v => v.LugarId == id)
                .GroupBy(v => v.LugarId)
                .Select(g => new { Avg = g.Average(v => (double)v.Puntaje), Total = g.Count() })
                .FirstOrDefaultAsync();

            int? miPuntaje = null;
            if (userId != null)
                miPuntaje = (await _db.ValoracionesLugar
                    .FirstOrDefaultAsync(v => v.UsuarioId == userId && v.LugarId == id))?.Puntaje;

            return Ok(new LugarConPromedioDto(
                lugar.Id, lugar.Nombre, lugar.Tipo, lugar.Direccion,
                lugar.Distancia, lugar.PrecioPromedio, lugar.Calificacion, lugar.Descripcion,
                prom?.Avg, prom?.Total ?? 0, miPuntaje
            ));
        }

        /// <summary>Crea o actualiza la valoración (1-5) del usuario autenticado para un lugar.</summary>
        [HttpPost("{id:int}/valorar")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Valorar(int id, [FromBody] ValoracionLugarDto dto)
        {
            if (dto.Puntaje < 1 || dto.Puntaje > 5)
                return ValidationProblem("El puntaje debe estar entre 1 y 5.");

            var lugar = await _db.Lugares.FindAsync(id);
            if (lugar == null) return NotFound(new { message = "Lugar no encontrado." });

            var userId = _userManager.GetUserId(User)!;

            var existing = await _db.ValoracionesLugar
                .FirstOrDefaultAsync(v => v.UsuarioId == userId && v.LugarId == id);

            if (existing != null)
            {
                existing.Puntaje        = dto.Puntaje;
                existing.FechaValoracion = DateTime.Now;
            }
            else
            {
                _db.ValoracionesLugar.Add(new ValoracionLugar
                {
                    UsuarioId       = userId,
                    LugarId         = id,
                    Puntaje         = dto.Puntaje,
                    FechaValoracion = DateTime.Now
                });
            }

            await _db.SaveChangesAsync();

            var promedio = await _db.ValoracionesLugar
                .Where(v => v.LugarId == id)
                .AverageAsync(v => (double)v.Puntaje);

            return Ok(new { message = "Valoración guardada.", promedioActual = Math.Round(promedio, 2) });
        }
    }
}
