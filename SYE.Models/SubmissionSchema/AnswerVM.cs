using Newtonsoft.Json;

namespace SYE.Models.SubmissionSchema
{
    public class AnswerVM
    {
        [JsonProperty("page_id")]
        public string PageId { get; set; }


        [JsonProperty("question_id")]
        public string QuestionId { get; set; }


        [JsonProperty("question")]
        public string Question { get; set; }

        
        [JsonProperty("answer")]
        public string Answer { get; set; }

        [JsonProperty("document_order")]
        public int? DocumentOrder { get; set; }
    }
}
