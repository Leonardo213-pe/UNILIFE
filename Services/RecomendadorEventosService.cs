using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Unilife.Data;
using Unilife.Models;

namespace Unilife.Services
{
    // Recomendador híbrido: afinidad por carrera+tipo + proximidad de fecha
    // El resultado se cachea 30 min por (carrera) para no recalcular en cada request
    public class RecomendadorEventosService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMemoryCache _cache;

        public RecomendadorEventosService(IServiceScopeFactory scopeFactory, IMemoryCache cache)
        {
            _scopeFactory = scopeFactory;
            _cache        = cache;
        }

        public async Task<List<Evento>> ObtenerEventosRecomendadosAsync(string? carrera, int topN = 6)
        {
            var cacheKey = $"eventos_rec_{carrera ?? "general"}";

            if (_cache.TryGetValue(cacheKey, out List<Evento>? cached) && cached != null)
                return cached;

            var result = await CalcularAsync(carrera, topN);

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
            return result;
        }

        private async Task<List<Evento>> CalcularAsync(string? carrera, int topN)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var hoy = DateTime.Today;

            var eventos = await db.Eventos
                .Where(e => e.Fecha >= hoy)
                .ToListAsync();

            if (!eventos.Any())
                return new List<Evento>();

            // Tipos preferidos para la carrera (heurística por dominio académico)
            var tiposPreferidos = TiposParaCarrera(carrera);

            var maxDias = 60.0;

            var scored = eventos.Select(e =>
            {
                double score = 0;

                // 1) Afinidad de carrera: evento de su carrera o general
                if (!string.IsNullOrEmpty(carrera) && e.Carrera == carrera)
                    score += 3.0;
                else if (e.EsGeneral)
                    score += 1.0;

                // 2) Afinidad de tipo según carrera
                if (tiposPreferidos.Contains(e.TipoEvento))
                    score += 2.0;

                // 3) Proximidad de fecha (más cerca = más score)
                var dias = (e.Fecha - hoy).TotalDays;
                var proximidad = Math.Max(0, 1.0 - dias / maxDias);
                score += proximidad * 2.0;

                return new { Evento = e, Score = score };
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Evento.Fecha)
            .Take(topN)
            .Select(x => x.Evento)
            .ToList();

            // Garantiza que nunca devuelve lista vacía
            if (!scored.Any())
                scored = eventos.OrderBy(e => e.Fecha).Take(topN).ToList();

            return scored;
        }

        private static HashSet<string> TiposParaCarrera(string? carrera) => carrera switch
        {
            "Ingeniería en Sistemas"     => ["Taller", "Hackathon", "Charla", "Congreso"],
            "Ingeniería Civil"           => ["Congreso", "Charla", "Feria"],
            "Ingeniería Mecatrónica"     => ["Feria", "Taller", "Congreso"],
            "Ingeniería Industrial"      => ["Congreso", "Feria", "Charla"],
            "Derecho"                    => ["Charla", "Práctica", "Congreso"],
            "Psicología"                 => ["Congreso", "Charla", "Jornada"],
            "Administración de Empresas" => ["Feria", "Charla", "Congreso"],
            "Comunicación Social"        => ["Cultural", "Feria", "Taller"],
            "Medicina"                   => ["Congreso", "Jornada", "Charla"],
            "Enfermería"                 => ["Jornada", "Congreso", "Charla"],
            "Odontología"                => ["Jornada", "Congreso", "Charla"],
            "Nutrición"                  => ["Feria", "Jornada", "Charla"],
            _                            => ["Charla", "Taller", "Feria"]
        };
    }
}
