using System.Collections.Generic;
using GDSHelpers.Models.FormSchema;
using SYE.Models;

namespace SYE.ViewModels.Debug
{
    public class DebugInfoVm
    {
        public UserSessionVM UserSession { get; set; }

        public List<string> NavOrder { get; set; }

        public bool ServiceNotFound { get; set; }

        public PageVM CurrentPage { get; set; }

        public string ChangeMode { get; set; }

    }
}
