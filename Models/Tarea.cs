using System.ComponentModel.DataAnnotations;

namespace Unilife.Models
{
    public class Tarea
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Curso { get; set; } = string.Empty;

        [Display(Name = "Fecha límite")]
        [DataType(DataType.Date)]
        public DateTime FechaEntrega { get; set; }

        [Required]
        public string Prioridad { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente";

        public string? UsuarioId { get; set; }

        public bool EsVencida => Estado != "Completada" && FechaEntrega < DateTime.Today;
    }
}
