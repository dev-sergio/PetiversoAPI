using System.ComponentModel.DataAnnotations;

namespace PetiversoAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public Guid UserId { get; set; } = Guid.NewGuid(); // Gerado pelo banco, se configurado.

        public string Username { get; set; } = null!;
        [Required]
        public string PasswordHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class Session
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string SessionToken { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }

    public class LoginAttempt
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string Username { get; set; } = null!;
        public bool Success { get; set; }
        public DateTime AttemptedAt { get; set; }
    }

    public class ServiceResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    public class AuthResponse : ServiceResponse
    {
        public string? Username { get; set; }
        public string? SessionToken { get; set; }
        public Guid UserId { get; internal set; }
    }
}
