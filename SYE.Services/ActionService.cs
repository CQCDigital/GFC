using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GDSHelpers.Models.FormSchema;
using SYE.Models;
using SYE.Models.Enums;
using SYE.Models.SubmissionSchema;
using SYE.Repository;

namespace SYE.Services
{
    /// <summary>
    /// this service is specific to ths action collection that records users actions
    /// </summary>
    public interface IActionService
    {
        Task<UserActionVM> CreateAsync(UserActionVM item);
        Task DeleteAsync(string id);
        Task<UserActionVM> GetByIdAsync(string id);
        Task<IEnumerable<UserActionVM>> FindByAsync(Expression<Func<UserActionVM, bool>> predicate);
        Task<UserActionVM> UpdateAsync(string id, UserActionVM item);
    }

    [LifeTime(LifeTime.Scoped)]
    public class ActionService : IActionService
    {
        private readonly IGenericRepository<UserActionVM> _repo;

        public ActionService(IGenericRepository<UserActionVM> repo)
        {
            _repo = repo;
        }

        public Task<UserActionVM> CreateAsync(UserActionVM item)
        {
            return _repo.CreateAsync(item);
        }

        public Task DeleteAsync(string id)
        {
            return _repo.DeleteAsync(id);
        }

        public Task<UserActionVM> GetByIdAsync(string id)
        {
            return _repo.GetByIdAsync(id);
        }

        public Task<IEnumerable<UserActionVM>> FindByAsync(Expression<Func<UserActionVM, bool>> predicate)
        {
            return _repo.FindByAsync(predicate);
        }

        public Task<UserActionVM> UpdateAsync(string id, UserActionVM item)
        {
            return _repo.UpdateAsync(id, item);
        }
    }
}
