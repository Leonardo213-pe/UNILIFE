namespace Unilife.Models
{
    public class DashboardViewModel
    {
        public string Nombre { get; set; } = "Estudiante";
        public bool EsDocente { get; set; }

        // Estadísticas
        public int Completadas { get; set; }
        public int Pendientes { get; set; }
        public int EventosProximos { get; set; }
        public int Progreso { get; set; }

        // Docente-specific
        public List<Curso> MisCursos { get; set; } = new();
        public int TotalAlumnos { get; set; }

        // Listas comunes
        public List<Tarea> ProximasTareas { get; set; } = new();
        public List<Curso> Horario { get; set; } = new();
        public List<Evento> Recordatorios { get; set; } = new();
    }
}
