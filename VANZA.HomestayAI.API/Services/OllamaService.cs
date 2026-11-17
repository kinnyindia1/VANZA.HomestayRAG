
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace VANZA.HomestayAI.API.Services
{
    public class OllamaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public OllamaService(HttpClient httpClient,IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
        // Common method for communicating with Ollama
        private async Task<string> AskOllamaAsync(string systemPrompt,string userPrompt)
        {
            string endpoint = _configuration["Ollama:Endpoint"]?? throw new Exception("Ollama endpoint is missing in appsettings.json.");
            string model = _configuration["Ollama:ChatModel"]?? throw new Exception("Ollama chat model is missing in appsettings.json.");
            Console.WriteLine();
            Console.WriteLine("[OLLAMA] Preparing request...");
            Console.WriteLine($"[OLLAMA] Model: {model}");
            Console.WriteLine($"[OLLAMA] Endpoint: {endpoint}");
            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.1
            };
            string json = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(json,Encoding.UTF8,"application/json");
            Console.WriteLine("[OLLAMA] Sending HTTP request...");
            using var response = await _httpClient.PostAsync(endpoint,content);
            Console.WriteLine($"[OLLAMA] HTTP Status: {(int)response.StatusCode} {response.StatusCode}");
            string responseText = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("[OLLAMA ERROR] Request failed.");
                Console.WriteLine(responseText);
                throw new Exception($"Ollama returned HTTP {(int)response.StatusCode}: {responseText}");
            }
            using JsonDocument document = JsonDocument.Parse(responseText);
            string result = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
            Console.WriteLine("[OLLAMA] Response received successfully.");
            return result.Trim();
        }
        // Step 1: Decide whether the question needs SQL or documents
        public async Task<string> ClassifyQuestionAsync(string question)
        {
            Console.WriteLine();
            Console.WriteLine("[OLLAMA] Classifying user question...");
            Console.WriteLine($"[OLLAMA] Question: {question}");
            string systemPrompt = """
                You are a question classification assistant.

                Classify the user's question into exactly one category:

                SQL:
                Questions about live homestay database information,
                including rooms, guests, bookings, payments, revenue,
                occupancy, availability and business statistics.

                DOCUMENT:
                Questions about general knowledge, homestay policies,
                facilities, instructions, documents and other information
                that does not require querying the live SQL database.

                Return only one word:
                SQL
                or
                DOCUMENT
                """;
            string result = await AskOllamaAsync(systemPrompt,question);
            string mode = result.ToUpperInvariant().Contains("SQL")? "SQL" : "DOCUMENT";
            Console.WriteLine($"[OLLAMA] Classification result: {mode}");
            return mode;
        }
        // Step 2: Generate SQL dynamically using the live database schema
        public async Task<string> GenerateSqlAsync(string question, string schema)
        {
            Console.WriteLine();
            Console.WriteLine("[OLLAMA] Generating SQL...");
            Console.WriteLine($"[OLLAMA] Question: {question}");
            Console.WriteLine($"[OLLAMA] Schema length: {schema.Length} characters");
            string systemPrompt = """
                You are an expert Microsoft SQL Server query generator
                for a homestay management application.

                Generate SQL using ONLY the tables and columns provided
                in the database schema.

                IMPORTANT BUSINESS RULES:

                1. Actual revenue means completed payments received.
                2. Use Payments.Amount for payment amounts.
                3. Use Payments.PaidAt for payment dates.
                4. Only PaymentStatus = 'Completed' counts as received revenue.
                5. Bookings.TotalAmount represents the total booking value.
                6. Bookings.AdvancePaid represents advance payment.

                SQL RULES:

                1. Generate only one SQL SELECT statement.
                2. Never generate INSERT, UPDATE, DELETE, DROP,
                   ALTER, CREATE or TRUNCATE.
                3. Do not modify database data.
                4. Use only actual table and column names from the schema.
                5. Use SQL Server syntax.
                6. Do not invent tables or columns.
                7. Do not include explanations.
                8. Do not use Markdown code fences.
                9. Return only the SQL query.
                """;
            string userPrompt = $"""
                DATABASE SCHEMA:

                {schema}

                USER QUESTION:

                {question}

                Generate the SQL Server SELECT query.
                """;

            string result = await AskOllamaAsync( systemPrompt, userPrompt);
            Console.WriteLine();
            Console.WriteLine("================================================");
            Console.WriteLine("[OLLAMA] GENERATED SQL QUERY");
            Console.WriteLine("================================================");
            Console.WriteLine(result);
            Console.WriteLine("================================================");
            return result;
        }
        // Step 3: Convert SQL results into a human-readable answer
        public async Task<string> GenerateAnswerAsync(string question,string queryResult)
        {
            Console.WriteLine();
            Console.WriteLine("[OLLAMA] Generating final answer...");
            Console.WriteLine($"[OLLAMA] Original question: {question}");
            string systemPrompt = """
                You are VANZA AI, an intelligent homestay business assistant.

                Answer the user's question using only the supplied
                SQL query results.

                Rules:
                1. Give a clear and direct answer.
                2. Use the actual numbers from the results.
                3. Do not invent information.
                4. If no records are returned, explain that no matching
                   records were found.
                5. Use Indian Rupees (₹) for monetary amounts when applicable.
                6. Keep the answer easy to understand.
                """;
            string userPrompt = $"""
                USER QUESTION:

                {question}

                SQL QUERY RESULTS:

                {queryResult}

                Explain these results to the user.
                """;

            string answer = await AskOllamaAsync(systemPrompt,userPrompt);
            Console.WriteLine("[OLLAMA] Final answer generated.");
            Console.WriteLine($"[OLLAMA] Answer: {answer}");
            return answer;
        }
    }
}
