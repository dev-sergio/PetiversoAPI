﻿using System.ComponentModel.DataAnnotations;

namespace PetiversoAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public Guid UserId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string Username { get; set; } = null!;

        [Required, MaxLength(255)]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
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
        [Key]
        public int Id { get; set; }
        public Guid? UserId { get; internal set; }
        public string? Username { get; internal set; }
        public bool Success { get; internal set; }
        public DateTime AttemptedAt { get; internal set; }
    }
}