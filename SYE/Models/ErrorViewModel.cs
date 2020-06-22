using System;

namespace SYE.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public bool InsertLink { get; set; }
        public string ErrorTitle { get; set; }
        public string ErrorMessage { get; set; }
        public string LinkPreMessage { get; set; }
        public string LinkPostMessage { get; set; }
        public string LinkMessage { get; set; }
    }
}