using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Unilife.Models;
using Unilife.Models.Api;
using Unilife.Services;

namespace Unilife.Controllers.Api
{
    /// <summary>Recomendaciones personalizadas de eventos y lugares.</summary>
    [ApiController]
    [Route("api/recomendaciones")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Produces("application/json")]
    public class RecomendacionesApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RecomendadorEventosService   _recEventos;
        private readonly RecomendadorLugaresService   _recLugares;

        public RecomendacionesApiController(
            UserManager<ApplicationUser> userManager,
            RecomendadorEventosService   recEventos,
            RecomendadorLugaresService   recLugares)
        {
            _userManager = userManager;
            _recEventos  = recEventos;
            _recLugares  = recLugares;
        }

        /// <summary>Devuelve eventos recomendados para el usuario autenticado.</summary>
        /// <param name="topN">Cantidad máxima de resultados (1-20, default 6).</param>
        [HttpGet("eventos")]
        [ProducesResponseType(typeof(IEnumerable<EventoRecomendadoDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetEventosRecomendados([FromQuery] int topN = 6)
        {
            topN = Math.Clamp(topN, 1, 20);

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null)
                return Problem("Usuario no encontrado.", statusCode: 401);

            var eventos = await _recEventos.ObtenerEventosRecomendadosAsync(usuario.Carrera, topN);

            var dtos = eventos.Select(e => new EventoRecomendadoDto(
                e.Id, e.Titulo, e.Descripcion,
                e.Fecha.ToString("yyyy-MM-dd"),
                e.Hora.ToString(@"hh\:mm"),
                e.Lugar, e.TipoEvento, e.EsGeneral, e.Carrera
            ));

            return Ok(dtos);
        }

        /// <summary>Devuelve lugares recomendados para el usuario autenticado.</summary>
        /// <param name="topN">Cantidad máxima de resultados (1-20, default 6).</param>
        [HttpGet("lugares")]
        [ProducesResponseType(typeof(IEnumerable<LugarRecomendadoDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetLugaresRecomendados([FromQuery] int topN = 6)
        {
            topN = Math.Clamp(topN, 1, 20);

            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Problem("Usuario no encontrado.", statusCode: 401);

            var lugares = await _recLugares.ObtenerRecomendacionesAsync(userId, topN);

            var dtos = lugares.Select(l => new LugarRecomendadoDto(
                l.Id, l.Nombre, l.Tipo, l.Direccion,
                l.Distancia, l.PrecioPromedio, l.Calificacion, l.Descripcion
            ));

            return Ok(dtos);
        }
    }
}
