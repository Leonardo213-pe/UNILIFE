namespace Unilife.Models
{
    public class Actividad
    {
        public int Id { get; set; }
        public int ModuloId { get; set; }
        public Modulo Modulo { get; set; } = null!;
        public string Tipo { get; set; } = "Material"; // Tarea, Material, Diapositiva, Enlace
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string? Url { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public int Orden { get; set; }
    }
}
