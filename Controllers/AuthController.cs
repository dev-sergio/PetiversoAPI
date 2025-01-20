// Arquivo: Controllers/AuthController.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PetiversoAPI.Services;
using PetiversoAPI.DTOs;

namespace PetiversoAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            if (string.IsNullOrWhiteSpace(userDto.Username) || string.IsNullOrWhiteSpace(userDto.Password))
                return BadRequest(new { success = false, message = "Usuario e senha são obrigatórios." });

            if (userDto.Username.Length < 5 || userDto.Password.Length < 8)
                return BadRequest(new { success = false, message = "Usuario deve ter no mínimo 5 caracteres e senha, 8 caracteres." });

            var result = await _userService.RegisterUserAsync(userDto);
            if (!result.Success)
                return Conflict(new { success = false, message = result.Message });

            return Ok(new { success = true, message = "Usuário registrado com sucesso.", UserId = result.Data });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDto userDto)
        {
            if (string.IsNullOrWhiteSpace(userDto.Username) || string.IsNullOrWhiteSpace(userDto.Password))
                return BadRequest(new { success = false, message = "Username e senha são obrigatórios." });

            var result = await _userService.AuthenticateUserAsync(userDto);
            if (!result.Success)
                return Unauthorized(new { success = false, message = result.Message });

            var claims = new List<Claim>
            {
                new Claim("SessionToken", result.SessionToken!),
                new Claim(ClaimTypes.Name, result.Username!)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return Ok(new { success = true, message = "Login realizado com sucesso." });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var sessionToken = User.FindFirst("SessionToken")?.Value;
            if (sessionToken == null)
                return BadRequest(new { success = false, message = "Sessão não encontrada." });

            var result = await _userService.LogoutUserAsync(sessionToken);
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { success = true, message = "Logout realizado com sucesso." });
        }

        [HttpGet("authenticated")]
        public IActionResult IsAuthenticated()
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var username = isAuthenticated ? User.FindFirst(ClaimTypes.Name)?.Value : null;

            return Ok(new { success = true, authenticated = isAuthenticated, username = username });
        }
    }
}
