using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Unilife.Data;
using Unilife.Models;
using Unilife.Models.Api;

namespace Unilife.Controllers.Api
{
    [ApiController]
    [Route("api/eventos")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EventosApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventosApiController(ApplicationDbContext context) => _context = context;

        /// <summary>Lista eventos. Alumno ve generales + los de su carrera; Docente/Coordinador todos.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool soloProximos = false)
        {
            var rol     = User.FindFirstValue(ClaimTypes.Role);
            var carrera = User.FindFirstValue("carrera");
            var hoy     = DateTime.Today;

            IQueryable<Evento> query = _context.Eventos;

            if (rol == "Alumno")
                query = query.Where(e => e.EsGeneral || e.Carrera == carrera);

            if (soloProximos)
                query = query.Where(e => e.Fecha >= hoy);

            var eventos = await query.OrderBy(e => e.Fecha).ThenBy(e => e.Hora).ToListAsync();

            return Ok(eventos.Select(e => new EventoDto(
                Id:          e.Id,
                Titulo:      e.Titulo,
                Descripcion: e.Descripcion,
                Fecha:       e.Fecha,
                Hora:        e.Hora.ToString(@"hh\:mm"),
                Lugar:       e.Lugar,
                TipoEvento:  e.TipoEvento,
                EsGeneral:   e.EsGeneral,
                Carrera:     e.Carrera)));
        }

        /// <summary>Detalle de un evento.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var e = await _context.Eventos.FindAsync(id);
            if (e == null) return NotFound(new { mensaje = "Evento no encontrado." });

            return Ok(new EventoDto(e.Id, e.Titulo, e.Descripcion, e.Fecha,
                e.Hora.ToString(@"hh\:mm"), e.Lugar, e.TipoEvento, e.EsGeneral, e.Carrera));
        }

        /// <summary>Crea un evento. Solo Coordinador.</summary>
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Coordinador")]
        public async Task<IActionResult> Create([FromBody] EventoRequest req)
        {
            var evento = new Evento
            {
                Titulo      = req.Titulo,
                Descripcion = req.Descripcion,
                Fecha       = req.Fecha,
                Hora        = req.Hora,
                Lugar       = req.Lugar,
                TipoEvento  = req.TipoEvento,
                EsGeneral   = req.EsGeneral,
                Carrera     = req.Carrera
            };

            _context.Eventos.Add(evento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = evento.Id },
                new { evento.Id, evento.Titulo, evento.Fecha });
        }

        /// <summary>Actualiza un evento. Solo Coordinador.</summary>
        [HttpPut("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Coordinador")]
        public async Task<IActionResult> Update(int id, [FromBody] EventoRequest req)
        {
            var evento = await _context.Eventos.FindAsync(id);
            if (evento == null) return NotFound(new { mensaje = "Evento no encontrado." });

            evento.Titulo      = req.Titulo;
            evento.Descripcion = req.Descripcion;
            evento.Fecha       = req.Fecha;
            evento.Hora        = req.Hora;
            evento.Lugar       = req.Lugar;
            evento.TipoEvento  = req.TipoEvento;
            evento.EsGeneral   = req.EsGeneral;
            evento.Carrera     = req.Carrera;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Elimina un evento. Solo Coordinador.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Coordinador")]
        public async Task<IActionResult> Delete(int id)
        {
            var evento = await _context.Eventos.FindAsync(id);
            if (evento == null) return NotFound(new { mensaje = "Evento no encontrado." });

            _context.Eventos.Remove(evento);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
