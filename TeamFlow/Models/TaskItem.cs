using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TeamFlow.Models
{
    public enum TaskStatus { New, InProgress, Completed }
    public enum TaskPriority { Low, Medium, High }

    public class TaskItem
    {
        public Guid Id { get; set; }

        
        [Required]
        public string Title { get; set; } = "";

        public string? Description { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.New;

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public DateTime? Deadline { get; set; }

        public List<string> Tags { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid ProjectId { get; set; }

        [JsonIgnore]
        public Project? Project { get; set; }

    }
}
