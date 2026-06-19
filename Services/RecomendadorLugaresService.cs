using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using Unilife.Data;
using Unilife.Models;

namespace Unilife.Services
{
    public class RecomendadorLugaresService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly MLContext _ml = new(seed: 42);
        private readonly SemaphoreSlim _lock = new(1, 1);

        private ITransformer? _model;
        private HashSet<string> _usuariosEntrenados = [];
        private HashSet<int>    _lugaresEntrenados  = [];
        private DateTime _lastTrained = DateTime.MinValue;
        private static readonly TimeSpan _ttl = TimeSpan.FromHours(1);

        public RecomendadorLugaresService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<List<Lugar>> ObtenerRecomendacionesAsync(string userId, int topN = 6)
        {
            await EnsureModelAsync();

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var todos = await db.Lugares.ToListAsync();

            var misIds = await db.ValoracionesLugar
                .Where(v => v.UsuarioId == userId)
                .Select(v => v.LugarId)
                .ToListAsync();

            // Cold-start: sin modelo o usuario sin historial
            if (_model == null || !_usuariosEntrenados.Contains(userId) || misIds.Count == 0)
                return await FallbackAsync(db, misIds, todos, topN);

            try
            {
                var predEngine = _ml.Model
                    .CreatePredictionEngine<RatingData, RatingPrediction>(_model);

                var candidatos = todos
                    .Where(l => !misIds.Contains(l.Id) && _lugaresEntrenados.Contains(l.Id))
                    .Select(l => new
                    {
                        Lugar = l,
                        Score = predEngine.Predict(new RatingData
                        {
                            UserId = userId,
                            ItemId = l.Id.ToString()
                        }).Score
                    })
                    .OrderByDescending(x => x.Score)
                    .Take(topN)
                    .Select(x => x.Lugar)
                    .ToList();

                if (candidatos.Count >= topN / 2) return candidatos;
            }
            catch { /* degradar a fallback */ }

            return await FallbackAsync(db, misIds, todos, topN);
        }

        private async Task<List<Lugar>> FallbackAsync(
            ApplicationDbContext db, List<int> excluir, List<Lugar> todos, int topN)
        {
            var promedios = await db.ValoracionesLugar
                .GroupBy(v => v.LugarId)
                .Select(g => new { LugarId = g.Key, Avg = g.Average(v => (double)v.Puntaje) })
                .ToDictionaryAsync(x => x.LugarId, x => x.Avg);

            return todos
                .Where(l => !excluir.Contains(l.Id))
                .OrderByDescending(l => promedios.TryGetValue(l.Id, out var a) ? a : l.Calificacion)
                .Take(topN)
                .ToList();
        }

        private async Task EnsureModelAsync()
        {
            if (_model != null && DateTime.Now - _lastTrained < _ttl) return;

            await _lock.WaitAsync();
            try
            {
                if (_model != null && DateTime.Now - _lastTrained < _ttl) return;
                await EntrenarAsync();
            }
            finally { _lock.Release(); }
        }

        private async Task EntrenarAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var valoraciones = await db.ValoracionesLugar.ToListAsync();
            if (valoraciones.Count < 5) return;

            _usuariosEntrenados = valoraciones.Select(v => v.UsuarioId).ToHashSet();
            _lugaresEntrenados  = valoraciones.Select(v => v.LugarId).ToHashSet();

            var trainingData = _ml.Data.LoadFromEnumerable(
                valoraciones.Select(v => new RatingData
                {
                    UserId = v.UsuarioId,
                    ItemId = v.LugarId.ToString(),
                    Label  = v.Puntaje
                }));

            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = "UserKey",
                MatrixRowIndexColumnName    = "ItemKey",
                LabelColumnName             = nameof(RatingData.Label),
                NumberOfIterations          = 20,
                ApproximationRank           = 8,
                LearningRate                = 0.1,
                Quiet                       = true
            };

            var pipeline = _ml.Transforms.Conversion
                .MapValueToKey(outputColumnName: "UserKey", inputColumnName: nameof(RatingData.UserId))
                .Append(_ml.Transforms.Conversion.MapValueToKey(
                    outputColumnName: "ItemKey", inputColumnName: nameof(RatingData.ItemId)))
                .Append(_ml.Recommendation().Trainers.MatrixFactorization(options));

            _model       = pipeline.Fit(trainingData);
            _lastTrained = DateTime.Now;
        }
    }

    public class RatingData
    {
        public string UserId { get; set; } = "";
        public string ItemId { get; set; } = "";
        public float  Label  { get; set; }
    }

    public class RatingPrediction
    {
        public float Label { get; set; }
        public float Score { get; set; }
    }
}
