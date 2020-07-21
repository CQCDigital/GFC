using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SYE.Models.SubmissionSchema
{
    public class PfSurveyVM : GenericFormSubmission
    {
        public PfSurveyVM()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
