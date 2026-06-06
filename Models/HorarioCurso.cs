using System.ComponentModel.DataAnnotations;

namespace Unilife.Models
{
    public class HorarioCurso
    {
        public int Id { get; set; }

        public int CursoId { get; set; }
        public Curso Curso { get; set; } = null!;

        [Required]
        [Display(Name = "Tipo")]
        public string Tipo { get; set; } = "Teórico"; // "Teórico" o "Práctico"

        [Required]
        [Display(Name = "Día")]
        public string Dia { get; set; } = string.Empty;

        [Display(Name = "Hora de inicio")]
        [DataType(DataType.Time)]
        public TimeSpan HoraInicio { get; set; }

        [Display(Name = "Hora de fin")]
        [DataType(DataType.Time)]
        public TimeSpan HoraFin { get; set; }

        public string Pabellon { get; set; } = string.Empty;

        public string Aula { get; set; } = string.Empty;

        [Display(Name = "Código de aula")]
        public string CodigoAula => $"{Pabellon}{Aula}";
    }
}
