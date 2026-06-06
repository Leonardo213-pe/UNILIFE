using Microsoft.AspNetCore.Identity;

namespace Unilife.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;

        public string? Carrera { get; set; }
        public string? FotoPerfil { get; set; }
    }
}