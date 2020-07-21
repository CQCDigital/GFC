using SYE.Models.Enums;
using SYE.Repository;
using SYE.Models;

namespace SYE.Services
{
    public interface ICosmosService
    {
        int GetDocumentId(string guid);
    }

    [LifeTime(LifeTime.Scoped)]
    public class CosmosService : ICosmosService
    {
        private readonly IGenericRepository<DocumentVm> _repo;

        public CosmosService(IGenericRepository<DocumentVm> repo)
        {
            _repo = repo;
        }

        public int GetDocumentId(string guid)
        {
            return _repo.GetDocumentId(guid).DocumentId;
        }

    }
}
