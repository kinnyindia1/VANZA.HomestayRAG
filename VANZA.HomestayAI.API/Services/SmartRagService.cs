
using SmartRAG.Interfaces.Document;
using SmartRAG.Models.RequestResponse;

namespace VANZA.HomestayAI.API.Services
{
    public class SmartRagService
    {
        private readonly IDocumentSearchService _searchService;

        public SmartRagService(
            IDocumentSearchService searchService)
        {
            _searchService = searchService;
        }

        public async Task<string> AskDocumentsAsync(string question)
        {
            var request = new QueryIntelligenceRequest
            {
                Query = question,
                MaxResults = 5
            };

            var response = await _searchService
                .QueryIntelligenceAsync(request);

            return response.Answer;
        }
    }
}
