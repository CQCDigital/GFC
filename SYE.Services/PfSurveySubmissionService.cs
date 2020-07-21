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

namespace SYE.Services
{
    /// <summary>
    /// Submission service for the post-completion user survey form
    /// </summary>
    public interface IPfSurveySubmissionService
    {
        Task<PfSurveyVM> CreateAsync(PfSurveyVM item);
        Task DeleteAsync(string id);
        Task<PfSurveyVM> GetByIdAsync(string id);
        Task<IEnumerable<PfSurveyVM>> FindByAsync(Expression<Func<PfSurveyVM, bool>> predicate);
        Task<PfSurveyVM> UpdateAsync(string id, PfSurveyVM item);
    }

    /// <inheritdoc cref="IPfSurveySubmissionService"/>
    [LifeTime(LifeTime.Scoped)]
    public class PfSurveySubmissionService : IPfSurveySubmissionService
    {
        private readonly IGenericRepository<PfSurveyVM> _repo;

        public PfSurveySubmissionService(IGenericRepository<PfSurveyVM> repo)
        {
            _repo = repo;
        }

        public Task<PfSurveyVM> CreateAsync(PfSurveyVM item)
        {
            return _repo.CreateAsync(item);
        }

        public Task DeleteAsync(string id)
        {
            return _repo.DeleteAsync(id);
        }

        public Task<PfSurveyVM> GetByIdAsync(string id)
        {
            return _repo.GetByIdAsync(id);
        }

        public Task<IEnumerable<PfSurveyVM>> FindByAsync(Expression<Func<PfSurveyVM, bool>> predicate)
        {
            return _repo.FindByAsync(predicate);
        }

        public Task<PfSurveyVM> UpdateAsync(string id, PfSurveyVM item)
        {
            return _repo.UpdateAsync(id, item);
        }

    }
}
