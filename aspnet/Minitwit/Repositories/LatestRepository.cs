using Microsoft.EntityFrameworkCore;
using Minitwit.Models.Context;
using Minitwit.Models.Entity;
using Prometheus;

namespace Minitwit.Repositories
{
    public class LatestRepository : ILatestRepository
    {
        private readonly MinitwitContext _context;
        private readonly ILogger<LatestRepository> _logger;

        private static readonly Gauge getLatestTime = Metrics.CreateGauge("getlatest_time_s", "Time of GetLatest()");
        private static readonly Gauge insertLatestTime = Metrics.CreateGauge("insertlatest_time_s", "Time of InsertLatest()");

        public LatestRepository(MinitwitContext context, ILogger<LatestRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        //we can't handle a latest request if none are present... return empty latest
        public async Task<Latest> GetLatest()
        {
            _logger.LogDebug("Called GetLatest()");
            using (getLatestTime.NewTimer())
            {
                return await _context.Latest
                    .OrderByDescending(l => l.CreationTime)
                    .FirstOrDefaultAsync() ?? new Latest();
            }
        }

        public async Task InsertLatest(Latest latest)
        {
            _logger.LogDebug("Called InsertLatest()");
            using (insertLatestTime.NewTimer())
            {
                await _context.Latest.AddAsync(latest);
                await _context.SaveChangesAsync();
            }
        }
    }
}
