using System.ComponentModel.DataAnnotations;

namespace TeamFlow.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();

        [Required, MaxLength(100)]
        public string Username { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsEmailConfirmed { get; set; } = false;
        public DateTime? EmailTokenExpires { get; set; }

        public string? EmailConfirmationToken { get; set; }

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiration { get; set; }


        public ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
