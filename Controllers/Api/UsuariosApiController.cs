using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Unilife.Models;
using Unilife.Models.Api;

namespace Unilife.Controllers.Api
{
    [ApiController]
    [Route("api/usuarios")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsuariosApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsuariosApiController(UserManager<ApplicationUser> userManager) => _userManager = userManager;

        /// <summary>Lista todos los usuarios por rol. Solo Coordinador.</summary>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Coordinador")]
        public async Task<IActionResult> GetAll([FromQuery] string? rol)
        {
            List<ApplicationUser> usuarios;

            if (!string.IsNullOrEmpty(rol))
                usuarios = (await _userManager.GetUsersInRoleAsync(rol)).ToList();
            else
                usuarios = _userManager.Users.ToList();

            var resultado = new List<UsuarioDto>();
            foreach (var u in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(u);
                resultado.Add(new UsuarioDto(
                    Id:         u.Id,
                    Nombre:     u.Nombre,
                    Apellido:   u.Apellido,
                    Email:      u.Email!,
                    Carrera:    u.Carrera,
                    FotoPerfil: u.FotoPerfil,
                    Rol:        roles.FirstOrDefault() ?? "Sin rol"));
            }

            return Ok(resultado);
        }

        /// <summary>Devuelve el perfil del usuario autenticado.</summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user   = await _userManager.FindByIdAsync(userId!);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new UsuarioDto(
                Id:         user.Id,
                Nombre:     user.Nombre,
                Apellido:   user.Apellido,
                Email:      user.Email!,
                Carrera:    user.Carrera,
                FotoPerfil: user.FotoPerfil,
                Rol:        roles.FirstOrDefault() ?? "Sin rol"));
        }

        /// <summary>Actualiza nombre, apellido y email del usuario autenticado.</summary>
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] ActualizarPerfilRequest req)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user   = await _userManager.FindByIdAsync(userId!);
            if (user == null) return NotFound();

            user.Nombre   = req.Nombre;
            user.Apellido = req.Apellido;
            user.Email    = req.Email;
            user.UserName = req.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            return Ok(new { mensaje = "Perfil actualizado correctamente." });
        }
    }
}
