using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SYE.ViewModels
{
    public class Paging
    {
        public string StartPage { get; set; }
        public string HidePagingIfStartPageEqauls { get; set; }
        public int StartPagingAtPageNo { get; set; }
    }


    public class GetHelp
    {
        public string ContactNumber { get; set; }
        public string ContactHours { get; set; }
        public string ContactExcluding { get; set; }
    }

    public class GFCUrls
    {
        public string StartPage { get; set; }
        public string CQCHomepage { get; set; }
        public string RedirectUrl { get; set; }
        public string ConfirmationPage { get; set; }
    }

    public class SiteTextStrings
    {
        public string ReviewPage { get; set; }
        public string ReviewPageId { get; set; }
        public string BackLinkText { get; set; }
        public string SiteTitle { get; set; }
        public string SiteTitleSuffix { get; set; }
        public string DefaultServiceName { get; set; }
        public string EmptySearchError { get; set; }
        public string NonSelectedServiceName { get; set; }
        public List<string> CategoriesForExactlyWhereQuestion { get; set; }
    }

    public class PageIdStrings
    {
        public string TellUsWhichCharityPage { get; set; }
        public string WhereItHappenedPage { get; set; }
        public string WhenItHappenedPage { get; set; }
    }

    
    public class ApplicationSettings
    {
        public string AppName { get; set; }
        public string FormStartPage { get; set; }
        public string ExternalStartPage { get; set; }
        public string ServiceNotFoundPage { get; set; }
        public string DefaultBackLink { get; set; }
        public GetHelp GetHelp { get; set; }
        public string AllowedCorsDomains { get; set; }
        public GFCUrls GFCUrls { get; set; }
        public SiteTextStrings SiteTextStrings { get; set; }
        public PageIdStrings PageIdStrings { get; set; }
        public Paging Paging { get; set; }
        public QuestionStrings QuestionStrings { get; set; }
    }

    public class QuestionStrings
    {
        public GoodBadFeedbackQuestion GoodBadFeedbackQuestion { get; set; }

    }

    public class GoodBadFeedbackQuestion
    {
        public string id { get; set; }
        public string GoodFeedbackAnswer { get; set; }
    }


    public class CQCRedirection
    {
        public string GFCKey { get; set; }
        public string GFCPassword { get; set; }
    }

}
