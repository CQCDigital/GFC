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
    /// Submission service for the main GFC form schema
    /// </summary>
    public interface ISubmissionService
    {
        Task<SubmissionVM> CreateAsync(SubmissionVM item);
        Task DeleteAsync(string id);
        Task<SubmissionVM> GetByIdAsync(string id);
        Task<IEnumerable<SubmissionVM>> FindByAsync(Expression<Func<SubmissionVM, bool>> predicate);
        Task<SubmissionVM> UpdateAsync(string id, SubmissionVM item);
    }

    /// <inheritdoc cref="ISubmissionService"/>
    [LifeTime(LifeTime.Scoped)]
    public class SubmissionService : ISubmissionService
    {
        private readonly IGenericRepository<SubmissionVM> _repo;

        public SubmissionService(IGenericRepository<SubmissionVM> repo)
        {
            _repo = repo;
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

    }
}
