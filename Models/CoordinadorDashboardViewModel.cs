namespace Unilife.Models
{
    public class CoordinadorDashboardViewModel
    {
        // Totales
        public int TotalAlumnos { get; set; }
        public int TotalDocentes { get; set; }
        public int TotalCursos { get; set; }
        public int TotalEventos { get; set; }
        public int TotalLugares { get; set; }

        // Métricas de uso
        public int TareasTotales { get; set; }
        public int TareasCompletadas { get; set; }
        public int PorcentajeCompletado => TareasTotales == 0 ? 0 : (int)Math.Round(TareasCompletadas * 100.0 / TareasTotales);
        public int EventosProximos { get; set; }
        public int CursosConAlumnos { get; set; }
        public int TotalInscripciones { get; set; }
        public string CarreraConMasCursos { get; set; } = "—";
    }
}
