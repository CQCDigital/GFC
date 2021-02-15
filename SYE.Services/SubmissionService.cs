using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Configuration;
using SYE.Models;
using SYE.Models.Enums;
using SYE.Models.SubmissionSchema;
using SYE.Repository;
using Hangfire;
using SYE.Models.Enum;

namespace SYE.Services
{
    /// <summary>
    /// Submission service for the main GFC form schema
    /// </summary>
    public interface ISubmissionService
    {
        Task<SubmissionVM> CreateAsync(SubmissionVM item);
        Task DeleteAsync(string id);
        Task<SubmissionVM> GetByIdAsync(string id);
        Task<IEnumerable<SubmissionVM>> FindByAsync(Expression<Func<SubmissionVM, bool>> predicate);
        Task<SubmissionVM> UpdateAsync(string id, SubmissionVM item);
        Task<SubmissionVM> SendSubmission(string submissionId, SubmissionVM submissionDocument);
    }

    /// <inheritdoc cref="ISubmissionService"/>
    [LifeTime(LifeTime.Scoped)]
    public class SubmissionService : ISubmissionService
    {
        private readonly IGenericRepository<SubmissionVM> _repo;
        private readonly IServiceBusService _serviceBusService;
        private IBackgroundJobClient _backgroundJobClient;

        public SubmissionService(IGenericRepository<SubmissionVM> repo, IServiceBusService serviceBusService, IBackgroundJobClient backgroundJobClient)
        {
            _repo = repo;
            _serviceBusService = serviceBusService;
            _backgroundJobClient = backgroundJobClient;
        }

        public Task<SubmissionVM> CreateAsync(SubmissionVM item)
        {
            return _repo.CreateAsync(item);
        }

        public Task DeleteAsync(string id)
        {
            return _repo.DeleteAsync(id);
        }

        public Task<SubmissionVM> GetByIdAsync(string id)
        {
            return _repo.GetByIdAsync(id);
        }

        public Task<IEnumerable<SubmissionVM>> FindByAsync(Expression<Func<SubmissionVM, bool>> predicate)
        {
            return _repo.FindByAsync(predicate);
        }

        public Task<SubmissionVM> UpdateAsync(string id, SubmissionVM item)
        {
            return _repo.UpdateAsync(id, item);
        }

        /// <summary>
        /// Service-level method to send submission to appropriate providers
        /// </summary>
        /// <param name="submissionId">submission Id</param>
        /// <param name="submissionDocument">Submission content</param>
        /// <returns></returns>
        public Task<SubmissionVM> SendSubmission(string submissionId, SubmissionVM submissionDocument)
        {
            //Push cosmosDB submission
            var cosmosSubmission = UpdateAsync(submissionId, submissionDocument);

            //Start hangfire job for service bus submission
            try
            {
                _backgroundJobClient.Enqueue(() => _serviceBusService.SendMessageAsync(submissionDocument));
            }
            catch (Exception e)
            {
                //This exception will be handled further at a future point
                Console.WriteLine("Hangfire exception!");
                Console.WriteLine(e);
            }

            //Return cosmosDB submission
            return cosmosSubmission;
        }

    }
}
