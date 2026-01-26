namespace WorkFlow.Models
{
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string Role { get; set; } = "Employee"; // Employee, Manager, HR

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

