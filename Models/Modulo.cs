namespace Unilife.Models
{
    public class Modulo
    {
        public int Id { get; set; }
        public int CursoId { get; set; }
        public Curso Curso { get; set; } = null!;
        public string Nombre { get; set; } = "";
        public string Color { get; set; } = "teal";
        public int Orden { get; set; }
        public ICollection<Actividad> Actividades { get; set; } = new List<Actividad>();
    }
}
