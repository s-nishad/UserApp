using System;
using System.ComponentModel.DataAnnotations;

namespace UserApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime LastLoginTime { get; set; } = DateTime.UtcNow;
        public DateTime RegistrationTime { get; set; } = DateTime.UtcNow;
        public bool Status { get; set; } = true; // true for "Active", false for "Blocked"
    }
}