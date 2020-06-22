using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using SYE.Repository;

namespace SYE.Services.Wrappers
{
    /// <summary>
    /// This class wraps the index client for search to allow for testing so this client can be mocked out
    /// </summary>
    public interface ICustomSearchIndexClient
    {
        Task<DocumentSearchResult> SearchAsync(string searchTerm, SearchParameters parameters);
    }

    public class CustomSearchIndexClient : ICustomSearchIndexClient
    {
        private readonly SearchIndexClient _searchIndexClient;

        public CustomSearchIndexClient(string searchServiceName, string indexName, string apiKey)
        {
            _searchIndexClient = new SearchIndexClient(searchServiceName, indexName, new SearchCredentials(apiKey));
        }

        public async Task<DocumentSearchResult> SearchAsync(string searchTerm, SearchParameters parameters)
        {
            return await _searchIndexClient.Documents.SearchAsync(searchTerm, parameters).ConfigureAwait(false);
        }
    }
}
