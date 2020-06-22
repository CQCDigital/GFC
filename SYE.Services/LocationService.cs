using System.Threading.Tasks;
using SYE.Models;
using SYE.Models.Enums;
using SYE.Repository;

namespace SYE.Services
{
    public interface ILocationService
    {
        Task<Location> GetByIdAsync(string id);
    }

    [LifeTime(LifeTime.Scoped)]
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository<Location> _repo;

        
        public LocationService(ILocationRepository<Location> repo)
        {
            _repo = repo;
        }

        public Task<Location> GetByIdAsync(string id)
        {
            var loc = _repo.GetAsync(x => x.LocationId == id, null, x => x.LocationId).Result;
            return Task.FromResult(loc);
        }

    }
}
