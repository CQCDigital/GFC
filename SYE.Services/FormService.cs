using System.Linq;
using System.Threading.Tasks;
using GDSHelpers.Models.FormSchema;
using SYE.Models;
using SYE.Models.Enums;
using SYE.Repository;

namespace SYE.Services
{
    public interface IFormService
    {
        Task<FormVM> GetLatestForm();
        Task<FormVM> GetLatestFormByName(string formName);
        Task<FormVM> GetFormById(string id);
        Task<FormVM> FindByName(string formName);
        Task<FormVM> FindByVersion(string version);
        Task<FormVM> FindByNameAndVersion(string formName, string version);
    }

    [LifeTime(LifeTime.Scoped)]
    public class FormService : IFormService
    {
        private readonly IGenericRepository<FormVM> _repo;

        public FormService(IGenericRepository<FormVM> repo)
        {
            _repo = repo;
        }

        public Task<FormVM> GetLatestForm()
        {
            return _repo.GetAsync(null, null, (x => x.LastModified));
        }

        public Task<FormVM> GetLatestFormByName(string formName)
        {
            return _repo.GetAsync((x => x.FormName == formName), null, (x => x.LastModified));
        }

        public Task<FormVM> GetFormById(string id)
        {
            return _repo.GetByIdAsync(id);
        }

        public async Task<FormVM> FindByName(string formName)
        {
            return _repo.FindByAsync(m => m.FormName == formName).Result.FirstOrDefault();
        }

        public async Task<FormVM> FindByVersion(string version)
        {
            return _repo.FindByAsync(m => m.Version == version).Result.FirstOrDefault();
        }

        public async Task<FormVM> FindByNameAndVersion(string formName, string version)
        {
            return _repo.FindByAsync(m => m.FormName == formName && m.Version == version).Result.FirstOrDefault();
        }

    }
}
