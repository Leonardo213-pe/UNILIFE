namespace Unilife.Models
{
    public class CursoAlumno
    {
        public int CursoId { get; set; }
        public Curso Curso { get; set; } = null!;

        public string AlumnoId { get; set; } = string.Empty;
        public ApplicationUser Alumno { get; set; } = null!;
    }
}
