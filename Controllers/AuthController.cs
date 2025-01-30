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
    public class AuthController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            if (string.IsNullOrWhiteSpace(userDto.Username) || string.IsNullOrWhiteSpace(userDto.Password) || string.IsNullOrWhiteSpace(userDto.Email))
                return BadRequest(new { success = false, message = "Todos os campos são obrigatórios." });

            if (userDto.Username.Length < 5 || userDto.Password.Length < 8 || !userDto.Email.Contains('@'))
                return BadRequest(new { success = false, message = "Dados inválidos." });

            var result = await _userService.RegisterUserAsync(userDto);
            if (!result.Success)
                return Conflict(new { success = false, message = result.Message });

            return Ok(new { success = true, message = "Usuário registrado com sucesso.", UserId = result.Data });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (string.IsNullOrWhiteSpace(loginDto.Username) || string.IsNullOrWhiteSpace(loginDto.Password))
                return BadRequest(new { success = false, message = "Username e senha são obrigatórios." });

            var result = await _userService.AuthenticateUserAsync(loginDto);
            if (!result.Success) return Unauthorized(new { success = false, message = result.Message });            

            var claims = new List<Claim>
            {
                new("SessionToken", result.SessionToken!),
                new(ClaimTypes.NameIdentifier, result.UserId.ToString()!), // userId
                new(ClaimTypes.Name, result.Username!)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties
            {
                ExpiresUtc = DateTime.UtcNow.AddHours(8), // Validade do cookie
                IsPersistent = true
            });

            return Ok(new { success = true, message = "Login realizado com sucesso.", userId = result.UserId });
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

            return Ok(new { success = true, authenticated = isAuthenticated, username });
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetUserDetails(Guid userId)
        {
            var user = await _userService.FindUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { success = false, message = "Usuário não encontrado." });

            return Ok(new { success = true, data = user });
        }
    }
}