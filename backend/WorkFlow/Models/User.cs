using System.Text.Json.Serialization;

namespace WorkFlow.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public UserRole Role { get; set; }
        public int? ManagerId { get; set; }
        [JsonIgnore]
        public User? Manager { get; set; }
        [JsonIgnore]
        public ICollection<User> TeamMembers { get; set; } = new List<User>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
