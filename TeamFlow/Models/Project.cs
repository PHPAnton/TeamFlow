using System.ComponentModel.DataAnnotations;

namespace TeamFlow.Models
{
    public class Project
    {
        public Guid Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public Guid OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();
    }
}
