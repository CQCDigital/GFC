using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using SYE.Models;
using SYE.Models.Enums;
using SYE.Repository;
using SYE.Services.Helpers;
using SYE.Services.Wrappers;
using SearchResult = SYE.Models.SearchResult;

namespace SYE.Services
{
    public class SearchServiceResult
    {
        public long Count { get; set; }
        public IList<string> Facets { get; set; }
        public IList<SearchResult> Data { get; set; }
    }

    public interface ISearchService
    {
        Task<SearchServiceResult> GetPaginatedResult(string search, int currentPage, int pageSize, string refinementFacets, bool newSearch);
    }

    [LifeTime(LifeTime.Scoped)]
    public class SearchService : ISearchService
    {
        private readonly ICustomSearchIndexClient _indexClientWrapper;
        private static IList<string> _facets = new List<string>();

        public SearchService(ICustomSearchIndexClient indexClientWrapper)
        {
            _indexClientWrapper = indexClientWrapper;
        }

        public async Task<SearchServiceResult> GetPaginatedResult(string search, int currentPage, int pageSize, string refinementFacets, bool newSearch)
        {
            return await GetDataAsync(search, currentPage, pageSize, refinementFacets, newSearch);
        }

        #region Private Methods
        private async Task<SearchServiceResult> GetDataAsync(string search, int currentPage, int pageSize, string refinementFacets, bool newSearch)
        {
            if (newSearch)
            {
                //clear out all facets for previous search
                _facets.Clear();
                refinementFacets = string.Empty;
            }

            var sp = new SearchParameters
            {
                IncludeTotalResultCount = true,
                Skip = ((currentPage - 1) * pageSize),
                Top = pageSize,
                Facets = new List<String> { "syeInspectionCategories,count:100" }
            };

            //sp.Filter = "registrationStatus eq 'Registered'";
            if (!string.IsNullOrWhiteSpace(refinementFacets))
            {
                sp.QueryType = QueryType.Full;                
                sp.Filter = string.Concat(sp.Filter, "(", SearchHelper.BuildFilter(refinementFacets), ")");
            }

            var searchResult = await _indexClientWrapper.SearchAsync(search, sp).ConfigureAwait(false);

            if (searchResult.Facets?.Count == 1)
            {
                foreach (var item in searchResult?.Facets?.FirstOrDefault().Value?.Select(x => x.Value.ToString().Trim()))
                {
                    if (!_facets.Contains(item))
                    {
                        _facets.Add(item);
                    }
                }
            }

            var returnResults = new SearchServiceResult()
            {
                Count = searchResult.Count ?? 0,
                Facets = _facets.OrderBy(o => o).ToList(),
                Data = searchResult?.Results?.Select(x => SearchHelper.GetSearchResult(x.Document, (int )searchResult?.Results.IndexOf(x), currentPage))?.ToList() ?? new List<SearchResult>()
            };

            return returnResults;
        }
   
        #endregion

    }
}