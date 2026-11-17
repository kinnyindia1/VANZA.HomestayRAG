
using Microsoft.AspNetCore.Mvc;
using VANZA.HomestayAI.API.Models;
using VANZA.HomestayAI.API.Services;

namespace VANZA.HomestayAI.API.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly OllamaService _ollama;
        private readonly SqlService _sqlService;
        private readonly SmartRagService _rag;
        private readonly ChatHistoryService _chatHistory;
        public ChatController(OllamaService ollama,SqlService sqlService, SmartRagService rag, ChatHistoryService chatHistory)
        {
            _ollama = ollama;
            _sqlService = sqlService;
            _rag = rag;
            _chatHistory = chatHistory;
        }
        // GET: api/chat/conversations
        // Retrieve previous conversations
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            Console.WriteLine("[CHAT] Loading previous conversations...");
            var conversations = await _chatHistory.GetConversationsAsync();
            Console.WriteLine($"[CHAT] Conversations retrieved: {conversations.Count}");
            return Ok(conversations);
        }
        // GET: api/chat/conversations/1
        // Retrieve one conversation
        [HttpGet("conversations/{chatId:int}")]
        public async Task<IActionResult> GetConversation(int chatId)
        {
            Console.WriteLine($"[CHAT] Loading conversation: {chatId}");
            var conversation = await _chatHistory.GetConversationAsync(chatId);
            if (conversation == null)
            {
                Console.WriteLine("[CHAT] Conversation not found.");
                return NotFound(new
                {
                    message = "Conversation not found."
                });
            }
            return Ok(conversation);
        }
        // POST: api/chat/ask
        // Main AI question-processing endpoint
        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            int chatId = request.ChatId ?? 0;
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new
                    {
                        message = "Please enter a question."
                    });
                }
                Console.WriteLine();
                Console.WriteLine("================================================");
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] VANZA AI - NEW QUESTION");
                Console.WriteLine($"Question: {request.Message}");
                Console.WriteLine($"Requested Mode: {request.Mode}");
                Console.WriteLine("================================================");
                // STEP 1: Create or reuse conversation
                if (chatId <= 0)
                {
                    Console.WriteLine("[CHAT] Creating new conversation...");
                    chatId = await _chatHistory.CreateConversationAsync(
                        request.Message.Length > 80
                            ? request.Message.Substring(0, 80)
                            : request.Message);
                    Console.WriteLine($"[CHAT] New conversation ID: {chatId}");
                }
                else
                {
                    Console.WriteLine($"[CHAT] Reusing conversation ID: {chatId}");
                }
                // STEP 2: Save user question
                Console.WriteLine("[CHAT] Saving user message...");
                await _chatHistory.AddMessageAsync(chatId,"user",request.Message);
                Console.WriteLine("[CHAT] User message saved.");
                // STEP 3: Classify question
                Console.WriteLine();
                Console.WriteLine("[1] CLASSIFYING QUESTION");
                string mode = request.Mode?.Trim().ToUpperInvariant() ?? "AUTO";
                if (mode == "AUTO")
                {
                    mode = await _ollama.ClassifyQuestionAsync(request.Message);
                }
                Console.WriteLine($"[1] Selected mode: {mode}");
                string answer;
                string? generatedSql = null;
                string source;
                // STEP 4: SQL database question
                if (mode == "SQL")
                {
                    Console.WriteLine();
                    Console.WriteLine("************ SQL PROCESSING STARTED ************");
                    // Read live database schema
                    Console.WriteLine("[2] READING DATABASE SCHEMA");
                    string schema = await _sqlService.GetDatabaseSchemaAsync();
                    Console.WriteLine("[2] Schema retrieved successfully.");
                    // Generate SQL
                    Console.WriteLine();
                    Console.WriteLine("[3] GENERATING SQL");
                    generatedSql = await _ollama.GenerateSqlAsync(request.Message,schema);
                    // Remove Markdown code fences if the model returns them
                    generatedSql = generatedSql.Replace("```sql", "", StringComparison.OrdinalIgnoreCase).Replace("```", "").Trim();
                    Console.WriteLine();
                    Console.WriteLine("================================================");
                    Console.WriteLine("[3] FINAL GENERATED SQL");
                    Console.WriteLine("================================================");
                    Console.WriteLine(generatedSql);
                    Console.WriteLine("================================================");
                    // Validate SQL
                    Console.WriteLine();
                    Console.WriteLine("[4] VALIDATING SQL");
                    bool isValid = _sqlService.IsValidSelect(generatedSql);
                    Console.WriteLine($"[4] Validation result: {isValid}");
                    if (!isValid)
                    {
                        throw new Exception("Generated SQL failed read-only SELECT validation. Check the generated SQL in the console.");
                    }
                    // Execute SQL
                    Console.WriteLine();
                    Console.WriteLine("[5] EXECUTING SQL");
                    string queryResult = await _sqlService.ExecuteSelectAsync(generatedSql);
                    Console.WriteLine("[5] SQL execution completed.");
                    // Generate natural language answer
                    Console.WriteLine();
                    Console.WriteLine("[6] GENERATING AI ANSWER");
                    answer = await _ollama.GenerateAnswerAsync(request.Message, queryResult);
                    source = "SQL";
                    Console.WriteLine("[6] AI answer generated.");
                }
                // STEP 5: Document / RAG question
                else if (mode == "DOCUMENT")
                {
                    Console.WriteLine();
                    Console.WriteLine("************ DOCUMENT SEARCH STARTED ************");
                    Console.WriteLine("[RAG] Searching knowledge documents...");
                    answer = await _rag.AskDocumentsAsync(request.Message);
                    source = "DOCUMENT";
                    Console.WriteLine("[RAG] Document search completed.");
                    Console.WriteLine($"[RAG] Answer: {answer}");
                }
                else
                {
                    throw new Exception($"Unsupported question mode: {mode}");
                }
                // STEP 6: Save assistant response
                Console.WriteLine();
                Console.WriteLine("[7] SAVING ASSISTANT RESPONSE");
                await _chatHistory.AddMessageAsync(chatId,"assistant",answer,generatedSql);
                Console.WriteLine("[7] Assistant response saved.");
                // STEP 7: Return result to frontend
                Console.WriteLine();
                Console.WriteLine("================================================");
                Console.WriteLine("[SUCCESS] VANZA AI REQUEST COMPLETED");
                Console.WriteLine($"Chat ID: {chatId}");
                Console.WriteLine($"Source: {source}");
                Console.WriteLine($"Answer: {answer}");
                Console.WriteLine("================================================");
                return Ok(new
                {
                    chatId,
                    answer,
                    generatedSql,
                    source
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Console.WriteLine("[VANZA AI ERROR]");
                Console.WriteLine($"Time: {DateTime.Now}");
                Console.WriteLine($"Chat ID: {chatId}");
                Console.WriteLine($"Error Message: {ex.Message}");
                Console.WriteLine("FULL EXCEPTION:");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                return StatusCode(500, new
                {
                    message = "Unable to process your question.",
                    error = ex.ToString()
                });
            }
        }
    }
}
