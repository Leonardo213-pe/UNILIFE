namespace Unilife.Models
{
    public class PerfilViewModel
    {
        // Datos básicos
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Carrera { get; set; }
        public string Rol { get; set; } = "";
        public string? FotoPerfil { get; set; }

        public bool EsDocente { get; set; }
        public bool EsCoordinador { get; set; }

        // ── Alumno ───────────────────────────────────────
        public List<Curso> CursosInscritos { get; set; } = new();
        public List<Tarea> TareasProximas { get; set; } = new();

        // ── Docente ──────────────────────────────────────
        public List<Curso> MisCursos { get; set; } = new();

        // ── Coordinador ──────────────────────────────────
        public int TotalUsuarios { get; set; }
        public int TotalDocentes { get; set; }
        public int TotalAlumnos { get; set; }
        public int TotalCursos { get; set; }
        public int TotalCarreras { get; set; }
        public int TotalEventos { get; set; }
    }
}
