
namespace VANZA.HomestayAI.API.Models
{
    public class ChatRequest
    {
        public int? ChatId { get; set; }

        public string Message { get; set; } = string.Empty;

        public string Mode { get; set; } = "auto";
    }
}
