using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SYE.Models
{
    /// <summary>
    /// this class represents a generic action json
    /// </summary>
    public class UserActionVM
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("session")]
        public string Session { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("action_data")]
        public string ActionData { get; set; }

        [JsonProperty("action_date")]
        public DateTime ActionDate { get; set; }
    }
}
