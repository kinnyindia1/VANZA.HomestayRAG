
using Microsoft.EntityFrameworkCore;
using SmartRAG.Dashboard;
using SmartRAG.Enums;
using SmartRAG.Extensions;
using VANZA.HomestayAI.API.Data;
using VANZA.HomestayAI.API.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<VanzaDbContext>(options => options.UseSqlServer( builder.Configuration.GetConnectionString( "DefaultConnection")));
// Ollama
builder.Services.AddHttpClient<OllamaService>();
// Application services
builder.Services.AddScoped<SqlService>();
builder.Services.AddScoped<SmartRagService>();
builder.Services.AddScoped<ChatHistoryService>();
// SmartRAG
builder.Services.AddSmartRag(builder.Configuration, options =>
{
    options.AIProvider = AIProvider.Custom;
    options.StorageProvider = StorageProvider.Qdrant;
    options.EnableAutoSchemaAnalysis = false;
    options.Features.EnableDatabaseSearch = false;
    options.Features.EnableDocumentSearch = true;
    options.Features.EnableAudioSearch = false;
    options.Features.EnableImageSearch = false;
});
// Register API controllers
builder.Services.AddControllers();
builder.Services.AddSmartRagDashboard(options =>
{
    options.Path = "/smartrag";
});
// Allow the application to call Ollama
builder.Services.AddHttpClient();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VanzaDbContext>();
    await DatabaseSeeder.SeedAsync(db);
}
// Enable HTTPS
app.UseHttpsRedirection();
// Serve HTML, CSS and JavaScript from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSmartRagDashboard("/smartrag");
// Register API controllers
app.MapControllers();
app.MapSmartRagDashboard("/smartrag");
// Simple API test
app.MapGet("/api/health", () =>
{
    return Results.Ok(new
    {
        Status = "Running",
        Application = "VANZA Homestay RAG"
    });
});
// Open index.html when someone visits the website
app.MapFallbackToFile("index.html");
app.Run();
