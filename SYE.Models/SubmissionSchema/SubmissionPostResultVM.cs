using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SYE.Models.SubmissionSchema
{
    public class SubmissionPostResultVM
    {
        public SubmissionPostResultVM()
        {
            Id = Guid.NewGuid().ToString();
            DateCreated = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("date_created")]
        public string DateCreated { get; set; }

        [JsonProperty("number_items_sent")]
        public int NumberItemsSent { get; set; }

        [JsonProperty("number_items_posted")]
        public int NumberItemsPosted { get; set; }

        [JsonProperty("number_items_failed")]
        public int NumberItemsFailed { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("posted_submissions")]
        public List<string> PostedSubmissions { get; set; }

        [JsonProperty("failed_submissions")]
        public List<string> FailedSubmissions { get; set; }
    }
}
