using System.ComponentModel.DataAnnotations;

namespace Unilife.Models
{
    public class EditarPerfilViewModel
    {
        [Required]
        public string Nombre { get; set; } = "";

        [Required]
        public string Apellido { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        public string? ContrasenaActual { get; set; }

        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string? NuevaContrasena { get; set; }
    }
}
