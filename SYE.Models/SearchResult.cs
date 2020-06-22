
namespace SYE.Models
{
    /// <summary>
    /// this class represent a single search result item
    /// the class should contain only those fields necessary for results display
    /// </summary>
    public class SearchResult
    {
        public int Index { get; set; }
        public int Page { get; set; }
        public string Id { get; set; }
        public string ProviderId { get; set; }
        public string Name { get; set; }
        public string AlsoKnownAs { get; set; }
        public string Category { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string Town { get; set; }
        public string PostCode { get; set; }
        public string Region { get; set; }
    }
}
