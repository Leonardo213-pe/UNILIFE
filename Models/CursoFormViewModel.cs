using System.ComponentModel.DataAnnotations;

namespace Unilife.Models
{
    public class CursoFormViewModel
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

        public List<string> AlumnoIds { get; set; } = new();
    }
}
