using Microsoft.EntityFrameworkCore;
using Minitwit.Models.Context;
using Minitwit.Models.DTO;
using Minitwit.Models.Entity;
using Prometheus;

namespace Minitwit.Repositories
{
    public class MessageRepository: IMessageRepository
    {
        private readonly MinitwitContext _context;

        private static readonly Gauge getMessageTime = Metrics.CreateGauge("getmessage_time_s", "Time of GetMessage()");
        private static readonly Gauge getPrivateTimelineTime = Metrics.CreateGauge("getprivatetimeline_time_s", "Time of GetPrivateTimeline()");
        private static readonly Gauge getMessagesByAuthorTime = Metrics.CreateGauge("getmessagesbyauthor_time_s", "Time of GetMessagesByAuthor()");
        private static readonly Gauge getMessagesTime = Metrics.CreateGauge("getmessages_time_s", "Time of GetMessages()");
        private static readonly Gauge getFilteredMessagesTime = Metrics.CreateGauge("getfilteredmessages_time_s", "Time of GetFilteredMessages()");
        private static readonly Gauge insertMessageTime = Metrics.CreateGauge("insertmessage_time_s", "Time of InsertMessages()");
        private static readonly Gauge flagMessageTime = Metrics.CreateGauge("flagmessage_time_s", "Time of FlagMessages()");

        public MessageRepository(MinitwitContext context)
        {
            _context = context;
        }

        public async Task<Message> GetMessage(int id)
        {
            using (getMessageTime.NewTimer())
            {
                return await _context.Posts
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();
            }
        }

        public async Task<List<Message>> GetPrivateTimeline(int id, List<int> follows, int limit = 30)
        {
            using (getPrivateTimelineTime.NewTimer())
            {
                return await _context.Posts
                    .Include(p => p.Author)
                    .Where(p => !p.Flagged)
                    .Where(p =>
                        follows.Contains(p.AuthorId)
                        || p.AuthorId == id
                        )
                    .OrderByDescending(p => p.PublishDate)
                    .Take(limit)
                    .ToListAsync();
            }
        }

        public async Task<List<Message>> GetMessagesByAuthorId(int id, int limit = 30)
        {
            using (getMessagesByAuthorTime.NewTimer())
            {
                return await _context.Users
                    .Include(u => u.Messages)
                    .Where(u => u.Id == id)
                    .SelectMany(u =>
                                 u.Messages
                                .OrderByDescending(p => p.PublishDate)
                                .Where(p => !p.Flagged)
                                .Take(limit)
                    )
                    .ToListAsync();
            }
        }

        public async Task<List<FilteredMessageDTO>> GetFilteredMessagesByAuthorId(int id, int limit = 100)
        {
            return await _context.Users
                .Include(u => u.Messages)
                .Where(u => u.Id == id)
                .SelectMany(u =>
                             u.Messages
                            .OrderByDescending(p => p.PublishDate)
                            .Where(p => !p.Flagged)
                            .Take(limit)
                )
                .Select(p => new FilteredMessageDTO
                {
                    Content = p.Text,
                    PublishDate = p.PublishDate,
                    AuthorName = p.Author.UserName
                })
                .ToListAsync();
        }

        public async Task<List<Message>> GetMessages(int limit = 30)
        {
            using (getMessagesTime.NewTimer())
            {
                return await _context.Posts
                    .Include(p => p.Author)
                    .Where(p => !p.Flagged)
                    .OrderByDescending(p => p.PublishDate)
                    .Take(limit)
                    .ToListAsync();
            }
        }

        public async Task<List<FilteredMessageDTO>> GetFilteredMessages(int limit = 100)
        {
            using (getFilteredMessagesTime.NewTimer())
            {
                return await _context.Posts
                    .Include(p => p.Author)
                    .Where(p => !p.Flagged)
                    .OrderByDescending(p => p.PublishDate)
                    .Take(limit)
                    .Select(p => new FilteredMessageDTO
                    {
                        Content = p.Text,
                        PublishDate = p.PublishDate,
                        AuthorName = p.Author.UserName
                    })
                    .ToListAsync();
            }
        }

        public async Task InsertMessage(Message message)
        {
            using (insertMessageTime.NewTimer())
            {
                _context.Posts.Add(message);
                await _context.SaveChangesAsync();
            }
        }

        public async Task FlagMessage(int id, bool flagged)
        {
            using (flagMessageTime.NewTimer())
            {
                _context.Posts.Where(p => p.Id == id).FirstOrDefault().Flagged = flagged;
                await _context.SaveChangesAsync();
            }
        }
    }
}
