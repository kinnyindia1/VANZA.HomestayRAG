using Microsoft.Extensions.AI;

namespace VANZA.HomestayAI.API.Models
{
    public class ChatConversation
    {
        public int ChatId { get; set; }
        public string Title { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public List<ChatMessage> Messages { get; set; } = new();
    }
}
