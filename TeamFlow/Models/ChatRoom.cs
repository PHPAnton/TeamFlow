using System.ComponentModel.DataAnnotations;

namespace TeamFlow.Models
{
    public class ChatRoom
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        [Required]
        public string Name { get; set; } = "General";

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
