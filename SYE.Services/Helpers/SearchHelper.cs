using System.Linq;
using Document = Microsoft.Azure.Search.Models.Document;
using SearchResult = SYE.Models.SearchResult;

namespace SYE.Services.Helpers
{
    public static class SearchHelper
    {
        ///// <summary>
        ///// Generates a list of SearchResult objects form a list of Document results
        ///// </summary>
        ///// <param name="results"></param>
        ///// <returns></returns>
        //public static List<SearchResult> ConvertResults(List<Document> results)
        //{
        //    var returnList = new List<SearchResult>();
        //    if (results != null)
        //    {
        //        returnList = results.Select(doc => GetSearchResult(doc)).ToList();
        //    }
        //    return returnList;
        //}

        public static string BuildFilter(string refinementFacets)
        {
            return string.Join(" or ", refinementFacets.Split(',').Select(x => string.Concat("syeInspectionCategories/any(ins: ins eq '", x.Trim(), "')")));
        }

        /// <summary>
        /// Generates a SearchResult object from a document
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static SearchResult GetSearchResult(Document doc, int index, int currentPage)
        {      
            var searchResult = new SearchResult
            {
                Id = GetValue(doc, "id"),
                ProviderId = GetValue(doc, "providerId"),
                Name = GetValue(doc, "locationName"),
                AlsoKnownAs = GetValue(doc, "alsoKnownAs"),
                Address = GetValue(doc, "postalAddressLine1"),
                Address2 = GetValue(doc, "postalAddressLine2"),
                Town = GetValue(doc, "postalAddressTownCity"),
                PostCode = GetValue(doc, "postalCode"),
                Region = GetValue(doc, "region"),                
                Category = GetInspectionCategories(doc, "syeInspectionCategories"),
                Index = index + 1,
                Page = currentPage
            };
            return searchResult;
        }

        /// <summary>
        /// gets a single field value from a document
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string GetValue(Document doc, string key)
        {
            string returnStr = null;

            if (doc.ContainsKey(key))
            {
                doc.TryGetValue(key, out var obj);
                if (obj != null)
                {
                    returnStr = obj.ToString();
                }
            }
            return returnStr;
        }

        private static string GetInspectionCategories(Document inspectionCategory, string key)
        {
            return string.Join(", ", ((object[])inspectionCategory.Where(cn => cn.Key == "syeInspectionCategories").FirstOrDefault().Value).Distinct());
        }
    }
}
