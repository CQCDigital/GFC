
namespace SYE.ViewModels
{
    public class BackLinkVM
    {
        public BackLinkVM()
        {
            Show = false;
        }

        public bool Show { get; set; }

        public string Url { get; set; }

        public string Text { get; set; }

        public string BackLink =>
            !string.IsNullOrEmpty(Url) && !string.IsNullOrEmpty(Text)
                ? $"<a href=\"{Url}\" class=\"govuk-back-link\">{Text}</a>"
                : "";
    }
}
