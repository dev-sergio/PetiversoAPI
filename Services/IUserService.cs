using Microsoft.EntityFrameworkCore;
using PetiversoAPI.DTOs;
using PetiversoAPI.Models;
using static PetiversoAPI.Services.UserService;

namespace PetiversoAPI.Services
{
    public interface IUserService
    {
        Task<ServiceResponse<Guid?>> RegisterUserAsync(UserDto userDto);
        Task<AuthResponse> AuthenticateUserAsync(UserDto userDto);
        Task<ServiceResponse> LogoutUserAsync(string sessionToken);
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher _passwordHasher;

        public UserService(AppDbContext context, PasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<ServiceResponse<Guid?>> RegisterUserAsync(UserDto userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == userDto.Username))
            {
                return new ServiceResponse<Guid?>
                {
                    Success = false,
                    Message = "Já existe um usuário com esse nome."
                };
            }

            var user = new User
            {
                Username = userDto.Username,
                PasswordHash = _passwordHasher.HashPassword(userDto.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new ServiceResponse<Guid?>
            {
                Success = true,
                Data = user.UserId // UUID gerado pelo banco de dados.
            };
        }


        public class ServiceResponse<T>
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public T? Data { get; set; }
        }


        public async Task<AuthResponse> AuthenticateUserAsync(UserDto userDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);

            if (user == null || !_passwordHasher.VerifyPassword(user.PasswordHash, userDto.Password))
            {
                await RegisterLoginAttempt(user?.Id, userDto.Username, false);
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

            await RegisterLoginAttempt(user.Id, userDto.Username, true);

            return new AuthResponse { Success = true, Username = user.Username, SessionToken = sessionToken };
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

        private async Task RegisterLoginAttempt(int? userId, string username, bool success)
        {
            var loginAttempt = new LoginAttempt
            {
                UserId = userId,
                Username = username,
                Success = success,
                AttemptedAt = DateTime.UtcNow
            };

            _context.LoginAttempts.Add(loginAttempt);
            await _context.SaveChangesAsync();
        }
    }
}
