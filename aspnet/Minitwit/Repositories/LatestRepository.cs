using Microsoft.EntityFrameworkCore;
using Minitwit.Models.Context;
using Minitwit.Models.Entity;

namespace Minitwit.Repositories
{
    public class LatestRepository : ILatestRepository
    {
        private readonly MinitwitContext _context;

        public LatestRepository(MinitwitContext context)
        {
            _context = context;
        }

        public async Task<Latest?> GetLatest()
        {
            return await _context.Latest
                .OrderByDescending(l => l.CreationTime)
                .FirstOrDefaultAsync();
        }

        public async Task InsertLatest(Latest latest)
        {
            _context.Latest.Add(latest);
            await _context.SaveChangesAsync();
        }
    }
}
