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
        private readonly ILogger<MessageRepository> _logger;


        private static readonly Gauge getMessageTime = Metrics.CreateGauge("getmessage_time_s", "Time of GetMessage()");
        private static readonly Gauge getPrivateTimelineTime = Metrics.CreateGauge("getprivatetimeline_time_s", "Time of GetPrivateTimeline()");
        private static readonly Gauge getMessagesByAuthorTime = Metrics.CreateGauge("getmessagesbyauthor_time_s", "Time of GetMessagesByAuthor()");
        private static readonly Gauge getFilteredMessagesByAuthorTime = Metrics.CreateGauge("getfilteredmessagesbyauthor_time_s", "Time of GetFilteredMessagesByAuthor()");
        private static readonly Gauge getMessagesTime = Metrics.CreateGauge("getmessages_time_s", "Time of GetMessages()");
        private static readonly Gauge getFilteredMessagesTime = Metrics.CreateGauge("getfilteredmessages_time_s", "Time of GetFilteredMessages()");
        private static readonly Gauge insertMessageTime = Metrics.CreateGauge("insertmessage_time_s", "Time of InsertMessages()");
        private static readonly Gauge flagMessageTime = Metrics.CreateGauge("flagmessage_time_s", "Time of FlagMessages()");

        public MessageRepository(MinitwitContext context, ILogger<MessageRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Message>> GetPrivateTimeline(int id, List<int> follows, int limit = 30)
        {
            _logger.LogDebug($"Called GetPrivateTimeline() with arguments {id}, {follows}, {limit}");
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
            _logger.LogDebug($"Called GetMessagesByAuthorId() with arguments {id}, {limit}");
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
            _logger.LogDebug($"Called GetFilteredMessagesByAuthorId() with arguments {id}, {limit}");

            using (getFilteredMessagesByAuthorTime.NewTimer())
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
        }

        public async Task<List<Message>> GetMessages(int limit = 30)
        {
            _logger.LogDebug($"Called GetMessages() with arguments {limit}");

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
            _logger.LogDebug($"Called GetFilteredMessages() with arguments {limit}");

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
            _logger.LogDebug($"Called GetFilteredMessages() with arguments {message}");
            using (insertMessageTime.NewTimer())
            {
                _context.Posts.Add(message);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Inserted new message {message}");
            }
        }

        public async Task<bool> FlagMessage(int messageId, bool flagged)
        {
            _logger.LogDebug($"Called FlagMessage() with arguments {messageId}, {flagged}");

            using (flagMessageTime.NewTimer())
            {
                Message? message;
                using (getMessageTime.NewTimer())
                {
                     message = await _context.Posts
                        .FirstOrDefaultAsync(p => p.Id == messageId);
                }
                if (message == null) return false;

                message.Flagged = flagged;
                await _context.SaveChangesAsync();
                return true;
            }
        }
    }
}
