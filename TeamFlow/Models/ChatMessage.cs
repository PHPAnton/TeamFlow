using System.ComponentModel.DataAnnotations;

namespace TeamFlow.Models
{
    public class ChatMessage
    {
        public Guid Id { get; set; }

        public Guid ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; } = null!;

        public Guid SenderId { get; set; }
        public User Sender { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
