using System.ComponentModel.DataAnnotations;

namespace Unilife.Models
{
    public class Evento
    {
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan Hora { get; set; }

        [Required]
        public string Lugar { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tipo de evento")]
        public string TipoEvento { get; set; } = string.Empty;

        [Display(Name = "Carrera")]
        public string? Carrera { get; set; }

        [Display(Name = "Evento general")]
        public bool EsGeneral { get; set; } = true;
    }
}