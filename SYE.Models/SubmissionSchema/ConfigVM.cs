using Newtonsoft.Json;

namespace SYE.Models.SubmissionSchema
{
    public class ConfigVM
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("lastGeneratedRef")]
        public string LastGeneratedRef { get; set; }
    }
}
