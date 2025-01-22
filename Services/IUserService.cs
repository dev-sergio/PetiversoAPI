using Microsoft.EntityFrameworkCore;
using PetiversoAPI.DTOs;
using PetiversoAPI.Models;

namespace PetiversoAPI.Services
{
    public interface IUserService
    {
        Task<ServiceResponse<Guid?>> RegisterUserAsync(UserDto userDto);
        Task<AuthResponse> AuthenticateUserAsync(LoginDto loginDto);
        Task<ServiceResponse> LogoutUserAsync(string sessionToken);
        Task<UserResponseDto?> FindUserByIdAsync(Guid userId);
    }

    public class UserService(AppDbContext context, PasswordHasher passwordHasher) : IUserService
    {
        private readonly AppDbContext _context = context;
        private readonly PasswordHasher _passwordHasher = passwordHasher;

        public async Task<ServiceResponse<Guid?>> RegisterUserAsync(UserDto userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == userDto.Username || u.Email == userDto.Email))
            {
                return new ServiceResponse<Guid?>
                {
                    Success = false,
                    Message = "Username ou Email já está em uso."
                };
            }

            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                PasswordHash = _passwordHasher.HashPassword(userDto.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new ServiceResponse<Guid?>
            {
                Success = true,
                Data = user.UserId
            };
        }

        public async Task<AuthResponse> AuthenticateUserAsync(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null || !_passwordHasher.VerifyPassword(user.PasswordHash, loginDto.Password))
            {
                await RegistrarTentativaDeLogin(user?.UserId, loginDto.Username, false);
                return new AuthResponse { Success = false, Message = "Usuário ou senha inválidos." };
            }

            var sessionToken = Guid.NewGuid().ToString();

            var session = new Session
            {
                UserId = user.Id,
                SessionToken = sessionToken,
                ExpiresAt = DateTime.UtcNow.AddHours(8)
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            await RegistrarTentativaDeLogin(user.UserId, loginDto.Username, true);


            return new AuthResponse { Success = true, Username = user.Username, SessionToken = sessionToken, UserId = user.UserId };
        }

        public async Task<ServiceResponse> LogoutUserAsync(string sessionToken)
        {
            var session = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionToken == sessionToken);

            if (session == null)
            {
                return new ServiceResponse { Success = false, Message = "Sessão não encontrada." };
            }

            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();

            return new ServiceResponse { Success = true };
        }

        public async Task<UserResponseDto?> FindUserByIdAsync(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return null;

            return new UserResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };
        }

        private async Task RegistrarTentativaDeLogin(Guid? userId, string username, bool sucesso)
        {
            var tentativaDeLogin = new LoginAttempt
            {
                UserId = userId,
                Username = username,
                Success = sucesso,
                AttemptedAt = DateTime.UtcNow
            };

            _context.LoginAttempts.Add(tentativaDeLogin);
            await _context.SaveChangesAsync();
        }

    }

    public class ServiceResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    // Versão genérica:
    public class ServiceResponse<T> : ServiceResponse
    {
        public T? Data { get; set; }
    }


    public class AuthResponse : ServiceResponse<object>
    {
        public string? Username { get; set; }
        public string? SessionToken { get; set; }
        public Guid? UserId { get; set; }
    }
}