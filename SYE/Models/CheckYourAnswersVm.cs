using System.Collections.Generic;
using GDSHelpers.Models.FormSchema;

namespace SYE.Models
{
    public class CheckYourAnswersVm
    {
        public bool SendConfirmationEmail { get; set; }

        public FormVM FormVm { get; set; }

        public string LocationName { get; set; }

        public List<string> PageHistory { get; set; }

    }
}
