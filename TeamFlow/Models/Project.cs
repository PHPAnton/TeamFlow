using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TeamFlow.Models
{
    public class Project
    {
        public Guid Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        
        public User? Owner { get; set; }
        public ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();

        [JsonIgnore]
        public List<TaskItem> Tasks { get; set; } = new();

        public Guid OwnerId { get; set; }
        
    }
}
