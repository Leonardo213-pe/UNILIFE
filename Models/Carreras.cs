namespace Unilife.Models
{
    public static class Carreras
    {
        public static readonly List<string> Ingenierias = new()
        {
            "Ingeniería en Sistemas",
            "Ingeniería Civil",
            "Ingeniería Mecatrónica",
            "Ingeniería Industrial"
        };

        public static readonly List<string> CienciasSociales = new()
        {
            "Derecho",
            "Psicología",
            "Administración de Empresas",
            "Comunicación Social"
        };

        public static readonly List<string> CienciasMedicas = new()
        {
            "Medicina",
            "Enfermería",
            "Odontología",
            "Nutrición"
        };

        public static List<string> Todas =>
            Ingenierias.Concat(CienciasSociales).Concat(CienciasMedicas).ToList();
    }
}
