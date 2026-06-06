using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Unilife.Models;
using Unilife.Models.Api;

namespace Unilife.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;

        public AuthApiController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config)
        {
            _userManager  = userManager;
            _signInManager = signInManager;
            _config        = config;
        }

        /// <summary>Obtiene un token JWT con las credenciales de usuario.</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user == null)
                return Unauthorized(new { mensaje = "Credenciales incorrectas." });

            if (await _userManager.IsLockedOutAsync(user))
                return Unauthorized(new { mensaje = "Cuenta bloqueada temporalmente." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
                return Unauthorized(new { mensaje = "Credenciales incorrectas." });

            var roles = await _userManager.GetRolesAsync(user);
            var rol   = roles.FirstOrDefault() ?? "Sin rol";
            var token = GenerarToken(user, rol);

            return Ok(new TokenResponse(
                Token:   token,
                Nombre:  user.Nombre,
                Email:   user.Email!,
                Rol:     rol,
                Expira:  DateTime.UtcNow.AddHours(int.Parse(_config["Jwt:ExpiresHours"]!))
            ));
        }

        /// <summary>Devuelve los datos del usuario autenticado.</summary>
        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> Me()
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
                Rol:        roles.FirstOrDefault() ?? "Sin rol"
            ));
        }

        private string GenerarToken(ApplicationUser user, string rol)
        {
            var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expira  = DateTime.UtcNow.AddHours(int.Parse(_config["Jwt:ExpiresHours"]!));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email,          user.Email!),
                new Claim(ClaimTypes.Name,           $"{user.Nombre} {user.Apellido}"),
                new Claim(ClaimTypes.Role,           rol),
                new Claim("carrera",                 user.Carrera ?? ""),
            };

            var token = new JwtSecurityToken(
                issuer:             _config["Jwt:Issuer"],
                audience:           _config["Jwt:Audience"],
                claims:             claims,
                expires:            expira,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
