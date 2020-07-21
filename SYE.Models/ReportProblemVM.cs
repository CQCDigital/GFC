using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace SYE.Models
{
    /// <summary>
    /// this class represents the json that gets save specifically for report a problem action
    /// </summary>
    public class ReportProblemVM
    {
        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("email_address")]
        public string EmailAddress { get; set; }

        [JsonProperty("feedback")]
        public string Feedback { get; set; }
    }
}
