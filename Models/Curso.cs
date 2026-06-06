using System.ComponentModel.DataAnnotations;

namespace Unilife.Models
{
    public class Curso
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public string Carrera { get; set; } = string.Empty;

        [Required]
        public string Semestre { get; set; } = string.Empty;

        [Display(Name = "Docente")]
        public string? DocenteId { get; set; }
        public ApplicationUser? DocenteUser { get; set; }

        public ICollection<HorarioCurso> Horarios { get; set; } = new List<HorarioCurso>();
        public ICollection<CursoAlumno> Participantes { get; set; } = new List<CursoAlumno>();
        public ICollection<Modulo> Modulos { get; set; } = new List<Modulo>();
    }
}
