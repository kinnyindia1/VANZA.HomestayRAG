namespace VANZA.HomestayAI.API.Models
{
    public class ChatMessage
    {
        public int MessageId { get; set; }
        public int ChatId { get; set; }
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
        public string? GeneratedSql { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ChatConversation? Conversation { get; set; }
    }
}
