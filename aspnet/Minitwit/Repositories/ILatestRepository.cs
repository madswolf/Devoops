using Minitwit.Models.Entity;

namespace Minitwit.Repositories
{
    public interface ILatestRepository
    {
        public Task<Latest> GetLatest();

        public Task InsertLatest(Latest latest);
    }
}
