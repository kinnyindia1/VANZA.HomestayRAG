
using Microsoft.EntityFrameworkCore;
using VANZA.HomestayAI.API.Data;
using VANZA.HomestayAI.API.Models;

namespace VANZA.HomestayAI.API.Services
{
    public class ChatHistoryService
    {
        private readonly VanzaDbContext _db;

        public ChatHistoryService(VanzaDbContext db)
        {
            _db = db;
        }

        // Create a new conversation
        public async Task<int> CreateConversationAsync(string title)
        {
            var conversation = new ChatConversation
            {
                Title = title,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _db.ChatConversations.Add(conversation);

            await _db.SaveChangesAsync();

            return conversation.ChatId;
        }

        // Save a message
        public async Task AddMessageAsync(
            int chatId,
            string role,
            string content,
            string? generatedSql = null)
        {
            var conversation = await _db.ChatConversations
                .FirstOrDefaultAsync(x => x.ChatId == chatId);

            if (conversation == null)
                throw new Exception("Conversation not found.");

            var message = new ChatMessage
            {
                ChatId = chatId,
                Role = role,
                Content = content,
                GeneratedSql = generatedSql,
                CreatedAt = DateTime.Now
            };

            _db.ChatMessages.Add(message);

            conversation.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
        }

        // Get all conversations
        public async Task<List<ChatConversation>> GetConversationsAsync()
        {
            return await _db.ChatConversations
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();
        }

        // Get one conversation with its messages
        public async Task<ChatConversation?> GetConversationAsync(int chatId)
        {
            return await _db.ChatConversations
                .Include(x => x.Messages)
                .FirstOrDefaultAsync(x => x.ChatId == chatId);
        }
    }
}
