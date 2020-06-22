using System;
using SYE.Models.SubmissionSchema;
using SYE.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;
using ESBHelpers.Models;
using ESBHelpers.Helpers;
using Microsoft.Extensions.DependencyInjection;
using ESBHelpers.Config;
using Microsoft.Extensions.Logging;
using SYE.Models;
using SYE.Models.Enums;

namespace SYE.Services
{
    public interface IEsbService
    {
        Task<IEnumerable<SubmissionVM>> GetAllSubmisions();
        Task<IEnumerable<SubmissionVM>> GetAllSubmisions(string status);
        Task<SubmissionVM> GetSubmision(string id);
        Task<string> PostSubmision(SubmissionVM submission, ILogger logger);
    }

    [LifeTime(LifeTime.Scoped)]
    public class EsbService : IEsbService
    {
        private IEsbConfiguration<EsbConfig> _esbConfig;
        private readonly IGenericRepository<SubmissionVM> _repo;
        private IEsbWrapper _esbWrapper;

        public EsbService(IGenericRepository<SubmissionVM> repo, IServiceProvider service)
        {
            _esbConfig = service?.GetRequiredService<IEsbConfiguration<EsbConfig>>();
            _esbWrapper = service?.GetService<IEsbWrapper>();
            _repo = repo;
        }

        public async Task<IEnumerable<SubmissionVM>> GetAllSubmisions()
        {
            var results = await _repo.FindByAsync(x => x.SubmissionId != "");
            return results;
        }

        public async Task<IEnumerable<SubmissionVM>> GetAllSubmisions(string status)
        {
            var results = await _repo.FindByAsync(x => x.Status == status);
            return results;
        }

        public async Task<SubmissionVM> GetSubmision(string id)
        {
            var result = await _repo.GetAsync(x => x.SubmissionId == id, null, x => x.SubmissionId);
            return result;
        }

        public async Task<string> PostSubmision(SubmissionVM submission, ILogger logger)
        {
            var response = await _esbWrapper.PostSubmission(GetGenericAttachmentPayload(submission), logger);

            if (response.Success && !string.IsNullOrWhiteSpace(response.EnquiryId))
            {
                submission.Status = "Sent";
                var sub = await _repo.UpdateAsync(submission.Id, submission);
            }

            return response.EnquiryId;
        }
   
        private GenericAttachmentPayload GetGenericAttachmentPayload(SubmissionVM submission)
        {
            var description = string.Empty;
            var organisationId = string.Empty;
            if (submission.LocationId == "0")
            {
                organisationId = string.Empty;//no location selected
                description = "(GFC)";
            }
            else
            {
                organisationId = submission.LocationId;
                description = "(GFC) Location ID: " + submission.LocationId + " Provider ID: " + submission.ProviderId + " Location name: " + submission.LocationName;
            }

            //var submissionNumber = Guid.NewGuid().ToString().Substring(0, 8);//use this for testing because esb rejects duplicate submissionIds
            var submissionNumber = "GFC-" + submission.SubmissionId;
            var filename = submissionNumber + ".docx";

            var genPayload = new GenericAttachmentPayload
            {
                Payload = submission.Base64Attachment,
                OrganisationId = organisationId,
                Description = description,
                Filename = filename,
                SubType = PayloadType.Classified,
                SubmissionNumber = submissionNumber
            };

            return genPayload;
        }

    }

}
