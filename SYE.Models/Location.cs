using Newtonsoft.Json;

namespace SYE.Models
{
    public class Location
    {
        [JsonProperty("providerId")]
        public string ProviderId { get; set; }

        [JsonProperty("id")]
        public string LocationId { get; set; }

        [JsonProperty("locationName")]
        public string LocationName { get; set; }
    }
}
