using System.ComponentModel.DataAnnotations;

namespace Unilife.Models
{
    public class ValoracionLugar
    {
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        public int LugarId { get; set; }

        [Range(1, 5)]
        public int Puntaje { get; set; }

        public DateTime FechaValoracion { get; set; } = DateTime.Now;

        public Lugar? Lugar { get; set; }
    }
}
